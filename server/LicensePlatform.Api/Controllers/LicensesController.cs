using LicensePlatform.Application.Interfaces;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;
using LicensePlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LicensePlatform.Api.Controllers;

[ApiController]
[Route("api/v1/admin/licenses")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class LicensesController : ControllerBase
{
    private readonly ILicenseService _licenseService;
    private readonly ILogger<LicensesController> _logger;

    public LicensesController(ILicenseService licenseService, ILogger<LicensesController> logger)
    {
        _licenseService = licenseService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAllLicenses(
        [FromQuery] Guid? productId,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        LicenseStatus? licenseStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<LicenseStatus>(status, true, out var parsed))
            licenseStatus = parsed;

        var result = _licenseService.GetAllLicenses(page, pageSize, productId, licenseStatus, search);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetLicenseById(Guid id)
    {
        var license = _licenseService.GetLicenseById(id);
        if (license == null)
            return NotFound(new { message = $"License with id '{id}' not found." });
        return Ok(license);
    }

    [HttpPost]
    public IActionResult GenerateLicense([FromBody] GenerateLicenseRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var license = _licenseService.GenerateLicense(dto);
        return CreatedAtAction(nameof(GetLicenseById), new { id = license.LicenseId }, license);
    }

    [HttpPost("bulk")]
    public IActionResult BulkGenerateLicenses([FromBody] BulkGenerateLicenseRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var licenses = _licenseService.BulkGenerateLicenses(dto);
        return Ok(licenses);
    }

    [HttpPut("{id:guid}/extend")]
    public IActionResult ExtendLicense(Guid id, [FromBody] ExtendLicenseRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var license = _licenseService.ExtendLicense(id, dto);
        if (license == null)
            return NotFound(new { message = $"License with id '{id}' not found." });
        return Ok(license);
    }

    [HttpPut("{id:guid}/suspend")]
    public IActionResult SuspendLicense(Guid id)
    {
        var license = _licenseService.SuspendLicense(id, new SuspendLicenseRequest { Reason = "Suspended by admin" });
        if (license == null)
            return NotFound(new { message = $"License with id '{id}' not found." });
        return Ok(license);
    }

    [HttpPut("{id:guid}/revoke")]
    public IActionResult RevokeLicense(Guid id)
    {
        var license = _licenseService.RevokeLicense(id, new RevokeLicenseRequest { Reason = "Revoked by admin" });
        if (license == null)
            return NotFound(new { message = $"License with id '{id}' not found." });
        return Ok(license);
    }

    [HttpPut("{id:guid}/activate")]
    public IActionResult ActivateLicense(Guid id)
    {
        var license = _licenseService.ActivateLicense(id);
        if (license == null)
            return NotFound(new { message = $"License with id '{id}' not found." });
        return Ok(license);
    }

    [HttpPost("export")]
    public IActionResult ExportLicensesCsv([FromQuery] Guid? productId, [FromQuery] string? status)
    {
        LicenseStatus? licenseStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<LicenseStatus>(status, true, out var parsed))
            licenseStatus = parsed;

        var fileBytes = _licenseService.ExportLicensesCsv(productId ?? Guid.Empty, licenseStatus);
        return File(fileBytes, "text/csv", $"licenses_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
