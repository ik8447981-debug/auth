using System;
using System.Collections.Generic;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;

namespace LicensePlatform.Application.Interfaces
{
    public interface ICustomerService
    {
        PaginatedResponse<CustomerResponse> GetAllCustomers(int page, int pageSize, string? search);
        CustomerResponse GetCustomerById(Guid id);
        CustomerResponse GetCustomerByEmail(string email);
        CustomerResponse CreateCustomer(CreateCustomerRequest request);
        CustomerResponse UpdateCustomer(Guid id, CreateCustomerRequest request);
        bool DeleteCustomer(Guid id);
        List<LicenseResponse> GetCustomerLicenses(Guid customerId, Guid? productId);
    }
}
