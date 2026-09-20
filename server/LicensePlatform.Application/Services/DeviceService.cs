using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LicensePlatform.Application.Interfaces;
using LicensePlatform.Application.Validators;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Infrastructure.Data;
using LicensePlatform.Security.DeviceFingerprint;
using LicensePlatform.Security.KeyManagement;
using LicensePlatform.Security.LicenseResponse;
using LicensePlatform.Shared.Constants;
using LicensePlatform.Shared.DTOs;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;
using LicensePlatform.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Application.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly LicensePlatformDbContext _context;
        private readonly IDeviceFingerprintService _fingerprintService;
        private readonly ILicenseResponseSigner _responseSigner;
        private readonly ISigningKeyService _signingKeyService;
        private readonly IAuditService _auditService;

        public DeviceService(
            LicensePlatformDbContext context,
            IDeviceFingerprintService fingerprintService,
            ILicenseResponseSigner responseSigner,
            ISigningKeyService signingKeyService,
            IAuditService auditService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _fingerprintService = fingerprintService ?? throw new ArgumentNullException(nameof(fingerprintService));
            _responseSigner = responseSigner ?? throw new ArgumentNullException(nameof(responseSigner));
            _signingKeyService = signingKeyService ?? throw new ArgumentNullException(nameof(signingKeyService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public List<LicenseDeviceResponse> GetDevicesByLicense(Guid licenseId)
        {
            var license = _context.Licenses
                .FirstOrDefault(l => l.LicenseId == licenseId);

            if (license == null)
                throw new KeyNotFoundException($"License with ID '{licenseId}' not found.");

            return _context.LicenseDevices
                .Where(d => d.LicenseId == licenseId && d.Status != DeviceStatus.Removed)
                .OrderByDescending(d => d.LastSeen)
                .Select(d => new LicenseDeviceResponse
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
                })
                .ToList();
        }

        public List<LicenseDeviceResponse> GetDevicesByCustomer(Guid customerId)
        {
            var customer = _context.Customers
                .FirstOrDefault(c => c.CustomerId == customerId && c.IsActive);

            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID '{customerId}' not found.");

            var licenseIds = _context.Licenses
                .Where(l => l.CustomerId == customerId)
                .Select(l => l.LicenseId)
                .ToList();

            return _context.LicenseDevices
                .Where(d => licenseIds.Contains(d.LicenseId) && d.Status != DeviceStatus.Removed)
                .OrderByDescending(d => d.LastSeen)
                .Select(d => new LicenseDeviceResponse
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
                })
                .ToList();
        }

        public LicenseActivationResponse RegisterDevice(ActivateDeviceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LicenseKey))
                throw new ArgumentException("License key is required.");

            if (string.IsNullOrWhiteSpace(request.DeviceFingerprint))
                throw new ArgumentException("Device fingerprint is required.");

            var license = _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Plan)
                .Include(l => l.Devices)
                .FirstOrDefault(l => l.LicenseKey == request.LicenseKey.Trim().ToUpperInvariant());

            if (license == null)
            {
                return new LicenseActivationResponse
                {
                    Success = false,
                    Status = "Invalid",
                    ServerTime = DateTime.UtcNow
                };
            }

            if (license.Status == LicenseStatus.Revoked)
            {
                return new LicenseActivationResponse
                {
                    Success = false,
                    LicenseId = license.LicenseId,
                    Status = license.Status.ToString(),
                    ServerTime = DateTime.UtcNow
                };
            }

            if (license.Status == LicenseStatus.Suspended)
            {
                return new LicenseActivationResponse
                {
                    Success = false,
                    LicenseId = license.LicenseId,
                    Status = license.Status.ToString(),
                    ServerTime = DateTime.UtcNow
                };
            }

            if (license.ExpiryDate.HasValue && license.ExpiryDate.Value < DateTime.UtcNow)
            {
                return new LicenseActivationResponse
                {
                    Success = false,
                    LicenseId = license.LicenseId,
                    Status = "Expired",
                    ExpiresAt = license.ExpiryDate,
                    ServerTime = DateTime.UtcNow
                };
            }

            string hashedFingerprint = _fingerprintService.HashFingerprint(request.DeviceFingerprint);

            var existingDevice = license.Devices
                .FirstOrDefault(d => d.DeviceFingerprint == hashedFingerprint && d.Status != DeviceStatus.Removed);

            if (existingDevice != null)
            {
                existingDevice.LastSeen = DateTime.UtcNow;
                existingDevice.ApplicationVersion = request.ApplicationVersion ?? existingDevice.ApplicationVersion;
                existingDevice.OSVersion = request.OSVersion ?? existingDevice.OSVersion;
                existingDevice.Status = DeviceStatus.Active;
                _context.SaveChanges();

                return BuildActivationResponse(license, existingDevice, hashedFingerprint);
            }

            int activeDeviceCount = license.Devices.Count(d => d.Status == DeviceStatus.Active);
            if (activeDeviceCount >= license.MaxDevices)
            {
                return new LicenseActivationResponse
                {
                    Success = false,
                    LicenseId = license.LicenseId,
                    Status = ErrorCodes.DEVICE_LIMIT_REACHED,
                    ServerTime = DateTime.UtcNow
                };
            }

            string deviceName = $"Device-{activeDeviceCount + 1}";

            var device = new LicenseDevice
            {
                LicenseDeviceId = Guid.NewGuid(),
                LicenseId = license.LicenseId,
                DeviceFingerprint = hashedFingerprint,
                DeviceName = deviceName,
                Status = DeviceStatus.Active,
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                ActivationDate = DateTime.UtcNow,
                ApplicationVersion = request.ApplicationVersion ?? string.Empty,
                OSVersion = request.OSVersion ?? string.Empty
            };

            _context.LicenseDevices.Add(device);

            if (license.Status == LicenseStatus.Created)
            {
                license.Status = LicenseStatus.Active;
                license.ActivatedAt = DateTime.UtcNow;
            }

            license.LastValidatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            _auditService.Log(
                AuditAction.DeviceAdded,
                "Device",
                device.LicenseDeviceId.ToString(),
                null,
                "System",
                null,
                "Success",
                JsonSerializer.Serialize(new { license.LicenseKey, device.DeviceFingerprint }),
                null);

            return BuildActivationResponse(license, device, hashedFingerprint);
        }

        public LicenseValidationResponse ValidateDevice(ValidateLicenseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LicenseKey))
                throw new ArgumentException("License key is required.");

            if (string.IsNullOrWhiteSpace(request.DeviceFingerprint))
                throw new ArgumentException("Device fingerprint is required.");

            var license = _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Plan)
                .Include(l => l.Devices)
                .Include(l => l.LicenseFeatures)
                    .ThenInclude(lf => lf.Feature)
                .FirstOrDefault(l => l.LicenseKey == request.LicenseKey.Trim().ToUpperInvariant());

            if (license == null)
            {
                return new LicenseValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    Status = "Invalid",
                    Nonce = request.Nonce,
                    ServerTime = DateTime.UtcNow
                };
            }

            string hashedFingerprint = _fingerprintService.HashFingerprint(request.DeviceFingerprint);

            var device = license.Devices
                .FirstOrDefault(d => d.DeviceFingerprint == hashedFingerprint && d.Status != DeviceStatus.Removed);

            if (device == null)
            {
                LogValidationEvent(license.LicenseId, ValidationEventType.Validation, hashedFingerprint, false,
                    ErrorCodes.DEVICE_NOT_AUTHORIZED, "Device not registered for this license", request.ApplicationVersion);

                return new LicenseValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    LicenseId = license.LicenseId,
                    Status = ErrorCodes.DEVICE_NOT_AUTHORIZED,
                    Nonce = request.Nonce,
                    ServerTime = DateTime.UtcNow
                };
            }

            if (device.Status == DeviceStatus.Suspended)
            {
                LogValidationEvent(license.LicenseId, ValidationEventType.Validation, hashedFingerprint, false,
                    ErrorCodes.DEVICE_NOT_AUTHORIZED, "Device is suspended", request.ApplicationVersion);

                return new LicenseValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    LicenseId = license.LicenseId,
                    Status = ErrorCodes.DEVICE_NOT_AUTHORIZED,
                    Nonce = request.Nonce,
                    ServerTime = DateTime.UtcNow
                };
            }

            if (license.Status == LicenseStatus.Revoked)
            {
                LogValidationEvent(license.LicenseId, ValidationEventType.Validation, hashedFingerprint, false,
                    ErrorCodes.LICENSE_REVOKED, "License has been revoked", request.ApplicationVersion);

                return new LicenseValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    LicenseId = license.LicenseId,
                    Status = ErrorCodes.LICENSE_REVOKED,
                    Nonce = request.Nonce,
                    ServerTime = DateTime.UtcNow
                };
            }

            if (license.Status == LicenseStatus.Suspended)
            {
                LogValidationEvent(license.LicenseId, ValidationEventType.Validation, hashedFingerprint, false,
                    ErrorCodes.LICENSE_SUSPENDED, "License is suspended", request.ApplicationVersion);

                return new LicenseValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    LicenseId = license.LicenseId,
                    Status = ErrorCodes.LICENSE_SUSPENDED,
                    Nonce = request.Nonce,
                    ServerTime = DateTime.UtcNow
                };
            }

            if (license.ExpiryDate.HasValue && license.ExpiryDate.Value < DateTime.UtcNow)
            {
                license.Status = LicenseStatus.Expired;
                _context.SaveChanges();

                LogValidationEvent(license.LicenseId, ValidationEventType.Validation, hashedFingerprint, false,
                    ErrorCodes.LICENSE_EXPIRED, "License has expired", request.ApplicationVersion);

                return new LicenseValidationResponse
                {
                    Success = false,
                    IsValid = false,
                    LicenseId = license.LicenseId,
                    Status = ErrorCodes.LICENSE_EXPIRED,
                    ExpiresAt = license.ExpiryDate,
                    Nonce = request.Nonce,
                    ServerTime = DateTime.UtcNow
                };
            }

            device.LastSeen = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(request.ApplicationVersion))
                device.ApplicationVersion = request.ApplicationVersion;

            license.LastValidatedAt = DateTime.UtcNow;
            license.FailedValidationCount = 0;

            _context.SaveChanges();

            var features = license.LicenseFeatures
                .Where(lf => lf.Feature.IsEnabled)
                .Select(lf => lf.Feature.FeatureKey)
                .ToList();

            var signedResponse = new SignedLicenseResponse
            {
                LicenseId = license.LicenseId,
                ProductId = license.ProductId,
                CustomerId = license.CustomerId ?? Guid.Empty,
                Status = license.Status.ToString(),
                Plan = license.Plan?.PlanName ?? string.Empty,
                IssuedAt = license.StartDate,
                ExpiresAt = license.ExpiryDate ?? DateTime.MaxValue,
                DeviceId = device.LicenseDeviceId,
                Features = features,
                MaxDevices = license.MaxDevices,
                ServerTime = DateTime.UtcNow,
                LicenseVersion = 1,
                Nonce = request.Nonce
            };

            string privateKeyPem = string.Empty;
            int keyVersion = 1;

            try
            {
                var activeKey = _context.SigningKeys
                    .Where(k => k.IsActive)
                    .OrderByDescending(k => k.KeyVersion)
                    .FirstOrDefault();

                if (activeKey != null)
                {
                    privateKeyPem = activeKey.PublicKeyPem;
                    keyVersion = activeKey.KeyVersion;
                }
            }
            catch
            {
                keyVersion = _signingKeyService.GetActiveKeyVersion();
            }

            if (!string.IsNullOrEmpty(privateKeyPem))
            {
                signedResponse = _responseSigner.SignLicenseResponse(signedResponse, privateKeyPem, keyVersion);
            }

            LogValidationEvent(license.LicenseId, ValidationEventType.Validation, hashedFingerprint, true,
                null, null, request.ApplicationVersion);

            return new LicenseValidationResponse
            {
                Success = true,
                IsValid = true,
                LicenseId = license.LicenseId,
                Status = license.Status.ToString(),
                ExpiresAt = license.ExpiryDate,
                Features = features,
                ServerTime = DateTime.UtcNow,
                KeyVersion = signedResponse.KeyVersion,
                Nonce = request.Nonce,
                Signature = signedResponse.Signature
            };
        }

        public bool ResetDevice(Guid deviceId)
        {
            var device = _context.LicenseDevices
                .FirstOrDefault(d => d.LicenseDeviceId == deviceId);

            if (device == null)
                return false;

            device.Status = DeviceStatus.Removed;
            device.LastSeen = DateTime.UtcNow;
            _context.SaveChanges();

            _auditService.Log(
                AuditAction.DeviceReset,
                "Device",
                deviceId.ToString(),
                null,
                "System",
                null,
                "Success",
                null,
                null);

            return true;
        }

        public bool RemoveDevice(Guid deviceId)
        {
            var device = _context.LicenseDevices
                .FirstOrDefault(d => d.LicenseDeviceId == deviceId);

            if (device == null)
                return false;

            device.Status = DeviceStatus.Removed;
            device.LastSeen = DateTime.UtcNow;
            _context.SaveChanges();

            _auditService.Log(
                AuditAction.DeviceRemoved,
                "Device",
                deviceId.ToString(),
                null,
                "System",
                null,
                "Success",
                null,
                null);

            return true;
        }

        public bool SuspendDevice(Guid deviceId)
        {
            var device = _context.LicenseDevices
                .FirstOrDefault(d => d.LicenseDeviceId == deviceId);

            if (device == null)
                return false;

            device.Status = DeviceStatus.Suspended;
            device.LastSeen = DateTime.UtcNow;
            _context.SaveChanges();

            return true;
        }

        public int GetDeviceCount(Guid licenseId)
        {
            return _context.LicenseDevices
                .Count(d => d.LicenseId == licenseId && d.Status == DeviceStatus.Active);
        }

        public bool CheckDeviceLimit(Guid licenseId)
        {
            var license = _context.Licenses
                .Include(l => l.Devices)
                .FirstOrDefault(l => l.LicenseId == licenseId);

            if (license == null)
                return false;

            int activeCount = license.Devices.Count(d => d.Status == DeviceStatus.Active);
            return activeCount < license.MaxDevices;
        }

        private LicenseActivationResponse BuildActivationResponse(License license, LicenseDevice device, string hashedFingerprint)
        {
            var features = _context.LicenseFeatures
                .Include(lf => lf.Feature)
                .Where(lf => lf.LicenseId == license.LicenseId && lf.Feature.IsEnabled)
                .Select(lf => lf.Feature.FeatureKey)
                .ToList();

            string privateKeyPem = string.Empty;
            int keyVersion = 1;

            try
            {
                var activeKey = _context.SigningKeys
                    .Where(k => k.IsActive)
                    .OrderByDescending(k => k.KeyVersion)
                    .FirstOrDefault();

                if (activeKey != null)
                {
                    privateKeyPem = activeKey.PublicKeyPem;
                    keyVersion = activeKey.KeyVersion;
                }
            }
            catch
            {
                keyVersion = _signingKeyService.GetActiveKeyVersion();
            }

            var signedResponse = new SignedLicenseResponse
            {
                LicenseId = license.LicenseId,
                ProductId = license.ProductId,
                CustomerId = license.CustomerId ?? Guid.Empty,
                Status = license.Status.ToString(),
                Plan = license.Plan?.PlanName ?? string.Empty,
                IssuedAt = license.StartDate,
                ExpiresAt = license.ExpiryDate ?? DateTime.MaxValue,
                DeviceId = device.LicenseDeviceId,
                Features = features,
                MaxDevices = license.MaxDevices,
                ServerTime = DateTime.UtcNow,
                LicenseVersion = 1,
                Nonce = Guid.NewGuid().ToString("N")
            };

            if (!string.IsNullOrEmpty(privateKeyPem))
            {
                signedResponse = _responseSigner.SignLicenseResponse(signedResponse, privateKeyPem, keyVersion);
            }

            string signedState = string.Empty;
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = false };
                signedState = System.Text.Json.JsonSerializer.Serialize(signedResponse, options);
            }
            catch
            {
                signedState = signedResponse.Signature;
            }

            return new LicenseActivationResponse
            {
                Success = true,
                LicenseId = license.LicenseId,
                Status = license.Status.ToString(),
                ExpiresAt = license.ExpiryDate,
                Features = features,
                DeviceId = device.LicenseDeviceId,
                SignedLicenseState = signedState,
                ServerTime = DateTime.UtcNow,
                KeyVersion = signedResponse.KeyVersion,
                Nonce = signedResponse.Nonce
            };
        }

        private void LogValidationEvent(Guid licenseId, ValidationEventType eventType, string deviceFingerprint,
            bool wasSuccessful, string? errorCode, string? errorMessage, string? applicationVersion)
        {
            var validationEvent = new ValidationEvent
            {
                ValidationEventId = Guid.NewGuid(),
                LicenseId = licenseId,
                EventType = eventType,
                DeviceFingerprint = deviceFingerprint,
                IpAddress = string.Empty,
                Success = wasSuccessful,
                ErrorCode = errorCode,
                ServerTimestamp = DateTime.UtcNow
            };

            _context.ValidationEvents.Add(validationEvent);
            _context.SaveChanges();
        }
    }
}
