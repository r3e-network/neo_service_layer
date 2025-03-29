import React from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Box,
  Typography,
  Skeleton,
  Chip
} from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import WarningIcon from '@mui/icons-material/Warning';
import ErrorIcon from '@mui/icons-material/Error';
import { PriceSourceListProps, PriceSource } from '../types/types';
import { formatCurrency, formatPercent } from '@/utils/formatters';

export default function PriceSourceList({
  sources,
  stats,
  isLoading,
  className
}: PriceSourceListProps) {
  const getStatusIcon = (status: PriceSource['status']) => {
    switch (status) {
      case 'active':
        return <CheckCircleIcon color="success" />;
      case 'warning':
        return <WarningIcon color="warning" />;
      case 'error':
        return <ErrorIcon color="error" />;
      default:
        return null;
    }
  };

  const getStatusColor = (status: PriceSource['status']) => {
    switch (status) {
      case 'active':
        return 'success';
      case 'warning':
        return 'warning';
      case 'error':
        return 'error';
      default:
        return 'default';
    }
  };

  if (isLoading) {
    return (
      <Box className={className}>
        {[...Array(5)].map((_, index) => (
          <Box key={index} mb={2}>
            <Skeleton variant="rectangular" height={40} />
          </Box>
        ))}
      </Box>
    );
  }

  if (!sources.length) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight={200}
        className={className}
      >
        <Typography color="text.secondary">
          No price sources available
        </Typography>
      </Box>
    );
  }

  return (
    <TableContainer className={className}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Source</TableCell>
            <TableCell align="right">Price</TableCell>
            <TableCell align="right">Deviation</TableCell>
            <TableCell align="right">Weight</TableCell>
            <TableCell align="center">Status</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {sources.map((source) => (
            <TableRow key={source.id}>
              <TableCell component="th" scope="row">
                {source.name}
              </TableCell>
              <TableCell align="right">
                {formatCurrency(source.currentPrice)}
              </TableCell>
              <TableCell align="right">
                {formatPercent(source.deviation)}
              </TableCell>
              <TableCell align="right">
                {formatPercent(source.weight)}
              </TableCell>
              <TableCell align="center">
                <Chip
                  size="small"
                  icon={getStatusIcon(source.status)}
                  label={source.status}
                  color={getStatusColor(source.status)}
                />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Box mt={2} display="flex" justifyContent="space-between">
        <Typography variant="body2" color="text.secondary">
          Total Sources: {stats.totalSources}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Active: {stats.activeSources}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Avg Deviation: {formatPercent(stats.averageDeviation)}
        </Typography>
      </Box>
    </TableContainer>
  );
}