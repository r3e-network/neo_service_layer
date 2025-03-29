// @ts-ignore
import * as React from 'react';

type ToastType = 'success' | 'error' | 'info' | 'warning';

interface Toast {
  message: string;
  type: ToastType;
  id: number;
}

export const useToast = () => {
  const [toasts, setToasts] = React.useState<Toast[]>([]);
  const toastTimeouts = React.useRef<Map<number, NodeJS.Timeout>>(new Map());

  const removeToast = React.useCallback((id: number) => {
    setToasts(prev => prev.filter(toast => toast.id !== id));
    const timeout = toastTimeouts.current.get(id);
    if (timeout) {
      clearTimeout(timeout);
      toastTimeouts.current.delete(id);
    }
  }, []);

  const showToast = React.useCallback((message: string, type: ToastType = 'info') => {
    const id = Math.floor(Math.random() * 1000000);
    setToasts(prev => [...prev, { message, type, id }]);

    const timeout = setTimeout(() => {
      removeToast(id);
    }, 3000);

    toastTimeouts.current.set(id, timeout);
  }, [removeToast]);

  React.useEffect(() => {
    return () => {
      toastTimeouts.current.forEach(timeout => clearTimeout(timeout));
      toastTimeouts.current.clear();
    };
  }, []);

  return {
    toasts,
    showToast,
  };
};