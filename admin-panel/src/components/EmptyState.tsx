interface EmptyStateProps {
  icon?: React.ReactNode;
  title: string;
  description: string;
  action?: {
    label: string;
    onClick: () => void;
  };
}

export default function EmptyState({ icon, title, description, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16">
      {icon && (
        <div className="flex h-16 w-16 items-center justify-center rounded-full bg-surface-800 text-surface-500">
          {icon}
        </div>
      )}
      <h3 className="mt-4 text-lg font-medium text-surface-200">{title}</h3>
      <p className="mt-1 text-sm text-surface-400 text-center max-w-md">{description}</p>
      {action && (
        <button onClick={action.onClick} className="btn-primary mt-4">
          {action.label}
        </button>
      )}
    </div>
  );
}
