import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, Package, ExternalLink } from 'lucide-react';
import DataTable, { Column } from '@/components/DataTable';
import Modal from '@/components/Modal';
import SearchBar from '@/components/SearchBar';
import { useToast } from '@/components/Toast';
import type { Product } from '@/types';
import { getProducts, createProduct, deleteProduct } from '@/lib/api';
import { format } from 'date-fns';
import clsx from 'clsx';

const statusColors: Record<string, string> = {
  active: 'badge-success',
  disabled: 'badge-danger',
  archived: 'badge-neutral',
  maintenance: 'badge-warning',
};

export default function Products() {
  const [products, setProducts] = useState<Product[]>([]);
  const [filtered, setFiltered] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [createOpen, setCreateOpen] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [formData, setFormData] = useState({ name: '', code: '', description: '' });
  const [submitting, setSubmitting] = useState(false);
  const navigate = useNavigate();
  const { toast } = useToast();

  useEffect(() => {
    loadProducts();
  }, []);

  useEffect(() => {
    const lower = search.toLowerCase();
    setFiltered(
      products.filter(
        (p) =>
          p.name.toLowerCase().includes(lower) ||
          p.code.toLowerCase().includes(lower) ||
          p.description?.toLowerCase().includes(lower)
      )
    );
  }, [search, products]);

  async function loadProducts() {
    try {
      const data = await getProducts();
      setProducts(data);
      setFiltered(data);
    } catch {
      // Mock data fallback
      const mock: Product[] = [
        { id: '1', name: 'AppLocker Pro', code: 'ALP', description: 'Application lock manager', signingKey: 'sk_abc123', status: 'active', features: [], licenseCount: 450, deviceCount: 320, createdAt: '2024-01-15T00:00:00Z', updatedAt: '2024-03-01T00:00:00Z' },
        { id: '2', name: 'NetGuard', code: 'NGD', description: 'Network security suite', signingKey: 'sk_def456', status: 'active', features: [], licenseCount: 320, deviceCount: 280, createdAt: '2024-02-10T00:00:00Z', updatedAt: '2024-03-05T00:00:00Z' },
        { id: '3', name: 'CryptoVault', code: 'CVT', description: 'Encryption tool', signingKey: 'sk_ghi789', status: 'active', features: [], licenseCount: 280, deviceCount: 195, createdAt: '2024-03-01T00:00:00Z', updatedAt: '2024-03-10T00:00:00Z' },
        { id: '4', name: 'DataSync', code: 'DSY', description: 'Data synchronization tool', signingKey: 'sk_jkl012', status: 'maintenance', features: [], licenseCount: 150, deviceCount: 80, createdAt: '2024-01-20T00:00:00Z', updatedAt: '2024-02-28T00:00:00Z' },
        { id: '5', name: 'CloudBackup', code: 'CLB', description: 'Cloud backup solution', signingKey: 'sk_mno345', status: 'disabled', features: [], licenseCount: 100, deviceCount: 45, createdAt: '2023-12-01T00:00:00Z', updatedAt: '2024-01-15T00:00:00Z' },
      ];
      setProducts(mock);
      setFiltered(mock);
    } finally {
      setLoading(false);
    }
  }

  async function handleCreate() {
    if (!formData.name || !formData.code) return;
    setSubmitting(true);
    try {
      await createProduct(formData);
      toast('success', 'Product created successfully');
      setCreateOpen(false);
      setFormData({ name: '', code: '', description: '' });
      loadProducts();
    } catch {
      toast('error', 'Failed to create product');
    } finally {
      setSubmitting(false);
    }
  }

  async function handleDelete() {
    if (!deleteId) return;
    try {
      await deleteProduct(deleteId);
      toast('success', 'Product deleted');
      setDeleteId(null);
      loadProducts();
    } catch {
      toast('error', 'Failed to delete product');
    }
  }

  const columns: Column<Product>[] = [
    {
      key: 'name',
      header: 'Name',
      sortable: true,
      render: (item) => (
        <div className="flex items-center gap-3">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-600/10 text-brand-400">
            <Package className="h-4 w-4" />
          </div>
          <div>
            <p className="font-medium text-surface-100">{item.name}</p>
            <p className="text-xs text-surface-500">{item.code}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      sortable: true,
      render: (item) => (
        <span className={clsx(statusColors[item.status], 'capitalize')}>
          {item.status}
        </span>
      ),
    },
    {
      key: 'licenseCount',
      header: 'Licenses',
      sortable: true,
      render: (item) => <span className="text-surface-200">{item.licenseCount}</span>,
    },
    {
      key: 'deviceCount',
      header: 'Devices',
      sortable: true,
      render: (item) => <span className="text-surface-200">{item.deviceCount}</span>,
    },
    {
      key: 'createdAt',
      header: 'Created',
      sortable: true,
      render: (item) => (
        <span className="text-surface-400 text-xs">
          {format(new Date(item.createdAt), 'MMM dd, yyyy')}
        </span>
      ),
    },
    {
      key: 'actions',
      header: '',
      render: (item) => (
        <div className="flex items-center gap-2">
          <button
            onClick={(e) => { e.stopPropagation(); navigate(`/products/${item.id}`); }}
            className="rounded-lg p-1.5 text-surface-400 hover:bg-surface-800 hover:text-surface-200"
            title="View details"
          >
            <ExternalLink className="h-4 w-4" />
          </button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-surface-100">Products</h1>
          <p className="text-sm text-surface-400">Manage your software products</p>
        </div>
        <button onClick={() => setCreateOpen(true)} className="btn-primary flex items-center gap-2">
          <Plus className="h-4 w-4" />
          Create Product
        </button>
      </div>

      <div className="flex items-center gap-4">
        <SearchBar
          placeholder="Search products..."
          value={search}
          onChange={setSearch}
          className="max-w-sm"
        />
        <span className="text-sm text-surface-400">{filtered.length} products</span>
      </div>

      <DataTable
        columns={columns}
        data={filtered}
        loading={loading}
        keyExtractor={(item) => item.id}
        onSort={(key, dir) => {
          const sorted = [...filtered].sort((a, b) => {
            const aVal = a[key as keyof Product];
            const bVal = b[key as keyof Product];
            const cmp = String(aVal).localeCompare(String(bVal));
            return dir === 'asc' ? cmp : -cmp;
          });
          setFiltered(sorted);
        }}
      />

      {/* Create Product Modal */}
      <Modal open={createOpen} onClose={() => setCreateOpen(false)} title="Create Product">
        <div className="space-y-4">
          <div>
            <label className="label-text">Product Name</label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              className="input-field"
              placeholder="e.g. AppLocker Pro"
            />
          </div>
          <div>
            <label className="label-text">Product Code</label>
            <input
              type="text"
              value={formData.code}
              onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
              className="input-field"
              placeholder="e.g. ALP"
              maxLength={10}
            />
          </div>
          <div>
            <label className="label-text">Description</label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              className="input-field"
              rows={3}
              placeholder="Product description..."
            />
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button onClick={() => setCreateOpen(false)} className="btn-secondary">Cancel</button>
            <button
              onClick={handleCreate}
              disabled={!formData.name || !formData.code || submitting}
              className="btn-primary"
            >
              {submitting ? 'Creating...' : 'Create Product'}
            </button>
          </div>
        </div>
      </Modal>

      {/* Delete Confirmation */}
      <Modal open={!!deleteId} onClose={() => setDeleteId(null)} size="sm" title="Delete Product">
        <p className="text-sm text-surface-400">
          Are you sure you want to delete this product? This action cannot be undone.
        </p>
        <div className="flex justify-end gap-3 pt-4">
          <button onClick={() => setDeleteId(null)} className="btn-secondary">Cancel</button>
          <button onClick={handleDelete} className="btn-danger">Delete</button>
        </div>
      </Modal>
    </div>
  );
}
