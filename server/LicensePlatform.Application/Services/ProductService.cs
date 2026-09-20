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
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly LicensePlatformDbContext _context;
        private readonly IAuditService _auditService;

        public ProductService(LicensePlatformDbContext context, IAuditService auditService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public PaginatedResponse<ProductResponse> GetAllProducts(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _context.Products
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt);

            int totalCount = query.Count();

            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = products.Select(MapToResponse).ToList();

            return new PaginatedResponse<ProductResponse>(items, totalCount, page, pageSize);
        }

        public ProductDetailResponse GetProductById(Guid id)
        {
            var product = _context.Products
                .Include(p => p.Features)
                .Include(p => p.Plans)
                    .ThenInclude(pl => pl.PlanFeatures)
                .FirstOrDefault(p => p.ProductId == id && !p.IsDeleted);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID '{id}' not found.");

            var versions = _context.ProductVersions
                .Where(v => v.ProductId == id)
                .ToList();

            return MapToDetailResponse(product, versions);
        }

        public ProductDetailResponse GetProductByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Product code cannot be empty.");

            var product = _context.Products
                .Include(p => p.Features)
                .Include(p => p.Plans)
                    .ThenInclude(pl => pl.PlanFeatures)
                .FirstOrDefault(p => p.ProductCode == code.ToUpperInvariant() && !p.IsDeleted);

            if (product == null)
                throw new KeyNotFoundException($"Product with code '{code}' not found.");

            var versions = _context.ProductVersions
                .Where(v => v.ProductId == product.ProductId)
                .ToList();

            return MapToDetailResponse(product, versions);
        }

        public ProductResponse CreateProduct(CreateProductRequest request)
        {
            var codeValidation = ProductValidator.ValidateProductCode(request.ProductCode);
            if (!codeValidation.IsValid)
                throw new InvalidOperationException(codeValidation.ErrorMessage);

            var nameValidation = ProductValidator.ValidateProductName(request.ProductName);
            if (!nameValidation.IsValid)
                throw new InvalidOperationException(nameValidation.ErrorMessage);

            var displayValidation = ProductValidator.ValidateDisplayName(request.DisplayName);
            if (!displayValidation.IsValid)
                throw new InvalidOperationException(displayValidation.ErrorMessage);

            if (_context.Products.Any(p => p.ProductCode == request.ProductCode.ToUpperInvariant() && !p.IsDeleted))
                throw new InvalidOperationException($"A product with code '{request.ProductCode}' already exists.");

            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = request.ProductName.Trim(),
                DisplayName = request.DisplayName.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                ProductCode = request.ProductCode.Trim().ToUpperInvariant(),
                Status = ProductStatus.Active,
                CurrentVersion = "1.0.0",
                MinimumSupportedVersion = "1.0.0",
                LatestVersion = "1.0.0",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            _auditService.Log(
                AuditAction.ProductCreated,
                "Product",
                product.ProductId.ToString(),
                null,
                "System",
                null,
                "Success",
                JsonSerializer.Serialize(new { product.ProductCode, product.ProductName }),
                null);

            return MapToResponse(product);
        }

        public ProductResponse UpdateProduct(Guid id, UpdateProductRequest request)
        {
            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == id && !p.IsDeleted);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID '{id}' not found.");

            if (request.ProductName != null)
            {
                var nameValidation = ProductValidator.ValidateProductName(request.ProductName);
                if (!nameValidation.IsValid)
                    throw new InvalidOperationException(nameValidation.ErrorMessage);
                product.ProductName = request.ProductName.Trim();
            }

            if (request.DisplayName != null)
            {
                var displayValidation = ProductValidator.ValidateDisplayName(request.DisplayName);
                if (!displayValidation.IsValid)
                    throw new InvalidOperationException(displayValidation.ErrorMessage);
                product.DisplayName = request.DisplayName.Trim();
            }

            if (request.Description != null)
                product.Description = request.Description.Trim();

            if (request.Status != null)
            {
                if (Enum.TryParse<ProductStatus>(request.Status, true, out var status))
                    product.Status = status;
                else
                    throw new InvalidOperationException($"Invalid status value: '{request.Status}'.");
            }

            product.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            _auditService.Log(
                AuditAction.ProductUpdated,
                "Product",
                id.ToString(),
                null,
                "System",
                null,
                "Success",
                JsonSerializer.Serialize(new { request.ProductName, request.DisplayName, request.Status }),
                null);

            return MapToResponse(product);
        }

        public bool DeleteProduct(Guid id)
        {
            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == id && !p.IsDeleted);

            if (product == null)
                return false;

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            _auditService.Log(
                AuditAction.ProductDisabled,
                "Product",
                id.ToString(),
                null,
                "System",
                null,
                "Success",
                null,
                null);

            return true;
        }

        public List<ProductFeatureResponse> GetProductFeatures(Guid productId)
        {
            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == productId && !p.IsDeleted);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID '{productId}' not found.");

            return _context.ProductFeatures
                .Where(f => f.ProductId == productId)
                .Select(f => new ProductFeatureResponse
                {
                    FeatureId = f.FeatureId,
                    FeatureKey = f.FeatureKey,
                    FeatureName = f.FeatureName,
                    Description = f.Description,
                    IsEnabled = f.IsEnabled
                })
                .ToList();
        }

        public ProductFeatureResponse AddProductFeature(Guid productId, ProductFeature feature)
        {
            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == productId && !p.IsDeleted);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID '{productId}' not found.");

            if (_context.ProductFeatures.Any(f => f.ProductId == productId && f.FeatureKey == feature.FeatureKey))
                throw new InvalidOperationException($"A feature with key '{feature.FeatureKey}' already exists for this product.");

            var newFeature = new ProductFeature
            {
                FeatureId = Guid.NewGuid(),
                ProductId = productId,
                FeatureKey = feature.FeatureKey,
                FeatureName = feature.FeatureName,
                Description = feature.Description,
                IsEnabled = feature.IsEnabled
            };

            _context.ProductFeatures.Add(newFeature);
            _context.SaveChanges();

            return new ProductFeatureResponse
            {
                FeatureId = newFeature.FeatureId,
                FeatureKey = newFeature.FeatureKey,
                FeatureName = newFeature.FeatureName,
                Description = newFeature.Description,
                IsEnabled = newFeature.IsEnabled
            };
        }

        public ProductFeatureResponse UpdateProductFeature(Guid productId, Guid featureId, bool isEnabled)
        {
            var feature = _context.ProductFeatures
                .FirstOrDefault(f => f.FeatureId == featureId && f.ProductId == productId);

            if (feature == null)
                throw new KeyNotFoundException($"Feature with ID '{featureId}' not found for product '{productId}'.");

            feature.IsEnabled = isEnabled;
            _context.SaveChanges();

            return new ProductFeatureResponse
            {
                FeatureId = feature.FeatureId,
                FeatureKey = feature.FeatureKey,
                FeatureName = feature.FeatureName,
                Description = feature.Description,
                IsEnabled = feature.IsEnabled
            };
        }

        public bool RemoveProductFeature(Guid productId, Guid featureId)
        {
            var feature = _context.ProductFeatures
                .FirstOrDefault(f => f.FeatureId == featureId && f.ProductId == productId);

            if (feature == null)
                return false;

            _context.ProductFeatures.Remove(feature);
            _context.SaveChanges();
            return true;
        }

        private static ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                DisplayName = product.DisplayName,
                Description = product.Description,
                ProductCode = product.ProductCode,
                Status = product.Status.ToString(),
                CurrentVersion = product.CurrentVersion,
                LatestVersion = product.LatestVersion,
                CreatedAt = product.CreatedAt,
                LicenseCount = product.Licenses?.Count ?? 0,
                CustomerCount = product.Customers?.Count ?? 0,
                ActiveDeviceCount = 0
            };
        }

        private static ProductDetailResponse MapToDetailResponse(Product product, List<ProductVersion>? versions = null)
        {
            var response = new ProductDetailResponse
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                DisplayName = product.DisplayName,
                Description = product.Description,
                ProductCode = product.ProductCode,
                Status = product.Status.ToString(),
                CurrentVersion = product.CurrentVersion,
                LatestVersion = product.LatestVersion,
                CreatedAt = product.CreatedAt,
                LicenseCount = product.Licenses?.Count ?? 0,
                CustomerCount = product.Customers?.Count ?? 0,
                ActiveDeviceCount = 0,
                Features = product.Features?.Select(f => new ProductFeatureResponse
                {
                    FeatureId = f.FeatureId,
                    FeatureKey = f.FeatureKey,
                    FeatureName = f.FeatureName,
                    Description = f.Description,
                    IsEnabled = f.IsEnabled
                }).ToList() ?? new List<ProductFeatureResponse>(),
                Plans = product.Plans?.Select(p => new LicensePlanResponse
                {
                    PlanId = p.PlanId,
                    ProductId = p.ProductId,
                    PlanName = p.PlanName,
                    DurationDays = p.DurationDays,
                    MaxDevices = p.MaxDevices,
                    OfflineGraceHours = p.OfflineGraceHours,
                    ValidationIntervalMinutes = p.ValidationIntervalMinutes,
                    Status = p.IsActive ? "Active" : "Inactive",
                    FeatureIds = p.PlanFeatures?.Select(pf => pf.FeatureId).ToList() ?? new List<Guid>(),
                    CreatedAt = p.CreatedAt
                }).ToList() ?? new List<LicensePlanResponse>(),
                Versions = versions?.OrderByDescending(v => v.ReleasedAt).Select(v => new ProductVersionDto
                {
                    VersionId = v.VersionId,
                    VersionNumber = v.VersionNumber,
                    ReleasedAt = v.ReleasedAt,
                    IsLatest = v.VersionNumber == product.LatestVersion
                }).ToList() ?? new List<ProductVersionDto>()
            };

            return response;
        }
    }
}
