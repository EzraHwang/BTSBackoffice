namespace BTSBackoffice.Web.Models.DTOs;

public class DashboardDataDto
{
    // Ticket Analytics
    public int TotalTicketsSold { get; set; }
    public int TotalUsers { get; set; }
    public int TotalPurchasingUsers { get; set; }

    // Daily Ticket Statistics
    public List<DailyDataPoint> DailyTickets { get; set; } = new();

    // Ticket Type Distribution & Analytics
    public List<TicketTypeAnalytics> TicketTypes { get; set; } = new();

    // Time-based Purchase Analytics
    public List<HourlyPurchaseData> HourlyPurchaseDistribution { get; set; } = new();

    // Station Analytics (for Train tickets)
    public List<StationAnalytics> PopularStations { get; set; } = new();
}

public class DailyDataPoint
{
    public string Date { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class TicketTypeAnalytics
{
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Users { get; set; }
    public decimal Percentage { get; set; }
}

public class HourlyPurchaseData
{
    public int Hour { get; set; }
    public int PurchaseCount { get; set; }
    public int UserCount { get; set; }
    public string TimeLabel { get; set; } = string.Empty;
}

public class StationAnalytics
{
    public string StationName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public int UserCount { get; set; }
}