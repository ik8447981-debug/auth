using System;
using System.Collections.Generic;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;

namespace LicensePlatform.Application.Interfaces
{
    public interface ILicenseService
    {
        PaginatedResponse<LicenseResponse> GetAllLicenses(int page, int pageSize, Guid? productId, LicenseStatus? status, string? search);
        LicenseDetailResponse GetLicenseById(Guid id);
        LicenseDetailResponse GetLicenseByKey(string key);
        LicenseResponse GenerateLicense(GenerateLicenseRequest request);
        List<LicenseResponse> BulkGenerateLicenses(BulkGenerateLicenseRequest request);
        LicenseResponse ExtendLicense(Guid id, ExtendLicenseRequest request);
        LicenseResponse SuspendLicense(Guid id, SuspendLicenseRequest request);
        LicenseResponse RevokeLicense(Guid id, RevokeLicenseRequest request);
        LicenseResponse ActivateLicense(Guid id);
        PaginatedResponse<LicenseResponse> GetLicensesByProduct(Guid productId, int page, int pageSize);
        byte[] ExportLicensesCsv(Guid productId, LicenseStatus? status);
        List<LicenseResponse> SearchLicenses(string query);
    }
}
