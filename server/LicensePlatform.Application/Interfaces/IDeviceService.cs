using System;
using System.Collections.Generic;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;

namespace LicensePlatform.Application.Interfaces
{
    public interface IDeviceService
    {
        List<LicenseDeviceResponse> GetDevicesByLicense(Guid licenseId);
        List<LicenseDeviceResponse> GetDevicesByCustomer(Guid customerId);
        LicenseActivationResponse RegisterDevice(ActivateDeviceRequest request);
        LicenseValidationResponse ValidateDevice(ValidateLicenseRequest request);
        bool ResetDevice(Guid deviceId);
        bool RemoveDevice(Guid deviceId);
        bool SuspendDevice(Guid deviceId);
        int GetDeviceCount(Guid licenseId);
        bool CheckDeviceLimit(Guid licenseId);
    }
}
