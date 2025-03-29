import React from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Typography,
  Box,
  Chip,
  IconButton,
  Tooltip,
  TablePagination,
  Menu,
  MenuItem,
  Checkbox,
  FormControlLabel,
  TextField,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  SelectChangeEvent
} from '@mui/material';
import InfoIcon from '@mui/icons-material/Info';
import FilterListIcon from '@mui/icons-material/FilterList';
import { formatGas, formatDate } from '../utils/formatters';
import { GasTransaction, GasTransactionType, GasTransactionStatus } from '../types/types';

interface GasTransactionListProps {
  transactions: GasTransaction[];
  totalTransactions: number;
  onFetchTransactions: (
    address: string,
    options?: {
      limit?: number;
      offset?: number;
      type?: GasTransactionType;
      status?: GasTransactionStatus;
      startDate?: number;
      endDate?: number;
    }
  ) => void;
  address: string;
}

export function GasTransactionList({
  transactions,
  totalTransactions,
  onFetchTransactions,
  address
}: GasTransactionListProps) {
  const [page, setPage] = React.useState(0);
  const [rowsPerPage, setRowsPerPage] = React.useState(10);
  const [filterAnchorEl, setFilterAnchorEl] = React.useState<null | HTMLElement>(null);
  const [selectedTypes, setSelectedTypes] = React.useState<GasTransactionType[]>([]);
  const [selectedStatuses, setSelectedStatuses] = React.useState<GasTransactionStatus[]>([]);
  const [startDate, setStartDate] = React.useState<string>('');
  const [endDate, setEndDate] = React.useState<string>('');

  const handleChangePage = (event: unknown, newPage: number) => {
    setPage(newPage);
    onFetchTransactions(address, {
      limit: rowsPerPage,
      offset: newPage * rowsPerPage,
      type: selectedTypes.length > 0 ? selectedTypes : undefined,
      status: selectedStatuses.length > 0 ? selectedStatuses : undefined,
      startDate: startDate ? parseInt(startDate, 10) : undefined,
      endDate: endDate ? parseInt(endDate, 10) : undefined
    });
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newRowsPerPage = parseInt(event.target.value, 10);
    setRowsPerPage(newRowsPerPage);
    setPage(0);
    onFetchTransactions(address, {
      limit: newRowsPerPage,
      offset: 0,
      type: selectedTypes.length > 0 ? selectedTypes : undefined,
      status: selectedStatuses.length > 0 ? selectedStatuses : undefined,
      startDate: startDate ? parseInt(startDate, 10) : undefined,
      endDate: endDate ? parseInt(endDate, 10) : undefined
    });
  };

  const getStatusColor = (status: GasTransactionStatus) => {
    switch (status) {
      case 'confirmed':
        return 'success';
      case 'pending':
        return 'warning';
      case 'failed':
      case 'cancelled':
        return 'error';
      default:
        return 'default';
    }
  };

  const getTypeColor = (type: GasTransactionType) => {
    switch (type) {
      case 'deposit':
        return 'success';
      case 'withdraw':
        return 'error';
      case 'reserve':
        return 'warning';
      case 'release':
        return 'info';
      case 'consume':
        return 'secondary';
      default:
        return 'default';
    }
  };

  return (
    <Paper>
      <Box p={2} display="flex" justifyContent="space-between" alignItems="center">
        <Typography variant="h6">Transactions</Typography>
        <Box display="flex" gap={2}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Type</InputLabel>
            <Select
              value={selectedTypes}
              onChange={(event) => setSelectedTypes(event.target.value as GasTransactionType[])}
              label="Type"
              multiple
            >
              <MenuItem value="deposit">Deposit</MenuItem>
              <MenuItem value="withdraw">Withdraw</MenuItem>
              <MenuItem value="reserve">Reserve</MenuItem>
              <MenuItem value="release">Release</MenuItem>
              <MenuItem value="consume">Consume</MenuItem>
            </Select>
          </FormControl>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Status</InputLabel>
            <Select
              value={selectedStatuses}
              onChange={(event) => setSelectedStatuses(event.target.value as GasTransactionStatus[])}
              label="Status"
              multiple
            >
              <MenuItem value="pending">Pending</MenuItem>
              <MenuItem value="confirmed">Confirmed</MenuItem>
              <MenuItem value="failed">Failed</MenuItem>
              <MenuItem value="cancelled">Cancelled</MenuItem>
            </Select>
          </FormControl>
          <TextField
            label="Start Date"
            type="number"
            value={startDate}
            onChange={(event) => setStartDate(event.target.value)}
            InputProps={{
              endAdornment: <InputAdornment position="end">Block Number</InputAdornment>
            }}
          />
          <TextField
            label="End Date"
            type="number"
            value={endDate}
            onChange={(event) => setEndDate(event.target.value)}
            InputProps={{
              endAdornment: <InputAdornment position="end">Block Number</InputAdornment>
            }}
          />
        </Box>
      </Box>
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Time</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Amount</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Hash</TableCell>
              <TableCell>Fee</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {transactions.map((transaction) => (
              <TableRow key={transaction.id}>
                <TableCell>{formatDate(new Date(transaction.timestamp))}</TableCell>
                <TableCell>
                  <Chip
                    label={transaction.type}
                    size="small"
                    color={getTypeColor(transaction.type)}
                  />
                </TableCell>
                <TableCell>{formatGas(transaction.amount)}</TableCell>
                <TableCell>
                  <Chip
                    label={transaction.status}
                    size="small"
                    color={getStatusColor(transaction.status)}
                  />
                </TableCell>
                <TableCell>
                  {transaction.hash ? (
                    <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                      {transaction.hash.slice(0, 8)}...{transaction.hash.slice(-8)}
                    </Typography>
                  ) : (
                    '-'
                  )}
                </TableCell>
                <TableCell>
                  {transaction.fee ? formatGas(transaction.fee) : '-'}
                </TableCell>
                <TableCell align="right">
                  <Tooltip title="View Details">
                    <IconButton size="small">
                      <InfoIcon />
                    </IconButton>
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      <TablePagination
        component="div"
        count={totalTransactions}
        page={page}
        onPageChange={handleChangePage}
        rowsPerPage={rowsPerPage}
        onRowsPerPageChange={handleChangeRowsPerPage}
        rowsPerPageOptions={[5, 10, 25, 50]}
      />
    </Paper>
  );
}