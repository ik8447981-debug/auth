using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using LicensePlatform.Application.Interfaces;
using LicensePlatform.Application.Validators;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Infrastructure.Data;
using LicensePlatform.Security.LicenseGeneration;
using LicensePlatform.Shared.Constants;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Application.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly LicensePlatformDbContext _context;
        private readonly ILicenseKeyGenerator _keyGenerator;
        private readonly IAuditService _auditService;

        public LicenseService(
            LicensePlatformDbContext context,
            ILicenseKeyGenerator keyGenerator,
            IAuditService auditService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public PaginatedResponse<LicenseResponse> GetAllLicenses(
            int page, int pageSize, Guid? productId, LicenseStatus? status, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Customer)
                .Include(l => l.Plan)
                .Include(l => l.Devices)
                .AsQueryable();

            if (productId.HasValue)
                query = query.Where(l => l.ProductId == productId.Value);

            if (status.HasValue)
                query = query.Where(l => l.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.Trim().ToLowerInvariant();
                query = query.Where(l =>
                    l.LicenseKey.ToLower().Contains(searchLower) ||
                    (l.Customer != null && l.Customer.Name.ToLower().Contains(searchLower)) ||
                    l.Product.ProductName.ToLower().Contains(searchLower));
            }

            query = query.OrderByDescending(l => l.CreatedAt);

            int totalCount = query.Count();

            var licenses = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = licenses.Select(MapToResponse).ToList();

            return new PaginatedResponse<LicenseResponse>(items, totalCount, page, pageSize);
        }

        public LicenseDetailResponse GetLicenseById(Guid id)
        {
            var license = _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Customer)
                .Include(l => l.Plan)
                .Include(l => l.Devices)
                .Include(l => l.LicenseFeatures)
                    .ThenInclude(lf => lf.Feature)
                .FirstOrDefault(l => l.LicenseId == id);

            if (license == null)
                throw new KeyNotFoundException($"License with ID '{id}' not found.");

            return MapToDetailResponse(license);
        }

        public LicenseDetailResponse GetLicenseByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("License key cannot be empty.");

            var license = _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Customer)
                .Include(l => l.Plan)
                .Include(l => l.Devices)
                .Include(l => l.LicenseFeatures)
                    .ThenInclude(lf => lf.Feature)
                .FirstOrDefault(l => l.LicenseKey == key.Trim().ToUpperInvariant());

            if (license == null)
                throw new KeyNotFoundException($"License with key '{key}' not found.");

            return MapToDetailResponse(license);
        }

        public LicenseResponse GenerateLicense(GenerateLicenseRequest request)
        {
            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == request.ProductId && !p.IsDeleted);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID '{request.ProductId}' not found.");

            var customer = _context.Customers
                .FirstOrDefault(c => c.CustomerId == request.CustomerId && c.IsActive);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID '{request.CustomerId}' not found.");

            var plan = _context.LicensePlans
                .Include(p => p.Product)
                .FirstOrDefault(p => p.PlanId == request.PlanId);
            if (plan == null)
                throw new KeyNotFoundException($"License plan with ID '{request.PlanId}' not found.");

            if (plan.ProductId != request.ProductId)
                throw new InvalidOperationException("The selected plan does not belong to the specified product.");

            if (!Enum.TryParse<LicenseType>(request.LicenseType, true, out var licenseType))
                throw new InvalidOperationException($"Invalid license type: '{request.LicenseType}'.");

            DateTime startDate = request.StartDate == default ? DateTime.UtcNow : request.StartDate;
            DateTime? expiryDate = request.ExpiryDate;

            if (plan.DurationDays > 0 && !expiryDate.HasValue)
            {
                expiryDate = startDate.AddDays(plan.DurationDays);
            }

            string licenseKey = GenerateUniqueKey(product.ProductCode);

            int maxDevices = request.MaxDevices > 0 ? request.MaxDevices : plan.MaxDevices;

            var license = new License
            {
                LicenseId = Guid.NewGuid(),
                LicenseKey = licenseKey,
                ProductId = request.ProductId,
                CustomerId = request.CustomerId,
                PlanId = request.PlanId,
                LicenseType = licenseType,
                Status = LicenseStatus.Created,
                StartDate = startDate,
                ExpiryDate = expiryDate,
                MaxDevices = maxDevices,
                FailedValidationCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Licenses.Add(license);

            if (request.FeatureIds != null && request.FeatureIds.Count > 0)
            {
                foreach (var featureId in request.FeatureIds)
                {
                    var feature = _context.ProductFeatures
                        .FirstOrDefault(f => f.FeatureId == featureId && f.ProductId == request.ProductId);
                    if (feature != null)
                    {
                        _context.LicenseFeatures.Add(new LicenseFeature
                        {
                            LicenseFeatureId = Guid.NewGuid(),
                            LicenseId = license.LicenseId,
                            FeatureId = featureId
                        });
                    }
                }
            }

            _context.SaveChanges();

            _auditService.Log(
                AuditAction.LicenseCreated,
                "License",
                license.LicenseId.ToString(),
                null,
                "System",
                null,
                "Success",
                JsonSerializer.Serialize(new { license.LicenseKey, ProductName = product.ProductName, CustomerName = customer.Name }),
                null);

            return MapToResponse(license);
        }

        public List<LicenseResponse> BulkGenerateLicenses(BulkGenerateLicenseRequest request)
        {
            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == request.ProductId && !p.IsDeleted);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID '{request.ProductId}' not found.");

            var plan = _context.LicensePlans
                .FirstOrDefault(p => p.PlanId == request.PlanId);
            if (plan == null)
                throw new KeyNotFoundException($"License plan with ID '{request.PlanId}' not found.");

            if (plan.ProductId != request.ProductId)
                throw new InvalidOperationException("The selected plan does not belong to the specified product.");

            var licenses = new List<License>();
            int maxRetries = request.Count * 3;
            int attempts = 0;

            while (licenses.Count < request.Count && attempts < maxRetries)
            {
                attempts++;
                string licenseKey = GenerateUniqueKey(product.ProductCode);

                if (_context.Licenses.Any(l => l.LicenseKey == licenseKey))
                    continue;

                DateTime startDate = DateTime.UtcNow;
                DateTime? expiryDate = plan.DurationDays > 0 ? startDate.AddDays(plan.DurationDays) : null;

                var license = new License
                {
                    LicenseId = Guid.NewGuid(),
                    LicenseKey = licenseKey,
                    ProductId = request.ProductId,
                    CustomerId = request.CustomerId,
                    PlanId = request.PlanId,
                    LicenseType = plan.DurationDays < 0 ? LicenseType.Lifetime : LicenseType.Custom,
                    Status = LicenseStatus.Created,
                    StartDate = startDate,
                    ExpiryDate = expiryDate,
                    MaxDevices = request.MaxDevices > 0 ? request.MaxDevices : plan.MaxDevices,
                    FailedValidationCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                licenses.Add(license);
                _context.Licenses.Add(license);
            }

            if (licenses.Count < request.Count)
                throw new InvalidOperationException($"Unable to generate {request.Count} unique licenses. Generated {licenses.Count} before collision limit was reached.");

            if (request.FeatureIds != null && request.FeatureIds.Count > 0)
            {
                foreach (var license in licenses)
                {
                    foreach (var featureId in request.FeatureIds)
                    {
                        var feature = _context.ProductFeatures
                            .FirstOrDefault(f => f.FeatureId == featureId && f.ProductId == request.ProductId);
                        if (feature != null)
                        {
                            _context.LicenseFeatures.Add(new LicenseFeature
                            {
                                LicenseFeatureId = Guid.NewGuid(),
                                LicenseId = license.LicenseId,
                                FeatureId = featureId
                            });
                        }
                    }
                }
            }

            _context.SaveChanges();

            return licenses.Select(l =>
            {
                l.Product = product;
                return MapToResponse(l);
            }).ToList();
        }

        public LicenseResponse ExtendLicense(Guid id, ExtendLicenseRequest request)
        {
            var license = _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Plan)
                .FirstOrDefault(l => l.LicenseId == id);

            if (license == null)
                throw new KeyNotFoundException($"License with ID '{id}' not found.");

            var validation = LicenseValidator.ValidateExtension(license, request.NewExpiryDate);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage);

            license.ExpiryDate = request.NewExpiryDate;
            license.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            _auditService.Log(
                AuditAction.LicenseExtended,
                "License",
                id.ToString(),
                null,
                "System",
                null,
                "Success",
                JsonSerializer.Serialize(new { request.NewExpiryDate, request.Reason }),
                null);

            return MapToResponse(license);
        }

        public LicenseResponse SuspendLicense(Guid id, SuspendLicenseRequest request)
        {
            var license = _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Plan)
                .FirstOrDefault(l => l.LicenseId == id);

            if (license == null)
                throw new KeyNotFoundException($"License with ID '{id}' not found.");

            var validation = LicenseValidator.ValidateSuspension(license);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage);

            license.Status = LicenseStatus.Suspended;
            license.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            _auditService.Log(
                AuditAction.LicenseSuspended,
                "License",
                id.ToString(),
                null,
                "System",
                null,
                "Success",
                JsonSerializer.Serialize(new { request.Reason }),
                null);

            return MapToResponse(license);
        }

        public LicenseResponse RevokeLicense(Guid id, RevokeLicenseRequest request)
        {
            var license = _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Plan)
                .FirstOrDefault(l => l.LicenseId == id);

            if (license == null)
                throw new KeyNotFoundException($"License with ID '{id}' not found.");

            var validation = LicenseValidator.ValidateRevocation(license);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage);

            license.Status = LicenseStatus.Revoked;
            license.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            _auditService.Log(
                AuditAction.LicenseRevoked,
                "License",
                id.ToString(),
                null,
                "System",
                null,
                "Success",
                JsonSerializer.Serialize(new { request.Reason }),
                null);

            return MapToResponse(license);
        }

        public LicenseResponse ActivateLicense(Guid id)
        {
            var license = _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Plan)
                .FirstOrDefault(l => l.LicenseId == id);

            if (license == null)
                throw new KeyNotFoundException($"License with ID '{id}' not found.");

            if (license.Status != LicenseStatus.Created)
                throw new InvalidOperationException($"Only licenses with 'Created' status can be activated. Current status: {license.Status}.");

            var validation = LicenseValidator.ValidateActivation(license);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage);

            license.Status = LicenseStatus.Active;
            license.ActivatedAt = DateTime.UtcNow;
            license.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            _auditService.Log(
                AuditAction.LicenseActivated,
                "License",
                id.ToString(),
                null,
                "System",
                null,
                "Success",
                null,
                null);

            return MapToResponse(license);
        }

        public PaginatedResponse<LicenseResponse> GetLicensesByProduct(Guid productId, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Customer)
                .Include(l => l.Plan)
                .Include(l => l.Devices)
                .Where(l => l.ProductId == productId)
                .OrderByDescending(l => l.CreatedAt);

            int totalCount = query.Count();

            var licenses = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = licenses.Select(MapToResponse).ToList();

            return new PaginatedResponse<LicenseResponse>(items, totalCount, page, pageSize);
        }

        public byte[] ExportLicensesCsv(Guid productId, LicenseStatus? status)
        {
            var query = _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Customer)
                .Include(l => l.Plan)
                .Include(l => l.Devices)
                .Where(l => l.ProductId == productId)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(l => l.Status == status.Value);

            var licenses = query.OrderByDescending(l => l.CreatedAt).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("LicenseKey,Status,Type,CustomerName,CustomerEmail,PlanName,MaxDevices,ActiveDevices,StartDate,ExpiryDate,CreatedAt");

            foreach (var l in licenses)
            {
                sb.AppendLine(string.Join(",",
                    EscapeCsv(l.LicenseKey),
                    l.Status.ToString(),
                    l.LicenseType.ToString(),
                    EscapeCsv(l.Customer?.Name ?? ""),
                    EscapeCsv(l.Customer?.Email ?? ""),
                    EscapeCsv(l.Plan?.PlanName ?? ""),
                    l.MaxDevices.ToString(),
                    l.Devices.Count(d => d.Status == DeviceStatus.Active).ToString(),
                    l.StartDate.ToString("yyyy-MM-dd"),
                    l.ExpiryDate?.ToString("yyyy-MM-dd") ?? "Lifetime",
                    l.CreatedAt.ToString("yyyy-MM-dd")));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public List<LicenseResponse> SearchLicenses(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<LicenseResponse>();

            string searchLower = query.Trim().ToLowerInvariant();

            return _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Customer)
                .Include(l => l.Plan)
                .Include(l => l.Devices)
                .Where(l =>
                    l.LicenseKey.ToLower().Contains(searchLower) ||
                    (l.Customer != null && l.Customer.Name.ToLower().Contains(searchLower)) ||
                    (l.Customer != null && l.Customer.Email.ToLower().Contains(searchLower)) ||
                    l.Product.ProductName.ToLower().Contains(searchLower))
                .OrderByDescending(l => l.CreatedAt)
                .Take(50)
                .ToList()
                .Select(MapToResponse)
                .ToList();
        }

        private string GenerateUniqueKey(string productCode)
        {
            const int maxAttempts = 100;
            for (int i = 0; i < maxAttempts; i++)
            {
                string key = _keyGenerator.GenerateKey(productCode);
                if (!_context.Licenses.Any(l => l.LicenseKey == key))
                    return key;
            }
            throw new InvalidOperationException("Unable to generate a unique license key after maximum attempts.");
        }

        private static string EscapeCsv(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        private static LicenseResponse MapToResponse(License license)
        {
            return new LicenseResponse
            {
                LicenseId = license.LicenseId,
                LicenseKey = license.LicenseKey,
                ProductId = license.ProductId,
                ProductName = license.Product?.ProductName ?? string.Empty,
                CustomerId = license.CustomerId,
                CustomerName = license.Customer?.Name,
                PlanId = license.PlanId,
                PlanName = license.Plan?.PlanName ?? string.Empty,
                Status = license.Status.ToString(),
                Type = license.LicenseType.ToString(),
                StartDate = license.StartDate,
                ExpiryDate = license.ExpiryDate,
                MaxDevices = license.MaxDevices,
                ActiveDeviceCount = license.Devices?.Count(d => d.Status == DeviceStatus.Active) ?? 0,
                CreatedAt = license.CreatedAt,
                ActivatedAt = license.ActivatedAt
            };
        }

        private static LicenseDetailResponse MapToDetailResponse(License license)
        {
            return new LicenseDetailResponse
            {
                LicenseId = license.LicenseId,
                LicenseKey = license.LicenseKey,
                ProductId = license.ProductId,
                ProductName = license.Product?.ProductName ?? string.Empty,
                CustomerId = license.CustomerId,
                CustomerName = license.Customer?.Name,
                PlanId = license.PlanId,
                PlanName = license.Plan?.PlanName ?? string.Empty,
                Status = license.Status.ToString(),
                Type = license.LicenseType.ToString(),
                StartDate = license.StartDate,
                ExpiryDate = license.ExpiryDate,
                MaxDevices = license.MaxDevices,
                ActiveDeviceCount = license.Devices?.Count(d => d.Status == DeviceStatus.Active) ?? 0,
                CreatedAt = license.CreatedAt,
                ActivatedAt = license.ActivatedAt,
                Features = license.LicenseFeatures?.Select(lf => new Shared.DTOs.FeatureDto
                {
                    FeatureId = lf.FeatureId,
                    FeatureKey = lf.Feature?.FeatureKey ?? string.Empty,
                    FeatureName = lf.Feature?.FeatureName ?? string.Empty
                }).ToList() ?? new List<Shared.DTOs.FeatureDto>(),
                Devices = license.Devices?.Select(d => new LicenseDeviceResponse
                {
                    DeviceId = d.LicenseDeviceId,
                    DeviceFingerprint = d.DeviceFingerprint,
                    DeviceName = d.DeviceName,
                    FirstSeen = d.FirstSeen,
                    LastSeen = d.LastSeen,
                    ActivationDate = d.ActivationDate,
                    Status = d.Status.ToString(),
                    ApplicationVersion = d.ApplicationVersion,
                    OSVersion = d.OSVersion
                }).ToList() ?? new List<LicenseDeviceResponse>(),
                ValidationHistory = new List<Shared.DTOs.Responses.AuditLogResponse>()
            };
        }
    }
}
