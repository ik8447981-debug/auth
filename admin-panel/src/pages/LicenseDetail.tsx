import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Key,
  Monitor,
  Clock,
  Shield,
  Copy,
  Pause,
  Trash2,
  RefreshCw,
} from 'lucide-react';
import LoadingSpinner from '@/components/LoadingSpinner';
import ConfirmDialog from '@/components/ConfirmDialog';
import { useToast } from '@/components/Toast';
import type { License, Device, LicenseValidation } from '@/types';
import { getLicense, getLicenseValidations, suspendLicense, revokeLicense } from '@/lib/api';
import { format } from 'date-fns';
import clsx from 'clsx';

const statusColors: Record<string, string> = {
  active: 'badge-success',
  expired: 'badge-neutral',
  suspended: 'badge-warning',
  revoked: 'badge-danger',
  pending: 'badge-info',
};

export default function LicenseDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [license, setLicense] = useState<License | null>(null);
  const [validations, setValidations] = useState<LicenseValidation[]>([]);
  const [loading, setLoading] = useState(true);
  const [confirmAction, setConfirmAction] = useState<'suspend' | 'revoke' | null>(null);

  useEffect(() => {
    if (!id) return;
    async function load() {
      try {
        const [l, v] = await Promise.all([getLicense(id!), getLicenseValidations(id!)]);
        setLicense(l);
        setValidations(v);
      } catch {
        setLicense({
          id: id!,
          key: 'KEY-8F3A-2D1C-9B7E-4A6F',
          productId: '1',
          productName: 'AppLocker Pro',
          customerId: 'c1',
          customerName: 'John Doe',
          customerEmail: 'john@example.com',
          planId: 'p2',
          planName: 'Pro',
          status: 'active',
          maxDevices: 3,
          usedDevices: 2,
          features: ['auto_lock', 'schedule_lock', 'password_protect'],
          activatedAt: '2024-02-01T00:00:00Z',
          expiresAt: '2025-02-01T00:00:00Z',
          createdAt: '2024-02-01T00:00:00Z',
          updatedAt: '2024-03-01T00:00:00Z',
        });
        setValidations([
          { id: 'v1', licenseId: id!, deviceId: 'DEV-001', result: 'valid', timestamp: '2024-03-15T10:30:00Z', ip: '192.168.1.100', metadata: { os: 'Windows 11', version: '2.1.0' } },
          { id: 'v2', licenseId: id!, deviceId: 'DEV-002', result: 'valid', timestamp: '2024-03-14T14:20:00Z', ip: '10.0.0.45', metadata: { os: 'Windows 10', version: '2.0.8' } },
          { id: 'v3', licenseId: id!, deviceId: 'DEV-001', result: 'expired', timestamp: '2024-03-10T09:15:00Z', ip: '192.168.1.100', metadata: { os: 'Windows 11', version: '2.1.0' } },
        ]);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, [id]);

  const handleConfirmAction = async () => {
    if (!license || !confirmAction) return;
    try {
      if (confirmAction === 'suspend') {
        await suspendLicense(license.id);
        toast('success', 'License suspended');
      } else {
        await revokeLicense(license.id);
        toast('success', 'License revoked');
      }
      setLicense((prev) => prev ? { ...prev, status: confirmAction === 'suspend' ? 'suspended' : 'revoked' } : prev);
    } catch {
      toast('error', `Failed to ${confirmAction} license`);
    }
    setConfirmAction(null);
  };

  if (loading) {
    return <div className="flex items-center justify-center py-20"><LoadingSpinner size="lg" /></div>;
  }

  if (!license) {
    return <div className="text-center py-20"><p className="text-surface-400">License not found</p></div>;
  }

  const mockDevices: Device[] = [
    { id: 'd1', licenseId: license.id, licenseKey: license.key, productId: license.productId, productName: license.productName, customerId: license.customerId, customerName: license.customerName, fingerprint: 'FP-001-ABC', hostname: 'DESKTOP-WORK-01', os: 'Windows 11', status: 'active', lastSeenAt: '2024-03-15T10:30:00Z', activatedAt: '2024-02-05T00:00:00Z', createdAt: '2024-02-05T00:00:00Z' },
    { id: 'd2', licenseId: license.id, licenseKey: license.key, productId: license.productId, productName: license.productName, customerId: license.customerId, customerName: license.customerName, fingerprint: 'FP-002-DEF', hostname: 'LAPTOP-HOME-02', os: 'Windows 10', status: 'active', lastSeenAt: '2024-03-14T14:20:00Z', activatedAt: '2024-02-10T00:00:00Z', createdAt: '2024-02-10T00:00:00Z' },
  ];

  return (
    <div className="space-y-6">
      <button onClick={() => navigate('/licenses')} className="flex items-center gap-2 text-sm text-surface-400 hover:text-surface-200">
        <ArrowLeft className="h-4 w-4" /> Back to Licenses
      </button>

      {/* License Info Card */}
      <div className="card p-6">
        <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
          <div className="space-y-4">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-brand-600/10 text-brand-400">
                <Key className="h-5 w-5" />
              </div>
              <div>
                <h1 className="text-xl font-bold text-surface-100 font-mono">{license.key}</h1>
                <p className="text-sm text-surface-400">{license.productName} — {license.planName}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
              <div>
                <p className="text-xs text-surface-500">Status</p>
                <span className={clsx(statusColors[license.status], 'capitalize mt-1 inline-block')}>{license.status}</span>
              </div>
              <div>
                <p className="text-xs text-surface-500">Customer</p>
                <p className="text-sm text-surface-200">{license.customerName}</p>
                <p className="text-xs text-surface-400">{license.customerEmail}</p>
              </div>
              <div>
                <p className="text-xs text-surface-500">Devices</p>
                <p className="text-sm text-surface-200">{license.usedDevices} / {license.maxDevices}</p>
              </div>
              <div>
                <p className="text-xs text-surface-500">Expires</p>
                <p className="text-sm text-surface-200">{license.expiresAt ? format(new Date(license.expiresAt), 'MMM dd, yyyy') : 'Never'}</p>
              </div>
            </div>

            <div>
              <p className="text-xs text-surface-500 mb-1">Entitled Features</p>
              <div className="flex flex-wrap gap-1.5">
                {license.features.length > 0 ? license.features.map((f) => (
                  <span key={f} className="badge badge-info">{f}</span>
                )) : <span className="text-xs text-surface-500">No features</span>}
              </div>
            </div>
          </div>

          <div className="flex flex-wrap gap-2">
            <button onClick={() => { navigator.clipboard.writeText(license.key); toast('success', 'Key copied'); }} className="btn-secondary flex items-center gap-2 text-sm">
              <Copy className="h-4 w-4" /> Copy Key
            </button>
            <button className="btn-secondary flex items-center gap-2 text-sm">
              <Clock className="h-4 w-4" /> Extend
            </button>
            {license.status === 'active' && (
              <button onClick={() => setConfirmAction('suspend')} className="btn-secondary flex items-center gap-2 text-sm text-yellow-400">
                <Pause className="h-4 w-4" /> Suspend
              </button>
            )}
            {license.status !== 'revoked' && (
              <button onClick={() => setConfirmAction('revoke')} className="btn-danger flex items-center gap-2 text-sm">
                <Trash2 className="h-4 w-4" /> Revoke
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Devices */}
      <div className="card p-5">
        <h3 className="flex items-center gap-2 mb-4 text-sm font-semibold text-surface-200">
          <Monitor className="h-4 w-4" /> Registered Devices ({mockDevices.length})
        </h3>
        <div className="space-y-2">
          {mockDevices.map((device) => (
            <div key={device.id} className="flex items-center justify-between rounded-lg bg-surface-800/50 px-4 py-3">
              <div className="flex items-center gap-3">
                <Monitor className="h-4 w-4 text-surface-400" />
                <div>
                  <p className="text-sm font-medium text-surface-200">{device.hostname}</p>
                  <p className="text-xs text-surface-500">{device.os} • {device.fingerprint}</p>
                </div>
              </div>
              <div className="flex items-center gap-3">
                <span className="text-xs text-surface-500">Last seen: {format(new Date(device.lastSeenAt), 'MMM dd, HH:mm')}</span>
                <span className={clsx(device.status === 'active' ? 'badge-success' : 'badge-neutral', 'capitalize')}>{device.status}</span>
                <button className="rounded p-1.5 text-surface-400 hover:bg-surface-800 hover:text-surface-200" title="Reset device">
                  <RefreshCw className="h-3.5 w-3.5" />
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Validation History */}
      <div className="card p-5">
        <h3 className="flex items-center gap-2 mb-4 text-sm font-semibold text-surface-200">
          <Shield className="h-4 w-4" /> Validation History
        </h3>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-surface-700">
                <th className="px-4 py-2 text-left text-xs font-semibold uppercase text-surface-400">Timestamp</th>
                <th className="px-4 py-2 text-left text-xs font-semibold uppercase text-surface-400">Device</th>
                <th className="px-4 py-2 text-left text-xs font-semibold uppercase text-surface-400">Result</th>
                <th className="px-4 py-2 text-left text-xs font-semibold uppercase text-surface-400">IP</th>
              </tr>
            </thead>
            <tbody>
              {validations.map((v) => (
                <tr key={v.id} className="table-row">
                  <td className="px-4 py-3 text-sm text-surface-200">{format(new Date(v.timestamp), 'MMM dd, yyyy HH:mm')}</td>
                  <td className="px-4 py-3 text-sm text-surface-300 font-mono">{v.deviceId}</td>
                  <td className="px-4 py-3">
                    <span className={clsx(
                      v.result === 'valid' ? 'badge-success' : v.result === 'expired' ? 'badge-warning' : 'badge-danger',
                      'capitalize'
                    )}>{v.result}</span>
                  </td>
                  <td className="px-4 py-3 text-sm text-surface-400 font-mono">{v.ip}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <ConfirmDialog
        open={!!confirmAction}
        onClose={() => setConfirmAction(null)}
        onConfirm={handleConfirmAction}
        title={confirmAction === 'suspend' ? 'Suspend License' : 'Revoke License'}
        message={confirmAction === 'suspend'
          ? 'This will temporarily disable the license. The user will not be able to activate or validate. You can reactivate it later.'
          : 'This will permanently revoke the license. This action cannot be undone.'}
        confirmLabel={confirmAction === 'suspend' ? 'Suspend' : 'Revoke'}
        variant={confirmAction === 'revoke' ? 'danger' : 'warning'}
      />
    </div>
  );
}
