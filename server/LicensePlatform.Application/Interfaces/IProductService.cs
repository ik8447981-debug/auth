using System;
using System.Collections.Generic;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;

namespace LicensePlatform.Application.Interfaces
{
    public interface IProductService
    {
        PaginatedResponse<ProductResponse> GetAllProducts(int page, int pageSize);
        ProductDetailResponse GetProductById(Guid id);
        ProductDetailResponse GetProductByCode(string code);
        ProductResponse CreateProduct(CreateProductRequest request);
        ProductResponse UpdateProduct(Guid id, UpdateProductRequest request);
        bool DeleteProduct(Guid id);
        List<ProductFeatureResponse> GetProductFeatures(Guid productId);
        ProductFeatureResponse AddProductFeature(Guid productId, ProductFeature feature);
        ProductFeatureResponse UpdateProductFeature(Guid productId, Guid featureId, bool isEnabled);
        bool RemoveProductFeature(Guid productId, Guid featureId);
    }
}
