using BTSBackoffice.Web.Models.Enums;
using Newtonsoft.Json;

namespace BTSBackoffice.Web.Models.DTOs;

// N8N API Request Models
public class GetOrderInfosRequest
{
    [JsonProperty("StartTime")]
    public string StartTime { get; set; } = string.Empty;

    [JsonProperty("EndTime")]
    public string EndTime { get; set; } = string.Empty;

    [JsonProperty("TicketType")]
    public string TicketType { get; set; } = string.Empty;
}

// N8N API Response Models - Direct array response
public class N8NOrderInfoResponse
{
    public bool Success { get; set; } = true;
    public List<N8NOrderInfo> Data { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class N8NOrderInfo
{
    [JsonProperty("row_number")]
    public int RowNumber { get; set; }

    [JsonProperty("PaymentRefno")]
    public string PaymentRefno { get; set; } = string.Empty;

    [JsonProperty("RecipientEmail")]
    public string RecipientEmail { get; set; } = string.Empty;

    [JsonProperty("Ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonProperty("TicketId")]
    public string TicketId { get; set; } = string.Empty;

    [JsonProperty("FamilyName")]
    public string FamilyName { get; set; } = string.Empty;

    [JsonProperty("GivenName")]
    public string GivenName { get; set; } = string.Empty;

    [JsonProperty("IsAdult")]
    public bool IsAdult { get; set; }

    [JsonProperty("Session")]
    public string Session { get; set; } = string.Empty;

    [JsonProperty("ArrivalTime")]
    public string ArrivalTime { get; set; } = string.Empty;

    [JsonProperty("Prize")]
    public decimal Prize { get; set; }

    [JsonProperty("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("BundleName")]
    public string BundleName { get; set; } = string.Empty;

    [JsonProperty("EntranceName")]
    public string EntranceName { get; set; } = string.Empty;

    [JsonProperty("From")]
    public string From { get; set; } = string.Empty;

    [JsonProperty("To")]
    public string To { get; set; } = string.Empty;

    [JsonProperty("Phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonProperty("PassportNumber")]
    public string PassportNumber { get; set; } = string.Empty;

    [JsonProperty("Birthday")]
    public string Birthday { get; set; } = string.Empty;

    [JsonProperty("Gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonProperty("Status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("TicketUrl")]
    public string TicketUrl { get; set; } = string.Empty;

    [JsonProperty("BossEmailId")]
    public string BossEmailId { get; set; } = string.Empty;

    [JsonProperty("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("LastUpdateAt")]
    public DateTime LastUpdateAt { get; set; }

    // Helper properties for dashboard calculations
    public DateTime OrderDate => CreatedAt;
    public decimal TotalAmount => Prize;
    public int Quantity => 1; // Each record is one ticket
    public string TicketType => Type;
}

// Dashboard aggregated data from N8N responses
public class N8NDashboardAggregation
{
    public decimal TotalRevenue { get; set; }
    public int TotalTickets { get; set; }
    public decimal TotalRefunds { get; set; }
    public decimal TotalVendorSettlement { get; set; }
    public decimal TotalStripeAmount { get; set; }
    public Dictionary<string, List<DailyDataPoint>> DailyData { get; set; } = new();
    public Dictionary<string, int> TicketTypeDistribution { get; set; } = new();
}