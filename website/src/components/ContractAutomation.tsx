'use client';

// @ts-ignore
import * as React from 'react';
import { PlusIcon, PauseIcon, PlayIcon, TrashIcon } from '@heroicons/react/24/outline';
import { useWallet } from '../app/hooks/useWallet';

interface AutomationTask {
  id: string;
  name: string;
  contract: string;
  method: string;
  schedule: string;
  lastRun: string | null;
  nextRun: string | null;
  status: 'active' | 'paused' | 'failed';
  stats: {
    totalRuns: number;
    successRate: number;
    avgGasUsed: number;
    avgExecutionTime: number;
  };
  conditions: {
    type: 'time' | 'event' | 'price';
    value: string;
  }[];
}

export function ContractAutomation() {
  const { isConnected, connect, signMessage } = useWallet();
  const [tasks, setTasks] = React.useState<AutomationTask[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [showNewTaskModal, setShowNewTaskModal] = React.useState(false);
  const [newTask, setNewTask] = React.useState({
    name: '',
    contract: '',
    method: '',
    schedule: '',
    conditions: [],
  });

  React.useEffect(() => {
    fetchTasks();
  }, []);

  const fetchTasks = async () => {
    try {
      const signature = await signMessage('fetch-tasks');
      const response = await fetch('/.netlify/functions/contract-automation-status', {
        headers: {
          'x-signature': signature,
        },
      });
      const data = await response.json();
      setTasks(data);
    } catch (error) {
      console.error('Failed to fetch tasks:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateTask = async () => {
    try {
      const signature = await signMessage(JSON.stringify(newTask));
      const response = await fetch('/.netlify/functions/contract-automation-status', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'x-signature': signature,
        },
        body: JSON.stringify(newTask),
      });
      const data = await response.json();
      setTasks([...tasks, data]);
      setShowNewTaskModal(false);
      setNewTask({
        name: '',
        contract: '',
        method: '',
        schedule: '',
        conditions: [],
      });
    } catch (error) {
      console.error('Failed to create task:', error);
    }
  };

  const handleToggleTask = async (taskId: string, currentStatus: string) => {
    try {
      const signature = await signMessage(`toggle-task-${taskId}`);
      const response = await fetch(`/.netlify/functions/contract-automation-status/${taskId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'X-Signature': signature,
        },
        body: JSON.stringify({
          status: currentStatus === 'active' ? 'paused' : 'active',
        }),
      });

      if (!response.ok) {
        throw new Error(`Failed to toggle task status: ${response.statusText}`);
      }

      // Update task status in UI
      setTasks(tasks.map(task => 
        task.id === taskId
          ? { ...task, status: task.status === 'active' ? 'paused' : 'active' }
          : task
      ));
    } catch (error) {
      console.error('Error toggling task status:', error);
      // Show error notification
    }
  };

  const handleDeleteTask = async (taskId: string) => {
    try {
      const signature = await signMessage(`delete-task-${taskId}`);
      await fetch(`/.netlify/functions/contract-automation-status/${taskId}`, {
        method: 'DELETE',
        headers: {
          'x-signature': signature,
        },
      });
      setTasks(tasks.filter(task => task.id !== taskId));
    } catch (error) {
      console.error('Failed to delete task:', error);
    }
  };

  if (!isConnected) {
    return (
      <div className="text-center py-12">
        <h3 className="text-lg font-medium text-gray-900">Connect Wallet</h3>
        <p className="mt-2 text-sm text-gray-500">
          Please connect your wallet to manage automation tasks.
        </p>
        <button
          onClick={connect}
          className="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          Connect Wallet
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-gray-900">Contract Automation</h2>
        <button
          onClick={() => setShowNewTaskModal(true)}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <PlusIcon className="h-5 w-5 mr-2" />
          New Task
        </button>
      </div>

      {loading ? (
        <div className="space-y-4">
          {[...Array(3)].map((_, i) => (
            <div
              key={i}
              className="animate-pulse bg-gray-100 rounded-lg h-24"
            />
          ))}
        </div>
      ) : (
        <div className="space-y-4">
          <div>
            {tasks.map((task) => (
              <div
                key={task.id}
                className="bg-white rounded-lg shadow p-6"
              >
                <div className="flex items-center justify-between">
                  <div>
                    <h3 className="text-lg font-medium text-gray-900">
                      {task.name}
                    </h3>
                    <p className="mt-1 text-sm text-gray-500">
                      Contract: {task.contract}
                    </p>
                  </div>
                  <div className="flex items-center space-x-4">
                    <button
                      onClick={() => handleToggleTask(task.id, task.status)}
                      className={`p-2 rounded-md ${
                        task.status === 'active'
                          ? 'text-green-600 hover:bg-green-50'
                          : 'text-gray-400 hover:bg-gray-50'
                      }`}
                    >
                      {task.status === 'active' ? (
                        <PauseIcon className="h-5 w-5" />
                      ) : (
                        <PlayIcon className="h-5 w-5" />
                      )}
                    </button>
                    <button
                      onClick={() => handleDeleteTask(task.id)}
                      className="p-2 rounded-md text-red-600 hover:bg-red-50"
                    >
                      <TrashIcon className="h-5 w-5" />
                    </button>
                  </div>
                </div>
                <div className="mt-4 grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <p className="text-gray-500">Last Run</p>
                    <p className="font-medium">
                      {task.lastRun
                        ? new Date(task.lastRun).toLocaleString()
                        : 'Never'}
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-500">Next Run</p>
                    <p className="font-medium">
                      {task.nextRun
                        ? new Date(task.nextRun).toLocaleString()
                        : 'Not scheduled'}
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-500">Success Rate</p>
                    <p className="font-medium">{task.stats.successRate}%</p>
                  </div>
                  <div>
                    <p className="text-gray-500">Average Gas Used</p>
                    <p className="font-medium">{task.stats.avgGasUsed}</p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* New Task Modal */}
      {showNewTaskModal && (
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center">
          <div className="bg-white rounded-lg p-6 max-w-lg w-full">
            <h3 className="text-lg font-medium text-gray-900">Create New Task</h3>
            <div className="mt-4 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Task Name
                </label>
                <input
                  type="text"
                  value={newTask.name}
                  onChange={(e) => setNewTask({ ...newTask, name: e.target.value })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Contract Address
                </label>
                <input
                  type="text"
                  value={newTask.contract}
                  onChange={(e) => setNewTask({ ...newTask, contract: e.target.value })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Method Name
                </label>
                <input
                  type="text"
                  value={newTask.method}
                  onChange={(e) => setNewTask({ ...newTask, method: e.target.value })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">
                  Schedule (Cron Expression)
                </label>
                <input
                  type="text"
                  value={newTask.schedule}
                  onChange={(e) => setNewTask({ ...newTask, schedule: e.target.value })}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div className="flex justify-end space-x-4">
                <button
                  onClick={() => setShowNewTaskModal(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 rounded-md"
                >
                  Cancel
                </button>
                <button
                  onClick={handleCreateTask}
                  className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md"
                >
                  Create Task
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}