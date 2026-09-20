using LicensePlatform.Application.Interfaces;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LicensePlatform.Api.Controllers;

[ApiController]
[Route("api/v1/admin/plans")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PlansController : ControllerBase
{
    private readonly ILicensePlanService _planService;
    private readonly ILogger<PlansController> _logger;

    public PlansController(ILicensePlanService planService, ILogger<PlansController> logger)
    {
        _planService = planService;
        _logger = logger;
    }

    [HttpGet("product/{productId:guid}")]
    public IActionResult GetPlansByProduct(Guid productId)
    {
        var plans = _planService.GetPlansByProduct(productId);
        return Ok(plans);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetPlanById(Guid id)
    {
        var plan = _planService.GetPlanById(id);
        if (plan == null)
            return NotFound(new { message = $"Plan with id '{id}' not found." });
        return Ok(plan);
    }

    [HttpPost]
    public IActionResult CreatePlan([FromBody] CreateLicensePlanRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var plan = _planService.CreatePlan(dto);
        return CreatedAtAction(nameof(GetPlanById), new { id = plan.PlanId }, plan);
    }

    [HttpPut("{id:guid}")]
    public IActionResult UpdatePlan(Guid id, [FromBody] CreateLicensePlanRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var plan = _planService.UpdatePlan(id, dto);
        if (plan == null)
            return NotFound(new { message = $"Plan with id '{id}' not found." });
        return Ok(plan);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeletePlan(Guid id)
    {
        var success = _planService.DeletePlan(id);
        if (!success)
            return NotFound(new { message = $"Plan with id '{id}' not found." });
        return NoContent();
    }
}
