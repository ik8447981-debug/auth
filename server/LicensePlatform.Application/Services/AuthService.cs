using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using LicensePlatform.Application.Interfaces;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Infrastructure.Data;
using LicensePlatform.Shared.Constants;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LicensePlatform.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly LicensePlatformDbContext _context;
        private readonly IAuditService _auditService;
        private readonly string _jwtSecret;
        private readonly int _tokenExpirationMinutes;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        private static readonly Dictionary<string, (int Count, DateTime WindowStart)> _failedAttempts = new();
        private static readonly object _lock = new();

        public AuthService(
            LicensePlatformDbContext context,
            IAuditService auditService,
            string jwtSecret,
            int tokenExpirationMinutes = 60,
            string issuer = "LicensePlatform",
            string audience = "LicensePlatformApp")
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _jwtSecret = jwtSecret ?? throw new ArgumentNullException(nameof(jwtSecret));
            _tokenExpirationMinutes = tokenExpirationMinutes;
            _jwtIssuer = issuer;
            _jwtAudience = audience;
        }

        public AdminLoginResponse Login(AdminLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new AdminLoginResponse
                {
                    Success = false,
                    Error = "Username and password are required."
                };
            }

            string rateLimitKey = $"login_{request.Username.ToLowerInvariant()}";

            lock (_lock)
            {
                if (_failedAttempts.TryGetValue(rateLimitKey, out var attempt))
                {
                    if (attempt.Count >= SystemConstants.MaxRetryCount &&
                        (DateTime.UtcNow - attempt.WindowStart).TotalMinutes < 15)
                    {
                        return new AdminLoginResponse
                        {
                            Success = false,
                            Error = "Too many failed login attempts. Please try again later."
                        };
                    }

                    if ((DateTime.UtcNow - attempt.WindowStart).TotalMinutes >= 15)
                    {
                        _failedAttempts.Remove(rateLimitKey);
                    }
                }
            }

            var admin = _context.AdminUsers
                .FirstOrDefault(a => a.Username.ToLower() == request.Username.Trim().ToLower() && a.IsActive);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
            {
                lock (_lock)
                {
                    if (_failedAttempts.TryGetValue(rateLimitKey, out var attempt))
                    {
                        _failedAttempts[rateLimitKey] = (attempt.Count + 1, attempt.WindowStart);
                    }
                    else
                    {
                        _failedAttempts[rateLimitKey] = (1, DateTime.UtcNow);
                    }
                }

                _auditService.Log(
                    AuditAction.AdminLoginFailed,
                    "Admin",
                    request.Username,
                    null,
                    request.Username,
                    null,
                    "Failed",
                    JsonSerializer.Serialize(new { Reason = "Invalid credentials" }),
                    null);

                return new AdminLoginResponse
                {
                    Success = false,
                    Error = "Invalid username or password."
                };
            }

            lock (_lock)
            {
                _failedAttempts.Remove(rateLimitKey);
            }

            admin.LastLoginAt = DateTime.UtcNow;
            _context.SaveChanges();

            string token = GenerateJwtToken(admin);
            DateTime expiresAt = DateTime.UtcNow.AddMinutes(_tokenExpirationMinutes);

            _auditService.Log(
                AuditAction.AdminLogin,
                "Admin",
                admin.AdminUserId.ToString(),
                admin.AdminUserId.ToString(),
                admin.Username,
                null,
                "Success",
                null,
                null);

            return new AdminLoginResponse
            {
                Success = true,
                Token = token,
                ExpiresAt = expiresAt,
                Admin = new AdminUserResponse
                {
                    AdminUserId = admin.AdminUserId,
                    Username = admin.Username,
                    Email = admin.Email,
                    Role = admin.Role.ToString(),
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                    LastLoginAt = admin.LastLoginAt
                }
            };
        }

        public AdminUserResponse CreateAdmin(AdminCreateRequest request, AdminRole callerRole)
        {
            if (callerRole != AdminRole.SuperAdmin)
                throw new UnauthorizedAccessException("Only SuperAdmin can create new admin users.");

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("Username is required.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.");

            if (_context.AdminUsers.Any(a => a.Username.ToLower() == request.Username.Trim().ToLower()))
                throw new InvalidOperationException($"An admin with username '{request.Username}' already exists.");

            if (_context.AdminUsers.Any(a => a.Email.ToLower() == request.Email.Trim().ToLower()))
                throw new InvalidOperationException($"An admin with email '{request.Email}' already exists.");

            if (!Enum.TryParse<AdminRole>(request.Role, true, out var role))
                throw new InvalidOperationException($"Invalid role: '{request.Role}'.");

            var admin = new AdminUser
            {
                AdminUserId = Guid.NewGuid(),
                Username = request.Username.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            return new AdminUserResponse
            {
                AdminUserId = admin.AdminUserId,
                Username = admin.Username,
                Email = admin.Email,
                Role = admin.Role.ToString(),
                IsActive = admin.IsActive,
                CreatedAt = admin.CreatedAt,
                LastLoginAt = admin.LastLoginAt
            };
        }

        public List<AdminUserResponse> GetAdminUsers()
        {
            return _context.AdminUsers
                .Where(a => a.IsActive)
                .OrderBy(a => a.Username)
                .Select(a => new AdminUserResponse
                {
                    AdminUserId = a.AdminUserId,
                    Username = a.Username,
                    Email = a.Email,
                    Role = a.Role.ToString(),
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    LastLoginAt = a.LastLoginAt
                })
                .ToList();
        }

        public bool UpdateAdminRole(Guid id, AdminRole newRole, AdminRole callerRole)
        {
            if (callerRole != AdminRole.SuperAdmin)
                throw new UnauthorizedAccessException("Only SuperAdmin can change admin roles.");

            var admin = _context.AdminUsers
                .FirstOrDefault(a => a.AdminUserId == id && a.IsActive);

            if (admin == null)
                return false;

            if (admin.Role == AdminRole.SuperAdmin && newRole != AdminRole.SuperAdmin)
            {
                int superAdminCount = _context.AdminUsers.Count(a => a.Role == AdminRole.SuperAdmin && a.IsActive);
                if (superAdminCount <= 1)
                    throw new InvalidOperationException("Cannot downgrade the last SuperAdmin.");
            }

            admin.Role = newRole;
            _context.SaveChanges();
            return true;
        }

        public bool DeactivateAdmin(Guid id)
        {
            var admin = _context.AdminUsers
                .FirstOrDefault(a => a.AdminUserId == id && a.IsActive);

            if (admin == null)
                return false;

            if (admin.Role == AdminRole.SuperAdmin)
            {
                int superAdminCount = _context.AdminUsers.Count(a => a.Role == AdminRole.SuperAdmin && a.IsActive);
                if (superAdminCount <= 1)
                    throw new InvalidOperationException("Cannot deactivate the last SuperAdmin.");
            }

            admin.IsActive = false;
            _context.SaveChanges();
            return true;
        }

        public AdminUserResponse ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(SystemConstants.ClockToleranceSeconds)
                }, out SecurityToken validatedToken);

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                    throw new SecurityTokenException("Invalid token claims.");

                var admin = _context.AdminUsers
                    .FirstOrDefault(a => a.AdminUserId == userId && a.IsActive);

                if (admin == null)
                    throw new SecurityTokenException("Admin user not found or inactive.");

                return new AdminUserResponse
                {
                    AdminUserId = admin.AdminUserId,
                    Username = admin.Username,
                    Email = admin.Email,
                    Role = admin.Role.ToString(),
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                    LastLoginAt = admin.LastLoginAt
                };
            }
            catch (SecurityTokenException)
            {
                throw new SecurityTokenException("Invalid or expired token.");
            }
            catch (Exception ex) when (!(ex is SecurityTokenException))
            {
                throw new SecurityTokenException("Token validation failed.");
            }
        }

        public bool ChangePassword(Guid id, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword))
                throw new ArgumentException("Current password is required.");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                throw new ArgumentException("New password must be at least 8 characters.");

            var admin = _context.AdminUsers
                .FirstOrDefault(a => a.AdminUserId == id && a.IsActive);

            if (admin == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, admin.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect.");

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.SaveChanges();
            return true;
        }

        private string GenerateJwtToken(AdminUser admin)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, admin.AdminUserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, admin.Username),
                new Claim(JwtRegisteredClaimNames.Email, admin.Email),
                new Claim(ClaimTypes.Role, admin.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_tokenExpirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
