import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard,
  Package,
  Key,
  Users,
  Monitor,
  BarChart3,
  FileText,
  Settings,
  X,
  Zap,
} from 'lucide-react';
import clsx from 'clsx';

const navigation = [
  { name: 'Dashboard', href: '/', icon: LayoutDashboard },
  { name: 'Products', href: '/products', icon: Package },
  { name: 'Licenses', href: '/licenses', icon: Key },
  { name: 'Customers', href: '/customers', icon: Users },
  { name: 'Devices', href: '/devices', icon: Monitor },
  { name: 'Analytics', href: '/analytics', icon: BarChart3 },
  { name: 'Audit Logs', href: '/audit-logs', icon: FileText },
  { name: 'Settings', href: '/settings', icon: Settings },
];

interface SidebarProps {
  onClose?: () => void;
}

export default function Sidebar({ onClose }: SidebarProps) {
  return (
    <div className="flex h-full flex-col bg-surface-900 border-r border-surface-800">
      {/* Logo */}
      <div className="flex h-16 items-center justify-between px-6 border-b border-surface-800">
        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-600">
            <Zap className="h-5 w-5 text-white" />
          </div>
          <span className="text-lg font-bold text-gradient">LicenseHub</span>
        </div>
        <button
          onClick={onClose}
          className="rounded-lg p-1 text-surface-400 hover:bg-surface-800 hover:text-surface-200 lg:hidden"
        >
          <X className="h-5 w-5" />
        </button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 px-3 py-4">
        {navigation.map((item) => (
          <NavLink
            key={item.name}
            to={item.href}
            end={item.href === '/'}
            onClick={onClose}
            className={({ isActive }) =>
              clsx(
                'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all duration-150',
                isActive
                  ? 'bg-brand-600/10 text-brand-400 ring-1 ring-brand-600/20'
                  : 'text-surface-400 hover:bg-surface-800 hover:text-surface-200'
              )
            }
          >
            <item.icon className="h-5 w-5 flex-shrink-0" />
            {item.name}
          </NavLink>
        ))}
      </nav>

      {/* Footer */}
      <div className="border-t border-surface-800 px-4 py-4">
        <div className="rounded-lg bg-surface-800/50 p-3">
          <p className="text-xs text-surface-500">LicenseHub Admin</p>
          <p className="text-xs text-surface-600">v1.0.0</p>
        </div>
      </div>
    </div>
  );
}
