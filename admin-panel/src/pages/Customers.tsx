import { useState, useEffect } from 'react';
import { Plus, Users, Eye } from 'lucide-react';
import DataTable, { Column } from '@/components/DataTable';
import SearchBar from '@/components/SearchBar';
import Modal from '@/components/Modal';
import { useToast } from '@/components/Toast';
import type { Customer } from '@/types';
import { getCustomers, createCustomer } from '@/lib/api';
import { format } from 'date-fns';
import clsx from 'clsx';

const statusColors: Record<string, string> = {
  active: 'badge-success',
  inactive: 'badge-neutral',
  banned: 'badge-danger',
};

export default function Customers() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [filtered, setFiltered] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [createOpen, setCreateOpen] = useState(false);
  const [detailCustomer, setDetailCustomer] = useState<Customer | null>(null);
  const [formData, setFormData] = useState({ name: '', email: '', phone: '', company: '' });
  const [submitting, setSubmitting] = useState(false);
  const { toast } = useToast();

  useEffect(() => {
    loadCustomers();
  }, []);

  useEffect(() => {
    const lower = search.toLowerCase();
    setFiltered(
      customers.filter(
        (c) =>
          c.name.toLowerCase().includes(lower) ||
          c.email.toLowerCase().includes(lower) ||
          c.company?.toLowerCase().includes(lower)
      )
    );
  }, [search, customers]);

  async function loadCustomers() {
    try {
      const data = await getCustomers();
      setCustomers(data);
      setFiltered(data);
    } catch {
      const mock: Customer[] = [
        { id: 'c1', name: 'John Doe', email: 'john@example.com', phone: '+1-555-0101', company: 'Acme Corp', status: 'active', licenseCount: 3, createdAt: '2024-01-15T00:00:00Z', updatedAt: '2024-03-01T00:00:00Z' },
        { id: 'c2', name: 'Jane Smith', email: 'jane@example.com', phone: '+1-555-0102', company: 'TechStart Inc', status: 'active', licenseCount: 5, createdAt: '2024-02-01T00:00:00Z', updatedAt: '2024-03-10T00:00:00Z' },
        { id: 'c3', name: 'Bob Wilson', email: 'bob@example.com', phone: '+1-555-0103', company: 'Wilson Ltd', status: 'inactive', licenseCount: 1, createdAt: '2023-11-20T00:00:00Z', updatedAt: '2024-02-15T00:00:00Z' },
        { id: 'c4', name: 'Alice Brown', email: 'alice@example.com', phone: '+1-555-0104', company: 'Design Studio', status: 'active', licenseCount: 2, createdAt: '2024-03-01T00:00:00Z', updatedAt: '2024-03-15T00:00:00Z' },
        { id: 'c5', name: 'Charlie Davis', email: 'charlie@example.com', phone: '+1-555-0105', company: 'DataFlow Inc', status: 'active', licenseCount: 8, createdAt: '2023-10-05T00:00:00Z', updatedAt: '2024-03-12T00:00:00Z' },
        { id: 'c6', name: 'Diana Evans', email: 'diana@example.com', phone: '+1-555-0106', company: 'Evans Consulting', status: 'banned', licenseCount: 0, createdAt: '2024-01-10T00:00:00Z', updatedAt: '2024-03-01T00:00:00Z' },
      ];
      setCustomers(mock);
      setFiltered(mock);
    } finally {
      setLoading(false);
    }
  }

  async function handleCreate() {
    if (!formData.name || !formData.email) return;
    setSubmitting(true);
    try {
      await createCustomer(formData);
      toast('success', 'Customer created');
      setCreateOpen(false);
      setFormData({ name: '', email: '', phone: '', company: '' });
      loadCustomers();
    } catch {
      toast('error', 'Failed to create customer');
    } finally {
      setSubmitting(false);
    }
  }

  const columns: Column<Customer>[] = [
    {
      key: 'name',
      header: 'Name',
      sortable: true,
      render: (item) => (
        <div className="flex items-center gap-3">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-brand-600/10 text-brand-400 text-sm font-medium">
            {item.name[0].toUpperCase()}
          </div>
          <div>
            <p className="font-medium text-surface-100">{item.name}</p>
            <p className="text-xs text-surface-500">{item.email}</p>
          </div>
        </div>
      ),
    },
    { key: 'company', header: 'Company', sortable: true, render: (item) => <span className="text-surface-300">{item.company || '-'}</span> },
    {
      key: 'status',
      header: 'Status',
      sortable: true,
      render: (item) => <span className={clsx(statusColors[item.status], 'capitalize')}>{item.status}</span>,
    },
    { key: 'licenseCount', header: 'Licenses', sortable: true, render: (item) => <span className="text-surface-200">{item.licenseCount}</span> },
    { key: 'createdAt', header: 'Created', sortable: true, render: (item) => <span className="text-xs text-surface-400">{format(new Date(item.createdAt), 'MMM dd, yyyy')}</span> },
    {
      key: 'actions',
      header: '',
      render: (item) => (
        <button onClick={() => setDetailCustomer(item)} className="rounded-lg p-1.5 text-surface-400 hover:bg-surface-800 hover:text-surface-200">
          <Eye className="h-4 w-4" />
        </button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-surface-100">Customers</h1>
          <p className="text-sm text-surface-400">Manage your customer accounts</p>
        </div>
        <button onClick={() => setCreateOpen(true)} className="btn-primary flex items-center gap-2">
          <Plus className="h-4 w-4" /> Add Customer
        </button>
      </div>

      <div className="flex items-center gap-4">
        <SearchBar placeholder="Search customers..." value={search} onChange={setSearch} className="max-w-sm" />
        <span className="text-sm text-surface-400">{filtered.length} customers</span>
      </div>

      <DataTable
        columns={columns}
        data={filtered}
        loading={loading}
        keyExtractor={(item) => item.id}
      />

      {/* Create Customer Modal */}
      <Modal open={createOpen} onClose={() => setCreateOpen(false)} title="Add Customer">
        <div className="space-y-4">
          <div>
            <label className="label-text">Name</label>
            <input type="text" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} className="input-field" placeholder="Full name" />
          </div>
          <div>
            <label className="label-text">Email</label>
            <input type="email" value={formData.email} onChange={(e) => setFormData({ ...formData, email: e.target.value })} className="input-field" placeholder="email@example.com" />
          </div>
          <div>
            <label className="label-text">Phone</label>
            <input type="tel" value={formData.phone} onChange={(e) => setFormData({ ...formData, phone: e.target.value })} className="input-field" placeholder="+1-555-0100" />
          </div>
          <div>
            <label className="label-text">Company</label>
            <input type="text" value={formData.company} onChange={(e) => setFormData({ ...formData, company: e.target.value })} className="input-field" placeholder="Company name" />
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button onClick={() => setCreateOpen(false)} className="btn-secondary">Cancel</button>
            <button onClick={handleCreate} disabled={!formData.name || !formData.email || submitting} className="btn-primary">
              {submitting ? 'Creating...' : 'Create Customer'}
            </button>
          </div>
        </div>
      </Modal>

      {/* Customer Detail Modal */}
      <Modal open={!!detailCustomer} onClose={() => setDetailCustomer(null)} title="Customer Details" size="lg">
        {detailCustomer && (
          <div className="space-y-4">
            <div className="flex items-center gap-4">
              <div className="flex h-12 w-12 items-center justify-center rounded-full bg-brand-600 text-lg font-bold text-white">
                {detailCustomer.name[0].toUpperCase()}
              </div>
              <div>
                <h3 className="text-lg font-semibold text-surface-100">{detailCustomer.name}</h3>
                <p className="text-sm text-surface-400">{detailCustomer.email}</p>
              </div>
              <span className={clsx(statusColors[detailCustomer.status], 'capitalize ml-auto')}>{detailCustomer.status}</span>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-xs text-surface-500">Phone</p>
                <p className="text-sm text-surface-200">{detailCustomer.phone || '-'}</p>
              </div>
              <div>
                <p className="text-xs text-surface-500">Company</p>
                <p className="text-sm text-surface-200">{detailCustomer.company || '-'}</p>
              </div>
              <div>
                <p className="text-xs text-surface-500">Licenses</p>
                <p className="text-sm text-surface-200">{detailCustomer.licenseCount}</p>
              </div>
              <div>
                <p className="text-xs text-surface-500">Created</p>
                <p className="text-sm text-surface-200">{format(new Date(detailCustomer.createdAt), 'MMM dd, yyyy')}</p>
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
