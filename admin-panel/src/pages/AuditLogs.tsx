import { useState, useEffect } from 'react';
import { FileText, Eye, Calendar } from 'lucide-react';
import DataTable, { Column } from '@/components/DataTable';
import SearchBar from '@/components/SearchBar';
import Modal from '@/components/Modal';
import type { AuditLog } from '@/types';
import { getAuditLogs } from '@/lib/api';
import { format } from 'date-fns';
import clsx from 'clsx';

export default function AuditLogs() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [actionFilter, setActionFilter] = useState('all');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [detailLog, setDetailLog] = useState<AuditLog | null>(null);

  useEffect(() => {
    loadLogs();
  }, [page, actionFilter, startDate, endDate]);

  async function loadLogs() {
    setLoading(true);
    try {
      const res = await getAuditLogs({
        page,
        pageSize: 15,
        action: actionFilter === 'all' ? undefined : actionFilter,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
      });
      setLogs(res.data);
      setTotalPages(res.totalPages);
    } catch {
      const mock: AuditLog[] = [
        { id: 'a1', timestamp: '2024-03-15T10:30:00Z', actorId: 'u1', actorName: 'Admin', actorEmail: 'admin@example.com', action: 'license.generate', targetType: 'license', targetId: 'KEY-8F3A', targetName: 'AppLocker Pro', result: 'success', ip: '192.168.1.1', metadata: { plan: 'Pro', maxDevices: 3 } },
        { id: 'a2', timestamp: '2024-03-15T09:15:00Z', actorId: 'u1', actorName: 'Admin', actorEmail: 'admin@example.com', action: 'license.suspend', targetType: 'license', targetId: 'KEY-5A1F', targetName: 'CryptoVault', result: 'success', ip: '192.168.1.1', metadata: { reason: 'Payment overdue' } },
        { id: 'a3', timestamp: '2024-03-14T16:45:00Z', actorId: 'u2', actorName: 'SuperAdmin', actorEmail: 'super@example.com', action: 'product.create', targetType: 'product', targetId: 'p6', targetName: 'NewProduct', result: 'success', ip: '10.0.0.1', metadata: {} },
        { id: 'a4', timestamp: '2024-03-14T14:20:00Z', actorId: 'u1', actorName: 'Admin', actorEmail: 'admin@example.com', action: 'license.revoke', targetType: 'license', targetId: 'KEY-2F7C', targetName: 'AppLocker Pro', result: 'success', ip: '192.168.1.1', metadata: { reason: 'Abuse' } },
        { id: 'a5', timestamp: '2024-03-14T11:00:00Z', actorId: 'u3', actorName: 'Support', actorEmail: 'support@example.com', action: 'license.extend', targetType: 'license', targetId: 'KEY-9D4B', targetName: 'NetGuard', result: 'failure', ip: '172.16.0.1', metadata: { error: 'License not found' } },
        { id: 'a6', timestamp: '2024-03-13T09:30:00Z', actorId: 'u1', actorName: 'Admin', actorEmail: 'admin@example.com', action: 'customer.create', targetType: 'customer', targetId: 'c7', targetName: 'New Customer', result: 'success', ip: '192.168.1.1', metadata: { email: 'new@example.com' } },
      ];
      let filtered = mock;
      if (actionFilter !== 'all') filtered = filtered.filter((l) => l.action === actionFilter);
      setLogs(filtered);
      setTotalPages(1);
    } finally {
      setLoading(false);
    }
  }

  const columns: Column<AuditLog>[] = [
    {
      key: 'timestamp',
      header: 'Timestamp',
      sortable: true,
      render: (item) => <span className="text-xs text-surface-400">{format(new Date(item.timestamp), 'MMM dd, HH:mm:ss')}</span>,
    },
    { key: 'actorName', header: 'Actor', render: (item) => (
      <div>
        <p className="text-sm text-surface-200">{item.actorName}</p>
        <p className="text-xs text-surface-500">{item.actorEmail}</p>
      </div>
    )},
    {
      key: 'action',
      header: 'Action',
      render: (item) => {
        const actionColors: Record<string, string> = {
          'license.generate': 'badge-info',
          'license.suspend': 'badge-warning',
          'license.revoke': 'badge-danger',
          'license.extend': 'badge-success',
          'product.create': 'badge-info',
          'customer.create': 'badge-info',
        };
        return <span className={clsx(actionColors[item.action] || 'badge-neutral')}>{item.action}</span>;
      },
    },
    { key: 'targetName', header: 'Target', render: (item) => <span className="text-sm text-surface-200">{item.targetName}</span> },
    {
      key: 'result',
      header: 'Result',
      render: (item) => (
        <span className={clsx(item.result === 'success' ? 'badge-success' : 'badge-danger')}>
          {item.result}
        </span>
      ),
    },
    { key: 'ip', header: 'IP', render: (item) => <span className="text-xs text-surface-400 font-mono">{item.ip}</span> },
    {
      key: 'details',
      header: '',
      render: (item) => (
        <button onClick={() => setDetailLog(item)} className="rounded-lg p-1.5 text-surface-400 hover:bg-surface-800 hover:text-surface-200">
          <Eye className="h-4 w-4" />
        </button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-surface-100">Audit Logs</h1>
        <p className="text-sm text-surface-400">Track all system actions and changes</p>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <SearchBar placeholder="Search logs..." value={search} onChange={setSearch} className="max-w-sm" />
        <select value={actionFilter} onChange={(e) => setActionFilter(e.target.value)} className="input-field w-auto">
          <option value="all">All Actions</option>
          <option value="license.generate">License Generate</option>
          <option value="license.suspend">License Suspend</option>
          <option value="license.revoke">License Revoke</option>
          <option value="license.extend">License Extend</option>
          <option value="product.create">Product Create</option>
          <option value="customer.create">Customer Create</option>
        </select>
        <div className="flex items-center gap-2">
          <Calendar className="h-4 w-4 text-surface-400" />
          <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} className="input-field w-auto" />
          <span className="text-surface-500">to</span>
          <input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} className="input-field w-auto" />
        </div>
      </div>

      <DataTable
        columns={columns}
        data={logs}
        loading={loading}
        keyExtractor={(item) => item.id}
        currentPage={page}
        totalPages={totalPages}
        onPageChange={setPage}
        pageSize={15}
        emptyMessage="No audit logs found"
      />

      {/* Detail Modal */}
      <Modal open={!!detailLog} onClose={() => setDetailLog(null)} title="Audit Log Details" size="lg">
        {detailLog && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-xs text-surface-500">Timestamp</p>
                <p className="text-sm text-surface-200">{format(new Date(detailLog.timestamp), 'PPpp')}</p>
              </div>
              <div>
                <p className="text-xs text-surface-500">Actor</p>
                <p className="text-sm text-surface-200">{detailLog.actorName} ({detailLog.actorEmail})</p>
              </div>
              <div>
                <p className="text-xs text-surface-500">Action</p>
                <p className="text-sm text-surface-200">{detailLog.action}</p>
              </div>
              <div>
                <p className="text-xs text-surface-500">Target</p>
                <p className="text-sm text-surface-200">{detailLog.targetType}: {detailLog.targetName} ({detailLog.targetId})</p>
              </div>
              <div>
                <p className="text-xs text-surface-500">Result</p>
                <span className={clsx(detailLog.result === 'success' ? 'badge-success' : 'badge-danger')}>{detailLog.result}</span>
              </div>
              <div>
                <p className="text-xs text-surface-500">IP Address</p>
                <p className="text-sm text-surface-200 font-mono">{detailLog.ip}</p>
              </div>
            </div>
            {Object.keys(detailLog.metadata).length > 0 && (
              <div>
                <p className="text-xs text-surface-500 mb-2">Metadata</p>
                <pre className="rounded-lg bg-surface-800 p-4 text-xs text-surface-200 overflow-x-auto">
                  {JSON.stringify(detailLog.metadata, null, 2)}
                </pre>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
}
