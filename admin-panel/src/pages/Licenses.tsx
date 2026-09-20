import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus,
  Download,
  MoreHorizontal,
  Eye,
  Pencil,
  Clock,
  Pause,
  Trash2,
  Copy,
  Zap,
} from 'lucide-react';
import DataTable, { Column } from '@/components/DataTable';
import SearchBar from '@/components/SearchBar';
import ProductSwitcher from '@/components/ProductSwitcher';
import { useToast } from '@/components/Toast';
import type { License, Product } from '@/types';
import { getLicenses, getProducts, suspendLicense, revokeLicense } from '@/lib/api';
import { format } from 'date-fns';
import clsx from 'clsx';

const statusColors: Record<string, string> = {
  active: 'badge-success',
  expired: 'badge-neutral',
  suspended: 'badge-warning',
  revoked: 'badge-danger',
  pending: 'badge-info',
};

export default function Licenses() {
  const [licenses, setLicenses] = useState<License[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [selectedProductId, setSelectedProductId] = useState<string | null>(null);
  const [actionMenuId, setActionMenuId] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const navigate = useNavigate();
  const { toast } = useToast();

  useEffect(() => {
    loadLicenses();
  }, [page, statusFilter, selectedProductId]);

  useEffect(() => {
    setPage(1);
  }, [search, statusFilter, selectedProductId]);

  async function loadLicenses() {
    setLoading(true);
    try {
      const res = await getLicenses({
        page,
        pageSize: 10,
        status: statusFilter === 'all' ? undefined : statusFilter,
        productId: selectedProductId || undefined,
        search: search || undefined,
      });
      setLicenses(res.data);
      setTotalPages(res.totalPages);
    } catch {
      // Mock data
      const mock: License[] = [
        { id: '1', key: 'KEY-8F3A-2D1C-9B7E', productId: '1', productName: 'AppLocker Pro', customerId: 'c1', customerName: 'John Doe', customerEmail: 'john@example.com', planId: 'p2', planName: 'Pro', status: 'active', maxDevices: 3, usedDevices: 2, features: ['auto_lock', 'schedule_lock'], activatedAt: '2024-02-01T00:00:00Z', expiresAt: '2025-02-01T00:00:00Z', createdAt: '2024-02-01T00:00:00Z', updatedAt: '2024-03-01T00:00:00Z' },
        { id: '2', key: 'KEY-3C8D-1E6A-4F2B', productId: '2', productName: 'NetGuard', customerId: 'c2', customerName: 'Jane Smith', customerEmail: 'jane@example.com', planId: 'p3', planName: 'Enterprise', status: 'active', maxDevices: 10, usedDevices: 5, features: ['auto_lock', 'schedule_lock', 'analytics'], activatedAt: '2024-01-20T00:00:00Z', expiresAt: '2025-01-20T00:00:00Z', createdAt: '2024-01-20T00:00:00Z', updatedAt: '2024-03-05T00:00:00Z' },
        { id: '3', key: 'KEY-7B2E-9A4F-6D3C', productId: '1', productName: 'AppLocker Pro', customerId: 'c3', customerName: 'Bob Wilson', customerEmail: 'bob@example.com', planId: 'p1', planName: 'Starter', status: 'expired', maxDevices: 1, usedDevices: 1, features: ['auto_lock'], activatedAt: '2023-06-01T00:00:00Z', expiresAt: '2024-06-01T00:00:00Z', createdAt: '2023-06-01T00:00:00Z', updatedAt: '2024-06-01T00:00:00Z' },
        { id: '4', key: 'KEY-5A1F-8C3D-2E9B', productId: '3', productName: 'CryptoVault', customerId: 'c4', customerName: 'Alice Brown', customerEmail: 'alice@example.com', planId: 'p2', planName: 'Pro', status: 'suspended', maxDevices: 3, usedDevices: 0, features: ['auto_lock'], activatedAt: null, expiresAt: null, createdAt: '2024-03-10T00:00:00Z', updatedAt: '2024-03-12T00:00:00Z' },
        { id: '5', key: 'KEY-9D4B-6E2A-1C8F', productId: '2', productName: 'NetGuard', customerId: 'c5', customerName: 'Charlie Davis', customerEmail: 'charlie@example.com', planId: 'p3', planName: 'Enterprise', status: 'active', maxDevices: 10, usedDevices: 8, features: ['auto_lock', 'schedule_lock', 'password_protect', 'analytics'], activatedAt: '2024-01-01T00:00:00Z', expiresAt: '2025-01-01T00:00:00Z', createdAt: '2024-01-01T00:00:00Z', updatedAt: '2024-03-15T00:00:00Z' },
        { id: '6', key: 'KEY-2F7C-4B9D-8A1E', productId: '1', productName: 'AppLocker Pro', customerId: 'c6', customerName: 'Diana Evans', customerEmail: 'diana@example.com', planId: 'p1', planName: 'Starter', status: 'revoked', maxDevices: 1, usedDevices: 0, features: [], activatedAt: null, expiresAt: null, createdAt: '2024-02-20T00:00:00Z', updatedAt: '2024-03-01T00:00:00Z' },
      ];
      let filtered = mock;
      if (statusFilter !== 'all') filtered = filtered.filter((l) => l.status === statusFilter);
      if (selectedProductId) filtered = filtered.filter((l) => l.productId === selectedProductId);
      if (search) {
        const lower = search.toLowerCase();
        filtered = filtered.filter(
          (l) => l.key.toLowerCase().includes(lower) || l.customerEmail.toLowerCase().includes(lower) || l.customerName.toLowerCase().includes(lower)
        );
      }
      setLicenses(filtered);
      setTotalPages(1);
    } finally {
      setLoading(false);
    }
  }

  const handleAction = async (action: string, license: License) => {
    setActionMenuId(null);
    switch (action) {
      case 'view':
        navigate(`/licenses/${license.id}`);
        break;
      case 'copy':
        navigator.clipboard.writeText(license.key);
        toast('success', 'License key copied to clipboard');
        break;
      case 'suspend':
        try {
          await suspendLicense(license.id);
          toast('success', 'License suspended');
          loadLicenses();
        } catch {
          toast('error', 'Failed to suspend license');
        }
        break;
      case 'revoke':
        try {
          await revokeLicense(license.id);
          toast('success', 'License revoked');
          loadLicenses();
        } catch {
          toast('error', 'Failed to revoke license');
        }
        break;
      default:
        toast('info', `Action: ${action}`);
    }
  };

  const columns: Column<License>[] = [
    {
      key: 'key',
      header: 'Key',
      sortable: true,
      render: (item) => (
        <span className="font-mono text-xs text-brand-400">{item.key}</span>
      ),
    },
    { key: 'productName', header: 'Product', sortable: true, render: (item) => <span className="text-surface-200">{item.productName}</span> },
    { key: 'customerName', header: 'Customer', sortable: true, render: (item) => (
      <div>
        <p className="text-sm text-surface-200">{item.customerName}</p>
        <p className="text-xs text-surface-500">{item.customerEmail}</p>
      </div>
    )},
    { key: 'planName', header: 'Plan', render: (item) => <span className="badge badge-info">{item.planName}</span> },
    {
      key: 'status',
      header: 'Status',
      sortable: true,
      render: (item) => <span className={clsx(statusColors[item.status], 'capitalize')}>{item.status}</span>,
    },
    { key: 'usedDevices', header: 'Devices', render: (item) => <span className="text-surface-300">{item.usedDevices}/{item.maxDevices}</span> },
    { key: 'createdAt', header: 'Created', sortable: true, render: (item) => <span className="text-xs text-surface-400">{format(new Date(item.createdAt), 'MMM dd, yyyy')}</span> },
    { key: 'expiresAt', header: 'Expires', sortable: true, render: (item) => <span className="text-xs text-surface-400">{item.expiresAt ? format(new Date(item.expiresAt), 'MMM dd, yyyy') : 'Never'}</span> },
    {
      key: 'actions',
      header: '',
      render: (item) => (
        <div className="relative">
          <button
            onClick={(e) => { e.stopPropagation(); setActionMenuId(actionMenuId === item.id ? null : item.id); }}
            className="rounded-lg p-1.5 text-surface-400 hover:bg-surface-800 hover:text-surface-200"
          >
            <MoreHorizontal className="h-4 w-4" />
          </button>
          {actionMenuId === item.id && (
            <>
              <div className="fixed inset-0 z-40" onClick={() => setActionMenuId(null)} />
              <div className="dropdown-menu right-0 z-50 w-48">
                <button onClick={() => handleAction('view', item)} className="dropdown-item"><Eye className="h-4 w-4" /> View Details</button>
                <button onClick={() => handleAction('copy', item)} className="dropdown-item"><Copy className="h-4 w-4" /> Copy Key</button>
                <button onClick={() => handleAction('edit', item)} className="dropdown-item"><Pencil className="h-4 w-4" /> Edit</button>
                <button onClick={() => handleAction('extend', item)} className="dropdown-item"><Clock className="h-4 w-4" /> Extend</button>
                {item.status === 'active' && (
                  <button onClick={() => handleAction('suspend', item)} className="dropdown-item text-yellow-400"><Pause className="h-4 w-4" /> Suspend</button>
                )}
                {item.status !== 'revoked' && (
                  <button onClick={() => handleAction('revoke', item)} className="dropdown-item text-red-400"><Trash2 className="h-4 w-4" /> Revoke</button>
                )}
                <button onClick={() => handleAction('reset', item)} className="dropdown-item"><Zap className="h-4 w-4" /> Reset Devices</button>
              </div>
            </>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-surface-100">Licenses</h1>
          <p className="text-sm text-surface-400">Manage all licenses across products</p>
        </div>
        <div className="flex items-center gap-3">
          <button className="btn-secondary flex items-center gap-2">
            <Download className="h-4 w-4" /> Export CSV
          </button>
          <button onClick={() => navigate('/licenses/bulk-generate')} className="btn-primary flex items-center gap-2">
            <Plus className="h-4 w-4" /> Generate License
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <SearchBar
          placeholder="Search by key, customer email..."
          value={search}
          onChange={setSearch}
          className="max-w-sm"
        />
        <ProductSwitcher selectedProductId={selectedProductId} onSelect={setSelectedProductId} />
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="input-field w-auto"
        >
          <option value="all">All Status</option>
          <option value="active">Active</option>
          <option value="expired">Expired</option>
          <option value="suspended">Suspended</option>
          <option value="revoked">Revoked</option>
          <option value="pending">Pending</option>
        </select>
        <span className="text-sm text-surface-400">{licenses.length} licenses</span>
      </div>

      <DataTable
        columns={columns}
        data={licenses}
        loading={loading}
        keyExtractor={(item) => item.id}
        currentPage={page}
        totalPages={totalPages}
        onPageChange={setPage}
        emptyMessage="No licenses found"
      />
    </div>
  );
}
