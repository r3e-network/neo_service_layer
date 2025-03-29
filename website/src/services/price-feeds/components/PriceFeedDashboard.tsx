import React from 'react';
import { Grid, Paper, Typography, Box } from '@mui/material';
import { usePriceFeed } from '@/services/price-feeds/hooks/usePriceFeed';
import { useAuth } from '@/app/hooks/useAuth';
import { PriceFeedDashboardProps } from '@/services/price-feeds/types/types';
import PriceChart from './PriceChart';
import PriceSourceList from './PriceSourceList';
import PriceFeedConfig from './PriceFeedConfig';
import MetricsCard from './MetricsCard';
import AlertBanner from './AlertBanner';
import { formatCurrency, formatPercent } from '@/utils/formatters';

export default function PriceFeedDashboard({ symbol, className }: PriceFeedDashboardProps) {
  const {
    currentPrice,
    lastUpdate,
    historicalPrices,
    sources,
    sourceStats,
    metrics,
    config,
    isLoading,
    error,
    updateConfig,
    refreshData
  } = usePriceFeed(symbol);

  const { isAuthenticated } = useAuth();

  const renderError = () => {
    if (!error) return null;
    return (
      <AlertBanner
        severity="error"
        message={error}
        onClose={() => refreshData()}
      />
    );
  };

  const renderPriceOverview = () => (
    <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
      <Typography variant="h6" gutterBottom>
        Current Price
      </Typography>
      <Box display="flex" alignItems="baseline" mb={1}>
        <Typography variant="h3" component="span">
          {currentPrice ? formatCurrency(currentPrice) : '-'}
        </Typography>
        {metrics?.priceChangePercent24h && (
          <Typography
            variant="subtitle1"
            component="span"
            color={metrics.priceChangePercent24h >= 0 ? 'success.main' : 'error.main'}
            sx={{ ml: 2 }}
          >
            {formatPercent(metrics.priceChangePercent24h)}
          </Typography>
        )}
      </Box>
      <Typography variant="body2" color="text.secondary">
        Last updated: {lastUpdate ? new Date(lastUpdate).toLocaleString() : '-'}
      </Typography>
    </Paper>
  );

  const renderMetricsCards = () => (
    <Grid container spacing={3} sx={{ mb: 3 }}>
      <Grid item xs={12} sm={6} md={3}>
        <MetricsCard
          title="24h Volume"
          value={metrics?.volume24h || 0}
          previousValue={metrics?.volume24h ? metrics.volume24h - (metrics.volumeChange24h || 0) : 0}
          format="currency"
          isLoading={isLoading}
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <MetricsCard
          title="Active Sources"
          value={sourceStats.activeSources}
          previousValue={sourceStats.activeSources - (metrics?.sourceCountChange24h || 0)}
          format="number"
          isLoading={isLoading}
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <MetricsCard
          title="Average Deviation"
          value={sourceStats.averageDeviation}
          format="percent"
          precision={4}
          isLoading={isLoading}
        />
      </Grid>
      <Grid item xs={12} sm={6} md={3}>
        <MetricsCard
          title="Average Latency"
          value={sourceStats.averageLatency}
          format="number"
          precision={0}
          isLoading={isLoading}
        />
      </Grid>
    </Grid>
  );

  return (
    <Box className={className}>
      {renderError()}
      {renderPriceOverview()}
      {renderMetricsCards()}

      <Grid container spacing={3}>
        <Grid item xs={12} lg={8}>
          <Paper elevation={2} sx={{ p: 3, mb: { xs: 3, lg: 0 } }}>
            <Typography variant="h6" gutterBottom>
              Price History
            </Typography>
            <PriceChart
              data={historicalPrices || []}
              onTimeRangeChange={() => {}}
              isLoading={isLoading}
            />
          </Paper>
        </Grid>

        <Grid item xs={12} lg={4}>
          <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Price Sources
            </Typography>
            <PriceSourceList
              sources={sources}
              stats={sourceStats}
              isLoading={isLoading}
            />
          </Paper>

          {isAuthenticated && (
            <Paper elevation={2} sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Configuration
              </Typography>
              <PriceFeedConfig
                config={config}
                onConfigUpdate={updateConfig}
                onConfigReset={refreshData}
                validationErrors={[]}
                isLoading={isLoading}
              />
            </Paper>
          )}
        </Grid>
      </Grid>
    </Box>
  );
}