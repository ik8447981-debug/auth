using LicensePlatform.Application.Interfaces;
using LicensePlatform.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LicensePlatform.Api.Controllers;

[ApiController]
[Route("api/v1/admin/devices")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(IDeviceService deviceService, ILogger<DevicesController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetDevices(
        [FromQuery] Guid? licenseId,
        [FromQuery] Guid? customerId)
    {
        List<LicenseDeviceResponse> result;
        if (licenseId.HasValue)
            result = _deviceService.GetDevicesByLicense(licenseId.Value);
        else if (customerId.HasValue)
            result = _deviceService.GetDevicesByCustomer(customerId.Value);
        else
            result = new List<LicenseDeviceResponse>();

        return Ok(result);
    }

    [HttpPost("{id:guid}/reset")]
    public IActionResult ResetDevice(Guid id)
    {
        var success = _deviceService.ResetDevice(id);
        if (!success)
            return NotFound(new { message = $"Device with id '{id}' not found." });
        return Ok(new { message = "Device reset successfully." });
    }

    [HttpPost("{id:guid}/suspend")]
    public IActionResult SuspendDevice(Guid id)
    {
        var success = _deviceService.SuspendDevice(id);
        if (!success)
            return NotFound(new { message = $"Device with id '{id}' not found." });
        return Ok(new { message = "Device suspended successfully." });
    }

    [HttpDelete("{id:guid}")]
    public IActionResult RemoveDevice(Guid id)
    {
        var success = _deviceService.RemoveDevice(id);
        if (!success)
            return NotFound(new { message = $"Device with id '{id}' not found." });
        return NoContent();
    }
}
