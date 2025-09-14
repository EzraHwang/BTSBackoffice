using BTSBackoffice.Web.Models.DTOs;
using BTSBackoffice.Web.Models.ViewModels;

namespace BTSBackoffice.Web.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync(string timeRange, DateTime? startDate = null, DateTime? endDate = null);
}

public class DashboardService : IDashboardService
{
    private readonly IN8NApiService _n8nApiService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IN8NApiService n8nApiService, ILogger<DashboardService> logger)
    {
        _n8nApiService = n8nApiService;
        _logger = logger;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync(string timeRange, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var apiData = await _n8nApiService.GetDashboardDataAsync(timeRange, startDate, endDate);

            var viewModel = new DashboardViewModel
            {
                TotalTicketsSold = apiData.TotalTicketsSold,
                TotalUsers = apiData.TotalUsers,
                TotalPurchasingUsers = apiData.TotalPurchasingUsers,
                SelectedRange = timeRange,
                CustomStartDate = startDate,
                CustomEndDate = endDate,
                LastSync = DateTime.UtcNow.AddHours(8), // GMT+8
                DailyTickets = apiData.DailyTickets.Select(d => new SeriesPoint
                {
                    Date = DateOnly.ParseExact(d.Date, "yyyy-MM-dd"),
                    Value = d.Value
                }).ToList(),
                TicketTypes = apiData.TicketTypes,
                HourlyPurchaseDistribution = apiData.HourlyPurchaseDistribution,
                PopularStations = apiData.PopularStations
            };

            _logger.LogInformation("Dashboard data retrieved successfully for range: {TimeRange}", timeRange);
            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data for range: {TimeRange}", timeRange);
            throw;
        }
    }

    private static string GetChartColor(int index)
    {
        var colors = new[]
        {
            "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF",
            "#FF9F40", "#C9CBCF", "#4BC0C0", "#FF6384", "#36A2EB"
        };
        return colors[index % colors.Length];
    }
}