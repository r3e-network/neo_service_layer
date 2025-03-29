import React from 'react';
import { Box, Paper, Typography, LinearProgress, Grid, Tooltip } from '@mui/material';
import AccountBalanceWallet from '@mui/icons-material/AccountBalanceWallet';
import Speed from '@mui/icons-material/Speed';
import TrendingUp from '@mui/icons-material/TrendingUp';
import Warning from '@mui/icons-material/Warning';
import { formatNumber, formatPercent } from '../../../utils/formatters';
import { GasMetrics } from '../types/types';

interface GasMetricsCardProps {
  metrics: GasMetrics;
}

// Helper function to format gas values
const formatGas = (value: number): string => {
  return `${formatNumber(value)} GAS`;
};

const GasMetricsCard: React.FC<GasMetricsCardProps> = ({ metrics }) => {
  if (!metrics) {
    return (
      <Box sx={{ p: 2 }}>
        <Typography variant="body1">Loading metrics...</Typography>
      </Box>
    );
  }

  // Inline MetricItem component
  const MetricItem = ({
    title,
    value,
    icon,
    color,
    tooltip
  }: {
    title: string;
    value: string | number;
    icon: React.ReactNode;
    color?: string;
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
          <Typography
            variant="h5"
            color={color || 'text.primary'}
            sx={{ fontWeight: 'medium' }}
          >
            {value}
          </Typography>
        </Box>
      </Tooltip>
    </Paper>
  );

  // Create gas distribution data based on the metrics
  const gasDistribution = {
    available: metrics.totalAvailable,
    reserved: metrics.totalReserved,
    total: metrics.totalBalance
  };

  return (
    <Grid container spacing={2}>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <MetricItem
          title="Total Balance"
          value={formatGas(metrics.totalBalance)}
          icon={<AccountBalanceWallet color="primary" />}
          tooltip="Total gas balance across all accounts"
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <MetricItem
          title="Average Gas Price"
          value={formatGas(metrics.averageGasPrice)}
          icon={<Speed color="primary" />}
          tooltip="Current average gas price"
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <MetricItem
          title="Transactions/min"
          value={formatNumber(metrics?.transactionsPerMinute || 0)} 
          icon={<TrendingUp color="primary" />}
          tooltip="Number of gas transactions per minute"
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 6, md: 3 }}>
        <MetricItem
          title="Success Rate"
          value={formatPercent(metrics.successRate)}
          icon={<Warning />}
          color={metrics.successRate < 95 ? 'error.main' : 'success.main'}
          tooltip="Transaction success rate"
        />
      </Grid>

      <Grid size={{ xs: 12 }}>
        <Paper sx={{ p: 2 }}>
          <Typography variant="h6" gutterBottom>
            Account Breakdown
          </Typography>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="body2" color="text.secondary">
                Active
              </Typography>
              <Typography variant="body1">
                {metrics?.accountBreakdown?.active || 0}
              </Typography>
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="body2" color="text.secondary">
                Low / Depleted / Locked
              </Typography>
              <Typography variant="body1">
                {(metrics?.accountBreakdown?.low || 0) + 
                 (metrics?.accountBreakdown?.depleted || 0) + 
                 (metrics?.accountBreakdown?.locked || 0)}
              </Typography>
            </Grid>
          </Grid>
        </Paper>
      </Grid>

      <Grid size={{ xs: 12 }}>
        <Paper sx={{ p: 2 }}>
          <Typography variant="subtitle2" gutterBottom>
            Gas Distribution
          </Typography>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Box sx={{ mb: 1 }}>
                <Box display="flex" justifyContent="space-between" mb={0.5}>
                  <Typography variant="body2">Available</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {formatGas(gasDistribution.available || 0)}
                  </Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={(gasDistribution.available / gasDistribution.total) * 100 || 0}
                  color="success"
                  sx={{ height: 8, borderRadius: 1 }}
                />
              </Box>
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Box sx={{ mb: 1 }}>
                <Box display="flex" justifyContent="space-between" mb={0.5}>
                  <Typography variant="body2">Reserved</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {formatGas(gasDistribution.reserved || 0)}
                  </Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={(gasDistribution.reserved / gasDistribution.total) * 100 || 0}
                  color="warning"
                  sx={{ height: 8, borderRadius: 1 }}
                />
              </Box>
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Box sx={{ mb: 1 }}>
                <Box display="flex" justifyContent="space-between" mb={0.5}>
                  <Typography variant="body2">Total</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {formatGas(gasDistribution.total || 0)}
                  </Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={100}
                  color="info"
                  sx={{ height: 8, borderRadius: 1 }}
                />
              </Box>
            </Grid>
          </Grid>
        </Paper>
      </Grid>
    </Grid>
  );
};

export default GasMetricsCard;