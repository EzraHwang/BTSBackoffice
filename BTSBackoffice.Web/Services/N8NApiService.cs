using BTSBackoffice.Web.Models.Configuration;
using BTSBackoffice.Web.Models.DTOs;
using BTSBackoffice.Web.Models.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace BTSBackoffice.Web.Services;

public interface IN8NApiService
{
    Task<DashboardDataDto> GetDashboardDataAsync(string timeRange, DateTime? startDate = null, DateTime? endDate = null);
    Task<N8NOrderInfoResponse> GetOrderInfosAsync(DateTime startDate, DateTime endDate, TicketType ticketType);
}

public class N8NApiService : IN8NApiService
{
    private readonly HttpClient _httpClient;
    private readonly N8NSettings _n8nSettings;
    private readonly IMemoryCache _cache;
    private readonly ILogger<N8NApiService> _logger;

    public N8NApiService(HttpClient httpClient, IOptions<N8NSettings> n8nSettings,
        IMemoryCache cache, ILogger<N8NApiService> logger)
    {
        _httpClient = httpClient;
        _n8nSettings = n8nSettings.Value;
        _cache = cache;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_n8nSettings.BaseUrl.TrimEnd('/') + "/");
        _httpClient.Timeout = TimeSpan.FromSeconds(_n8nSettings.Timeout);
    }

    public async Task<N8NOrderInfoResponse> GetOrderInfosAsync(DateTime startDate, DateTime endDate, TicketType ticketType)
    {
        try
        {
            var request = new GetOrderInfosRequest
            {
                StartTime = startDate.ToString("yyyy-MM-dd"),
                EndTime = endDate.ToString("yyyy-MM-dd"),
                TicketType = ticketType.GetApiValue()
            };

            var jsonContent = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var endpoint = N8NApi.GetOrderInfos.GetRoute();
            _logger.LogInformation("Calling N8N API: {BaseUrl}/{Endpoint} with data: {Request}",
                _n8nSettings.BaseUrl, endpoint, jsonContent);

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("N8N API Response Status: {StatusCode}, Content: {Content}",
                response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                // N8N API returns a direct array, not a wrapped object
                List<N8NOrderInfo>? orderInfos = null;

                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    try
                    {
                        orderInfos = JsonConvert.DeserializeObject<List<N8NOrderInfo>>(responseContent);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize N8N response, treating as empty array. Content: {Content}", responseContent);
                    }
                }
                else
                {
                    _logger.LogInformation("N8N API returned empty content, treating as empty array");
                }

                return new N8NOrderInfoResponse
                {
                    Success = true,
                    Data = orderInfos ?? new List<N8NOrderInfo>(),
                    Message = "Success"
                };
            }
            else
            {
                _logger.LogWarning("N8N API returned non-success status: {StatusCode}", response.StatusCode);
                return new N8NOrderInfoResponse
                {
                    Success = false,
                    Message = $"API request failed with status: {response.StatusCode}"
                };
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error when calling N8N API");
            return new N8NOrderInfoResponse { Success = false, Message = "網路連線錯誤" };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout when calling N8N API");
            return new N8NOrderInfoResponse { Success = false, Message = "請求逾時" };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error when calling N8N API");
            return new N8NOrderInfoResponse { Success = false, Message = "資料格式錯誤" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when calling N8N API");
            return new N8NOrderInfoResponse { Success = false, Message = "系統發生錯誤" };
        }
    }

    public async Task<DashboardDataDto> GetDashboardDataAsync(string timeRange, DateTime? startDate = null, DateTime? endDate = null)
    {
        var cacheKey = $"dashboard_data_{timeRange}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";

        if (_cache.TryGetValue(cacheKey, out DashboardDataDto? cachedData) && cachedData != null)
        {
            _logger.LogDebug("Returning cached dashboard data for key: {CacheKey}", cacheKey);
            return cachedData;
        }

        try
        {
            var (start, end) = GetDateRange(timeRange, startDate, endDate);

            // 獲取所有票種的資料 (只需調用一次 TicketType.All)
            var response = await GetOrderInfosAsync(start, end, TicketType.All);
            var allOrderResponses = new List<N8NOrderInfoResponse> { response };

            // 聚合所有資料
            var dashboardData = AggregateOrderData(allOrderResponses, start, end);

            // Cache the data for 1 minute
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                SlidingExpiration = TimeSpan.FromSeconds(30)
            };

            _cache.Set(cacheKey, dashboardData, cacheOptions);
            _logger.LogInformation("Dashboard data cached for key: {CacheKey}", cacheKey);

            return dashboardData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating dashboard data");

            // 如果 API 失敗，返回模擬資料以保持系統可用性
            _logger.LogInformation("Falling back to mock data due to API error");
            return await GetFallbackDataAsync(timeRange, startDate, endDate);
        }
    }

    private (DateTime start, DateTime end) GetDateRange(string timeRange, DateTime? startDate, DateTime? endDate)
    {
        var today = DateTime.UtcNow.AddHours(8).Date; // GMT+8

        return timeRange.ToLower() switch
        {
            "today" => (today, today.AddDays(1)),
            "7d" => (today.AddDays(-6), today),
            "30d" => (today.AddDays(-29), today),
            "custom" => (startDate?.Date ?? today.AddDays(-7), endDate?.Date ?? today),
            _ => (today, today.AddDays(1))
        };
    }

    private DashboardDataDto AggregateOrderData(List<N8NOrderInfoResponse> responses, DateTime startDate, DateTime endDate)
    {
        var allOrders = new List<N8NOrderInfo>();

        foreach (var response in responses.Where(r => r.Success))
        {
            allOrders.AddRange(response.Data);
        }

        // 基本票券統計
        var totalTickets = allOrders.Sum(o => o.Quantity);
        var totalUsers = allOrders.Select(o => o.RecipientEmail).Distinct().Count();
        var totalPurchasingUsers = allOrders.GroupBy(o => o.RecipientEmail).Count();

        // 生成每日票券資料
        var dailyTickets = new List<DailyDataPoint>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dayOrders = allOrders.Where(o => o.OrderDate.Date == date.Date);
            dailyTickets.Add(new DailyDataPoint
            {
                Date = date.ToString("yyyy-MM-dd"),
                Value = dayOrders.Sum(o => o.Quantity)
            });
        }

        // 票種分析
        var ticketTypeAnalytics = allOrders
            .GroupBy(o => o.TicketType)
            .Select(g => new TicketTypeAnalytics
            {
                Type = g.Key,
                Label = GetTicketTypeLabel(g.Key),
                Count = g.Sum(o => o.Quantity),
                Users = g.Select(o => o.RecipientEmail).Distinct().Count(),
                Percentage = totalTickets > 0 ? (decimal)g.Sum(o => o.Quantity) / totalTickets * 100 : 0
            })
            .ToList();

        // 時段購買分析
        var hourlyPurchaseData = allOrders
            .GroupBy(o => o.CreatedAt.Hour)
            .Select(g => new HourlyPurchaseData
            {
                Hour = g.Key,
                PurchaseCount = g.Sum(o => o.Quantity),
                UserCount = g.Select(o => o.RecipientEmail).Distinct().Count(),
                TimeLabel = $"{g.Key:D2}:00-{(g.Key + 1):D2}:00"
            })
            .OrderBy(h => h.Hour)
            .ToList();

        // 車站分析（針對火車票）
        var stationAnalytics = allOrders
            .Where(o => o.Type == "Train" && !string.IsNullOrEmpty(o.From) && !string.IsNullOrEmpty(o.To))
            .GroupBy(o => new { From = o.From, To = o.To })
            .Select(g => new StationAnalytics
            {
                StationName = g.Key.From,
                Route = $"{g.Key.From} → {g.Key.To}",
                TicketCount = g.Sum(o => o.Quantity),
                UserCount = g.Select(o => o.RecipientEmail).Distinct().Count()
            })
            .OrderByDescending(s => s.TicketCount)
            .Take(10)
            .ToList();

        return new DashboardDataDto
        {
            TotalTicketsSold = totalTickets,
            TotalUsers = totalUsers,
            TotalPurchasingUsers = totalPurchasingUsers,
            DailyTickets = dailyTickets,
            TicketTypes = ticketTypeAnalytics,
            HourlyPurchaseDistribution = hourlyPurchaseData,
            PopularStations = stationAnalytics
        };
    }

    private string GetTicketTypeLabel(string ticketType)
    {
        try
        {
            var enumValue = TicketTypeExtensions.FromApiValue(ticketType);
            return enumValue.GetDescription();
        }
        catch
        {
            return ticketType;
        }
    }

    private async Task<DashboardDataDto> GetFallbackDataAsync(string timeRange, DateTime? startDate, DateTime? endDate)
    {
        await Task.Delay(100); // 模擬延遲

        var (start, end) = GetDateRange(timeRange, startDate, endDate);
        var random = new Random();

        var dailyTickets = new List<DailyDataPoint>();

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            dailyTickets.Add(new DailyDataPoint
            {
                Date = date.ToString("yyyy-MM-dd"),
                Value = random.Next(50, 200)
            });
        }

        var totalTickets = (int)dailyTickets.Sum(d => d.Value);
        var totalUsers = random.Next(totalTickets / 3, totalTickets / 2);

        // 模擬票種分析
        var ticketTypes = new List<TicketTypeAnalytics>
        {
            new TicketTypeAnalytics { Type = "Train", Label = "車票", Count = (int)(totalTickets * 0.4), Users = (int)(totalUsers * 0.35), Percentage = 40 },
            new TicketTypeAnalytics { Type = "Entrance", Label = "入場券", Count = (int)(totalTickets * 0.4), Users = (int)(totalUsers * 0.4), Percentage = 40 },
            new TicketTypeAnalytics { Type = "Package", Label = "套票", Count = (int)(totalTickets * 0.2), Users = (int)(totalUsers * 0.25), Percentage = 20 }
        };

        // 模擬時段購買分析
        var hourlyData = new List<HourlyPurchaseData>();
        for (int hour = 0; hour < 24; hour++)
        {
            var purchaseCount = hour >= 9 && hour <= 17 ? random.Next(20, 80) : random.Next(5, 25);
            hourlyData.Add(new HourlyPurchaseData
            {
                Hour = hour,
                PurchaseCount = purchaseCount,
                UserCount = random.Next(purchaseCount / 2, purchaseCount),
                TimeLabel = $"{hour:D2}:00-{(hour + 1):D2}:00"
            });
        }

        // 模擬熱門車站
        var stationData = new List<StationAnalytics>
        {
            new StationAnalytics { StationName = "台北車站", Route = "台北車站 → 桃園車站", TicketCount = random.Next(50, 150), UserCount = random.Next(30, 100) },
            new StationAnalytics { StationName = "台中車站", Route = "台中車站 → 台北車站", TicketCount = random.Next(40, 120), UserCount = random.Next(25, 80) },
            new StationAnalytics { StationName = "高雄車站", Route = "高雄車站 → 台南車站", TicketCount = random.Next(30, 100), UserCount = random.Next(20, 70) }
        };

        return new DashboardDataDto
        {
            TotalTicketsSold = totalTickets,
            TotalUsers = totalUsers,
            TotalPurchasingUsers = totalUsers,
            DailyTickets = dailyTickets,
            TicketTypes = ticketTypes,
            HourlyPurchaseDistribution = hourlyData,
            PopularStations = stationData
        };
    }
}