using BTSBackoffice.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTSBackoffice.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index(string range = "today", DateTime? startDate = null, DateTime? endDate = null)
    {
        // Check authentication
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var viewModel = await _dashboardService.GetDashboardDataAsync(range, startDate, endDate);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard for range: {Range}", range);
            TempData["Error"] = ex.Message;

            // Return empty view model to prevent crash
            var emptyViewModel = new BTSBackoffice.Web.Models.ViewModels.DashboardViewModel
            {
                SelectedRange = range,
                CustomStartDate = startDate,
                CustomEndDate = endDate,
                LastSync = DateTime.UtcNow.AddHours(8)
            };

            return View(emptyViewModel);
        }
    }

    [HttpGet("/api/dashboard")]
    public async Task<IActionResult> GetDashboardData(string range = "today", DateTime? startDate = null, DateTime? endDate = null)
    {
        // Check authentication
        if (HttpContext.Session.GetString("IsAuthenticated") != "true")
        {
            return Unauthorized();
        }

        try
        {
            var viewModel = await _dashboardService.GetDashboardDataAsync(range, startDate, endDate);
            return Json(new
            {
                success = true,
                data = viewModel
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API error loading dashboard for range: {Range}", range);
            return Json(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}