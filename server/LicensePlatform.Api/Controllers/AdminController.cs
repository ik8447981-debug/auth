using LicensePlatform.Application.Interfaces;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;
using LicensePlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LicensePlatform.Api.Controllers;

[ApiController]
[Route("api/v1/admin/auth")]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAuthService authService,
        IConfiguration configuration,
        ILogger<AdminController> logger)
    {
        _authService = authService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] AdminLoginRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = _authService.Login(dto);
        if (!result.Success)
            return Unauthorized(new { message = result.Error ?? "Invalid credentials." });

        return Ok(new
        {
            result.Token,
            result.ExpiresAt,
            user = result.Admin != null ? new
            {
                Id = result.Admin.AdminUserId,
                result.Admin.Email,
                result.Admin.Username,
                result.Admin.Role
            } : null
        });
    }

    [HttpPost("users")]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult CreateAdmin([FromBody] AdminCreateRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var admin = _authService.CreateAdmin(dto, AdminRole.SuperAdmin);
        return CreatedAtAction(nameof(GetAdminUsers), new { }, admin);
    }

    [HttpGet("users")]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult GetAdminUsers()
    {
        var users = _authService.GetAdminUsers();
        return Ok(users);
    }

    [HttpPut("users/{id:guid}/role")]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult UpdateAdminRole(Guid id, [FromBody] UpdateRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!Enum.TryParse<AdminRole>(dto.Role, true, out var newRole))
            return BadRequest(new { message = $"Invalid role '{dto.Role}'." });

        var success = _authService.UpdateAdminRole(id, newRole, AdminRole.SuperAdmin);
        if (!success)
            return NotFound(new { message = $"Admin user with id '{id}' not found." });
        return Ok(new { message = "Role updated successfully." });
    }

    [HttpPut("users/{id:guid}/deactivate")]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult DeactivateAdmin(Guid id)
    {
        var success = _authService.DeactivateAdmin(id);
        if (!success)
            return NotFound(new { message = $"Admin user with id '{id}' not found." });
        return NoContent();
    }
}

public record UpdateRoleDto(string Role);
