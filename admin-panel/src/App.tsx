import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './lib/auth';
import Layout from './components/Layout';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Products from './pages/Products';
import ProductDetail from './pages/ProductDetail';
import Licenses from './pages/Licenses';
import LicenseDetail from './pages/LicenseDetail';
import Customers from './pages/Customers';
import Devices from './pages/Devices';
import Analytics from './pages/Analytics';
import AuditLogs from './pages/AuditLogs';
import Settings from './pages/Settings';
import BulkGenerate from './pages/BulkGenerate';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuth();
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return <>{children}</>;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <Layout />
          </ProtectedRoute>
        }
      >
        <Route index element={<Dashboard />} />
        <Route path="products" element={<Products />} />
        <Route path="products/:id" element={<ProductDetail />} />
        <Route path="licenses" element={<Licenses />} />
        <Route path="licenses/:id" element={<LicenseDetail />} />
        <Route path="customers" element={<Customers />} />
        <Route path="devices" element={<Devices />} />
        <Route path="analytics" element={<Analytics />} />
        <Route path="audit-logs" element={<AuditLogs />} />
        <Route path="settings" element={<Settings />} />
        <Route path="licenses/bulk-generate" element={<BulkGenerate />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
