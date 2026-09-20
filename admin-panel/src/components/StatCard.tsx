import { TrendingUp, TrendingDown } from 'lucide-react';
import clsx from 'clsx';

interface StatCardProps {
  icon: React.ReactNode;
  value: string | number;
  label: string;
  trend?: {
    value: number;
    isPositive: boolean;
  };
  className?: string;
}

export default function StatCard({ icon, value, label, trend, className }: StatCardProps) {
  return (
    <div className={clsx('card p-5', className)}>
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-brand-600/10 text-brand-400">
            {icon}
          </div>
          <div>
            <p className="text-2xl font-bold text-surface-100">{value}</p>
            <p className="text-sm text-surface-400">{label}</p>
          </div>
        </div>
        {trend && (
          <div
            className={clsx(
              'flex items-center gap-1 rounded-full px-2 py-1 text-xs font-medium',
              trend.isPositive
                ? 'bg-emerald-500/10 text-emerald-400'
                : 'bg-red-500/10 text-red-400'
            )}
          >
            {trend.isPositive ? (
              <TrendingUp className="h-3 w-3" />
            ) : (
              <TrendingDown className="h-3 w-3" />
            )}
            {Math.abs(trend.value)}%
          </div>
        )}
      </div>
    </div>
  );
}
