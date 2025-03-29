/**
 * PriceFeed Component
 * 
 * Displays real-time price feed data with configurable refresh intervals
 * and visualization options.
 */

import React from 'react';
import { Box, Typography, CircularProgress, Card, CardContent, CardHeader, Grid } from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import TrendingFlatIcon from '@mui/icons-material/TrendingFlat';

export interface PriceFeedProps {
  symbol: string;
  refreshInterval?: number; // in milliseconds
  endpoint?: string;
  showConfidence?: boolean;
  showTimestamp?: boolean;
}

export interface PriceData {
  price: number;
  timestamp: string;
  confidence: number;
}

/**
 * PriceFeed component for displaying real-time price data
 */
const PriceFeed: React.FC<PriceFeedProps> = ({
  symbol,
  refreshInterval = 30000,
  endpoint = '/api/price',
  showConfidence = true,
  showTimestamp = true,
}) => {
  const [data, setData] = React.useState<PriceData | null>(null);
  const [previousPrice, setPreviousPrice] = React.useState<number | null>(null);
  const [loading, setLoading] = React.useState<boolean>(true);
  const [error, setError] = React.useState<string | null>(null);

  // Fetch price data from the API
  const fetchPriceData = async () => {
    try {
      setLoading(true);
      const response = await fetch(`${endpoint}?symbol=${encodeURIComponent(symbol)}`);
      
      if (!response.ok) {
        throw new Error(`API error: ${response.status}`);
      }
      
      const result = await response.json();
      
      if (data?.price) {
        setPreviousPrice(data.price);
      }
      
      setData(result);
      setError(null);
    } catch (err) {
      setError(`Failed to fetch price data: ${err instanceof Error ? err.message : String(err)}`);
    } finally {
      setLoading(false);
    }
  };

  // Initial data fetch and refresh interval
  React.useEffect(() => {
    fetchPriceData();
    
    const intervalId = setInterval(fetchPriceData, refreshInterval);
    
    return () => clearInterval(intervalId);
  }, [symbol, refreshInterval, endpoint]);

  // Determine price trend
  const getTrend = () => {
    if (!previousPrice || !data) return 'flat';
    if (data.price > previousPrice) return 'up';
    if (data.price < previousPrice) return 'down';
    return 'flat';
  };

  // Format price with appropriate decimal places
  const formatPrice = (price: number) => {
    if (price >= 1000) return price.toFixed(2);
    if (price >= 100) return price.toFixed(3);
    if (price >= 10) return price.toFixed(4);
    if (price >= 1) return price.toFixed(5);
    return price.toFixed(6);
  };

  // Format timestamp to local date/time
  const formatTimestamp = (timestamp: string) => {
    return new Date(timestamp).toLocaleString();
  };

  // Render price trend icon
  const renderTrendIcon = () => {
    const trend = getTrend();
    
    if (trend === 'up') {
      return <TrendingUpIcon sx={{ color: 'success.main' }} />;
    } else if (trend === 'down') {
      return <TrendingDownIcon sx={{ color: 'error.main' }} />;
    } else {
      return <TrendingFlatIcon sx={{ color: 'text.secondary' }} />;
    }
  };

  return (
    <Card sx={{ minWidth: 275, maxWidth: 500 }}>
      <CardHeader 
        title={`${symbol} Price Feed`}
        action={
          <RefreshIcon 
            onClick={() => fetchPriceData()} 
            sx={{ cursor: 'pointer' }}
          />
        }
      />
      <CardContent>
        {loading && !data ? (
          <Box display="flex" justifyContent="center" alignItems="center" p={2}>
            <CircularProgress size={40} />
          </Box>
        ) : error ? (
          <Typography color="error" variant="body2">{error}</Typography>
        ) : data ? (
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Box display="flex" alignItems="center">
                <Typography variant="h4" component="div">
                  {formatPrice(data.price)}
                </Typography>
                <Box ml={1}>{renderTrendIcon()}</Box>
              </Box>
            </Grid>
            
            {showConfidence && (
              <Grid item xs={12}>
                <Typography variant="body2" color="text.secondary">
                  Confidence: {(data.confidence * 100).toFixed(2)}%
                </Typography>
              </Grid>
            )}
            
            {showTimestamp && (
              <Grid item xs={12}>
                <Typography variant="body2" color="text.secondary">
                  Last Updated: {formatTimestamp(data.timestamp)}
                </Typography>
              </Grid>
            )}
          </Grid>
        ) : (
          <Typography variant="body2">No data available</Typography>
        )}
      </CardContent>
    </Card>
  );
};

export default PriceFeed;