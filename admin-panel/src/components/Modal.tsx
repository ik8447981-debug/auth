import { useEffect, useRef } from 'react';
import { X } from 'lucide-react';
import clsx from 'clsx';

interface ModalProps {
  open: boolean;
  onClose: () => void;
  title?: string;
  children: React.ReactNode;
  size?: 'sm' | 'md' | 'lg' | 'xl';
  showCloseButton?: boolean;
}

export default function Modal({
  open,
  onClose,
  title,
  children,
  size = 'md',
  showCloseButton = true,
}: ModalProps) {
  const overlayRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;

    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };

    document.addEventListener('keydown', handleEscape);
    document.body.style.overflow = 'hidden';

    return () => {
      document.removeEventListener('keydown', handleEscape);
      document.body.style.overflow = '';
    };
  }, [open, onClose]);

  if (!open) return null;

  const sizeClasses = {
    sm: 'max-w-md',
    md: 'max-w-lg',
    lg: 'max-w-2xl',
    xl: 'max-w-4xl',
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div
        ref={overlayRef}
        className="fixed inset-0 bg-black/60 backdrop-blur-sm animate-fade-in"
        onClick={onClose}
      />

      {/* Modal */}
      <div
        className={clsx(
          'relative z-10 w-full mx-4 animate-slide-up',
          sizeClasses[size]
        )}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="rounded-xl bg-surface-900 border border-surface-700 shadow-2xl">
          {/* Header */}
          {(title || showCloseButton) && (
            <div className="flex items-center justify-between border-b border-surface-700 px-6 py-4">
              {title && (
                <h2 className="text-lg font-semibold text-surface-100">{title}</h2>
              )}
              {showCloseButton && (
                <button
                  onClick={onClose}
                  className="rounded-lg p-1.5 text-surface-400 hover:bg-surface-800 hover:text-surface-200 transition-colors"
                >
                  <X className="h-5 w-5" />
                </button>
              )}
            </div>
          )}

          {/* Content */}
          <div className="px-6 py-4 max-h-[70vh] overflow-y-auto">{children}</div>
        </div>
      </div>
    </div>
  );
}
