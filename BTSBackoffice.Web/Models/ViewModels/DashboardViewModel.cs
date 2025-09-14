using BTSBackoffice.Web.Models.DTOs;

namespace BTSBackoffice.Web.Models.ViewModels;

public class DashboardViewModel
{
    // Ticket Analytics
    public int TotalTicketsSold { get; set; }
    public int TotalUsers { get; set; }
    public int TotalPurchasingUsers { get; set; }

    // Daily Ticket Statistics
    public List<SeriesPoint> DailyTickets { get; set; } = new();

    // Ticket Type Distribution & Analytics
    public List<TicketTypeAnalytics> TicketTypes { get; set; } = new();

    // Time-based Purchase Analytics
    public List<HourlyPurchaseData> HourlyPurchaseDistribution { get; set; } = new();

    // Station Analytics (for Train tickets)
    public List<StationAnalytics> PopularStations { get; set; } = new();

    // UI State
    public DateTime LastSync { get; set; }
    public string SelectedRange { get; set; } = "today";
    public DateTime? CustomStartDate { get; set; }
    public DateTime? CustomEndDate { get; set; }
}

public class SeriesPoint
{
    public DateOnly Date { get; set; }
    public decimal Value { get; set; }
}