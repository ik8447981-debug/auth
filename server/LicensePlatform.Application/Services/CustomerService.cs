using System;
using System.Collections.Generic;
using System.Linq;
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
    public class CustomerService : ICustomerService
    {
        private readonly LicensePlatformDbContext _context;
        private readonly IAuditService _auditService;

        public CustomerService(LicensePlatformDbContext context, IAuditService auditService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public PaginatedResponse<CustomerResponse> GetAllCustomers(int page, int pageSize, string? search)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _context.Customers
                .Where(c => c.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.Trim().ToLowerInvariant();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchLower) ||
                    c.Email.ToLower().Contains(searchLower));
            }

            query = query.OrderByDescending(c => c.CreatedAt);

            int totalCount = query.Count();

            var customers = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = customers.Select(c => new CustomerResponse
            {
                CustomerId = c.CustomerId,
                Name = c.Name,
                Email = c.Email,
                Status = c.IsActive ? "Active" : "Inactive",
                CreatedAt = c.CreatedAt,
                LicenseCount = c.Licenses?.Count(l => l.Status != LicenseStatus.Revoked) ?? 0
            }).ToList();

            return new PaginatedResponse<CustomerResponse>(items, totalCount, page, pageSize);
        }

        public CustomerResponse GetCustomerById(Guid id)
        {
            var customer = _context.Customers
                .Include(c => c.Licenses)
                .FirstOrDefault(c => c.CustomerId == id && c.IsActive);

            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID '{id}' not found.");

            return new CustomerResponse
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Status = customer.IsActive ? "Active" : "Inactive",
                CreatedAt = customer.CreatedAt,
                LicenseCount = customer.Licenses?.Count(l => l.Status != LicenseStatus.Revoked) ?? 0
            };
        }

        public CustomerResponse GetCustomerByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.");

            var customer = _context.Customers
                .Include(c => c.Licenses)
                .FirstOrDefault(c => c.Email.ToLower() == email.Trim().ToLower() && c.IsActive);

            if (customer == null)
                throw new KeyNotFoundException($"Customer with email '{email}' not found.");

            return new CustomerResponse
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Status = customer.IsActive ? "Active" : "Inactive",
                CreatedAt = customer.CreatedAt,
                LicenseCount = customer.Licenses?.Count(l => l.Status != LicenseStatus.Revoked) ?? 0
            };
        }

        public CustomerResponse CreateCustomer(CreateCustomerRequest request)
        {
            var emailValidation = CustomerValidator.ValidateEmail(request.Email);
            if (!emailValidation.IsValid)
                throw new InvalidOperationException(emailValidation.ErrorMessage);

            var nameValidation = CustomerValidator.ValidateName(request.Name);
            if (!nameValidation.IsValid)
                throw new InvalidOperationException(nameValidation.ErrorMessage);

            if (_context.Customers.Any(c => c.Email.ToLower() == request.Email.Trim().ToLower() && c.IsActive))
                throw new InvalidOperationException($"A customer with email '{request.Email}' already exists.");

            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                ProductId = Guid.Empty,
                Name = request.Name.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Notes = request.Notes?.Trim() ?? string.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            _context.SaveChanges();

            _auditService.Log(
                AuditAction.CustomerCreated,
                "Customer",
                customer.CustomerId.ToString(),
                null,
                "System",
                null,
                "Success",
                JsonSerializer.Serialize(new { customer.Name, customer.Email }),
                null);

            return new CustomerResponse
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Status = "Active",
                CreatedAt = customer.CreatedAt,
                LicenseCount = 0
            };
        }

        public CustomerResponse UpdateCustomer(Guid id, CreateCustomerRequest request)
        {
            var customer = _context.Customers
                .FirstOrDefault(c => c.CustomerId == id && c.IsActive);

            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID '{id}' not found.");

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var emailValidation = CustomerValidator.ValidateEmail(request.Email);
                if (!emailValidation.IsValid)
                    throw new InvalidOperationException(emailValidation.ErrorMessage);

                if (_context.Customers.Any(c => c.Email.ToLower() == request.Email.Trim().ToLower() && c.CustomerId != id && c.IsActive))
                    throw new InvalidOperationException($"A customer with email '{request.Email}' already exists.");

                customer.Email = request.Email.Trim().ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var nameValidation = CustomerValidator.ValidateName(request.Name);
                if (!nameValidation.IsValid)
                    throw new InvalidOperationException(nameValidation.ErrorMessage);

                customer.Name = request.Name.Trim();
            }

            if (request.Notes != null)
                customer.Notes = request.Notes.Trim();

            customer.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            return new CustomerResponse
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Status = customer.IsActive ? "Active" : "Inactive",
                CreatedAt = customer.CreatedAt,
                LicenseCount = customer.Licenses?.Count(l => l.Status != LicenseStatus.Revoked) ?? 0
            };
        }

        public bool DeleteCustomer(Guid id)
        {
            var customer = _context.Customers
                .FirstOrDefault(c => c.CustomerId == id && c.IsActive);

            if (customer == null)
                return false;

            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
            return true;
        }

        public List<LicenseResponse> GetCustomerLicenses(Guid customerId, Guid? productId)
        {
            var customer = _context.Customers
                .FirstOrDefault(c => c.CustomerId == customerId && c.IsActive);

            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID '{customerId}' not found.");

            var query = _context.Licenses
                .Include(l => l.Product)
                .Include(l => l.Plan)
                .Include(l => l.Devices)
                .Where(l => l.CustomerId == customerId);

            if (productId.HasValue)
                query = query.Where(l => l.ProductId == productId.Value);

            return query
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new LicenseResponse
                {
                    LicenseId = l.LicenseId,
                    LicenseKey = l.LicenseKey,
                    ProductId = l.ProductId,
                    ProductName = l.Product.ProductName,
                    CustomerId = l.CustomerId,
                    CustomerName = customer.Name,
                    PlanId = l.PlanId,
                    PlanName = l.Plan.PlanName,
                    Status = l.Status.ToString(),
                    Type = l.LicenseType.ToString(),
                    StartDate = l.StartDate,
                    ExpiryDate = l.ExpiryDate,
                    MaxDevices = l.MaxDevices,
                    ActiveDeviceCount = l.Devices.Count(d => d.Status == Domain.Enums.DeviceStatus.Active),
                    CreatedAt = l.CreatedAt,
                    ActivatedAt = l.ActivatedAt
                })
                .ToList();
        }
    }
}
