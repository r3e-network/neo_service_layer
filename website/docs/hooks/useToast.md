# useToast Hook

## Overview
The `useToast` hook provides a toast notification system for React components. It handles showing notifications, managing their lifecycle, and automatic dismissal.

## Features
- Multiple toast types
- Auto-dismiss
- Queue management
- Custom styling
- TypeScript support

## API Reference

### Hook Signature
```typescript
function useToast(): {
  toasts: Toast[];
  showToast: (message: string, type?: ToastType) => void;
}
```

### Types
```typescript
type ToastType = 'success' | 'error' | 'info' | 'warning';

interface Toast {
  message: string;
  type: ToastType;
  id: number;
}
```

## Usage Examples

### Basic Usage
```typescript
import { useToast } from '../src/app/hooks/useToast';

function ToastExample() {
  const { showToast } = useToast();

  const handleClick = () => {
    showToast('Operation successful!', 'success');
  };

  return (
    <button onClick={handleClick}>
      Show Toast
    </button>
  );
}
```

### Different Toast Types
```typescript
function ToastTypes() {
  const { showToast } = useToast();

  return (
    <div>
      <button onClick={() => showToast('Success!', 'success')}>
        Success
      </button>
      <button onClick={() => showToast('Error!', 'error')}>
        Error
      </button>
      <button onClick={() => showToast('Info', 'info')}>
        Info
      </button>
      <button onClick={() => showToast('Warning!', 'warning')}>
        Warning
      </button>
    </div>
  );
}
```

### Toast Container
```typescript
function ToastContainer() {
  const { toasts } = useToast();

  return (
    <div className="toast-container">
      {toasts.map(toast => (
        <div
          key={toast.id}
          className={`toast toast-${toast.type}`}
        >
          {toast.message}
        </div>
      ))}
    </div>
  );
}
```

## Styling

### CSS Example
```css
.toast-container {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 1000;
}

.toast {
  margin-bottom: 10px;
  padding: 15px;
  border-radius: 4px;
  color: white;
  animation: slideIn 0.3s ease-in;
}

.toast-success {
  background-color: #4caf50;
}

.toast-error {
  background-color: #f44336;
}

.toast-info {
  background-color: #2196f3;
}

.toast-warning {
  background-color: #ff9800;
}

@keyframes slideIn {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}
```

## State Management

### Initial State
```typescript
const [toasts, setToasts] = useState<Toast[]>([]);
```

### Adding Toast
```typescript
const showToast = (message: string, type: ToastType = 'info') => {
  const id = Date.now();
  setToasts(prev => [...prev, { message, type, id }]);

  // Auto-remove after 3 seconds
  setTimeout(() => {
    setToasts(prev => prev.filter(toast => toast.id !== id));
  }, 3000);
};
```

## Error Handling

### Example
```typescript
function SafeToast() {
  const { showToast } = useToast();

  const handleOperation = async () => {
    try {
      await someOperation();
      showToast('Operation successful!', 'success');
    } catch (error) {
      showToast(error.message, 'error');
    }
  };

  return <button onClick={handleOperation}>Try Operation</button>;
}
```

## Testing

### Unit Tests
```typescript
describe('useToast', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.runOnlyPendingTimers();
    jest.useRealTimers();
  });

  it('should show toast', () => {
    const { result } = renderHook(() => useToast());
    
    act(() => {
      result.current.showToast('Test message');
    });

    expect(result.current.toasts).toHaveLength(1);
    expect(result.current.toasts[0]).toEqual({
      message: 'Test message',
      type: 'info',
      id: expect.any(Number),
    });
  });

  it('should auto-dismiss toast', () => {
    const { result } = renderHook(() => useToast());
    
    act(() => {
      result.current.showToast('Test message');
    });

    expect(result.current.toasts).toHaveLength(1);

    act(() => {
      jest.advanceTimersByTime(3000);
    });

    expect(result.current.toasts).toHaveLength(0);
  });
});
```

## Performance

### Optimization
1. Limit maximum toasts
2. Clean up timeouts
3. Memoize callbacks
4. Batch updates

### Example
```typescript
const MAX_TOASTS = 5;

const showToast = useCallback((message: string, type: ToastType = 'info') => {
  const id = Date.now();
  
  setToasts(prev => {
    const filtered = prev.slice(-MAX_TOASTS + 1);
    return [...filtered, { message, type, id }];
  });

  const timer = setTimeout(() => {
    setToasts(prev => prev.filter(toast => toast.id !== id));
  }, 3000);

  return () => clearTimeout(timer);
}, []);
```

## Integration

### With Components
```typescript
function App() {
  return (
    <div>
      <ToastContainer />
      <MainContent />
    </div>
  );
}
```

### With API Calls
```typescript
async function submitForm() {
  const { showToast } = useToast();
  
  try {
    await api.post('/data');
    showToast('Form submitted successfully!', 'success');
  } catch (error) {
    showToast('Failed to submit form', 'error');
  }
}
```

## Accessibility

### ARIA Support
```typescript
function AccessibleToast({ message, type }: Toast) {
  return (
    <div
      role="alert"
      aria-live="polite"
      className={`toast toast-${type}`}
    >
      {message}
    </div>
  );
}
```

## Configuration

### Default Options
```typescript
const DEFAULT_CONFIG = {
  duration: 3000,
  position: 'top-right',
  maxToasts: 5,
  animation: true,
};
```

## Troubleshooting

### Common Issues
1. Toasts not showing
2. Toasts not dismissing
3. Animation issues
4. Z-index conflicts

### Solutions
1. Check mount point
2. Verify timeouts
3. Check CSS conflicts
4. Adjust z-index