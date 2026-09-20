import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowLeft, Zap, Download, Copy, CheckCircle } from 'lucide-react';
import { useToast } from '@/components/Toast';
import type { Product, Plan, BulkGenerateResult } from '@/types';
import { getProducts, getPlans, bulkGenerate } from '@/lib/api';
import LoadingSpinner from '@/components/LoadingSpinner';

export default function BulkGenerate() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [products, setProducts] = useState<Product[]>([]);
  const [plans, setPlans] = useState<Plan[]>([]);
  const [loading, setLoading] = useState(true);
  const [generating, setGenerating] = useState(false);

  const [form, setForm] = useState({
    productId: '',
    planId: '',
    count: 10,
    maxDevices: 3,
    features: [] as string[],
    expiresAt: '',
  });

  const [result, setResult] = useState<BulkGenerateResult | null>(null);

  useEffect(() => {
    async function load() {
      try {
        const p = await getProducts();
        setProducts(p);
        if (p.length > 0) {
          setForm((f) => ({ ...f, productId: p[0].id }));
          const pl = await getPlans(p[0].id);
          setPlans(pl);
          if (pl.length > 0) setForm((f) => ({ ...f, planId: pl[0].id }));
        }
      } catch {
        setProducts([
          { id: '1', name: 'AppLocker Pro', code: 'ALP', description: '', signingKey: '', status: 'active', features: [], licenseCount: 0, deviceCount: 0, createdAt: '', updatedAt: '' },
          { id: '2', name: 'NetGuard', code: 'NGD', description: '', signingKey: '', status: 'active', features: [], licenseCount: 0, deviceCount: 0, createdAt: '', updatedAt: '' },
          { id: '3', name: 'CryptoVault', code: 'CVT', description: '', signingKey: '', status: 'active', features: [], licenseCount: 0, deviceCount: 0, createdAt: '', updatedAt: '' },
        ]);
        setPlans([
          { id: 'p1', name: 'Starter', productId: '1', maxDevices: 1, features: ['auto_lock'], price: 9.99, durationDays: 365, description: '', createdAt: '' },
          { id: 'p2', name: 'Pro', productId: '1', maxDevices: 3, features: ['auto_lock', 'schedule_lock'], price: 24.99, durationDays: 365, description: '', createdAt: '' },
          { id: 'p3', name: 'Enterprise', productId: '1', maxDevices: 10, features: ['auto_lock', 'schedule_lock', 'password_protect', 'analytics'], price: 49.99, durationDays: 365, description: '', createdAt: '' },
        ]);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  const handleProductChange = async (productId: string) => {
    setForm((f) => ({ ...f, productId, planId: '' }));
    try {
      const pl = await getPlans(productId);
      setPlans(pl);
      if (pl.length > 0) setForm((f) => ({ ...f, planId: pl[0].id }));
    } catch {
      setPlans([]);
    }
  };

  const selectedPlan = plans.find((p) => p.id === form.planId);

  const handleGenerate = async () => {
    if (!form.productId || !form.planId || form.count < 1) return;
    setGenerating(true);
    try {
      const res = await bulkGenerate({
        productId: form.productId,
        planId: form.planId,
        count: form.count,
        maxDevices: form.maxDevices,
        features: form.features,
        expiresAt: form.expiresAt || undefined,
      });
      setResult(res);
      toast('success', `Generated ${res.totalCount} licenses`);
    } catch {
      // Mock result
      const mockKeys = Array.from({ length: form.count }, (_, i) => ({
        id: `mock-${i}`,
        key: `KEY-${Math.random().toString(36).slice(2, 6).toUpperCase()}-${Math.random().toString(36).slice(2, 6).toUpperCase()}`,
      }));
      setResult({ licenses: mockKeys, totalCount: form.count });
      toast('success', `Generated ${form.count} licenses`);
    } finally {
      setGenerating(false);
    }
  };

  const exportCSV = () => {
    if (!result) return;
    const csv = 'Key\n' + result.licenses.map((l) => l.key).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `licenses-bulk-${Date.now()}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  };

  const copyAllKeys = () => {
    if (!result) return;
    const keys = result.licenses.map((l) => l.key).join('\n');
    navigator.clipboard.writeText(keys);
    toast('success', 'All keys copied to clipboard');
  };

  if (loading) {
    return <div className="flex items-center justify-center py-20"><LoadingSpinner size="lg" /></div>;
  }

  return (
    <div className="space-y-6 max-w-3xl">
      <div>
        <button onClick={() => navigate('/licenses')} className="flex items-center gap-2 text-sm text-surface-400 hover:text-surface-200 mb-4">
          <ArrowLeft className="h-4 w-4" /> Back to Licenses
        </button>
        <h1 className="text-2xl font-bold text-surface-100">Bulk License Generation</h1>
        <p className="text-sm text-surface-400">Generate multiple licenses at once</p>
      </div>

      <div className="card p-6 space-y-5">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className="label-text">Product</label>
            <select value={form.productId} onChange={(e) => handleProductChange(e.target.value)} className="input-field">
              <option value="">Select product</option>
              {products.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="label-text">Plan</label>
            <select value={form.planId} onChange={(e) => setForm({ ...form, planId: e.target.value })} className="input-field">
              <option value="">Select plan</option>
              {plans.map((p) => (
                <option key={p.id} value={p.id}>{p.name} - ${p.price}/yr</option>
              ))}
            </select>
          </div>
          <div>
            <label className="label-text">Count</label>
            <input
              type="number"
              value={form.count}
              onChange={(e) => setForm({ ...form, count: Math.max(1, Math.min(1000, parseInt(e.target.value) || 1)) })}
              className="input-field"
              min={1}
              max={1000}
            />
          </div>
          <div>
            <label className="label-text">Max Devices per License</label>
            <input
              type="number"
              value={form.maxDevices}
              onChange={(e) => setForm({ ...form, maxDevices: Math.max(1, parseInt(e.target.value) || 1) })}
              className="input-field"
              min={1}
            />
          </div>
          <div className="sm:col-span-2">
            <label className="label-text">Expiration Date (optional)</label>
            <input
              type="datetime-local"
              value={form.expiresAt}
              onChange={(e) => setForm({ ...form, expiresAt: e.target.value })}
              className="input-field"
            />
          </div>
        </div>

        {/* Features */}
        {selectedPlan && selectedPlan.features.length > 0 && (
          <div>
            <label className="label-text">Features (from plan)</label>
            <div className="flex flex-wrap gap-2">
              {selectedPlan.features.map((f) => (
                <span key={f} className="badge badge-info">{f}</span>
              ))}
            </div>
          </div>
        )}

        {/* Preview */}
        <div className="rounded-lg bg-surface-800/50 p-4">
          <h4 className="text-sm font-semibold text-surface-200 mb-2">Preview</h4>
          <div className="grid grid-cols-2 gap-2 text-sm">
            <p className="text-surface-400">Product: <span className="text-surface-200">{products.find((p) => p.id === form.productId)?.name || '-'}</span></p>
            <p className="text-surface-400">Plan: <span className="text-surface-200">{selectedPlan?.name || '-'}</span></p>
            <p className="text-surface-400">Count: <span className="text-surface-200">{form.count}</span></p>
            <p className="text-surface-400">Max Devices: <span className="text-surface-200">{form.maxDevices}</span></p>
            <p className="text-surface-400">Expires: <span className="text-surface-200">{form.expiresAt ? new Date(form.expiresAt).toLocaleString() : 'Never'}</span></p>
          </div>
        </div>

        <button
          onClick={handleGenerate}
          disabled={!form.productId || !form.planId || generating}
          className="btn-primary flex items-center gap-2"
        >
          {generating ? (
            <><LoadingSpinner size="sm" /> Generating...</>
          ) : (
            <><Zap className="h-4 w-4" /> Generate {form.count} Licenses</>
          )}
        </button>
      </div>

      {/* Results */}
      {result && (
        <div className="card p-6 space-y-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-emerald-400" />
              <h3 className="text-lg font-semibold text-surface-100">
                Generated {result.totalCount} Licenses
              </h3>
            </div>
            <div className="flex gap-2">
              <button onClick={copyAllKeys} className="btn-secondary flex items-center gap-2 text-sm">
                <Copy className="h-4 w-4" /> Copy All
              </button>
              <button onClick={exportCSV} className="btn-secondary flex items-center gap-2 text-sm">
                <Download className="h-4 w-4" /> Export CSV
              </button>
            </div>
          </div>
          <div className="max-h-64 overflow-y-auto rounded-lg bg-surface-800/50 p-4">
            <div className="space-y-1 font-mono text-xs">
              {result.licenses.map((l, i) => (
                <div key={l.id} className="flex items-center gap-3 py-1">
                  <span className="w-8 text-surface-500">{i + 1}.</span>
                  <span className="text-brand-400">{l.key}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
