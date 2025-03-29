import { renderHook, act } from '@testing-library/react';
import { useToast } from '../../src/app/hooks/useToast';

describe('useToast Hook', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.runOnlyPendingTimers();
    jest.useRealTimers();
  });

  it('should initialize with empty toasts array', () => {
    const { result } = renderHook(() => useToast());
    expect(result.current.toasts).toEqual([]);
  });

  it('should add a toast with default type', () => {
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

  it('should add a toast with specified type', () => {
    const { result } = renderHook(() => useToast());
    
    act(() => {
      result.current.showToast('Test message', 'success');
    });

    expect(result.current.toasts).toHaveLength(1);
    expect(result.current.toasts[0]).toEqual({
      message: 'Test message',
      type: 'success',
      id: expect.any(Number),
    });
  });

  it('should remove toast after timeout', () => {
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

  it('should handle multiple toasts', () => {
    const { result } = renderHook(() => useToast());
    
    act(() => {
      result.current.showToast('First message');
      result.current.showToast('Second message', 'error');
    });

    expect(result.current.toasts).toHaveLength(2);
    expect(result.current.toasts[0].message).toBe('First message');
    expect(result.current.toasts[1].message).toBe('Second message');

    act(() => {
      jest.advanceTimersByTime(3000);
    });

    expect(result.current.toasts).toHaveLength(0);
  });

  it('should maintain unique IDs for toasts', () => {
    const { result } = renderHook(() => useToast());
    
    act(() => {
      result.current.showToast('First message');
      result.current.showToast('Second message');
    });

    const [firstToast, secondToast] = result.current.toasts;
    expect(firstToast.id).not.toBe(secondToast.id);
  });
});