// @ts-ignore
import * as React from 'react';
import {
  Box,
  Paper,
  Typography,
  Grid,
  CircularProgress,
  Alert,
  Tabs,
  Tab
} from '@mui/material';
import { useApi } from '../hooks/useApi';
import { ApiKeysList } from './ApiKeysList';
import { ApiEndpointsList } from './ApiEndpointsList';
import { ApiUsageMetrics } from './ApiUsageMetrics';
import { ApiErrorLogs } from './ApiErrorLogs';
import { ApiKeyDialog } from './ApiKeyDialog';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`api-tabpanel-${index}`}
      aria-labelledby={`api-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

export default function ApiDashboard() {
  const [selectedTab, setSelectedTab] = React.useState(0);
  const [showNewKeyDialog, setShowNewKeyDialog] = React.useState(false);
  const {
    endpoints,
    apiKeys,
    usage,
    metrics,
    errors,
    loading,
    error,
    createApiKey,
    updateApiKey,
    deleteApiKey,
    fetchUsage,
    fetchMetrics,
    fetchErrors
  } = useApi();

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setSelectedTab(newValue);
  };

  const handleCreateApiKey = async (
    name: string,
    options: {
      allowedEndpoints?: string[];
      allowedIPs?: string[];
      expiresAt?: number;
      rateLimit?: {
        requestsPerMinute: number;
        burstLimit: number;
      };
    }
  ) => {
    try {
      await createApiKey(name, options);
      setShowNewKeyDialog(false);
    } catch (err) {
      console.error('Failed to create API key:', err);
    }
  };

  const handleUpdateKey = async (
    id: string,
    updates: {
      name?: string;
      allowedEndpoints?: string[];
      allowedIPs?: string[];
      expiresAt?: number;
      rateLimit?: {
        requestsPerMinute: number;
        burstLimit: number;
      };
    }
  ): Promise<void> => {
    await updateApiKey(id, updates);
    // The updateApiKey function already updates the API keys list internally
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ height: '100%' }}>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error.message}
        </Alert>
      )}

      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1">
          API Management
        </Typography>
      </Box>

      <Paper>
        <Tabs
          value={selectedTab}
          onChange={handleTabChange}
          aria-label="api management tabs"
        >
          <Tab label="API Keys" id="api-tab-0" />
          <Tab label="Endpoints" id="api-tab-1" />
          <Tab label="Usage & Metrics" id="api-tab-2" />
          <Tab label="Error Logs" id="api-tab-3" />
        </Tabs>

        <TabPanel value={selectedTab} index={0}>
          <ApiKeysList
            apiKeys={apiKeys}
            onCreateKey={() => setShowNewKeyDialog(true)}
            onUpdateKey={handleUpdateKey}
            onDeleteKey={deleteApiKey}
          />
        </TabPanel>

        <TabPanel value={selectedTab} index={1}>
          <ApiEndpointsList endpoints={endpoints} />
        </TabPanel>

        <TabPanel value={selectedTab} index={2}>
          <ApiUsageMetrics
            usage={usage}
            metrics={metrics}
            onFetchUsage={fetchUsage}
            onFetchMetrics={fetchMetrics}
          />
        </TabPanel>

        <TabPanel value={selectedTab} index={3}>
          <ApiErrorLogs
            errors={errors}
            onFetchErrors={fetchErrors}
          />
        </TabPanel>
      </Paper>

      <ApiKeyDialog
        open={showNewKeyDialog}
        onClose={() => setShowNewKeyDialog(false)}
        onSubmit={handleCreateApiKey}
        endpoints={endpoints}
      />
    </Box>
  );
}