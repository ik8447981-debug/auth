import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import { Menu, X } from 'lucide-react';

export default function Layout() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <div className="flex h-screen overflow-hidden bg-surface-950">
      {/* Mobile sidebar overlay */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/50 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <div
        className={`fixed inset-y-0 left-0 z-50 w-64 transform transition-transform duration-200 ease-in-out lg:translate-x-0 lg:static lg:z-auto ${
          sidebarOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <Sidebar onClose={() => setSidebarOpen(false)} />
      </div>

      {/* Main content */}
      <div className="flex flex-1 flex-col overflow-hidden">
        {/* Top bar */}
        <header className="flex h-16 items-center justify-between border-b border-surface-800 bg-surface-900/80 px-4 backdrop-blur-sm lg:px-6">
          <button
            onClick={() => setSidebarOpen(true)}
            className="rounded-lg p-2 text-surface-400 hover:bg-surface-800 hover:text-surface-200 lg:hidden"
          >
            <Menu className="h-5 w-5" />
          </button>

          <div className="flex items-center gap-4 lg:hidden">
            <span className="text-lg font-semibold text-gradient">LicenseHub</span>
          </div>

          <div className="hidden lg:block" />

          <div className="flex items-center gap-3">
            <ThemeToggle />
            <UserMenu />
          </div>
        </header>

        {/* Page content */}
        <main className="flex-1 overflow-y-auto p-4 lg:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

function ThemeToggle() {
  const [dark, setDark] = useState(() => {
    return document.documentElement.classList.contains('dark');
  });

  const toggle = () => {
    setDark(!dark);
    document.documentElement.classList.toggle('dark', !dark);
  };

  return (
    <button
      onClick={toggle}
      className="rounded-lg p-2 text-surface-400 hover:bg-surface-800 hover:text-surface-200 transition-colors"
      title={dark ? 'Switch to light mode' : 'Switch to dark mode'}
    >
      {dark ? (
        <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
        </svg>
      ) : (
        <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
        </svg>
      )}
    </button>
  );
}

function UserMenu() {
  const [open, setOpen] = useState(false);
  const user = JSON.parse(localStorage.getItem('auth_user') || '{}');

  return (
    <div className="relative">
      <button
        onClick={() => setOpen(!open)}
        className="flex items-center gap-2 rounded-lg p-2 text-surface-300 hover:bg-surface-800 hover:text-surface-100 transition-colors"
      >
        <div className="flex h-8 w-8 items-center justify-center rounded-full bg-brand-600 text-sm font-medium text-white">
          {(user.username || 'A')[0].toUpperCase()}
        </div>
        <span className="hidden text-sm font-medium lg:block">{user.username || 'Admin'}</span>
      </button>

      {open && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setOpen(false)} />
          <div className="dropdown-menu right-0 w-56">
            <div className="border-b border-surface-700 px-4 py-3">
              <p className="text-sm font-medium text-surface-100">{user.username || 'Admin'}</p>
              <p className="text-xs text-surface-400">{user.email || 'admin@example.com'}</p>
            </div>
            <button
              onClick={() => {
                localStorage.removeItem('auth_token');
                localStorage.removeItem('auth_user');
                window.location.href = '/login';
              }}
              className="dropdown-item text-red-400 hover:text-red-300"
            >
              Sign out
            </button>
          </div>
        </>
      )}
    </div>
  );
}
