import React from 'react';
import {
  Box,
  Grid,
  Typography,
  Tabs,
  Tab,
  Button,
  Menu,
  MenuItem
} from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import WarningIcon from '@mui/icons-material/Warning';
import ErrorIcon from '@mui/icons-material/Error';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import { useMetrics } from '../hooks/useMetrics';
import { ServiceMetricsCard } from './ServiceMetricsCard';
import { SystemMetricsCard } from './SystemMetricsCard';
import { AlertsList } from './AlertsList';
import { ServiceMetrics } from '../types/types';

// Time range options for metrics
const timeRangeOptions = [
  { value: '1h', label: 'Last Hour' },
  { value: '6h', label: 'Last 6 Hours' },
  { value: '24h', label: 'Last 24 Hours' },
  { value: '7d', label: 'Last 7 Days' },
  { value: '30d', label: 'Last 30 Days' }
];

// Tab options
const tabs = [
  { value: 'overview', label: 'Overview' },
  { value: 'services', label: 'Services' },
  { value: 'alerts', label: 'Alerts' }
];

export default function MetricsDashboard() {
  const [timeRange, setTimeRange] = React.useState('24h');
  const {
    loading,
    error,
    systemMetrics,
    serviceMetrics,
    serviceHealth,
    alerts
  } = useMetrics();

  const [activeTab, setActiveTab] = React.useState('overview');
  const [timeRangeMenuAnchor, setTimeRangeMenuAnchor] = React.useState<null | HTMLElement>(null);

  const handleTabChange = (event: React.SyntheticEvent, newValue: string) => {
    setActiveTab(newValue);
  };

  const handleTimeRangeClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setTimeRangeMenuAnchor(event.currentTarget);
  };

  const handleTimeRangeSelect = (value: string) => {
    setTimeRange(value);
    setTimeRangeMenuAnchor(null);
  };

  const handleRefresh = () => {
    window.location.reload();
  };

  // Filter alerts by severity
  const criticalAlerts = alerts ? alerts.filter(alert => alert.severity === 'critical') : [];
  const warnings = alerts ? alerts.filter(alert => alert.severity === 'warning') : [];

  return (
    <Box>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">System Metrics</Typography>
        <Box display="flex" gap={2}>
          <Button 
            variant="outlined" 
            onClick={handleTimeRangeClick}
            endIcon={<Box component="span" sx={{ ml: 1 }}>▼</Box>}
          >
            {timeRangeOptions.find(option => option.value === timeRange)?.label || 'Time Range'}
          </Button>
          <Menu
            anchorEl={timeRangeMenuAnchor}
            open={Boolean(timeRangeMenuAnchor)}
            onClose={() => setTimeRangeMenuAnchor(null)}
          >
            {timeRangeOptions.map(option => (
              <MenuItem 
                key={option.value} 
                onClick={() => handleTimeRangeSelect(option.value)}
                selected={timeRange === option.value}
              >
                {option.label}
              </MenuItem>
            ))}
          </Menu>
          <Button 
            variant="contained" 
            startIcon={<RefreshIcon />}
            onClick={handleRefresh}
          >
            Refresh
          </Button>
        </Box>
      </Box>

      {/* Status Summary */}
      <Box mb={4}>
        <Grid container spacing={3}>
          <Grid item xs={12} md={4}>
            <Box 
              sx={{ 
                p: 2, 
                bgcolor: 'background.paper', 
                borderRadius: 1,
                boxShadow: 1,
                display: 'flex',
                alignItems: 'center',
                gap: 2
              }}
            >
              <ErrorIcon color="error" fontSize="large" />
              <Box>
                <Typography variant="body2" color="text.secondary">Critical Alerts</Typography>
                <Typography variant="h5">{criticalAlerts.length}</Typography>
              </Box>
            </Box>
          </Grid>
          <Grid item xs={12} md={4}>
            <Box 
              sx={{ 
                p: 2, 
                bgcolor: 'background.paper', 
                borderRadius: 1,
                boxShadow: 1,
                display: 'flex',
                alignItems: 'center',
                gap: 2
              }}
            >
              <WarningIcon color="warning" fontSize="large" />
              <Box>
                <Typography variant="body2" color="text.secondary">Warnings</Typography>
                <Typography variant="h5">{warnings.length}</Typography>
              </Box>
            </Box>
          </Grid>
          <Grid item xs={12} md={4}>
            <Box 
              sx={{ 
                p: 2, 
                bgcolor: 'background.paper', 
                borderRadius: 1,
                boxShadow: 1,
                display: 'flex',
                alignItems: 'center',
                gap: 2
              }}
            >
              <CheckCircleIcon color="success" fontSize="large" />
              <Box>
                <Typography variant="body2" color="text.secondary">Healthy Services</Typography>
                <Typography variant="h5">
                  {serviceHealth ? serviceHealth.filter(s => s.status === 'healthy').length : 0} / {serviceHealth ? serviceHealth.length : 0}
                </Typography>
              </Box>
            </Box>
          </Grid>
        </Grid>
      </Box>

      {/* Tabs */}
      <Box mb={3}>
        <Tabs 
          value={activeTab} 
          onChange={handleTabChange}
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          {tabs.map(tab => (
            <Tab key={tab.value} value={tab.value} label={tab.label} />
          ))}
        </Tabs>
      </Box>

      {/* Tab Content */}
      {activeTab === 'overview' && (
        <Grid container spacing={4}>
          <Grid item xs={12}>
            {systemMetrics && <SystemMetricsCard metrics={systemMetrics} />}
          </Grid>
        </Grid>
      )}

      {activeTab === 'services' && (
        <Grid container spacing={4}>
          {serviceMetrics && Object.entries(serviceMetrics as Record<string, ServiceMetrics>).map(([serviceName, metrics]) => (
            <Grid item xs={12} md={6} key={serviceName}>
              <ServiceMetricsCard 
                serviceName={serviceName} 
                metrics={metrics} 
                health={serviceHealth?.find(h => h.serviceName === serviceName)} 
              />
            </Grid>
          ))}
        </Grid>
      )}

      {activeTab === 'alerts' && (
        <Grid container spacing={4}>
          <Grid item xs={12}>
            <AlertsList alerts={alerts || []} />
          </Grid>
        </Grid>
      )}

      {/* Loading and Error States */}
      {loading && (
        <Box textAlign="center" py={4}>
          <Typography>Loading metrics data...</Typography>
        </Box>
      )}

      {error && (
        <Box 
          textAlign="center" 
          py={4} 
          sx={{ 
            bgcolor: 'error.light', 
            color: 'error.contrastText',
            p: 2,
            borderRadius: 1
          }}
        >
          <Typography>Error loading metrics: {error.message}</Typography>
        </Box>
      )}
    </Box>
  );
}