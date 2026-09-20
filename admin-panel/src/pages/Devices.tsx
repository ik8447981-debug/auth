import { useState, useEffect } from 'react';
import { Monitor, RefreshCw } from 'lucide-react';
import DataTable, { Column } from '@/components/DataTable';
import SearchBar from '@/components/SearchBar';
import ProductSwitcher from '@/components/ProductSwitcher';
import ConfirmDialog from '@/components/ConfirmDialog';
import { useToast } from '@/components/Toast';
import type { Device } from '@/types';
import { getDevices, resetDevice } from '@/lib/api';
import { format } from 'date-fns';
import clsx from 'clsx';

const statusColors: Record<string, string> = {
  active: 'badge-success',
  deactivated: 'badge-neutral',
  expired: 'badge-danger',
};

export default function Devices() {
  const [devices, setDevices] = useState<Device[]>([]);
  const [filtered, setFiltered] = useState<Device[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [selectedProductId, setSelectedProductId] = useState<string | null>(null);
  const [resetId, setResetId] = useState<string | null>(null);
  const { toast } = useToast();

  useEffect(() => {
    loadDevices();
  }, []);

  useEffect(() => {
    let result = devices;
    if (statusFilter !== 'all') result = result.filter((d) => d.status === statusFilter);
    if (selectedProductId) result = result.filter((d) => d.productId === selectedProductId);
    if (search) {
      const lower = search.toLowerCase();
      result = result.filter(
        (d) =>
          d.licenseKey.toLowerCase().includes(lower) ||
          d.customerName.toLowerCase().includes(lower) ||
          d.hostname.toLowerCase().includes(lower) ||
          d.fingerprint.toLowerCase().includes(lower)
      );
    }
    setFiltered(result);
  }, [search, statusFilter, selectedProductId, devices]);

  async function loadDevices() {
    try {
      const data = await getDevices();
      setDevices(data);
      setFiltered(data);
    } catch {
      const mock: Device[] = [
        { id: 'd1', licenseId: '1', licenseKey: 'KEY-8F3A-2D1C', productId: '1', productName: 'AppLocker Pro', customerId: 'c1', customerName: 'John Doe', fingerprint: 'FP-001-ABC', hostname: 'DESKTOP-WORK-01', os: 'Windows 11', status: 'active', lastSeenAt: '2024-03-15T10:30:00Z', activatedAt: '2024-02-05T00:00:00Z', createdAt: '2024-02-05T00:00:00Z' },
        { id: 'd2', licenseId: '1', licenseKey: 'KEY-8F3A-2D1C', productId: '1', productName: 'AppLocker Pro', customerId: 'c1', customerName: 'John Doe', fingerprint: 'FP-002-DEF', hostname: 'LAPTOP-HOME-02', os: 'Windows 10', status: 'active', lastSeenAt: '2024-03-14T14:20:00Z', activatedAt: '2024-02-10T00:00:00Z', createdAt: '2024-02-10T00:00:00Z' },
        { id: 'd3', licenseId: '2', licenseKey: 'KEY-3C8D-1E6A', productId: '2', productName: 'NetGuard', customerId: 'c2', customerName: 'Jane Smith', fingerprint: 'FP-003-GHI', hostname: 'SRV-PROD-01', os: 'Ubuntu 22.04', status: 'active', lastSeenAt: '2024-03-15T08:00:00Z', activatedAt: '2024-01-25T00:00:00Z', createdAt: '2024-01-25T00:00:00Z' },
        { id: 'd4', licenseId: '2', licenseKey: 'KEY-3C8D-1E6A', productId: '2', productName: 'NetGuard', customerId: 'c2', customerName: 'Jane Smith', fingerprint: 'FP-004-JKL', hostname: 'SRV-STAGING-02', os: 'Ubuntu 22.04', status: 'deactivated', lastSeenAt: '2024-02-28T16:45:00Z', activatedAt: '2024-01-30T00:00:00Z', createdAt: '2024-01-30T00:00:00Z' },
        { id: 'd5', licenseId: '3', licenseKey: 'KEY-7B2E-9A4F', productId: '1', productName: 'AppLocker Pro', customerId: 'c3', customerName: 'Bob Wilson', fingerprint: 'FP-005-MNO', hostname: 'PC-OLD-01', os: 'Windows 10', status: 'expired', lastSeenAt: '2024-01-15T12:00:00Z', activatedAt: '2023-06-10T00:00:00Z', createdAt: '2023-06-10T00:00:00Z' },
      ];
      setDevices(mock);
      setFiltered(mock);
    } finally {
      setLoading(false);
    }
  }

  async function handleReset() {
    if (!resetId) return;
    try {
      await resetDevice(resetId);
      toast('success', 'Device reset successfully');
      setResetId(null);
      loadDevices();
    } catch {
      toast('error', 'Failed to reset device');
    }
  }

  const columns: Column<Device>[] = [
    { key: 'licenseKey', header: 'License Key', render: (item) => <span className="font-mono text-xs text-brand-400">{item.licenseKey}</span> },
    { key: 'productName', header: 'Product', render: (item) => <span className="text-surface-200">{item.productName}</span> },
    { key: 'customerName', header: 'Customer', render: (item) => <span className="text-surface-200">{item.customerName}</span> },
    { key: 'hostname', header: 'Hostname', render: (item) => (
      <div>
        <p className="text-sm text-surface-200">{item.hostname}</p>
        <p className="text-xs text-surface-500">{item.os}</p>
      </div>
    )},
    { key: 'lastSeenAt', header: 'Last Seen', sortable: true, render: (item) => <span className="text-xs text-surface-400">{format(new Date(item.lastSeenAt), 'MMM dd, HH:mm')}</span> },
    {
      key: 'status',
      header: 'Status',
      render: (item) => <span className={clsx(statusColors[item.status], 'capitalize')}>{item.status}</span>,
    },
    {
      key: 'actions',
      header: '',
      render: (item) => (
        <button
          onClick={() => setResetId(item.id)}
          className="rounded-lg p-1.5 text-surface-400 hover:bg-surface-800 hover:text-surface-200"
          title="Reset device"
        >
          <RefreshCw className="h-4 w-4" />
        </button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-surface-100">Devices</h1>
        <p className="text-sm text-surface-400">Manage registered devices</p>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <SearchBar placeholder="Search devices..." value={search} onChange={setSearch} className="max-w-sm" />
        <ProductSwitcher selectedProductId={selectedProductId} onSelect={setSelectedProductId} />
        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="input-field w-auto">
          <option value="all">All Status</option>
          <option value="active">Active</option>
          <option value="deactivated">Deactivated</option>
          <option value="expired">Expired</option>
        </select>
        <span className="text-sm text-surface-400">{filtered.length} devices</span>
      </div>

      <DataTable columns={columns} data={filtered} loading={loading} keyExtractor={(item) => item.id} emptyMessage="No devices found" />

      <ConfirmDialog
        open={!!resetId}
        onClose={() => setResetId(null)}
        onConfirm={handleReset}
        title="Reset Device"
        message="This will deactivate the device and free up a device slot. The user will need to reactivate on this device."
        confirmLabel="Reset"
        variant="warning"
      />
    </div>
  );
}
