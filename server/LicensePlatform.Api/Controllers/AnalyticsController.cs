using LicensePlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LicensePlatform.Api.Controllers;

[ApiController]
[Route("api/v1/admin/analytics")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("global")]
    public IActionResult GetGlobalAnalytics()
    {
        var analytics = _analyticsService.GetGlobalAnalytics();
        return Ok(analytics);
    }

    [HttpGet("product/{productId:guid}")]
    public IActionResult GetProductAnalytics(Guid productId)
    {
        var analytics = _analyticsService.GetProductAnalytics(productId);
        return Ok(analytics);
    }

    [HttpGet("activations")]
    public IActionResult GetActivationsByDay(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? productId)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        var data = _analyticsService.GetActivationsByDay(start, end, productId);
        return Ok(data);
    }

    [HttpGet("validations")]
    public IActionResult GetValidationsByDay(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? productId)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        var data = _analyticsService.GetValidationsByDay(start, end, productId);
        return Ok(data);
    }
}
