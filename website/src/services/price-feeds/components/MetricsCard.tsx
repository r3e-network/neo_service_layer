import React from 'react';
import { Paper, Typography, Box, Skeleton } from '@mui/material';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import { MetricsCardProps } from '../types/types';
import { formatCurrency, formatNumber, formatPercent } from '@/utils/formatters';

export default function MetricsCard({
  title,
  value,
  previousValue,
  format = 'number',
  precision,
  isLoading,
  className
}: MetricsCardProps) {
  const formatValue = (val: number | string) => {
    if (typeof val === 'string') return val;
    
    switch (format) {
      case 'currency':
        return formatCurrency(val);
      case 'percent':
        return formatPercent(val, precision);
      default:
        return formatNumber(val, precision);
    }
  };

  const calculateChange = () => {
    if (previousValue === undefined || typeof value === 'string') return null;
    if (typeof previousValue === 'string') return null;
    
    const change = ((value as number) - previousValue) / previousValue;
    return {
      value: change,
      isPositive: change >= 0
    };
  };

  const change = calculateChange();

  if (isLoading) {
    return (
      <Paper elevation={2} className={className} sx={{ p: 2 }}>
        <Skeleton variant="text" width="60%" />
        <Skeleton variant="text" width="80%" height={40} />
        <Skeleton variant="text" width="40%" />
      </Paper>
    );
  }

  return (
    <Paper elevation={2} className={className} sx={{ p: 2 }}>
      <Typography variant="subtitle2" color="text.secondary" gutterBottom>
        {title}
      </Typography>
      
      <Typography variant="h4" component="div" gutterBottom>
        {formatValue(value)}
      </Typography>

      {change && (
        <Box display="flex" alignItems="center">
          {change.isPositive ? (
            <TrendingUpIcon color="success" fontSize="small" />
          ) : (
            <TrendingDownIcon color="error" fontSize="small" />
          )}
          <Typography
            variant="body2"
            color={change.isPositive ? 'success.main' : 'error.main'}
            sx={{ ml: 0.5 }}
          >
            {formatPercent(Math.abs(change.value))}
          </Typography>
        </Box>
      )}
    </Paper>
  );
}