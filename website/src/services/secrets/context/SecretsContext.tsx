import React from 'react';
import { Secret, SecretPermission } from '../types/types';
import { useSecrets } from '../hooks/useSecrets';

interface SecretsContextType {
  secrets: Secret[];
  isLoading: boolean;
  error: Error | null;
  selectedSecret: Secret | null;
  setSelectedSecret: (secret: Secret | null) => void;
  refreshSecrets: () => Promise<void>;
  createSecret: (name: string, value: string, permissions: SecretPermission[]) => Promise<void>;
  updateSecret: (id: string, updates: Partial<Secret>) => Promise<void>;
  deleteSecret: (id: string) => Promise<void>;
  updatePermission: (secretId: string, userId: string, permission: SecretPermission) => Promise<void>;
  rotateSecret: (id: string) => Promise<void>;
}

const SecretsContext = React.createContext<SecretsContextType | null>(null);

export function useSecretsContext() {
  const context = React.useContext(SecretsContext);
  if (!context) {
    throw new Error('useSecretsContext must be used within a SecretsProvider');
  }
  return context;
}

export function SecretsProvider({ children }: { children: React.ReactNode }) {
  const [selectedSecret, setSelectedSecret] = React.useState<Secret | null>(null);
  const {
    secrets,
    loading: isLoading,
    error,
    refresh: refreshSecrets,
    createSecret,
    updateSecret,
    deleteSecret,
    updatePermission,
    rotateSecret
  } = useSecrets();

  const value = {
    secrets,
    isLoading,
    error,
    selectedSecret,
    setSelectedSecret,
    refreshSecrets,
    createSecret,
    updateSecret,
    deleteSecret,
    updatePermission,
    rotateSecret
  };

  return (
    <SecretsContext.Provider value={value}>
      {children}
    </SecretsContext.Provider>
  );
}