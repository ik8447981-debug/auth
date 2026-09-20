import axios from 'axios';
import type {
  Product,
  License,
  Customer,
  Device,
  AuditLog,
  AnalyticsData,
  DashboardStats,
  PaginatedResponse,
  LoginRequest,
  LoginResponse,
  CreateProductRequest,
  CreateLicenseRequest,
  BulkGenerateRequest,
  BulkGenerateResult,
  CreateCustomerRequest,
  AuditLogFilter,
  LicenseFilter,
  Plan,
  LicenseValidation,
} from '@/types';

const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('auth_user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Auth
export async function login(data: LoginRequest): Promise<LoginResponse> {
  const response = await api.post<LoginResponse>('/auth/login', data);
  return response.data;
}

// Products
export async function getProducts(): Promise<Product[]> {
  const response = await api.get<Product[]>('/products');
  return response.data;
}

export async function getProduct(id: string): Promise<Product> {
  const response = await api.get<Product>(`/products/${id}`);
  return response.data;
}

export async function createProduct(data: CreateProductRequest): Promise<Product> {
  const response = await api.post<Product>('/products', data);
  return response.data;
}

export async function updateProduct(id: string, data: Partial<CreateProductRequest>): Promise<Product> {
  const response = await api.put<Product>(`/products/${id}`, data);
  return response.data;
}

export async function deleteProduct(id: string): Promise<void> {
  await api.delete(`/products/${id}`);
}

export async function getProductFeatures(productId: string): Promise<import('@/types').Feature[]> {
  const response = await api.get<import('@/types').Feature[]>(`/products/${productId}/features`);
  return response.data;
}

export async function updateProductFeature(productId: string, featureId: string, enabled: boolean): Promise<void> {
  await api.put(`/products/${productId}/features/${featureId}`, { enabled });
}

// Licenses
export async function getLicenses(filter?: LicenseFilter): Promise<PaginatedResponse<License>> {
  const response = await api.get<PaginatedResponse<License>>('/licenses', { params: filter });
  return response.data;
}

export async function getLicense(id: string): Promise<License> {
  const response = await api.get<License>(`/licenses/${id}`);
  return response.data;
}

export async function getLicenseValidations(licenseId: string): Promise<LicenseValidation[]> {
  const response = await api.get<LicenseValidation[]>(`/licenses/${licenseId}/validations`);
  return response.data;
}

export async function generateLicense(data: CreateLicenseRequest): Promise<License> {
  const response = await api.post<License>('/licenses', data);
  return response.data;
}

export async function bulkGenerate(data: BulkGenerateRequest): Promise<BulkGenerateResult> {
  const response = await api.post<BulkGenerateResult>('/licenses/bulk', data);
  return response.data;
}

export async function extendLicense(id: string, expiresAt: string): Promise<License> {
  const response = await api.post<License>(`/licenses/${id}/extend`, { expiresAt });
  return response.data;
}

export async function suspendLicense(id: string): Promise<License> {
  const response = await api.post<License>(`/licenses/${id}/suspend`);
  return response.data;
}

export async function revokeLicense(id: string): Promise<License> {
  const response = await api.post<License>(`/licenses/${id}/revoke`);
  return response.data;
}

// Customers
export async function getCustomers(): Promise<Customer[]> {
  const response = await api.get<Customer[]>('/customers');
  return response.data;
}

export async function getCustomer(id: string): Promise<Customer> {
  const response = await api.get<Customer>(`/customers/${id}`);
  return response.data;
}

export async function createCustomer(data: CreateCustomerRequest): Promise<Customer> {
  const response = await api.post<Customer>('/customers', data);
  return response.data;
}

export async function getCustomerLicenses(customerId: string): Promise<License[]> {
  const response = await api.get<License[]>(`/customers/${customerId}/licenses`);
  return response.data;
}

// Devices
export async function getDevices(): Promise<Device[]> {
  const response = await api.get<Device[]>('/devices');
  return response.data;
}

export async function resetDevice(id: string): Promise<void> {
  await api.post(`/devices/${id}/reset`);
}

// Analytics
export async function getAnalytics(params?: { startDate?: string; endDate?: string; productId?: string }): Promise<AnalyticsData> {
  const response = await api.get<AnalyticsData>('/analytics', { params });
  return response.data;
}

export async function getProductAnalytics(productId: string): Promise<AnalyticsData> {
  const response = await api.get<AnalyticsData>(`/analytics/products/${productId}`);
  return response.data;
}

export async function getDashboardStats(): Promise<DashboardStats> {
  const response = await api.get<DashboardStats>('/analytics/dashboard');
  return response.data;
}

// Audit Logs
export async function getAuditLogs(filter?: AuditLogFilter): Promise<PaginatedResponse<AuditLog>> {
  const response = await api.get<PaginatedResponse<AuditLog>>('/audit-logs', { params: filter });
  return response.data;
}

// Plans
export async function getPlans(productId: string): Promise<Plan[]> {
  const response = await api.get<Plan[]>(`/products/${productId}/plans`);
  return response.data;
}

export async function createPlan(productId: string, data: Omit<Plan, 'id' | 'productId' | 'createdAt'>): Promise<Plan> {
  const response = await api.post<Plan>(`/products/${productId}/plans`, data);
  return response.data;
}

export async function updatePlan(productId: string, planId: string, data: Partial<Plan>): Promise<Plan> {
  const response = await api.put<Plan>(`/products/${productId}/plans/${planId}`, data);
  return response.data;
}

export async function deletePlan(productId: string, planId: string): Promise<void> {
  await api.delete(`/products/${productId}/plans/${planId}`);
}

export default api;
