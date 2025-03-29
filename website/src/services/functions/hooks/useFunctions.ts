// @ts-ignore
import * as React from 'react';
import {
  UserFunction,
  FunctionExecution,
  FunctionTemplate,
  FunctionTrigger,
  FunctionConfig,
  FunctionPermissions
} from '../types';
import { functionsApi } from '../api/functionsApi';
import { REFRESH_INTERVAL } from '../constants';

export function useFunctions(functionId?: string) {
  const [functions, setFunctions] = React.useState<UserFunction[]>([]);
  const [selectedFunction, setSelectedFunction] = React.useState<UserFunction | null>(null);
  const [executions, setExecutions] = React.useState<FunctionExecution[]>([]);
  const [totalExecutions, setTotalExecutions] = React.useState(0);
  const [templates, setTemplates] = React.useState<FunctionTemplate[]>([]);
  const [logs, setLogs] = React.useState<string[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<Error | null>(null);

  const fetchFunctions = React.useCallback(async () => {
    try {
      const functions = await functionsApi.getFunctions();
      setFunctions(functions);
      if (functionId) {
        const func = functions.find(f => f.id === functionId);
        setSelectedFunction(func || null);
      }
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch functions'));
    }
  }, [functionId]);

  const fetchExecutions = React.useCallback(async (
    funcId: string,
    options?: {
      limit?: number;
      offset?: number;
      status?: string;
    }
  ) => {
    try {
      const { executions, total } = await functionsApi.getFunctionExecutions(
        funcId,
        options
      );
      setExecutions(executions);
      setTotalExecutions(total);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch executions'));
    }
  }, []);

  const fetchTemplates = React.useCallback(async (category?: string) => {
    try {
      const templates = await functionsApi.getTemplates(category);
      setTemplates(templates);
      setError(null);
    } catch (err) {
      console.error('Failed to fetch templates:', err);
    }
  }, []);

  const fetchLogs = React.useCallback(async (
    funcId: string,
    options?: {
      limit?: number;
      offset?: number;
      startTime?: number;
      endTime?: number;
    }
  ) => {
    try {
      const { logs } = await functionsApi.getLogs(funcId, options);
      setLogs(logs);
      setError(null);
    } catch (err) {
      console.error('Failed to fetch logs:', err);
    }
  }, []);

  React.useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      await fetchFunctions();
      if (functionId) {
        await Promise.all([
          fetchExecutions(functionId),
          fetchLogs(functionId)
        ]);
      }
      await fetchTemplates();
      setLoading(false);
    };

    fetchData();
    const interval = setInterval(fetchData, REFRESH_INTERVAL);
    return () => clearInterval(interval);
  }, [
    functionId,
    fetchFunctions,
    fetchExecutions,
    fetchLogs,
    fetchTemplates
  ]);

  const createFunction = React.useCallback(async (
    name: string,
    code: string,
    language: string,
    config: Partial<FunctionConfig>
  ) => {
    try {
      const func = await functionsApi.createFunction(name, code, language, config);
      setFunctions(prev => [...prev, func]);
      return func;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to create function');
    }
  }, []);

  const updateFunction = React.useCallback(async (
    id: string,
    updates: Partial<UserFunction>
  ) => {
    try {
      const updated = await functionsApi.updateFunction(id, updates);
      setFunctions(prev =>
        prev.map(f => (f.id === id ? updated : f))
      );
      if (id === selectedFunction?.id) {
        setSelectedFunction(updated);
      }
      return updated;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to update function');
    }
  }, [selectedFunction]);

  const deleteFunction = React.useCallback(async (id: string) => {
    try {
      await functionsApi.deleteFunction(id);
      setFunctions(prev => prev.filter(f => f.id !== id));
      if (id === selectedFunction?.id) {
        setSelectedFunction(null);
      }
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to delete function');
    }
  }, [selectedFunction]);

  const deployFunction = React.useCallback(async (id: string) => {
    try {
      const deployed = await functionsApi.deployFunction(id);
      setFunctions(prev =>
        prev.map(f => (f.id === id ? deployed : f))
      );
      if (id === selectedFunction?.id) {
        setSelectedFunction(deployed);
      }
      return deployed;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to deploy function');
    }
  }, [selectedFunction]);

  const executeFunction = React.useCallback(async (
    id: string,
    params?: Record<string, any>
  ) => {
    try {
      const execution = await functionsApi.executeFunction(id, params);
      setExecutions(prev => [execution, ...prev]);
      return execution;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to execute function');
    }
  }, []);

  const createTrigger = React.useCallback(async (
    functionId: string,
    trigger: Omit<FunctionTrigger, 'id'>
  ) => {
    try {
      const newTrigger = await functionsApi.createTrigger(functionId, trigger);
      setFunctions(prev =>
        prev.map(f => {
          if (f.id === functionId) {
            return {
              ...f,
              triggers: [...f.triggers, newTrigger]
            };
          }
          return f;
        })
      );
      return newTrigger;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to create trigger');
    }
  }, []);

  const updateTrigger = React.useCallback(async (
    functionId: string,
    triggerId: string,
    updates: Partial<FunctionTrigger>
  ) => {
    try {
      const updated = await functionsApi.updateTrigger(functionId, triggerId, updates);
      setFunctions(prev =>
        prev.map(f => {
          if (f.id === functionId) {
            return {
              ...f,
              triggers: f.triggers.map(t =>
                t.id === triggerId ? updated : t
              )
            };
          }
          return f;
        })
      );
      return updated;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to update trigger');
    }
  }, []);

  const deleteTrigger = React.useCallback(async (
    functionId: string,
    triggerId: string
  ) => {
    try {
      await functionsApi.deleteTrigger(functionId, triggerId);
      setFunctions(prev =>
        prev.map(f => {
          if (f.id === functionId) {
            return {
              ...f,
              triggers: f.triggers.filter(t => t.id !== triggerId)
            };
          }
          return f;
        })
      );
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to delete trigger');
    }
  }, []);

  const updatePermissions = React.useCallback(async (
    id: string,
    permissions: Partial<FunctionPermissions>
  ) => {
    try {
      const updated = await functionsApi.updatePermissions(id, permissions);
      setFunctions(prev =>
        prev.map(f => {
          if (f.id === id) {
            return {
              ...f,
              permissions: updated
            };
          }
          return f;
        })
      );
      return updated;
    } catch (err) {
      throw err instanceof Error ? err : new Error('Failed to update permissions');
    }
  }, []);

  return {
    functions,
    selectedFunction,
    executions,
    totalExecutions,
    templates,
    logs,
    loading,
    error,
    createFunction,
    updateFunction,
    deleteFunction,
    deployFunction,
    executeFunction,
    createTrigger,
    updateTrigger,
    deleteTrigger,
    updatePermissions,
    fetchExecutions,
    fetchLogs
  };
}