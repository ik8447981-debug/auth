import { useState, useEffect, useRef } from 'react';
import { Search, X } from 'lucide-react';

interface SearchBarProps {
  placeholder?: string;
  value?: string;
  onChange: (value: string) => void;
  debounceMs?: number;
  className?: string;
}

export default function SearchBar({
  placeholder = 'Search...',
  value: externalValue,
  onChange,
  debounceMs = 300,
  className,
}: SearchBarProps) {
  const [internalValue, setInternalValue] = useState(externalValue || '');
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (externalValue !== undefined) {
      setInternalValue(externalValue);
    }
  }, [externalValue]);

  const handleChange = (val: string) => {
    setInternalValue(val);
    if (timerRef.current) clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => {
      onChange(val);
    }, debounceMs);
  };

  const handleClear = () => {
    setInternalValue('');
    onChange('');
  };

  return (
    <div className={className}>
      <div className="relative">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-surface-400" />
        <input
          type="text"
          value={internalValue}
          onChange={(e) => handleChange(e.target.value)}
          placeholder={placeholder}
          className="input-field pl-10 pr-10"
        />
        {internalValue && (
          <button
            onClick={handleClear}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-surface-400 hover:text-surface-200"
          >
            <X className="h-4 w-4" />
          </button>
        )}
      </div>
    </div>
  );
}
