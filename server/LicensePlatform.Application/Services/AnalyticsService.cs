using System;
using System.Collections.Generic;
using System.Linq;
using LicensePlatform.Application.Interfaces;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Infrastructure.Data;
using LicensePlatform.Shared.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly LicensePlatformDbContext _context;

        public AnalyticsService(LicensePlatformDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public AnalyticsResponse GetGlobalAnalytics()
        {
            DateTime today = DateTime.UtcNow.Date;
            DateTime tomorrow = today.AddDays(1);

            var totalProducts = _context.Products.Count(p => !p.IsDeleted);
            var totalLicenses = _context.Licenses.Count();
            var activeLicenses = _context.Licenses.Count(l => l.Status == LicenseStatus.Active);
            var expiredLicenses = _context.Licenses.Count(l => l.Status == LicenseStatus.Expired);
            var revokedLicenses = _context.Licenses.Count(l => l.Status == LicenseStatus.Revoked);
            var activeDevices = _context.LicenseDevices.Count(d => d.Status == Domain.Enums.DeviceStatus.Active);

            var activationsToday = _context.LicenseDevices
                .Count(d => d.ActivationDate >= today && d.ActivationDate < tomorrow);

            var validationsToday = _context.ValidationEvents
                .Count(v => v.ServerTimestamp >= today && v.ServerTimestamp < tomorrow);

            var apiErrorsToday = _context.ValidationEvents
                .Count(v => v.ServerTimestamp >= today && v.ServerTimestamp < tomorrow && !v.Success);

            return new AnalyticsResponse
            {
                TotalProducts = totalProducts,
                TotalLicenses = totalLicenses,
                ActiveLicenses = activeLicenses,
                ExpiredLicenses = expiredLicenses,
                RevokedLicenses = revokedLicenses,
                ActiveDevices = activeDevices,
                ActivationsToday = activationsToday,
                ValidationsToday = validationsToday,
                ApiErrorsToday = apiErrorsToday
            };
        }

        public ProductAnalyticsResponse GetProductAnalytics(Guid productId)
        {
            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == productId && !p.IsDeleted);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID '{productId}' not found.");

            DateTime today = DateTime.UtcNow.Date;
            DateTime tomorrow = today.AddDays(1);

            var licenses = _context.Licenses
                .Where(l => l.ProductId == productId)
                .ToList();

            var totalLicenses = licenses.Count;
            var activeLicenses = licenses.Count(l => l.Status == LicenseStatus.Active);
            var expiredLicenses = licenses.Count(l => l.Status == LicenseStatus.Expired);
            var revokedLicenses = licenses.Count(l => l.Status == LicenseStatus.Revoked);

            var licenseIds = licenses.Select(l => l.LicenseId).ToList();

            var activeDevices = _context.LicenseDevices
                .Count(d => licenseIds.Contains(d.LicenseId) && d.Status == Domain.Enums.DeviceStatus.Active);

            var activationsToday = _context.LicenseDevices
                .Count(d => licenseIds.Contains(d.LicenseId) &&
                           d.ActivationDate >= today && d.ActivationDate < tomorrow);

            var validationsToday = _context.ValidationEvents
                .Count(v => licenseIds.Contains(v.LicenseId) &&
                           v.ServerTimestamp >= today && v.ServerTimestamp < tomorrow);

            return new ProductAnalyticsResponse
            {
                TotalLicenses = totalLicenses,
                ActiveLicenses = activeLicenses,
                ExpiredLicenses = expiredLicenses,
                RevokedLicenses = revokedLicenses,
                ActiveDevices = activeDevices,
                ActivationsToday = activationsToday,
                ValidationsToday = validationsToday
            };
        }

        public Dictionary<DateTime, int> GetActivationsByDay(DateTime from, DateTime to, Guid? productId)
        {
            var query = _context.LicenseDevices
                .Where(d => d.ActivationDate >= from && d.ActivationDate <= to);

            if (productId.HasValue)
            {
                var licenseIds = _context.Licenses
                    .Where(l => l.ProductId == productId.Value)
                    .Select(l => l.LicenseId)
                    .ToList();

                query = query.Where(d => licenseIds.Contains(d.LicenseId));
            }

            var grouped = query
                .GroupBy(d => d.ActivationDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionary(g => g.Date, g => g.Count);

            var result = new Dictionary<DateTime, int>();
            for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
            {
                result[date] = grouped.TryGetValue(date, out int count) ? count : 0;
            }

            return result;
        }

        public Dictionary<DateTime, int> GetValidationsByDay(DateTime from, DateTime to, Guid? productId)
        {
            var query = _context.ValidationEvents
                .Where(v => v.ServerTimestamp >= from && v.ServerTimestamp <= to);

            if (productId.HasValue)
            {
                var licenseIds = _context.Licenses
                    .Where(l => l.ProductId == productId.Value)
                    .Select(l => l.LicenseId)
                    .ToList();

                query = query.Where(v => licenseIds.Contains(v.LicenseId));
            }

            var grouped = query
                .GroupBy(v => v.ServerTimestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionary(g => g.Date, g => g.Count);

            var result = new Dictionary<DateTime, int>();
            for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
            {
                result[date] = grouped.TryGetValue(date, out int count) ? count : 0;
            }

            return result;
        }

        public Dictionary<string, int> GetLicenseExpiryDistribution(Guid? productId)
        {
            var query = _context.Licenses
                .Where(l => l.ExpiryDate.HasValue)
                .AsQueryable();

            if (productId.HasValue)
                query = query.Where(l => l.ProductId == productId.Value);

            var licenses = query.Select(l => l.ExpiryDate!.Value).ToList();

            var now = DateTime.UtcNow;
            var distribution = new Dictionary<string, int>
            {
                ["Expired"] = licenses.Count(e => e < now),
                ["Expiring within 7 days"] = licenses.Count(e => e >= now && e <= now.AddDays(7)),
                ["Expiring within 30 days"] = licenses.Count(e => e > now.AddDays(7) && e <= now.AddDays(30)),
                ["Expiring within 90 days"] = licenses.Count(e => e > now.AddDays(30) && e <= now.AddDays(90)),
                ["Valid > 90 days"] = licenses.Count(e => e > now.AddDays(90))
            };

            return distribution;
        }
    }
}
