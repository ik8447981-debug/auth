using LicensePlatform.Application.Interfaces;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LicensePlatform.Api.Controllers;

[ApiController]
[Route("api/v1/admin/customers")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAllCustomers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = _customerService.GetAllCustomers(page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetCustomerById(Guid id)
    {
        var customer = _customerService.GetCustomerById(id);
        if (customer == null)
            return NotFound(new { message = $"Customer with id '{id}' not found." });
        return Ok(customer);
    }

    [HttpPost]
    public IActionResult CreateCustomer([FromBody] CreateCustomerRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = _customerService.CreateCustomer(dto);
        return CreatedAtAction(nameof(GetCustomerById), new { id = customer.CustomerId }, customer);
    }

    [HttpPut("{id:guid}")]
    public IActionResult UpdateCustomer(Guid id, [FromBody] CreateCustomerRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = _customerService.UpdateCustomer(id, dto);
        if (customer == null)
            return NotFound(new { message = $"Customer with id '{id}' not found." });
        return Ok(customer);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteCustomer(Guid id)
    {
        var success = _customerService.DeleteCustomer(id);
        if (!success)
            return NotFound(new { message = $"Customer with id '{id}' not found." });
        return NoContent();
    }

    [HttpGet("{id:guid}/licenses")]
    public IActionResult GetCustomerLicenses(Guid id, [FromQuery] Guid? productId = null)
    {
        var licenses = _customerService.GetCustomerLicenses(id, productId);
        return Ok(licenses);
    }
}
