import React from 'react';
import {
  Grid,
  Paper,
  Typography,
  Box,
  LinearProgress,
  Tooltip
} from '@mui/material';
import ErrorIcon from '@mui/icons-material/Error';
import WarningIcon from '@mui/icons-material/Warning';
import InfoIcon from '@mui/icons-material/Info';
import SpeedIcon from '@mui/icons-material/Speed';
import { LogStats as LogStatsType } from '../types/types';
import { formatNumber, formatPercentage } from '../utils/formatters';

interface LogStatsProps {
  stats: LogStatsType | null;
}

export function LogStats({ stats }: LogStatsProps) {
  if (!stats) {
    return null;
  }

  const StatCard = ({
    title,
    value,
    icon,
    color,
    tooltip
  }: {
    title: string;
    value: string | number;
    icon: React.ReactNode;
    color: string;
    tooltip?: string;
  }) => (
    <Paper sx={{ p: 2 }}>
      <Tooltip title={tooltip || ''}>
        <Box>
          <Box display="flex" alignItems="center" gap={1} mb={1}>
            {icon}
            <Typography variant="body2" color="text.secondary">
              {title}
            </Typography>
          </Box>
          <Typography variant="h5" color={color}>
            {value}
          </Typography>
        </Box>
      </Tooltip>
    </Paper>
  );

  return (
    <Grid container spacing={2}>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          title="Total Logs"
          value={formatNumber(stats.totalLogs)}
          icon={<InfoIcon color="info" />}
          color="text.primary"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          title="Error Rate"
          value={formatPercentage(stats.timeRangeStats.errorRate)}
          icon={<ErrorIcon color="error" />}
          color={stats.timeRangeStats.errorRate > 5 ? 'error.main' : 'text.primary'}
          tooltip="Percentage of error logs in the selected time range"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          title="Logs per Minute"
          value={formatNumber(stats.timeRangeStats.logsPerMinute)}
          icon={<SpeedIcon color="primary" />}
          color="text.primary"
          tooltip="Average number of logs per minute in the selected time range"
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <StatCard
          title="Warning Count"
          value={formatNumber(stats.warningCount)}
          icon={<WarningIcon color="warning" />}
          color="warning.main"
        />
      </Grid>

      <Grid item xs={12}>
        <Paper sx={{ p: 2 }}>
          <Typography variant="subtitle2" gutterBottom>
            Service Distribution
          </Typography>
          <Grid container spacing={2}>
            {Object.entries(stats.serviceBreakdown).map(([service, count]) => (
              <Grid item xs={12} sm={6} md={4} key={service}>
                <Box sx={{ mb: 1 }}>
                  <Box display="flex" justifyContent="space-between" mb={0.5}>
                    <Typography variant="body2">{service}</Typography>
                    <Typography variant="body2" color="text.secondary">
                      {formatNumber(count as number)}
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={((count as number) / stats.totalLogs) * 100}
                    sx={{ height: 8, borderRadius: 1 }}
                  />
                </Box>
              </Grid>
            ))}
          </Grid>
        </Paper>
      </Grid>

      <Grid item xs={12}>
        <Paper sx={{ p: 2 }}>
          <Typography variant="subtitle2" gutterBottom>
            Log Level Distribution
          </Typography>
          <Grid container spacing={2}>
            {Object.entries(stats.levelBreakdown).map(([level, count]) => (
              <Grid item xs={12} sm={6} md={4} key={level}>
                <Box sx={{ mb: 1 }}>
                  <Box display="flex" justifyContent="space-between" mb={0.5}>
                    <Typography variant="body2">{level}</Typography>
                    <Typography variant="body2" color="text.secondary">
                      {formatNumber(count as number)}
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={((count as number) / stats.totalLogs) * 100}
                    sx={{
                      height: 8,
                      borderRadius: 1,
                      '& .MuiLinearProgress-bar': {
                        backgroundColor:
                          level === 'error' || level === 'critical'
                            ? 'error.main'
                            : level === 'warn'
                            ? 'warning.main'
                            : level === 'info'
                            ? 'success.main'
                            : 'info.main'
                      }
                    }}
                  />
                </Box>
              </Grid>
            ))}
          </Grid>
        </Paper>
      </Grid>
    </Grid>
  );
}