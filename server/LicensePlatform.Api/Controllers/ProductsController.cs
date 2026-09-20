using LicensePlatform.Application.Interfaces;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;
using LicensePlatform.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LicensePlatform.Api.Controllers;

[ApiController]
[Route("api/v1/admin/products")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = _productService.GetAllProducts(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetProductById(Guid id)
    {
        var product = _productService.GetProductById(id);
        if (product == null)
            return NotFound(new { message = $"Product with id '{id}' not found." });
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateProduct([FromBody] CreateProductRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = _productService.CreateProduct(dto);
        return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateProduct(Guid id, [FromBody] UpdateProductRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = _productService.UpdateProduct(id, dto);
        if (product == null)
            return NotFound(new { message = $"Product with id '{id}' not found." });
        return Ok(product);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult DeleteProduct(Guid id)
    {
        var success = _productService.DeleteProduct(id);
        if (!success)
            return NotFound(new { message = $"Product with id '{id}' not found." });
        return NoContent();
    }

    [HttpGet("{id:guid}/features")]
    public IActionResult GetProductFeatures(Guid id)
    {
        var features = _productService.GetProductFeatures(id);
        return Ok(features);
    }

    [HttpPost("{id:guid}/features")]
    [Authorize(Roles = "Admin")]
    public IActionResult AddProductFeature(Guid id, [FromBody] CreateProductFeatureRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var feature = new ProductFeature
        {
            FeatureKey = dto.FeatureKey,
            FeatureName = dto.FeatureName,
            Description = dto.Description,
            IsEnabled = dto.IsEnabled
        };
        var result = _productService.AddProductFeature(id, feature);
        return CreatedAtAction(nameof(GetProductFeatures), new { id }, result);
    }

    [HttpPut("{id:guid}/features/{featureId:guid}")]
    [Authorize(Roles = "Admin")]
    public IActionResult UpdateProductFeature(Guid id, Guid featureId, [FromBody] UpdateProductFeatureRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var feature = _productService.UpdateProductFeature(id, featureId, dto.IsEnabled);
        if (feature == null)
            return NotFound(new { message = $"Feature with id '{featureId}' not found for product '{id}'." });
        return Ok(feature);
    }

    [HttpDelete("{id:guid}/features/{featureId:guid}")]
    [Authorize(Roles = "Admin")]
    public IActionResult RemoveProductFeature(Guid id, Guid featureId)
    {
        var success = _productService.RemoveProductFeature(id, featureId);
        if (!success)
            return NotFound(new { message = $"Feature with id '{featureId}' not found for product '{id}'." });
        return NoContent();
    }
}

public record CreateProductFeatureRequest(string FeatureKey, string FeatureName, string Description, bool IsEnabled);
public record UpdateProductFeatureRequest(bool IsEnabled);
