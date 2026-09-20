import { useState, useEffect } from 'react';
import {
  Package,
  Key,
  Shield,
  Monitor,
  Activity,
  CheckCircle,
  ArrowRight,
} from 'lucide-react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  BarChart,
  Bar,
  Legend,
} from 'recharts';
import StatCard from '@/components/StatCard';
import LoadingSpinner from '@/components/LoadingSpinner';
import type { DashboardStats, AnalyticsData } from '@/types';
import { getDashboardStats, getAnalytics } from '@/lib/api';
import { format } from 'date-fns';

const CHART_COLORS = ['#6366f1', '#22d3ee', '#f59e0b', '#10b981', '#f43f5e', '#8b5cf6'];

const mockActivity = [
  {
    icon: <Key className="h-4 w-4 text-blue-400" />,
    iconBg: 'bg-blue-500/10',
    message: 'License KEY-8F3A-2D1C generated for AppLocker Pro',
    time: '2 minutes ago',
    badge: 'New',
    badgeClass: 'badge-info',
  },
  {
    icon: <Shield className="h-4 w-4 text-emerald-400" />,
    iconBg: 'bg-emerald-500/10',
    message: 'License activated by john@example.com on device DESKTOP-01',
    time: '15 minutes ago',
    badge: 'Active',
    badgeClass: 'badge-success',
  },
  {
    icon: <Activity className="h-4 w-4 text-yellow-400" />,
    iconBg: 'bg-yellow-500/10',
    message: 'License KEY-7B2E-9A4F suspended by admin',
    time: '1 hour ago',
    badge: 'Suspended',
    badgeClass: 'badge-warning',
  },
  {
    icon: <CheckCircle className="h-4 w-4 text-emerald-400" />,
    iconBg: 'bg-emerald-500/10',
    message: 'Validation successful for KEY-3C8D-1E6A from 192.168.1.45',
    time: '2 hours ago',
    badge: 'Valid',
    badgeClass: 'badge-success',
  },
  {
    icon: <Key className="h-4 w-4 text-brand-400" />,
    iconBg: 'bg-brand-500/10',
    message: 'Bulk generation: 50 licenses created for NetGuard Enterprise plan',
    time: '3 hours ago',
    badge: 'Bulk',
    badgeClass: 'badge-info',
  },
];

function generateMockAnalytics(): AnalyticsData {
  const days = Array.from({ length: 14 }, (_, i) => {
    const d = new Date();
    d.setDate(d.getDate() - (13 - i));
    return format(d, 'MMM dd');
  });

  return {
    activationsPerDay: days.map((date) => ({ date, count: Math.floor(Math.random() * 80) + 10 })),
    validationsPerDay: days.map((date) => ({ date, count: Math.floor(Math.random() * 400) + 50 })),
    newLicensesPerDay: days.map((date) => ({ date, count: Math.floor(Math.random() * 30) + 5 })),
    expiredLicensesPerDay: days.map((date) => ({ date, count: Math.floor(Math.random() * 10) })),
    productUsage: [
      { name: 'AppLocker Pro', value: 450 },
      { name: 'NetGuard', value: 320 },
      { name: 'CryptoVault', value: 280 },
      { name: 'DataSync', value: 150 },
      { name: 'CloudBackup', value: 100 },
    ],
    planDistribution: [
      { name: 'Free', value: 200 },
      { name: 'Starter', value: 500 },
      { name: 'Pro', value: 800 },
      { name: 'Enterprise', value: 350 },
    ],
    licenseStatusDistribution: [
      { status: 'Active', count: 1203 },
      { status: 'Expired', count: 412 },
      { status: 'Suspended', count: 89 },
      { status: 'Revoked', count: 143 },
    ],
    topProducts: [
      { name: 'AppLocker Pro', activations: 342, validations: 12450 },
      { name: 'NetGuard', activations: 280, validations: 8930 },
      { name: 'CryptoVault', activations: 195, validations: 6720 },
    ],
  };
}

export default function Dashboard() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [analytics, setAnalytics] = useState<AnalyticsData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      try {
        const [s, a] = await Promise.all([
          getDashboardStats().catch(() => ({
            totalProducts: 12,
            totalLicenses: 1847,
            activeLicenses: 1203,
            activeDevices: 892,
            activationsToday: 47,
            validationsToday: 312,
          })),
          getAnalytics().catch(() => generateMockAnalytics()),
        ]);
        setStats(s);
        setAnalytics(a);
      } catch {
        setStats({
          totalProducts: 12,
          totalLicenses: 1847,
          activeLicenses: 1203,
          activeDevices: 892,
          activationsToday: 47,
          validationsToday: 312,
        });
        setAnalytics(generateMockAnalytics());
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-surface-100">Dashboard</h1>
        <p className="text-sm text-surface-400">
          Overview of your license platform
        </p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
        <StatCard icon={<Package className="h-5 w-5" />} value={stats?.totalProducts ?? 0} label="Total Products" />
        <StatCard icon={<Key className="h-5 w-5" />} value={stats?.totalLicenses ?? 0} label="Total Licenses" />
        <StatCard icon={<Shield className="h-5 w-5" />} value={stats?.activeLicenses ?? 0} label="Active Licenses" />
        <StatCard icon={<Monitor className="h-5 w-5" />} value={stats?.activeDevices ?? 0} label="Active Devices" />
        <StatCard icon={<Activity className="h-5 w-5" />} value={stats?.activationsToday ?? 0} label="Activations Today" trend={{ value: 12, isPositive: true }} />
        <StatCard icon={<CheckCircle className="h-5 w-5" />} value={stats?.validationsToday ?? 0} label="Validations Today" trend={{ value: 5, isPositive: true }} />
      </div>

      {/* Charts Row 1 */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Activations Trend */}
        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-surface-200">Activations Trend</h3>
          <ResponsiveContainer width="100%" height={280}>
            <LineChart data={analytics?.activationsPerDay ?? []}>
              <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
              <XAxis dataKey="date" stroke="#64748b" fontSize={12} />
              <YAxis stroke="#64748b" fontSize={12} />
              <Tooltip
                contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }}
                labelStyle={{ color: '#94a3b8' }}
              />
              <Line type="monotone" dataKey="count" stroke="#6366f1" strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </div>

        {/* Product Usage Pie */}
        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-surface-200">Product Usage</h3>
          <ResponsiveContainer width="100%" height={280}>
            <PieChart>
              <Pie
                data={analytics?.productUsage ?? []}
                cx="50%"
                cy="50%"
                innerRadius={60}
                outerRadius={100}
                paddingAngle={2}
                dataKey="value"
                nameKey="name"
              >
                {(analytics?.productUsage ?? []).map((_, index) => (
                  <Cell key={index} fill={CHART_COLORS[index % CHART_COLORS.length]} />
                ))}
              </Pie>
              <Tooltip
                contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }}
              />
              <Legend
                formatter={(value: string) => <span className="text-surface-300 text-xs">{value}</span>}
              />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Charts Row 2 */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* License Status Distribution */}
        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-surface-200">License Status Distribution</h3>
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={analytics?.licenseStatusDistribution ?? []}>
              <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
              <XAxis dataKey="status" stroke="#64748b" fontSize={12} />
              <YAxis stroke="#64748b" fontSize={12} />
              <Tooltip
                contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }}
              />
              <Bar dataKey="count" fill="#6366f1" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        {/* Validations Trend */}
        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-surface-200">Validations Trend</h3>
          <ResponsiveContainer width="100%" height={280}>
            <LineChart data={analytics?.validationsPerDay ?? []}>
              <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
              <XAxis dataKey="date" stroke="#64748b" fontSize={12} />
              <YAxis stroke="#64748b" fontSize={12} />
              <Tooltip
                contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }}
                labelStyle={{ color: '#94a3b8' }}
              />
              <Line type="monotone" dataKey="count" stroke="#22d3ee" strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Recent Activity */}
      <div className="card p-5">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-sm font-semibold text-surface-200">Recent Activity</h3>
          <button className="flex items-center gap-1 text-xs text-brand-400 hover:text-brand-300">
            View all <ArrowRight className="h-3 w-3" />
          </button>
        </div>
        <div className="space-y-3">
          {mockActivity.map((item, i) => (
            <div key={i} className="flex items-center gap-3 rounded-lg bg-surface-800/30 px-4 py-3">
              <div className={`flex h-8 w-8 items-center justify-center rounded-full ${item.iconBg}`}>
                {item.icon}
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm text-surface-200 truncate">{item.message}</p>
                <p className="text-xs text-surface-500">{item.time}</p>
              </div>
              <span className={`badge ${item.badgeClass}`}>{item.badge}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
