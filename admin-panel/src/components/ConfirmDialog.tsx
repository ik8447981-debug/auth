import { AlertTriangle } from 'lucide-react';
import Modal from './Modal';

interface ConfirmDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: 'danger' | 'warning';
  loading?: boolean;
}

export default function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  message,
  confirmLabel = 'Confirm',
  cancelLabel = 'Cancel',
  variant = 'danger',
  loading = false,
}: ConfirmDialogProps) {
  return (
    <Modal open={open} onClose={onClose} size="sm" showCloseButton={false}>
      <div className="flex flex-col items-center text-center py-4">
        <div
          className={`flex h-12 w-12 items-center justify-center rounded-full ${
            variant === 'danger' ? 'bg-red-500/10 text-red-400' : 'bg-yellow-500/10 text-yellow-400'
          }`}
        >
          <AlertTriangle className="h-6 w-6" />
        </div>
        <h3 className="mt-4 text-lg font-semibold text-surface-100">{title}</h3>
        <p className="mt-2 text-sm text-surface-400">{message}</p>
        <div className="mt-6 flex items-center gap-3">
          <button onClick={onClose} className="btn-secondary" disabled={loading}>
            {cancelLabel}
          </button>
          <button
            onClick={onConfirm}
            className={variant === 'danger' ? 'btn-danger' : 'btn-primary'}
            disabled={loading}
          >
            {loading ? 'Processing...' : confirmLabel}
          </button>
        </div>
      </div>
    </Modal>
  );
}
