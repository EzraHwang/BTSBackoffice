using System.ComponentModel;

namespace BTSBackoffice.Web.Models.Enums;

public enum TicketType
{
    [Description("全部")]
    All,

    [Description("車票")]
    Train,

    [Description("入場券")]
    Entrance,

    [Description("套票")]
    Package
}

public static class TicketTypeExtensions
{
    public static string GetDescription(this TicketType ticketType)
    {
        var field = ticketType.GetType().GetField(ticketType.ToString());
        var attribute = (DescriptionAttribute?)field?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
        return attribute?.Description ?? ticketType.ToString();
    }

    public static string GetApiValue(this TicketType ticketType)
    {
        return ticketType switch
        {
            TicketType.All => "All",
            TicketType.Train => "Train",
            TicketType.Entrance => "Entrance",
            TicketType.Package => "Package",
            _ => ticketType.ToString()
        };
    }

    public static TicketType FromApiValue(string apiValue)
    {
        return apiValue switch
        {
            "All" => TicketType.All,
            "Train" => TicketType.Train,
            "Entrance" => TicketType.Entrance,
            "Package" => TicketType.Package,
            _ => throw new ArgumentException($"Unknown ticket type: {apiValue}")
        };
    }
}