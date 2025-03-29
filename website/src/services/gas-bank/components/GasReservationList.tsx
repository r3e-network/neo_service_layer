// @ts-ignore
import * as React from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Button,
  Typography,
  Tooltip,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Box
} from '@mui/material';
// @ts-ignore
import CancelIcon from '@mui/icons-material/Cancel';
// @ts-ignore
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
// @ts-ignore
import TimerIcon from '@mui/icons-material/Timer';
import { GasReservation, GasAccount } from '../types/types';
import { formatGas, formatDate, formatDuration } from '../utils/formatters';

interface GasReservationListProps {
  reservations: GasReservation[];
  account: GasAccount;
}

export function GasReservationList({
  reservations,
  account
}: GasReservationListProps) {
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'active':
        return 'success';
      case 'consumed':
        return 'info';
      case 'expired':
        return 'error';
      case 'released':
        return 'warning';
      default:
        return 'default';
    }
  };

  const getTimeRemaining = (expiresAt: number) => {
    const now = Date.now();
    const remaining = expiresAt - now;
    return remaining > 0 ? formatDuration(remaining) : 'Expired';
  };

  return (
    <Paper>
      <Box p={2} display="flex" justifyContent="space-between" alignItems="center">
        <Typography variant="h6">Gas Reservations</Typography>
        <Button
          variant="contained"
          color="primary"
          startIcon={<TimerIcon />}
        >
          New Reservation
        </Button>
      </Box>
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Created</TableCell>
              <TableCell>Purpose</TableCell>
              <TableCell>Amount</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Expires In</TableCell>
              <TableCell>Metadata</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {reservations.map((reservation) => (
              <TableRow key={reservation.id}>
                <TableCell>
                  {formatDate(new Date(reservation.expiresAt - 24 * 60 * 60 * 1000))} {/* Assuming 24h reservation */}
                </TableCell>
                <TableCell>
                  <Typography variant="body2">
                    {reservation.purpose}
                  </Typography>
                </TableCell>
                <TableCell>{formatGas(reservation.amount)}</TableCell>
                <TableCell>
                  <Chip
                    label={reservation.status}
                    size="small"
                    color={getStatusColor(reservation.status)}
                  />
                </TableCell>
                <TableCell>
                  <Box display="flex" alignItems="center" gap={1}>
                    <TimerIcon fontSize="small" />
                    <Typography variant="body2">
                      {getTimeRemaining(reservation.expiresAt)}
                    </Typography>
                  </Box>
                </TableCell>
                <TableCell>
                  <Tooltip title={JSON.stringify(reservation.metadata, null, 2)}>
                    <Typography
                      variant="body2"
                      sx={{
                        maxWidth: 200,
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                        whiteSpace: 'nowrap'
                      }}
                    >
                      {Object.entries(reservation.metadata)
                        .map(([key, value]) => `${key}: ${value}`)
                        .join(', ')}
                    </Typography>
                  </Tooltip>
                </TableCell>
                <TableCell align="right">
                  {reservation.status === 'active' && (
                    <>
                      <Tooltip title="Consume Gas">
                        <IconButton size="small" color="primary">
                          <CheckCircleIcon />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Release Reservation">
                        <IconButton size="small" color="error">
                          <CancelIcon />
                        </IconButton>
                      </Tooltip>
                    </>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  );
}