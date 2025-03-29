import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Grid,
  Box,
  LinearProgress,
  Divider,
  Tooltip
} from '@mui/material';
import MemoryIcon from '@mui/icons-material/Memory';
import StorageIcon from '@mui/icons-material/Storage';
import SpeedIcon from '@mui/icons-material/Speed';
import NetworkCheckIcon from '@mui/icons-material/NetworkCheck';
import { SystemMetrics } from '../types/types';
import { formatBytes, formatPercentage, formatNumber } from '../utils/formatters';

interface SystemMetricsCardProps {
  metrics: SystemMetrics;
}

export function SystemMetricsCard({ metrics }: SystemMetricsCardProps) {
  const renderMetricProgress = (
    label: string,
    used: number,
    total: number,
    icon: React.ReactNode
  ) => {
    const percentage = (used / total) * 100;
    return (
      <Box sx={{ mb: 2 }}>
        <Box display="flex" alignItems="center" gap={1} mb={1}>
          {icon}
          <Typography variant="body1">{label}</Typography>
        </Box>
        <Box display="flex" alignItems="center" gap={2}>
          <Box flex={1}>
            <LinearProgress
              variant="determinate"
              value={percentage}
              color={percentage > 90 ? 'error' : percentage > 70 ? 'warning' : 'primary'}
            />
          </Box>
          <Typography variant="body2" color="text.secondary" minWidth={100}>
            {formatBytes(used)} / {formatBytes(total)}
          </Typography>
        </Box>
      </Box>
    );
  };

  return (
    <Card>
      <CardContent>
        <Typography variant="h5" gutterBottom>
          System Resources
        </Typography>

        {/* CPU Usage */}
        <Box sx={{ mb: 3 }}>
          <Box display="flex" alignItems="center" gap={1} mb={1}>
            <SpeedIcon color="primary" />
            <Typography variant="body1">CPU Usage</Typography>
          </Box>
          <Box display="flex" alignItems="center" gap={2}>
            <Box flex={1}>
              <LinearProgress
                variant="determinate"
                value={metrics.cpu.usage}
                color={metrics.cpu.usage > 90 ? 'error' : metrics.cpu.usage > 70 ? 'warning' : 'primary'}
              />
            </Box>
            <Typography variant="body2" color="text.secondary" minWidth={100}>
              {formatPercentage(metrics.cpu.usage)} ({metrics.cpu.cores} cores)
            </Typography>
          </Box>
        </Box>

        {/* Memory Usage */}
        {renderMetricProgress(
          'Memory Usage',
          metrics.memory.used,
          metrics.memory.total,
          <MemoryIcon color="primary" />
        )}

        {/* Disk Usage */}
        {renderMetricProgress(
          'Disk Usage',
          metrics.disk.used,
          metrics.disk.total,
          <StorageIcon color="primary" />
        )}

        {/* Network */}
        <Box sx={{ mt: 3 }}>
          <Box display="flex" alignItems="center" gap={1} mb={2}>
            <NetworkCheckIcon color="primary" />
            <Typography variant="body1">Network</Typography>
          </Box>
          <Grid container spacing={2}>
            <Grid item xs={6}>
              <Tooltip title="Incoming network traffic">
                <Typography variant="body2" color="text.secondary">
                  Incoming
                </Typography>
              </Tooltip>
              <Typography variant="h6">
                {formatBytes(metrics.network.bytesIn)}/s
              </Typography>
            </Grid>
            <Grid item xs={6}>
              <Tooltip title="Outgoing network traffic">
                <Typography variant="body2" color="text.secondary">
                  Outgoing
                </Typography>
              </Tooltip>
              <Typography variant="h6">
                {formatBytes(metrics.network.bytesOut)}/s
              </Typography>
            </Grid>
            <Grid item xs={12}>
              <Tooltip title="Active network connections">
                <Typography variant="body2" color="text.secondary">
                  Active Connections
                </Typography>
              </Tooltip>
              <Typography variant="h6">
                {formatNumber(metrics.network.connectionsActive)}
              </Typography>
            </Grid>
          </Grid>
        </Box>
      </CardContent>
    </Card>
  );
}