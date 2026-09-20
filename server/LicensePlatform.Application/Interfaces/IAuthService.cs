using System;
using System.Collections.Generic;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;

namespace LicensePlatform.Application.Interfaces
{
    public interface IAuthService
    {
        AdminLoginResponse Login(AdminLoginRequest request);
        AdminUserResponse CreateAdmin(AdminCreateRequest request, AdminRole callerRole);
        List<AdminUserResponse> GetAdminUsers();
        bool UpdateAdminRole(Guid id, AdminRole newRole, AdminRole callerRole);
        bool DeactivateAdmin(Guid id);
        AdminUserResponse ValidateToken(string token);
        bool ChangePassword(Guid id, string oldPassword, string newPassword);
    }

    public class AdminLoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public AdminUserResponse? Admin { get; set; }
        public string? Error { get; set; }
    }
}
