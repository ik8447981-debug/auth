import { useState, useEffect, useRef } from 'react';
import { ChevronDown, Package, Check } from 'lucide-react';
import type { Product } from '@/types';
import { getProducts } from '@/lib/api';

interface ProductSwitcherProps {
  selectedProductId: string | null;
  onSelect: (productId: string | null) => void;
}

export default function ProductSwitcher({ selectedProductId, onSelect }: ProductSwitcherProps) {
  const [products, setProducts] = useState<Product[]>([]);
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    getProducts().then(setProducts).catch(console.error);
  }, []);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const selected = selectedProductId
    ? products.find((p) => p.id === selectedProductId)
    : null;

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen(!open)}
        className="flex items-center gap-2 rounded-lg border border-surface-700 bg-surface-800 px-3 py-2 text-sm text-surface-200 hover:border-surface-600 transition-colors"
      >
        <Package className="h-4 w-4 text-surface-400" />
        <span className="max-w-[150px] truncate">
          {selected ? selected.name : 'All Products'}
        </span>
        <span className="text-xs text-surface-500">
          ({selected ? '1' : products.length})
        </span>
        <ChevronDown className={`h-4 w-4 text-surface-400 transition-transform ${open ? 'rotate-180' : ''}`} />
      </button>

      {open && (
        <div className="dropdown-menu left-0 mt-2 w-64 max-h-80 overflow-y-auto">
          <button
            onClick={() => { onSelect(null); setOpen(false); }}
            className="dropdown-item"
          >
            <Package className="h-4 w-4 text-surface-400" />
            <span className="flex-1">All Products</span>
            {!selectedProductId && <Check className="h-4 w-4 text-brand-400" />}
          </button>
          {products.map((product) => (
            <button
              key={product.id}
              onClick={() => { onSelect(product.id); setOpen(false); }}
              className="dropdown-item"
            >
              <Package className="h-4 w-4 text-surface-400" />
              <span className="flex-1 truncate">{product.name}</span>
              {selectedProductId === product.id && <Check className="h-4 w-4 text-brand-400" />}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
