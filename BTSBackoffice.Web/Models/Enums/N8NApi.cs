using System.ComponentModel;

namespace BTSBackoffice.Web.Models.Enums;

public enum N8NApi
{
    [Description("get-order-infos")]
    GetOrderInfos,

    [Description("get-revenue-summary")]
    GetRevenueSummary,

    [Description("get-refund-data")]
    GetRefundData,

    [Description("get-vendor-settlement")]
    GetVendorSettlement
}

public static class N8NApiExtensions
{
    private static readonly Dictionary<N8NApi, string> ApiRouteMapping = new()
    {
        { N8NApi.GetOrderInfos, "get-order-infos" },
        { N8NApi.GetRevenueSummary, "get-revenue-summary" },
        { N8NApi.GetRefundData, "get-refund-data" },
        { N8NApi.GetVendorSettlement, "get-vendor-settlement" }
    };

    public static string GetRoute(this N8NApi api)
    {
        return ApiRouteMapping.TryGetValue(api, out var route) ? route : api.ToString().ToLower();
    }

    public static string GetDescription(this N8NApi api)
    {
        var field = api.GetType().GetField(api.ToString());
        var attribute = (DescriptionAttribute?)field?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
        return attribute?.Description ?? api.ToString();
    }

    public static Dictionary<N8NApi, string> GetAllRoutes()
    {
        return new Dictionary<N8NApi, string>(ApiRouteMapping);
    }
}