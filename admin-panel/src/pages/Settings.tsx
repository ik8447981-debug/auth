import { useState } from 'react';
import { Key, Users, Save, Copy, RefreshCw } from 'lucide-react';
import { useToast } from '@/components/Toast';

function SettingsIcon(props: React.SVGProps<SVGSVGElement>) {
  return (
    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <path d="M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z" />
      <circle cx="12" cy="12" r="3" />
    </svg>
  );
}

export default function Settings() {
  const { toast } = useToast();
  const [activeSection, setActiveSection] = useState<'system' | 'signing' | 'admins'>('system');

  const [systemSettings, setSystemSettings] = useState({
    platformName: 'LicenseHub',
    maxDevicesDefault: 3,
    licenseExpiryDefault: 365,
    allowDeviceReset: true,
    enableAuditLogging: true,
    sessionTimeout: 30,
    rateLimitPerMinute: 60,
  });

  const [signingKeys] = useState([
    { id: 'k1', name: 'Production Key', key: 'sk_prod_abc123def456ghi789', createdAt: '2024-01-01T00:00:00Z', lastUsed: '2024-03-15T10:30:00Z', status: 'active' },
    { id: 'k2', name: 'Staging Key', key: 'sk_stg_xyz789uvw456rst123', createdAt: '2024-02-01T00:00:00Z', lastUsed: '2024-03-10T14:00:00Z', status: 'active' },
    { id: 'k3', name: 'Legacy Key', key: 'sk_leg_old987ghi654jkl321', createdAt: '2023-06-01T00:00:00Z', lastUsed: '2023-12-15T00:00:00Z', status: 'rotated' },
  ]);

  const [admins] = useState([
    { id: 'u1', username: 'admin', email: 'admin@example.com', role: 'superadmin', lastLogin: '2024-03-15T10:30:00Z', status: 'active' },
    { id: 'u2', username: 'support', email: 'support@example.com', role: 'admin', lastLogin: '2024-03-14T16:45:00Z', status: 'active' },
    { id: 'u3', username: 'viewer', email: 'viewer@example.com', role: 'admin', lastLogin: '2024-03-10T09:00:00Z', status: 'active' },
  ]);

  const sections = [
    { id: 'system' as const, label: 'System', icon: <SettingsIcon className="h-4 w-4" /> },
    { id: 'signing' as const, label: 'Signing Keys', icon: <Key className="h-4 w-4" /> },
    { id: 'admins' as const, label: 'Admin Users', icon: <Users className="h-4 w-4" /> },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-surface-100">Settings</h1>
        <p className="text-sm text-surface-400">Manage system configuration</p>
      </div>

      <div className="flex gap-1 border-b border-surface-700">
        {sections.map((s) => (
          <button
            key={s.id}
            onClick={() => setActiveSection(s.id)}
            className={`flex items-center gap-2 border-b-2 px-4 py-3 text-sm font-medium transition-colors ${
              activeSection === s.id
                ? 'border-brand-500 text-brand-400'
                : 'border-transparent text-surface-400 hover:text-surface-200'
            }`}
          >
            {s.icon}
            {s.label}
          </button>
        ))}
      </div>

      {activeSection === 'system' && (
        <div className="card p-6 space-y-6 max-w-2xl">
          <h3 className="text-lg font-semibold text-surface-100">System Configuration</h3>
          <div className="space-y-4">
            <div>
              <label className="label-text">Platform Name</label>
              <input
                type="text"
                value={systemSettings.platformName}
                onChange={(e) => setSystemSettings({ ...systemSettings, platformName: e.target.value })}
                className="input-field"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="label-text">Default Max Devices</label>
                <input
                  type="number"
                  value={systemSettings.maxDevicesDefault}
                  onChange={(e) => setSystemSettings({ ...systemSettings, maxDevicesDefault: parseInt(e.target.value) || 1 })}
                  className="input-field"
                  min={1}
                />
              </div>
              <div>
                <label className="label-text">Default License Expiry (days)</label>
                <input
                  type="number"
                  value={systemSettings.licenseExpiryDefault}
                  onChange={(e) => setSystemSettings({ ...systemSettings, licenseExpiryDefault: parseInt(e.target.value) || 30 })}
                  className="input-field"
                  min={1}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="label-text">Session Timeout (minutes)</label>
                <input
                  type="number"
                  value={systemSettings.sessionTimeout}
                  onChange={(e) => setSystemSettings({ ...systemSettings, sessionTimeout: parseInt(e.target.value) || 5 })}
                  className="input-field"
                  min={5}
                />
              </div>
              <div>
                <label className="label-text">Rate Limit (req/min)</label>
                <input
                  type="number"
                  value={systemSettings.rateLimitPerMinute}
                  onChange={(e) => setSystemSettings({ ...systemSettings, rateLimitPerMinute: parseInt(e.target.value) || 10 })}
                  className="input-field"
                  min={1}
                />
              </div>
            </div>
            <div className="space-y-3">
              <label className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={systemSettings.allowDeviceReset}
                  onChange={(e) => setSystemSettings({ ...systemSettings, allowDeviceReset: e.target.checked })}
                  className="h-4 w-4 rounded border-surface-600 bg-surface-800 text-brand-600 focus:ring-brand-500 focus:ring-offset-0"
                />
                <span className="text-sm text-surface-200">Allow device reset</span>
              </label>
              <label className="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  checked={systemSettings.enableAuditLogging}
                  onChange={(e) => setSystemSettings({ ...systemSettings, enableAuditLogging: e.target.checked })}
                  className="h-4 w-4 rounded border-surface-600 bg-surface-800 text-brand-600 focus:ring-brand-500 focus:ring-offset-0"
                />
                <span className="text-sm text-surface-200">Enable audit logging</span>
              </label>
            </div>
            <button onClick={() => toast('success', 'Settings saved')} className="btn-primary flex items-center gap-2">
              <Save className="h-4 w-4" /> Save Changes
            </button>
          </div>
        </div>
      )}

      {activeSection === 'signing' && (
        <div className="space-y-4 max-w-3xl">
          <div className="flex justify-end">
            <button className="btn-primary flex items-center gap-2">
              <RefreshCw className="h-4 w-4" /> Generate New Key
            </button>
          </div>
          {signingKeys.map((sk) => (
            <div key={sk.id} className="card p-5">
              <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <div>
                  <div className="flex items-center gap-2">
                    <h4 className="font-medium text-surface-100">{sk.name}</h4>
                    <span className={sk.status === 'active' ? 'badge-success' : 'badge-neutral'}>
                      {sk.status}
                    </span>
                  </div>
                  <div className="mt-2 flex items-center gap-2">
                    <code className="rounded bg-surface-800 px-2 py-1 text-xs text-surface-300 font-mono">{sk.key}</code>
                    <button
                      onClick={() => { navigator.clipboard.writeText(sk.key); toast('success', 'Key copied'); }}
                      className="rounded p-1 text-surface-400 hover:bg-surface-800 hover:text-surface-200"
                    >
                      <Copy className="h-3.5 w-3.5" />
                    </button>
                  </div>
                  <p className="mt-1 text-xs text-surface-500">
                    Created: {new Date(sk.createdAt).toLocaleDateString()} &bull; Last used: {new Date(sk.lastUsed).toLocaleDateString()}
                  </p>
                </div>
                {sk.status === 'active' && (
                  <button className="btn-secondary text-sm">Rotate</button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {activeSection === 'admins' && (
        <div className="card overflow-hidden max-w-3xl">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-surface-700 bg-surface-800/50">
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase text-surface-400">User</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase text-surface-400">Role</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase text-surface-400">Last Login</th>
                  <th className="px-4 py-3 text-left text-xs font-semibold uppercase text-surface-400">Status</th>
                </tr>
              </thead>
              <tbody>
                {admins.map((admin) => (
                  <tr key={admin.id} className="table-row">
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-3">
                        <div className="flex h-8 w-8 items-center justify-center rounded-full bg-brand-600/10 text-brand-400 text-sm font-medium">
                          {admin.username[0].toUpperCase()}
                        </div>
                        <div>
                          <p className="text-sm font-medium text-surface-200">{admin.username}</p>
                          <p className="text-xs text-surface-500">{admin.email}</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <span className={admin.role === 'superadmin' ? 'badge badge-info' : 'badge badge-neutral'}>
                        {admin.role}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-xs text-surface-400">
                      {new Date(admin.lastLogin).toLocaleString()}
                    </td>
                    <td className="px-4 py-3">
                      <span className="badge-success">active</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
