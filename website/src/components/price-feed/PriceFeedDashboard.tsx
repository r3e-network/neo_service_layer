import React from 'react';
import { Card, Grid, Typography, Box, Button, CircularProgress } from '@mui/material';
import PriceChart from './PriceChart';
import PriceFeedConfig from './PriceFeedConfig';
import PriceSourceList from './PriceSourceList';
import { useTheme } from '@mui/material/styles';
import MetricsCard from '../shared/MetricsCard';
import AlertBanner from '../shared/AlertBanner';
import { usePriceFeed } from '../../app/hooks/usePriceFeed';
import { useAuth } from '../../app/hooks/useAuth';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';

interface PriceFeedDashboardProps {
  symbol: string;
}

const PriceFeedDashboard: React.FC<PriceFeedDashboardProps> = ({ symbol }) => {
  const theme = useTheme();
  const {
    priceData,
    stats,
    error: priceError,
    isLoading,
    isConnected
  } = usePriceFeed({ symbol });

  const {
    isAuthenticated,
    wallet,
    error: authError,
    connectWallet,
    authenticate,
    disconnect
  } = useAuth();

  const handleConnect = async () => {
    try {
      await connectWallet();
      await authenticate();
    } catch (error) {
      console.error('Connection error:', error);
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Authentication Status */}
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'flex-end' }}>
        {isAuthenticated ? (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Typography variant="body2" color="textSecondary">
              Connected: {wallet?.address.slice(0, 6)}...{wallet?.address.slice(-4)}
            </Typography>
            <Button
              variant="outlined"
              size="small"
              onClick={disconnect}
            >
              Disconnect
            </Button>
          </Box>
        ) : (
          <Button
            variant="contained"
            startIcon={<AccountBalanceWalletIcon />}
            onClick={handleConnect}
          >
            Connect Wallet
          </Button>
        )}
      </Box>

      {/* Error Messages */}
      {(priceError || authError) && (
        <AlertBanner
          message={priceError || authError || ''}
          severity="error"
        />
      )}
      
      {/* WebSocket Status */}
      {isAuthenticated && (
        <Box sx={{ mb: 2 }}>
          <Typography
            variant="body2"
            color={isConnected ? 'success.main' : 'error.main'}
            sx={{ display: 'flex', alignItems: 'center', gap: 1 }}
          >
            <Box
              sx={{
                width: 8,
                height: 8,
                borderRadius: '50%',
                bgcolor: isConnected ? 'success.main' : 'error.main'
              }}
            />
            {isConnected ? 'Live Updates' : 'Connecting...'}
          </Typography>
        </Box>
      )}

      <Grid container spacing={3}>
        {/* Price Overview */}
        <Grid sx={{ gridColumn: { xs: 'span 12', md: 'span 4' } }}>
          <Card sx={{ p: 2 }}>
            {isLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                <CircularProgress />
              </Box>
            ) : (
              <>
                <Typography variant="h6" gutterBottom>
                  Current Price
                </Typography>
                <Typography variant="h3">
                  ${priceData?.price.toFixed(2) || '-.--'}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Confidence: {(priceData?.confidence || 0) * 100}%
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Last Update: {priceData?.timestamp ? new Date(priceData.timestamp).toLocaleString() : '-'}
                </Typography>
              </>
            )}
          </Card>
        </Grid>

        {/* Statistics Cards */}
        <Grid sx={{ gridColumn: { xs: 'span 12', md: 'span 8' } }}>
          <Grid container spacing={{ xs: 2 }}>
            <Grid sx={{ gridColumn: { xs: 'span 12', sm: 'span 6' } }}>
              <MetricsCard
                title="Volatility"
                value={`${((stats?.volatility || 0) * 100).toFixed(2)}%`}
                subtitle="24h Change"
                trend={stats?.volatility && stats.volatility > 0.1 ? 'up' : 'down'}
              />
            </Grid>
            <Grid sx={{ gridColumn: { xs: 'span 12', sm: 'span 6' } }}>
              <MetricsCard
                title="Source Agreement"
                value={`${((1 - (stats?.stdDev || 0) / (stats?.mean || 1)) * 100).toFixed(2)}%`}
                subtitle="Data Source Consensus"
                trend={stats?.outliers && stats.outliers.length === 0 ? 'up' : 'down'}
              />
            </Grid>
          </Grid>
        </Grid>

        {/* Price Chart */}
        <Grid sx={{ gridColumn: 'span 12' }}>
          <Card sx={{ p: 2 }}>
            <PriceChart symbol={symbol} />
          </Card>
        </Grid>

        {/* Price Sources */}
        <Grid sx={{ gridColumn: { xs: 'span 12', md: 'span 8' } }}>
          <Card sx={{ p: 2 }}>
            <PriceSourceList sources={priceData?.details.sources || []} />
          </Card>
        </Grid>

        {/* Configuration */}
        <Grid sx={{ gridColumn: { xs: 'span 12', md: 'span 4' } }}>
          <Card sx={{ p: 2 }}>
            {isAuthenticated ? (
              <PriceFeedConfig symbol={symbol} />
            ) : (
              <Box sx={{ p: 3, textAlign: 'center' }}>
                <Typography variant="body1" gutterBottom>
                  Connect your wallet to access configuration settings
                </Typography>
                <Button
                  variant="contained"
                  startIcon={<AccountBalanceWalletIcon />}
                  onClick={handleConnect}
                  sx={{ mt: 2 }}
                >
                  Connect Wallet
                </Button>
              </Box>
            )}
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default PriceFeedDashboard;