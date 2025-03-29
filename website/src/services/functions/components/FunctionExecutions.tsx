import React from 'react';
import {
  Box,
  Paper,
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
  Typography,
  Button
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/KeyboardArrowDown';
import ExpandLessIcon from '@mui/icons-material/KeyboardArrowUp';
import RefreshIcon from '@mui/icons-material/Refresh';
import { UserFunction, FunctionExecution, ExecutionStatus } from '../types/types';

interface FunctionExecutionsProps {
  executions: FunctionExecution[];
  totalExecutions: number;
  functionId: string;
  onFetchExecutions: (
    id: string,
    options?: { limit?: number; offset?: number; status?: string }
  ) => void;
  onRefresh: () => void;
}

const statusColors: Record<ExecutionStatus, 'success' | 'error' | 'warning' | 'default'> = {
  'completed': 'success',
  'failed': 'error',
  'running': 'warning',
  'pending': 'default',
  'timeout': 'error',
  'cancelled': 'default'
};

export function FunctionExecutions({
  executions,
  totalExecutions,
  functionId,
  onFetchExecutions,
  onRefresh
}: FunctionExecutionsProps) {
  const [page, setPage] = React.useState(0);
  const [rowsPerPage, setRowsPerPage] = React.useState(10);
  const [expandedRow, setExpandedRow] = React.useState<string | null>(null);

  const handleChangePage = (event: unknown, newPage: number) => {
    setPage(newPage);
    onFetchExecutions(functionId, {
      offset: newPage * rowsPerPage,
      limit: rowsPerPage
    });
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newRowsPerPage = parseInt(event.target.value, 10);
    setRowsPerPage(newRowsPerPage);
    setPage(0);
    onFetchExecutions(functionId, {
      offset: 0,
      limit: newRowsPerPage
    });
  };

  const handleRefresh = () => {
    onFetchExecutions(functionId, {
      offset: page * rowsPerPage,
      limit: rowsPerPage
    });
  };

  const formatDuration = (start: number, end: number) => {
    const duration = end - start;
    if (duration < 1000) return `${duration}ms`;
    return `${(duration / 1000).toFixed(2)}s`;
  };

  return (
    <Box>
      <Box display="flex" justifyContent="flex-end" mb={2}>
        <Button
          startIcon={<RefreshIcon />}
          onClick={handleRefresh}
        >
          Refresh
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell padding="checkbox" />
              <TableCell>Execution ID</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Started</TableCell>
              <TableCell>Duration</TableCell>
              <TableCell>Memory Usage</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {executions.map((execution) => (
              <React.Fragment key={execution.id}>
                <TableRow
                  hover
                  sx={{
                    '&:last-child td, &:last-child th': { border: 0 },
                    cursor: 'pointer'
                  }}
                >
                  <TableCell padding="checkbox">
                    <IconButton
                      size="small"
                      onClick={() => setExpandedRow(
                        expandedRow === execution.id ? null : execution.id
                      )}
                    >
                      {expandedRow === execution.id ? (
                        <ExpandLessIcon />
                      ) : (
                        <ExpandMoreIcon />
                      )}
                    </IconButton>
                  </TableCell>
                  <TableCell component="th" scope="row">
                    {execution.id}
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={execution.status.toLowerCase()}
                      color={statusColors[execution.status as keyof typeof statusColors]}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>
                    {new Date(execution.startTime).toLocaleString()}
                  </TableCell>
                  <TableCell>
                    {execution.endTime
                      ? formatDuration(execution.startTime, execution.endTime)
                      : '-'}
                  </TableCell>
                  <TableCell>
                    {execution.metrics && execution.metrics.memory
                      ? `${(execution.metrics.memory / 1024 / 1024).toFixed(2)} MB`
                      : '-'}
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell
                    sx={{ 
                      paddingBottom: 0, 
                      paddingTop: 0,
                      gridColumn: 'span 6'
                    }}
                  >
                    <Collapse
                      in={expandedRow === execution.id}
                      timeout="auto"
                      unmountOnExit
                    >
                      <Box sx={{ margin: 2 }}>
                        <Typography variant="h6" gutterBottom component="div">
                          Execution Details
                        </Typography>
                        <Box
                          sx={{
                            display: 'grid',
                            gridTemplateColumns: 'repeat(2, 1fr)',
                            gap: 2
                          }}
                        >
                          <Paper sx={{ p: 2 }}>
                            <Typography variant="subtitle2" gutterBottom>
                              Parameters
                            </Typography>
                            <pre
                              style={{
                                margin: 0,
                                whiteSpace: 'pre-wrap',
                                wordBreak: 'break-word'
                              }}
                            >
                              {execution.result && typeof execution.result === 'object' 
                                ? JSON.stringify(execution.result.parameters || {}, null, 2)
                                : '{}'}
                            </pre>
                          </Paper>
                          <Paper sx={{ p: 2 }}>
                            <Typography variant="subtitle2" gutterBottom>
                              Result
                            </Typography>
                            <pre
                              style={{
                                margin: 0,
                                whiteSpace: 'pre-wrap',
                                wordBreak: 'break-word'
                              }}
                            >
                              {execution.result 
                                ? (typeof execution.result === 'object'
                                   ? JSON.stringify(execution.result.data || execution.result, null, 2)
                                   : String(execution.result))
                                : execution.error || 'No result'}
                            </pre>
                          </Paper>
                        </Box>
                        {execution.error && (
                          <Paper sx={{ p: 2, mt: 2, bgcolor: '#ffebee' }}>
                            <Typography variant="subtitle2" color="error" gutterBottom>
                              Error
                            </Typography>
                            <pre
                              style={{
                                margin: 0,
                                whiteSpace: 'pre-wrap',
                                wordBreak: 'break-word',
                                color: '#d32f2f'
                              }}
                            >
                              {execution.error}
                            </pre>
                          </Paper>
                        )}
                      </Box>
                    </Collapse>
                  </TableCell>
                </TableRow>
              </React.Fragment>
            ))}
          </TableBody>
        </Table>
        <TablePagination
          rowsPerPageOptions={[5, 10, 25]}
          component="div"
          count={totalExecutions}
          rowsPerPage={rowsPerPage}
          page={page}
          onPageChange={handleChangePage}
          onRowsPerPageChange={handleChangeRowsPerPage}
        />
      </TableContainer>
    </Box>
  );
}