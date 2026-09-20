using LicensePlatform.Application.Interfaces;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;
using LicensePlatform.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LicensePlatform.Api.Controllers;

[ApiController]
[Route("api/v1/client")]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public class ClientController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IDeviceService deviceService, ILogger<ClientController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    [HttpPost("activate")]
    public IActionResult Activate([FromBody] ActivateDeviceRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Activation request received for license key: {LicenseKey}, Machine: {DeviceFingerprint}",
            dto.LicenseKey, dto.DeviceFingerprint);

        var result = _deviceService.RegisterDevice(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Status });

        return Ok(result);
    }

    [HttpPost("validate")]
    public IActionResult Validate([FromBody] ValidateLicenseRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = _deviceService.ValidateDevice(dto);
        if (!result.Success)
            return Unauthorized(new { message = result.Status });

        return Ok(result);
    }

    [HttpPost("deactivate")]
    public IActionResult Deactivate([FromBody] DeactivateDeviceRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Note: IDeviceService does not have a dedicated deactivate method.
        // This is a placeholder that returns a not-implemented response.
        // TODO: Implement device deactivation in IDeviceService.
        return StatusCode(501, new { message = "Device deactivation is not yet implemented." });
    }

    [HttpPost("heartbeat")]
    public IActionResult Heartbeat([FromBody] ClientHeartbeatRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Note: IDeviceService does not have a heartbeat method.
        // This is a placeholder that returns a not-implemented response.
        // TODO: Implement heartbeat in IDeviceService.
        return StatusCode(501, new { message = "Heartbeat is not yet implemented." });
    }
}

public record ClientHeartbeatRequest(string LicenseKey, string DeviceFingerprint);
