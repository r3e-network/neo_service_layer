// @ts-ignore
import * as React from 'react';
import { useWebSocket } from '../../../hooks/useWebSocket';
import { useAuth } from '../../../hooks/useAuth';
import { TRIGGER_CONSTANTS } from '../constants';
import {
  Trigger,
  TriggerFilter,
  TriggerMetrics,
  TriggerExecution,
  TriggerUpdatePayload,
  ValidationError
} from '../types/types';
import { validateTrigger, validateTriggerUpdate } from '../utils/validation';

export function useTriggers(filter?: TriggerFilter) {
  const [triggers, setTriggers] = React.useState<Trigger[]>([]);
  const [executions, setExecutions] = React.useState<TriggerExecution[]>([]);
  const [metrics, setMetrics] = React.useState<TriggerMetrics | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  const { isAuthenticated, user, signMessage } = useAuth();
  const { socket } = useWebSocket({
    url: TRIGGER_CONSTANTS.WEBSOCKET_URL || 'ws://localhost:8080/api/triggers/ws'
  });

  // Fetch triggers with optional filtering
  const fetchTriggers = React.useCallback(async () => {
    if (!isAuthenticated) return;

    try {
      setLoading(true);
      const queryParams = new URLSearchParams();
      if (filter) {
        Object.entries(filter).forEach(([key, value]) => {
          if (Array.isArray(value)) {
            value.forEach(v => queryParams.append(key, v));
          } else if (value !== undefined) {
            queryParams.append(key, value.toString());
          }
        });
      }

      const response = await fetch(`/api/triggers?${queryParams}`);
      if (!response.ok) throw new Error('Failed to fetch triggers');

      const data = await response.json();
      setTriggers(data.triggers);
      setExecutions(data.executions);
      setMetrics(data.metrics);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, filter]);

  // Create new trigger
  const createTrigger = async (trigger: Partial<Trigger>): Promise<Trigger> => {
    if (!isAuthenticated) throw new Error('Authentication required');

    const errors = validateTrigger(trigger);
    if (errors.length > 0) throw new Error(errors[0].message);

    const timestamp = Date.now();
    const signature = await signMessage(`create-trigger:${timestamp}`);

    const response = await fetch('/api/triggers', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Signature': signature,
        'X-Timestamp': timestamp.toString()
      },
      body: JSON.stringify(trigger)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    const newTrigger = await response.json();
    setTriggers(prev => [...prev, newTrigger]);
    return newTrigger;
  };

  // Update existing trigger
  const updateTrigger = async (
    triggerId: string,
    update: TriggerUpdatePayload
  ): Promise<Trigger> => {
    if (!isAuthenticated) throw new Error('Authentication required');

    const errors = validateTriggerUpdate(update);
    if (errors.length > 0) throw new Error(errors[0].message);

    const timestamp = Date.now();
    const signature = await signMessage(`update-trigger:${triggerId}:${timestamp}`);

    const response = await fetch(`/api/triggers/${triggerId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'X-Signature': signature,
        'X-Timestamp': timestamp.toString()
      },
      body: JSON.stringify(update)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    const updatedTrigger = await response.json();
    setTriggers(prev => prev.map(t => t.id === triggerId ? updatedTrigger : t));
    return updatedTrigger;
  };

  // Delete trigger
  const deleteTrigger = async (triggerId: string): Promise<void> => {
    if (!isAuthenticated) throw new Error('Authentication required');

    const timestamp = Date.now();
    const signature = await signMessage(`delete-trigger:${triggerId}:${timestamp}`);

    const response = await fetch(`/api/triggers/${triggerId}`, {
      method: 'DELETE',
      headers: {
        'X-Signature': signature,
        'X-Timestamp': timestamp.toString()
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    setTriggers(prev => prev.filter(t => t.id !== triggerId));
  };

  // Pause/Resume trigger
  const toggleTriggerStatus = async (triggerId: string, active?: boolean): Promise<Trigger> => {
    if (!isAuthenticated) throw new Error('Authentication required');

    const trigger = triggers.find(t => t.id === triggerId);
    if (!trigger) throw new Error('Trigger not found');

    // Determine the new status based on the active parameter or toggle the current status
    const newStatus = active !== undefined 
      ? active ? TRIGGER_CONSTANTS.TRIGGER_STATUS.ACTIVE : TRIGGER_CONSTANTS.TRIGGER_STATUS.PAUSED 
      : trigger.status === TRIGGER_CONSTANTS.TRIGGER_STATUS.ACTIVE 
        ? TRIGGER_CONSTANTS.TRIGGER_STATUS.PAUSED 
        : TRIGGER_CONSTANTS.TRIGGER_STATUS.ACTIVE;

    const timestamp = Date.now();
    const signature = await signMessage(
      `update-trigger-status:${triggerId}:${newStatus}:${timestamp}`
    );

    const response = await fetch(`/api/triggers/${triggerId}/status`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'X-Signature': signature,
        'X-Timestamp': timestamp.toString()
      },
      body: JSON.stringify({ status: newStatus })
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    const updatedTrigger = await response.json();
    setTriggers(prev => prev.map(t => t.id === triggerId ? updatedTrigger : t));
    return updatedTrigger;
  };

  // Get trigger executions
  const getTriggerExecutions = async (triggerId: string): Promise<TriggerExecution[]> => {
    if (!isAuthenticated) throw new Error('Authentication required');

    const response = await fetch(`/api/triggers/${triggerId}/executions`);
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }

    const executions = await response.json();
    return executions;
  };

  // WebSocket event handlers
  React.useEffect(() => {
    if (!socket || !isAuthenticated) return;

    const handleTriggerUpdate = (data: any) => {
      setTriggers(prev => prev.map(t => t.id === data.trigger.id ? data.trigger : t));
    };

    const handleExecutionUpdate = (data: any) => {
      setExecutions(prev => {
        const index = prev.findIndex(e => e.id === data.execution.id);
        if (index === -1) {
          return [...prev, data.execution];
        }
        return prev.map(e => e.id === data.execution.id ? data.execution : e);
      });
    };

    socket.on(TRIGGER_CONSTANTS.WEBSOCKET_EVENTS.TRIGGER_UPDATE, handleTriggerUpdate);
    socket.on(TRIGGER_CONSTANTS.WEBSOCKET_EVENTS.EXECUTION_UPDATE, handleExecutionUpdate);

    return () => {
      socket.off(TRIGGER_CONSTANTS.WEBSOCKET_EVENTS.TRIGGER_UPDATE, handleTriggerUpdate);
      socket.off(TRIGGER_CONSTANTS.WEBSOCKET_EVENTS.EXECUTION_UPDATE, handleExecutionUpdate);
    };
  }, [socket, isAuthenticated]);

  // Initial fetch
  React.useEffect(() => {
    if (isAuthenticated) {
      fetchTriggers();
    }
  }, [isAuthenticated, fetchTriggers]);

  return {
    triggers,
    executions,
    metrics,
    loading,
    error,
    createTrigger,
    updateTrigger,
    deleteTrigger,
    toggleTriggerStatus,
    getTriggerExecutions,
    refresh: fetchTriggers
  };
}