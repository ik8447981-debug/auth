import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Key,
  Users,
  Layers,
  CreditCard,
  BarChart3,
  Settings,
  ToggleLeft,
  ToggleRight,
  Plus,
  Pencil,
  Trash2,
} from 'lucide-react';
import LoadingSpinner from '@/components/LoadingSpinner';
import Modal from '@/components/Modal';
import { useToast } from '@/components/Toast';
import DataTable, { Column } from '@/components/DataTable';
import type { Product, License, Plan } from '@/types';
import { getProduct, getLicenses, getPlans, updateProductFeature } from '@/lib/api';
import { format } from 'date-fns';
import clsx from 'clsx';

type Tab = 'licenses' | 'customers' | 'features' | 'plans' | 'analytics' | 'settings';

const tabs: { id: Tab; label: string; icon: React.ReactNode }[] = [
  { id: 'licenses', label: 'Licenses', icon: <Key className="h-4 w-4" /> },
  { id: 'customers', label: 'Customers', icon: <Users className="h-4 w-4" /> },
  { id: 'features', label: 'Features', icon: <Layers className="h-4 w-4" /> },
  { id: 'plans', label: 'Plans', icon: <CreditCard className="h-4 w-4" /> },
  { id: 'analytics', label: 'Analytics', icon: <BarChart3 className="h-4 w-4" /> },
  { id: 'settings', label: 'Settings', icon: <Settings className="h-4 w-4" /> },
];

export default function ProductDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [product, setProduct] = useState<Product | null>(null);
  const [activeTab, setActiveTab] = useState<Tab>('licenses');
  const [loading, setLoading] = useState(true);
  const [licenses, setLicenses] = useState<License[]>([]);
  const [plans, setPlans] = useState<Plan[]>([]);

  useEffect(() => {
    if (!id) return;
    async function load() {
      try {
        const [p, l, pl] = await Promise.all([
          getProduct(id!),
          getLicenses({ productId: id! }),
          getPlans(id!),
        ]);
        setProduct(p);
        setLicenses(l.data || []);
        setPlans(pl);
      } catch {
        // Mock fallback
        setProduct({
          id: id!,
          name: 'AppLocker Pro',
          code: 'ALP',
          description: 'Application lock manager with advanced features',
          signingKey: 'sk_abc123def456ghi789',
          status: 'active',
          features: [
            { id: 'f1', name: 'Auto Lock', key: 'auto_lock', enabled: true, description: 'Automatically lock apps on idle' },
            { id: 'f2', name: 'Schedule Lock', key: 'schedule_lock', enabled: true, description: 'Lock apps on a schedule' },
            { id: 'f3', name: 'Password Protection', key: 'password_protect', enabled: true, description: 'Require password to unlock' },
            { id: 'f4', name: 'Advanced Analytics', key: 'analytics', enabled: false, description: 'Detailed usage analytics' },
          ],
          licenseCount: 450,
          deviceCount: 320,
          createdAt: '2024-01-15T00:00:00Z',
          updatedAt: '2024-03-01T00:00:00Z',
        });
        setLicenses([
          { id: '1', key: 'KEY-8F3A-2D1C-9B7E', productId: id!, productName: 'AppLocker Pro', customerId: 'c1', customerName: 'John Doe', customerEmail: 'john@example.com', planId: 'p1', planName: 'Pro', status: 'active', maxDevices: 3, usedDevices: 2, features: ['auto_lock', 'schedule_lock'], activatedAt: '2024-02-01T00:00:00Z', expiresAt: '2025-02-01T00:00:00Z', createdAt: '2024-02-01T00:00:00Z', updatedAt: '2024-03-01T00:00:00Z' },
          { id: '2', key: 'KEY-3C8D-1E6A-4F2B', productId: id!, productName: 'AppLocker Pro', customerId: 'c2', customerName: 'Jane Smith', customerEmail: 'jane@example.com', planId: 'p2', planName: 'Enterprise', status: 'active', maxDevices: 10, usedDevices: 5, features: ['auto_lock', 'schedule_lock', 'analytics'], activatedAt: '2024-01-20T00:00:00Z', expiresAt: '2025-01-20T00:00:00Z', createdAt: '2024-01-20T00:00:00Z', updatedAt: '2024-03-05T00:00:00Z' },
          { id: '3', key: 'KEY-7B2E-9A4F-6D3C', productId: id!, productName: 'AppLocker Pro', customerId: 'c3', customerName: 'Bob Wilson', customerEmail: 'bob@example.com', planId: 'p1', planName: 'Pro', status: 'expired', maxDevices: 3, usedDevices: 1, features: ['auto_lock'], activatedAt: '2023-06-01T00:00:00Z', expiresAt: '2024-06-01T00:00:00Z', createdAt: '2023-06-01T00:00:00Z', updatedAt: '2024-06-01T00:00:00Z' },
        ]);
        setPlans([
          { id: 'p1', name: 'Starter', productId: id!, maxDevices: 1, features: ['auto_lock'], price: 9.99, durationDays: 365, description: 'Basic features for individual users', createdAt: '2024-01-15T00:00:00Z' },
          { id: 'p2', name: 'Pro', productId: id!, maxDevices: 3, features: ['auto_lock', 'schedule_lock', 'password_protect'], price: 24.99, durationDays: 365, description: 'Advanced features for power users', createdAt: '2024-01-15T00:00:00Z' },
          { id: 'p3', name: 'Enterprise', productId: id!, maxDevices: 10, features: ['auto_lock', 'schedule_lock', 'password_protect', 'analytics'], price: 49.99, durationDays: 365, description: 'Full features for teams', createdAt: '2024-01-15T00:00:00Z' },
        ]);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, [id]);

  const handleToggleFeature = async (featureId: string, enabled: boolean) => {
    if (!id) return;
    try {
      await updateProductFeature(id, featureId, enabled);
      setProduct((prev) =>
        prev
          ? {
              ...prev,
              features: prev.features.map((f) =>
                f.id === featureId ? { ...f, enabled } : f
              ),
            }
          : prev
      );
      toast('success', `Feature ${enabled ? 'enabled' : 'disabled'}`);
    } catch {
      toast('error', 'Failed to update feature');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (!product) {
    return (
      <div className="text-center py-20">
        <p className="text-surface-400">Product not found</p>
        <button onClick={() => navigate('/products')} className="btn-primary mt-4">
          Back to Products
        </button>
      </div>
    );
  }

  const licenseColumns: Column<License>[] = [
    { key: 'key', header: 'Key', render: (item) => <span className="font-mono text-xs text-surface-200">{item.key}</span> },
    { key: 'customerName', header: 'Customer', render: (item) => <span className="text-surface-200">{item.customerName}</span> },
    { key: 'planName', header: 'Plan', render: (item) => <span className="badge badge-info">{item.planName}</span> },
    {
      key: 'status',
      header: 'Status',
      render: (item) => {
        const colors: Record<string, string> = { active: 'badge-success', expired: 'badge-neutral', suspended: 'badge-warning', revoked: 'badge-danger', pending: 'badge-info' };
        return <span className={clsx(colors[item.status], 'capitalize')}>{item.status}</span>;
      },
    },
    { key: 'usedDevices', header: 'Devices', render: (item) => <span className="text-surface-300">{item.usedDevices}/{item.maxDevices}</span> },
    { key: 'expiresAt', header: 'Expires', render: (item) => <span className="text-xs text-surface-400">{item.expiresAt ? format(new Date(item.expiresAt), 'MMM dd, yyyy') : 'Never'}</span> },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <button onClick={() => navigate('/products')} className="flex items-center gap-2 text-sm text-surface-400 hover:text-surface-200 mb-4">
          <ArrowLeft className="h-4 w-4" /> Back to Products
        </button>
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold text-surface-100">{product.name}</h1>
              <span className={clsx(
                product.status === 'active' ? 'badge-success' : product.status === 'disabled' ? 'badge-danger' : product.status === 'maintenance' ? 'badge-warning' : 'badge-neutral',
                'capitalize'
              )}>
                {product.status}
              </span>
            </div>
            <p className="mt-1 text-sm text-surface-400">{product.description}</p>
          </div>
          <div className="flex items-center gap-6 text-sm">
            <div>
              <p className="text-surface-500">Licenses</p>
              <p className="text-lg font-semibold text-surface-100">{product.licenseCount}</p>
            </div>
            <div>
              <p className="text-surface-500">Devices</p>
              <p className="text-lg font-semibold text-surface-100">{product.deviceCount}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="border-b border-surface-700">
        <div className="flex gap-1 overflow-x-auto">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={clsx(
                'flex items-center gap-2 border-b-2 px-4 py-3 text-sm font-medium transition-colors whitespace-nowrap',
                activeTab === tab.id
                  ? 'border-brand-500 text-brand-400'
                  : 'border-transparent text-surface-400 hover:text-surface-200'
              )}
            >
              {tab.icon}
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {/* Tab Content */}
      {activeTab === 'licenses' && (
        <DataTable
          columns={licenseColumns}
          data={licenses}
          keyExtractor={(item) => item.id}
          emptyMessage="No licenses for this product"
        />
      )}

      {activeTab === 'features' && (
        <div className="card p-5">
          <div className="space-y-4">
            {product.features.map((feature) => (
              <div key={feature.id} className="flex items-center justify-between rounded-lg bg-surface-800/50 px-4 py-3">
                <div>
                  <p className="text-sm font-medium text-surface-200">{feature.name}</p>
                  <p className="text-xs text-surface-500">{feature.description}</p>
                </div>
                <button
                  onClick={() => handleToggleFeature(feature.id, !feature.enabled)}
                  className="flex items-center gap-2"
                >
                  {feature.enabled ? (
                    <ToggleRight className="h-8 w-8 text-brand-400" />
                  ) : (
                    <ToggleLeft className="h-8 w-8 text-surface-500" />
                  )}
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'plans' && (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
          {plans.map((plan) => (
            <div key={plan.id} className="card p-5">
              <div className="flex items-start justify-between">
                <div>
                  <h3 className="text-lg font-semibold text-surface-100">{plan.name}</h3>
                  <p className="mt-1 text-2xl font-bold text-brand-400">${plan.price}<span className="text-sm font-normal text-surface-400">/yr</span></p>
                </div>
                <div className="flex gap-1">
                  <button className="rounded p-1.5 text-surface-400 hover:bg-surface-800 hover:text-surface-200">
                    <Pencil className="h-4 w-4" />
                  </button>
                  <button className="rounded p-1.5 text-surface-400 hover:bg-surface-800 hover:text-red-400">
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
              <p className="mt-2 text-sm text-surface-400">{plan.description}</p>
              <div className="mt-4 space-y-2">
                <p className="text-xs text-surface-500">Max Devices: <span className="text-surface-300">{plan.maxDevices}</span></p>
                <p className="text-xs text-surface-500">Duration: <span className="text-surface-300">{plan.durationDays} days</span></p>
                <div className="flex flex-wrap gap-1 mt-2">
                  {plan.features.map((f) => (
                    <span key={f} className="badge badge-info text-[10px]">{f}</span>
                  ))}
                </div>
              </div>
            </div>
          ))}
          <button className="card flex flex-col items-center justify-center border-dashed p-5 text-surface-400 hover:border-brand-500 hover:text-brand-400 transition-colors min-h-[200px]">
            <Plus className="h-8 w-8" />
            <span className="mt-2 text-sm font-medium">Add Plan</span>
          </button>
        </div>
      )}

      {activeTab === 'analytics' && (
        <div className="card p-8 text-center">
          <BarChart3 className="mx-auto h-12 w-12 text-surface-500" />
          <p className="mt-4 text-surface-400">Product-specific analytics will appear here</p>
        </div>
      )}

      {activeTab === 'customers' && (
        <div className="card p-8 text-center">
          <Users className="mx-auto h-12 w-12 text-surface-500" />
          <p className="mt-4 text-surface-400">Customer list for this product</p>
        </div>
      )}

      {activeTab === 'settings' && (
        <div className="card p-6 space-y-4 max-w-xl">
          <h3 className="text-lg font-semibold text-surface-100">Product Settings</h3>
          <div>
            <label className="label-text">Signing Key</label>
            <div className="flex items-center gap-2">
              <input type="text" readOnly value={product.signingKey} className="input-field font-mono text-xs" />
              <button onClick={() => { navigator.clipboard.writeText(product.signingKey); toast('success', 'Key copied'); }} className="btn-secondary whitespace-nowrap">Copy</button>
            </div>
          </div>
          <div>
            <label className="label-text">Status</label>
            <select className="input-field" defaultValue={product.status}>
              <option value="active">Active</option>
              <option value="disabled">Disabled</option>
              <option value="maintenance">Maintenance</option>
              <option value="archived">Archived</option>
            </select>
          </div>
          <button className="btn-primary">Save Changes</button>
        </div>
      )}
    </div>
  );
}
