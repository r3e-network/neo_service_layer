'use client';

import React from 'react';
import { usePriceFeed } from '@/hooks/usePriceFeed';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
  CircularProgress,
  Alert,
  Select,
  MenuItem,
  InputLabel,
  FormControl,
  Skeleton,
  Grid,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material';
import {
  ArrowTrendingUpIcon,
  ArrowTrendingDownIcon,
  ArrowPathIcon,
  CheckCircleIcon,
  ClockIcon,
} from '@heroicons/react/24/outline';

export default function PriceFeedDisplay() {
  const {
    loading,
    error,
    data,
    symbols,
    fetchPrice,
    selectedSymbol,
    setSelectedSymbol,
  } = usePriceFeed({
    autoFetch: true,
    refreshInterval: 10000, // 10 seconds
  });

  const handleSymbolChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setSelectedSymbol(e.target.value);
  };

  const handleRefresh = () => {
    if (selectedSymbol) {
      fetchPrice(selectedSymbol);
    }
  };

  const formatNumber = (num: number, precision = 2) => {
    return num.toLocaleString(undefined, {
      minimumFractionDigits: precision,
      maximumFractionDigits: precision,
    });
  };

  const formatTime = (timestamp: string) => {
    const date = new Date(timestamp);
    return date.toLocaleTimeString();
  };

  // Render hero section with current price
  const renderPriceHero = () => {
    if (loading && !data) {
      return (
        <Box sx={{ textAlign: 'center', py: 6 }}>
          <CircularProgress size={60} />
          <Typography variant="h6" sx={{ mt: 2 }}>
            Loading price data...
          </Typography>
        </Box>
      );
    }

    if (!data) {
      return (
        <Box sx={{ textAlign: 'center', py: 6 }}>
          <Typography variant="h6" color="text.secondary">
            Select a symbol to view price data
          </Typography>
        </Box>
      );
    }

    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <Typography variant="h2" component="h1" fontWeight="bold">
          ${formatNumber(data.price, data.price >= 100 ? 0 : data.price >= 1 ? 2 : 4)}
        </Typography>
        
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 1, mt: 1 }}>
          <Chip
            label={`Confidence: ${Math.round(data.confidence * 100)}%`}
            color={data.confidence > 0.9 ? "success" : data.confidence > 0.7 ? "primary" : "warning"}
            size="small"
          />
          
          <Chip
            label={`Updated: ${formatTime(data.timestamp)}`}
            variant="outlined"
            size="small"
          />
        </Box>
      </Box>
    );
  };

  // Render source details
  const renderSourceDetails = () => {
    if (!data || loading) {
      return (
        <Box sx={{ mt: 4 }}>
          <Skeleton variant="rectangular" height={200} />
        </Box>
      );
    }

    return (
      <Box sx={{ mt: 4 }}>
        <Typography variant="h6" gutterBottom>
          Source Details
        </Typography>
        
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Source</TableCell>
                <TableCell align="right">Price</TableCell>
                <TableCell align="right">Weight</TableCell>
                <TableCell align="right">Last Update</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data.details.sources.map((source) => (
                <TableRow key={source.name}>
                  <TableCell component="th" scope="row">
                    {source.name}
                  </TableCell>
                  <TableCell align="right">${formatNumber(source.price)}</TableCell>
                  <TableCell align="right">{Math.round(source.weight * 100)}%</TableCell>
                  <TableCell align="right">{formatTime(source.timestamp)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Box>
    );
  };

  // Render statistics section
  const renderStatistics = () => {
    if (!data?.stats || loading) {
      return (
        <Box sx={{ mt: 4 }}>
          <Skeleton variant="rectangular" height={150} />
        </Box>
      );
    }

    const { stats } = data;

    return (
      <Box sx={{ mt: 4 }}>
        <Typography variant="h6" gutterBottom>
          Price Statistics
        </Typography>
        
        <Grid container spacing={2}>
          <Grid item xs={6} md={3}>
            <Card variant="outlined">
              <CardContent>
                <Typography color="text.secondary" gutterBottom variant="body2">
                  Mean
                </Typography>
                <Typography variant="h6">${formatNumber(stats.mean)}</Typography>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={6} md={3}>
            <Card variant="outlined">
              <CardContent>
                <Typography color="text.secondary" gutterBottom variant="body2">
                  Median
                </Typography>
                <Typography variant="h6">${formatNumber(stats.median)}</Typography>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={6} md={3}>
            <Card variant="outlined">
              <CardContent>
                <Typography color="text.secondary" gutterBottom variant="body2">
                  Standard Deviation
                </Typography>
                <Typography variant="h6">${formatNumber(stats.stdDev)}</Typography>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={6} md={3}>
            <Card variant="outlined">
              <CardContent>
                <Typography color="text.secondary" gutterBottom variant="body2">
                  Volatility
                </Typography>
                <Typography variant="h6">{formatNumber(stats.volatility * 100)}%</Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </Box>
    );
  };

  return (
    <Box sx={{ py: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <FormControl sx={{ minWidth: 150 }}>
          <InputLabel id="symbol-select-label">Symbol</InputLabel>
          <Select
            labelId="symbol-select-label"
            value={selectedSymbol || ''}
            label="Symbol"
            onChange={handleSymbolChange as any}
          >
            {symbols.map((symbol) => (
              <MenuItem key={symbol} value={symbol}>
                {symbol}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
        
        <button
          onClick={handleRefresh}
          className="inline-flex items-center justify-center rounded-md bg-blue-600 px-3 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-600"
          disabled={loading}
        >
          <ArrowPathIcon className="h-5 w-5 mr-1" />
          Refresh
        </button>
      </Box>
      
      {error && (
        <Alert severity="error" sx={{ mb: 4 }}>
          {error}
        </Alert>
      )}
      
      <Card className="bg-white dark:bg-gray-800">
        <CardContent>
          {renderPriceHero()}
          {renderSourceDetails()}
          {renderStatistics()}
        </CardContent>
      </Card>
    </Box>
  );
} 