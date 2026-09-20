export interface User {
  id: string;
  username: string;
  email: string;
  role: 'admin' | 'superadmin';
  createdAt: string;
}

export interface Product {
  id: string;
  name: string;
  code: string;
  description: string;
  signingKey: string;
  status: 'active' | 'disabled' | 'archived' | 'maintenance';
  features: Feature[];
  licenseCount: number;
  deviceCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface Feature {
  id: string;
  name: string;
  key: string;
  enabled: boolean;
  description: string;
}

export interface Plan {
  id: string;
  name: string;
  productId: string;
  maxDevices: number;
  features: string[];
  price: number;
  durationDays: number;
  description: string;
  createdAt: string;
}

export interface License {
  id: string;
  key: string;
  productId: string;
  productName: string;
  customerId: string;
  customerName: string;
  customerEmail: string;
  planId: string;
  planName: string;
  status: 'active' | 'expired' | 'suspended' | 'revoked' | 'pending';
  maxDevices: number;
  usedDevices: number;
  features: string[];
  activatedAt: string | null;
  expiresAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface LicenseValidation {
  id: string;
  licenseId: string;
  deviceId: string;
  result: 'valid' | 'invalid' | 'expired' | 'suspended';
  timestamp: string;
  ip: string;
  metadata: Record<string, unknown>;
}

export interface Customer {
  id: string;
  name: string;
  email: string;
  phone: string;
  company: string;
  status: 'active' | 'inactive' | 'banned';
  licenseCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface Device {
  id: string;
  licenseId: string;
  licenseKey: string;
  productId: string;
  productName: string;
  customerId: string;
  customerName: string;
  fingerprint: string;
  hostname: string;
  os: string;
  status: 'active' | 'deactivated' | 'expired';
  lastSeenAt: string;
  activatedAt: string;
  createdAt: string;
}

export interface AuditLog {
  id: string;
  timestamp: string;
  actorId: string;
  actorName: string;
  actorEmail: string;
  action: string;
  targetType: string;
  targetId: string;
  targetName: string;
  result: 'success' | 'failure';
  ip: string;
  metadata: Record<string, unknown>;
}

export interface AnalyticsData {
  activationsPerDay: { date: string; count: number }[];
  validationsPerDay: { date: string; count: number }[];
  newLicensesPerDay: { date: string; count: number }[];
  expiredLicensesPerDay: { date: string; count: number }[];
  productUsage: { name: string; value: number }[];
  planDistribution: { name: string; value: number }[];
  licenseStatusDistribution: { status: string; count: number }[];
  topProducts: { name: string; activations: number; validations: number }[];
}

export interface DashboardStats {
  totalProducts: number;
  totalLicenses: number;
  activeLicenses: number;
  activeDevices: number;
  activationsToday: number;
  validationsToday: number;
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface CreateProductRequest {
  name: string;
  code: string;
  description: string;
}

export interface CreateLicenseRequest {
  productId: string;
  customerId: string;
  planId: string;
  maxDevices: number;
  features: string[];
  expiresAt?: string;
}

export interface BulkGenerateRequest {
  productId: string;
  planId: string;
  count: number;
  maxDevices: number;
  features: string[];
  expiresAt?: string;
}

export interface BulkGenerateResult {
  licenses: { key: string; id: string }[];
  totalCount: number;
}

export interface CreateCustomerRequest {
  name: string;
  email: string;
  phone: string;
  company: string;
}

export interface AuditLogFilter {
  action?: string;
  actorId?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}

export interface LicenseFilter {
  productId?: string;
  status?: string;
  planId?: string;
  search?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}
