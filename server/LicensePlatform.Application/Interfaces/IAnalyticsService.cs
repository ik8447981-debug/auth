using System;
using System.Collections.Generic;
using LicensePlatform.Shared.DTOs.Responses;

namespace LicensePlatform.Application.Interfaces
{
    public interface IAnalyticsService
    {
        AnalyticsResponse GetGlobalAnalytics();
        ProductAnalyticsResponse GetProductAnalytics(Guid productId);
        Dictionary<DateTime, int> GetActivationsByDay(DateTime from, DateTime to, Guid? productId);
        Dictionary<DateTime, int> GetValidationsByDay(DateTime from, DateTime to, Guid? productId);
        Dictionary<string, int> GetLicenseExpiryDistribution(Guid? productId);
    }
}
