import React from 'react';
import {
  Box,
  Paper,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Chip,
  IconButton,
  Collapse,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Button
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/KeyboardArrowDown';
import ExpandLessIcon from '@mui/icons-material/KeyboardArrowUp';
import RefreshIcon from '@mui/icons-material/Refresh';
import { ApiError } from '../types/types';

interface ApiErrorLogsProps {
  errors: ApiError[];
  onFetchErrors: (options?: {
    startTime?: number;
    endTime?: number;
    endpoint?: string;
    limit?: number;
  }) => void;
}

export function ApiErrorLogs({
  errors,
  onFetchErrors
}: ApiErrorLogsProps) {
  const [expandedError, setExpandedError] = React.useState<string | null>(null);
  const [page, setPage] = React.useState(0);
  const [rowsPerPage, setRowsPerPage] = React.useState(10);
  const [timeRange, setTimeRange] = React.useState('1h');
  const [endpoint, setEndpoint] = React.useState('');

  const handleChangePage = (event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleTimeRangeChange = (event: any) => {
    const range = event.target.value;
    setTimeRange(range);
    const now = Date.now();
    let startTime: number;

    switch (range) {
      case '1h':
        startTime = now - 3600000;
        break;
      case '24h':
        startTime = now - 86400000;
        break;
      case '7d':
        startTime = now - 604800000;
        break;
      case '30d':
        startTime = now - 2592000000;
        break;
      default:
        startTime = now - 3600000;
    }

    onFetchErrors({
      startTime,
      endTime: now,
      endpoint: endpoint || undefined,
      limit: rowsPerPage
    });
  };

  const handleEndpointChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newEndpoint = event.target.value;
    setEndpoint(newEndpoint);
  };

  const handleRefresh = () => {
    const now = Date.now();
    let startTime: number;

    switch (timeRange) {
      case '1h':
        startTime = now - 3600000;
        break;
      case '24h':
        startTime = now - 86400000;
        break;
      case '7d':
        startTime = now - 604800000;
        break;
      case '30d':
        startTime = now - 2592000000;
        break;
      default:
        startTime = now - 3600000;
    }

    onFetchErrors({
      startTime,
      endTime: now,
      endpoint: endpoint || undefined,
      limit: rowsPerPage
    });
  };

  const getErrorColor = (code: string): 'error' | 'warning' | 'info' => {
    if (code.startsWith('5')) return 'error';
    if (code.startsWith('4')) return 'warning';
    return 'info';
  };

  return (
    <Box>
      <Box
        display="flex"
        justifyContent="space-between"
        alignItems="center"
        mb={3}
        gap={2}
      >
        <Typography variant="h6">Error Logs</Typography>
        <Box display="flex" gap={2}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Time Range</InputLabel>
            <Select
              value={timeRange}
              label="Time Range"
              onChange={handleTimeRangeChange}
            >
              <MenuItem value="1h">Last Hour</MenuItem>
              <MenuItem value="24h">Last 24 Hours</MenuItem>
              <MenuItem value="7d">Last 7 Days</MenuItem>
              <MenuItem value="30d">Last 30 Days</MenuItem>
            </Select>
          </FormControl>
          <TextField
            size="small"
            label="Filter by Endpoint"
            value={endpoint}
            onChange={handleEndpointChange}
            sx={{ minWidth: 200 }}
          />
          <Button
            startIcon={<RefreshIcon />}
            onClick={handleRefresh}
            variant="outlined"
          >
            Refresh
          </Button>
        </Box>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell padding="checkbox" />
              <TableCell>Timestamp</TableCell>
              <TableCell>Error Code</TableCell>
              <TableCell>Message</TableCell>
              <TableCell>Endpoint</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {errors
              .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
              .map((error) => (
                <React.Fragment key={error.timestamp}>
                  <TableRow
                    hover
                    sx={{
                      '&:last-child td, &:last-child th': { border: 0 },
                      cursor: 'pointer'
                    }}
                    onClick={() =>
                      setExpandedError(
                        expandedError === error.timestamp.toString()
                          ? null
                          : error.timestamp.toString()
                      )
                    }
                  >
                    <TableCell padding="checkbox">
                      <IconButton size="small">
                        {expandedError === error.timestamp.toString() ? (
                          <ExpandLessIcon />
                        ) : (
                          <ExpandMoreIcon />
                        )}
                      </IconButton>
                    </TableCell>
                    <TableCell>
                      {new Date(error.timestamp).toLocaleString()}
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={error.code}
                        color={getErrorColor(error.code)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>{error.message}</TableCell>
                    <TableCell>
                      <Typography
                        component="code"
                        sx={{
                          bgcolor: 'grey.100',
                          p: 0.5,
                          borderRadius: 1,
                          fontFamily: 'monospace'
                        }}
                      >
                        {error.details?.endpoint || 'N/A'}
                      </Typography>
                    </TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell
                      sx={{
                        paddingBottom: 0,
                        paddingTop: 0,
                        gridColumn: 'span 5'
                      }}
                    >
                      <Collapse
                        in={expandedError === error.timestamp.toString()}
                        timeout="auto"
                        unmountOnExit
                      >
                        <Box sx={{ margin: 2 }}>
                          <Typography variant="subtitle2" gutterBottom>
                            Error Details
                          </Typography>
                          <Paper
                            sx={{
                              p: 2,
                              bgcolor: 'grey.900',
                              color: 'grey.100',
                              fontFamily: 'monospace',
                              fontSize: '0.875rem'
                            }}
                          >
                            <Box component="pre" sx={{ m: 0 }}>
                              {JSON.stringify(error.details, null, 2)}
                            </Box>
                          </Paper>
                        </Box>
                      </Collapse>
                    </TableCell>
                  </TableRow>
                </React.Fragment>
              ))}
          </TableBody>
        </Table>
        <TablePagination
          rowsPerPageOptions={[5, 10, 25, 50]}
          component="div"
          count={errors.length}
          rowsPerPage={rowsPerPage}
          page={page}
          onPageChange={handleChangePage}
          onRowsPerPageChange={handleChangeRowsPerPage}
        />
      </TableContainer>
    </Box>
  );
}