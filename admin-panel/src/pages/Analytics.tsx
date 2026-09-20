import { useState, useEffect } from 'react';
import { BarChart3, Calendar } from 'lucide-react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  Legend,
} from 'recharts';
import LoadingSpinner from '@/components/LoadingSpinner';
import ProductSwitcher from '@/components/ProductSwitcher';
import type { AnalyticsData } from '@/types';
import { getAnalytics } from '@/lib/api';
import { format } from 'date-fns';

const CHART_COLORS = ['#6366f1', '#22d3ee', '#f59e0b', '#10b981', '#f43f5e', '#8b5cf6'];

export default function Analytics() {
  const [analytics, setAnalytics] = useState<AnalyticsData | null>(null);
  const [loading, setLoading] = useState(true);
  const [selectedProductId, setSelectedProductId] = useState<string | null>(null);
  const [startDate, setStartDate] = useState(() => {
    const d = new Date();
    d.setDate(d.getDate() - 30);
    return format(d, 'yyyy-MM-dd');
  });
  const [endDate, setEndDate] = useState(() => format(new Date(), 'yyyy-MM-dd'));

  useEffect(() => {
    loadAnalytics();
  }, [selectedProductId, startDate, endDate]);

  async function loadAnalytics() {
    setLoading(true);
    try {
      const data = await getAnalytics({
        startDate,
        endDate,
        productId: selectedProductId || undefined,
      });
      setAnalytics(data);
    } catch {
      // Mock analytics
      const days = Array.from({ length: 30 }, (_, i) => {
        const d = new Date();
        d.setDate(d.getDate() - (29 - i));
        return format(d, 'MMM dd');
      });
      setAnalytics({
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
        topProducts: [],
      });
    } finally {
      setLoading(false);
    }
  }

  if (loading) {
    return <div className="flex items-center justify-center py-20"><LoadingSpinner size="lg" /></div>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-surface-100">Analytics</h1>
        <p className="text-sm text-surface-400">Platform-wide analytics and insights</p>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-4">
        <ProductSwitcher selectedProductId={selectedProductId} onSelect={setSelectedProductId} />
        <div className="flex items-center gap-2">
          <Calendar className="h-4 w-4 text-surface-400" />
          <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} className="input-field w-auto" />
          <span className="text-surface-500">to</span>
          <input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} className="input-field w-auto" />
        </div>
      </div>

      {/* Charts Grid */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Activations per day */}
        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-surface-200">Activations per Day</h3>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={analytics?.activationsPerDay ?? []}>
              <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
              <XAxis dataKey="date" stroke="#64748b" fontSize={11} tick={{ fill: '#64748b' }} />
              <YAxis stroke="#64748b" fontSize={11} />
              <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }} />
              <Line type="monotone" dataKey="count" stroke="#6366f1" strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </div>

        {/* Validations per day */}
        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-surface-200">Validations per Day</h3>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={analytics?.validationsPerDay ?? []}>
              <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
              <XAxis dataKey="date" stroke="#64748b" fontSize={11} />
              <YAxis stroke="#64748b" fontSize={11} />
              <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }} />
              <Line type="monotone" dataKey="count" stroke="#22d3ee" strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </div>

        {/* New licenses per day */}
        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-surface-200">New Licenses per Day</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={analytics?.newLicensesPerDay ?? []}>
              <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
              <XAxis dataKey="date" stroke="#64748b" fontSize={11} />
              <YAxis stroke="#64748b" fontSize={11} />
              <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }} />
              <Bar dataKey="count" fill="#10b981" radius={[2, 2, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        {/* Expired licenses per day */}
        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-surface-200">Expired Licenses per Day</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={analytics?.expiredLicensesPerDay ?? []}>
              <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
              <XAxis dataKey="date" stroke="#64748b" fontSize={11} />
              <YAxis stroke="#64748b" fontSize={11} />
              <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }} />
              <Bar dataKey="count" fill="#f43f5e" radius={[2, 2, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        {/* Product Usage Pie */}
        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-surface-200">Product Usage Distribution</h3>
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie data={analytics?.productUsage ?? []} cx="50%" cy="50%" innerRadius={60} outerRadius={110} paddingAngle={3} dataKey="value" nameKey="name">
                {(analytics?.productUsage ?? []).map((_, index) => (
                  <Cell key={index} fill={CHART_COLORS[index % CHART_COLORS.length]} />
                ))}
              </Pie>
              <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }} />
              <Legend formatter={(value: string) => <span className="text-surface-300 text-xs">{value}</span>} />
            </PieChart>
          </ResponsiveContainer>
        </div>

        {/* Plan Distribution */}
        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-surface-200">Plan Distribution</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={analytics?.planDistribution ?? []}>
              <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
              <XAxis dataKey="name" stroke="#64748b" fontSize={11} />
              <YAxis stroke="#64748b" fontSize={11} />
              <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }} />
              <Bar dataKey="value" fill="#8b5cf6" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        {/* License Status Distribution */}
        <div className="card p-5">
          <h3 className="mb-4 text-sm font-semibold text-surface-200">License Status Distribution</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={analytics?.licenseStatusDistribution ?? []}>
              <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
              <XAxis dataKey="status" stroke="#64748b" fontSize={11} />
              <YAxis stroke="#64748b" fontSize={11} />
              <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }} />
              <Bar dataKey="count" radius={[4, 4, 0, 0]}>
                {(analytics?.licenseStatusDistribution ?? []).map((entry, index) => (
                  <Cell key={index} fill={CHART_COLORS[index % CHART_COLORS.length]} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  );
}
