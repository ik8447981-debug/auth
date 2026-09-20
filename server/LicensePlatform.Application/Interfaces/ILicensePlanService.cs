using System;
using System.Collections.Generic;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;

namespace LicensePlatform.Application.Interfaces
{
    public interface ILicensePlanService
    {
        List<LicensePlanResponse> GetPlansByProduct(Guid productId);
        LicensePlanResponse GetPlanById(Guid id);
        LicensePlanResponse CreatePlan(CreateLicensePlanRequest request);
        LicensePlanResponse UpdatePlan(Guid id, CreateLicensePlanRequest request);
        bool DeletePlan(Guid id);
    }
}
