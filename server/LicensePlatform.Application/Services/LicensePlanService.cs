using System;
using System.Collections.Generic;
using System.Linq;
using LicensePlatform.Application.Interfaces;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Infrastructure.Data;
using LicensePlatform.Shared.Constants;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Application.Services
{
    public class LicensePlanService : ILicensePlanService
    {
        private readonly LicensePlatformDbContext _context;

        public LicensePlanService(LicensePlatformDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public List<LicensePlanResponse> GetPlansByProduct(Guid productId)
        {
            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == productId && !p.IsDeleted);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID '{productId}' not found.");

            return _context.LicensePlans
                .Include(p => p.PlanFeatures)
                .Where(p => p.ProductId == productId)
                .OrderBy(p => p.PlanName)
                .Select(p => new LicensePlanResponse
                {
                    PlanId = p.PlanId,
                    ProductId = p.ProductId,
                    PlanName = p.PlanName,
                    DurationDays = p.DurationDays,
                    MaxDevices = p.MaxDevices,
                    OfflineGraceHours = p.OfflineGraceHours,
                    ValidationIntervalMinutes = p.ValidationIntervalMinutes,
                    Status = p.IsActive ? "Active" : "Inactive",
                    FeatureIds = p.PlanFeatures.Select(pf => pf.FeatureId).ToList(),
                    CreatedAt = p.CreatedAt
                })
                .ToList();
        }

        public LicensePlanResponse GetPlanById(Guid id)
        {
            var plan = _context.LicensePlans
                .Include(p => p.PlanFeatures)
                .FirstOrDefault(p => p.PlanId == id);

            if (plan == null)
                throw new KeyNotFoundException($"License plan with ID '{id}' not found.");

            return new LicensePlanResponse
            {
                PlanId = plan.PlanId,
                ProductId = plan.ProductId,
                PlanName = plan.PlanName,
                DurationDays = plan.DurationDays,
                MaxDevices = plan.MaxDevices,
                OfflineGraceHours = plan.OfflineGraceHours,
                ValidationIntervalMinutes = plan.ValidationIntervalMinutes,
                Status = plan.IsActive ? "Active" : "Inactive",
                FeatureIds = plan.PlanFeatures.Select(pf => pf.FeatureId).ToList(),
                CreatedAt = plan.CreatedAt
            };
        }

        public LicensePlanResponse CreatePlan(CreateLicensePlanRequest request)
        {
            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == request.ProductId && !p.IsDeleted);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID '{request.ProductId}' not found.");

            if (string.IsNullOrWhiteSpace(request.PlanName))
                throw new ArgumentException("Plan name is required.");

            if (request.DurationDays < -1)
                throw new ArgumentException("Duration days must be -1 (lifetime) or a positive number.");

            if (request.MaxDevices < 1)
                throw new ArgumentException("Max devices must be at least 1.");

            var plan = new LicensePlan
            {
                PlanId = Guid.NewGuid(),
                ProductId = request.ProductId,
                PlanName = request.PlanName.Trim(),
                DurationDays = request.DurationDays,
                MaxDevices = request.MaxDevices,
                OfflineGraceHours = request.OfflineGraceHours > 0 ? request.OfflineGraceHours : SystemConstants.DefaultOfflineGraceHours,
                ValidationIntervalMinutes = request.ValidationIntervalMinutes > 0 ? request.ValidationIntervalMinutes : SystemConstants.DefaultValidationIntervalMinutes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.LicensePlans.Add(plan);

            if (request.FeatureIds != null && request.FeatureIds.Count > 0)
            {
                foreach (var featureId in request.FeatureIds)
                {
                    var feature = _context.ProductFeatures
                        .FirstOrDefault(f => f.FeatureId == featureId && f.ProductId == request.ProductId);

                    if (feature != null)
                    {
                        _context.PlanFeatures.Add(new PlanFeature
                        {
                            Id = Guid.NewGuid(),
                            PlanId = plan.PlanId,
                            FeatureId = featureId
                        });
                    }
                }
            }

            _context.SaveChanges();

            return new LicensePlanResponse
            {
                PlanId = plan.PlanId,
                ProductId = plan.ProductId,
                PlanName = plan.PlanName,
                DurationDays = plan.DurationDays,
                MaxDevices = plan.MaxDevices,
                OfflineGraceHours = plan.OfflineGraceHours,
                ValidationIntervalMinutes = plan.ValidationIntervalMinutes,
                Status = plan.IsActive ? "Active" : "Inactive",
                FeatureIds = request.FeatureIds ?? new List<Guid>(),
                CreatedAt = plan.CreatedAt
            };
        }

        public LicensePlanResponse UpdatePlan(Guid id, CreateLicensePlanRequest request)
        {
            var plan = _context.LicensePlans
                .Include(p => p.PlanFeatures)
                .FirstOrDefault(p => p.PlanId == id);

            if (plan == null)
                throw new KeyNotFoundException($"License plan with ID '{id}' not found.");

            if (!string.IsNullOrWhiteSpace(request.PlanName))
                plan.PlanName = request.PlanName.Trim();

            if (request.DurationDays >= -1)
                plan.DurationDays = request.DurationDays;

            if (request.MaxDevices >= 1)
                plan.MaxDevices = request.MaxDevices;

            if (request.OfflineGraceHours >= 0)
                plan.OfflineGraceHours = request.OfflineGraceHours;

            if (request.ValidationIntervalMinutes >= 1)
                plan.ValidationIntervalMinutes = request.ValidationIntervalMinutes;

            if (request.FeatureIds != null)
            {
                var existingFeatureIds = plan.PlanFeatures.Select(pf => pf.FeatureId).ToList();

                var toRemove = plan.PlanFeatures
                    .Where(pf => !request.FeatureIds.Contains(pf.FeatureId))
                    .ToList();
                _context.PlanFeatures.RemoveRange(toRemove);

                var toAdd = request.FeatureIds
                    .Where(fid => !existingFeatureIds.Contains(fid))
                    .ToList();

                foreach (var featureId in toAdd)
                {
                    var feature = _context.ProductFeatures
                        .FirstOrDefault(f => f.FeatureId == featureId && f.ProductId == plan.ProductId);

                    if (feature != null)
                    {
                        _context.PlanFeatures.Add(new PlanFeature
                        {
                            Id = Guid.NewGuid(),
                            PlanId = plan.PlanId,
                            FeatureId = featureId
                        });
                    }
                }
            }

            _context.SaveChanges();

            return new LicensePlanResponse
            {
                PlanId = plan.PlanId,
                ProductId = plan.ProductId,
                PlanName = plan.PlanName,
                DurationDays = plan.DurationDays,
                MaxDevices = plan.MaxDevices,
                OfflineGraceHours = plan.OfflineGraceHours,
                ValidationIntervalMinutes = plan.ValidationIntervalMinutes,
                Status = plan.IsActive ? "Active" : "Inactive",
                FeatureIds = request.FeatureIds ?? plan.PlanFeatures.Select(pf => pf.FeatureId).ToList(),
                CreatedAt = plan.CreatedAt
            };
        }

        public bool DeletePlan(Guid id)
        {
            var plan = _context.LicensePlans
                .Include(p => p.Licenses)
                .FirstOrDefault(p => p.PlanId == id);

            if (plan == null)
                return false;

            if (plan.Licenses != null && plan.Licenses.Count > 0)
                throw new InvalidOperationException("Cannot delete a plan that has associated licenses.");

            var planFeatures = _context.PlanFeatures
                .Where(pf => pf.PlanId == id)
                .ToList();
            _context.PlanFeatures.RemoveRange(planFeatures);

            _context.LicensePlans.Remove(plan);
            _context.SaveChanges();
            return true;
        }
    }
}
