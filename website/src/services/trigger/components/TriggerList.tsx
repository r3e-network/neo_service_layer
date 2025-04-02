// @ts-ignore
import * as React from 'react';
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
  IconButton,
  Chip,
  Typography,
  Tooltip,
  CircularProgress,
  Collapse
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import PowerSettingsNewIcon from '@mui/icons-material/PowerSettingsNew';
import { formatDuration, formatPercentage } from '../utils/formatters';
import { Trigger, TriggerExecution } from '../types/types';
import { TRIGGER_CONSTANTS } from '../constants';

interface TriggerListProps {
  triggers: Trigger[];
  executions?: Record<string, TriggerExecution[]>;
  loading: boolean;
  onEdit: (trigger: Trigger) => void;
  onDelete: (triggerId: string) => void;
  onToggleStatus: (triggerId: string, active: boolean) => void;
}

export default function TriggerList({
  triggers,
  executions,
  loading,
  onEdit,
  onDelete,
  onToggleStatus
}: TriggerListProps) {
  const [page, setPage] = React.useState(0);
  const [rowsPerPage, setRowsPerPage] = React.useState(10);
  const [expandedRow, setExpandedRow] = React.useState<string | null>(null);

  const handleChangePage = (event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleExpandRow = (triggerId: string) => {
    setExpandedRow(expandedRow === triggerId ? null : triggerId);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case TRIGGER_CONSTANTS.TRIGGER_STATUS.ACTIVE:
        return 'success';
      case TRIGGER_CONSTANTS.TRIGGER_STATUS.PAUSED:
        return 'warning';
      case TRIGGER_CONSTANTS.TRIGGER_STATUS.FAILED:
        return 'error';
      default:
        return 'default';
    }
  };

  const LoadingSkeleton = () => (
    <TableRow>
      <TableCell sx={{ gridColumn: 'span 1' }}><CircularProgress size={20} /></TableCell>
      <TableCell sx={{ gridColumn: 'span 1' }}><CircularProgress size={20} /></TableCell>
      <TableCell sx={{ gridColumn: 'span 1' }}><CircularProgress size={20} /></TableCell>
      <TableCell sx={{ gridColumn: 'span 1' }}><CircularProgress size={20} /></TableCell>
      <TableCell sx={{ gridColumn: 'span 1' }}><CircularProgress size={20} /></TableCell>
      <TableCell sx={{ gridColumn: 'span 1' }}><CircularProgress size={20} /></TableCell>
      <TableCell sx={{ gridColumn: 'span 1' }}><CircularProgress size={20} /></TableCell>
    </TableRow>
  );

  const renderExpandedRow = (trigger: Trigger) => {
    // Get executions for this trigger
    const triggerExecutions = executions?.[trigger.id] || [];
    
    return (
      <TableRow>
        <TableCell sx={{ 
          padding: 0, 
          gridColumn: 'span 7'
        }}>
          <Collapse in={expandedRow === trigger.id} timeout="auto" unmountOnExit>
            <Box sx={{ margin: 1 }}>
              <Typography variant="h6" gutterBottom component="div">
                Execution History
              </Typography>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ gridColumn: 'span 1' }}>Status</TableCell>
                    <TableCell sx={{ gridColumn: 'span 1' }}>Start Time</TableCell>
                    <TableCell sx={{ gridColumn: 'span 1' }}>End Time</TableCell>
                    <TableCell sx={{ gridColumn: 'span 1' }}>Duration</TableCell>
                    <TableCell sx={{ gridColumn: 'span 1' }}>Result</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {triggerExecutions.length > 0 ? (
                    triggerExecutions.map((execution) => (
                      <TableRow key={execution.id}>
                        <TableCell sx={{ gridColumn: 'span 1' }}>
                          <Chip
                            label={execution.status}
                            size="small"
                            color={getStatusColor(execution.status) as any}
                          />
                        </TableCell>
                        <TableCell sx={{ gridColumn: 'span 1' }}>{new Date(execution.startTime).toLocaleString()}</TableCell>
                        <TableCell sx={{ gridColumn: 'span 1' }}>{execution.endTime ? new Date(execution.endTime).toLocaleString() : '-'}</TableCell>
                        <TableCell sx={{ gridColumn: 'span 1' }}>
                          {execution.endTime && execution.startTime
                            ? formatDuration(execution.endTime - execution.startTime)
                            : '-'}
                        </TableCell>
                        <TableCell sx={{ gridColumn: 'span 1' }}>
                          <Tooltip title={execution.result ? JSON.stringify(execution.result) : ''}>
                            <Typography
                              variant="body2"
                              sx={{
                                maxWidth: 200,
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                whiteSpace: 'nowrap'
                              }}
                            >
                              {execution.result ? JSON.stringify(execution.result).substring(0, 50) + '...' : '-'}
                            </Typography>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))
                  ) : (
                    <TableRow>
                      <TableCell sx={{ gridColumn: 'span 5', textAlign: 'center' }}>
                        <Typography variant="body2" color="text.secondary">
                          No execution history available
                        </Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </Box>
          </Collapse>
        </TableCell>
      </TableRow>
    );
  };

  return (
    <Paper sx={{ width: '100%', mb: 2 }}>
      <TableContainer>
        <Table size="medium">
          <TableHead>
            <TableRow>
              <TableCell sx={{ gridColumn: 'span 1' }} />
              <TableCell sx={{ gridColumn: 'span 1' }}>Name</TableCell>
              <TableCell sx={{ gridColumn: 'span 1' }}>Type</TableCell>
              <TableCell sx={{ gridColumn: 'span 1' }}>Status</TableCell>
              <TableCell sx={{ gridColumn: 'span 1' }}>Last Execution</TableCell>
              <TableCell sx={{ gridColumn: 'span 1' }}>Success Rate</TableCell>
              <TableCell sx={{ gridColumn: 'span 1', textAlign: 'right' }}>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              Array.from(Array(3)).map((_, index) => (
                <LoadingSkeleton key={index} />
              ))
            ) : triggers.length === 0 ? (
              <TableRow>
                <TableCell sx={{ 
                  padding: '16px', 
                  textAlign: 'center',
                  gridColumn: 'span 7'
                }}>
                  <Box sx={{ py: 3 }}>
                    <PowerSettingsNewIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
                    <Typography variant="h6" color="text.secondary">
                      No triggers found
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Create a new trigger to get started
                    </Typography>
                  </Box>
                </TableCell>
              </TableRow>
            ) : (
              triggers
                .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
                .map((trigger) => (
                  <React.Fragment key={trigger.id}>
                    <TableRow hover>
                      <TableCell sx={{ gridColumn: 'span 1', padding: 'none' }}>
                        <IconButton
                          size="small"
                          onClick={() => handleExpandRow(trigger.id)}
                        >
                          {expandedRow === trigger.id ? (
                            <ExpandLessIcon />
                          ) : (
                            <ExpandMoreIcon />
                          )}
                        </IconButton>
                      </TableCell>
                      <TableCell sx={{ gridColumn: 'span 1' }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <PowerSettingsNewIcon fontSize="small" />
                          {trigger.name}
                        </Box>
                      </TableCell>
                      <TableCell sx={{ gridColumn: 'span 1' }}>
                        <Chip
                          label={trigger.condition.type}
                          size="small"
                          color="primary"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell sx={{ gridColumn: 'span 1' }}>
                        <Chip
                          label={trigger.status}
                          size="small"
                          color={getStatusColor(trigger.status) as any}
                        />
                      </TableCell>
                      <TableCell sx={{ gridColumn: 'span 1' }}>
                        {trigger.lastExecutedAt ? trigger.lastExecutedAt : 'Never'}
                      </TableCell>
                      <TableCell sx={{ gridColumn: 'span 1' }}>
                        {trigger.executionCount > 0
                          ? formatPercentage(
                              ((trigger.executionCount - (trigger.failureCount || 0)) / trigger.executionCount)
                            )
                          : 'N/A'}
                      </TableCell>
                      <TableCell sx={{ gridColumn: 'span 1', textAlign: 'right' }}>
                        <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                          <Tooltip title="Edit Trigger">
                            <IconButton size="small" onClick={() => onEdit(trigger)}>
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title={trigger.status === TRIGGER_CONSTANTS.TRIGGER_STATUS.ACTIVE ? 'Pause Trigger' : 'Activate Trigger'}>
                            <IconButton
                              size="small"
                              onClick={() => onToggleStatus(trigger.id, trigger.status !== TRIGGER_CONSTANTS.TRIGGER_STATUS.ACTIVE)}
                              color={trigger.status === TRIGGER_CONSTANTS.TRIGGER_STATUS.ACTIVE ? 'warning' : 'success'}
                            >
                              {trigger.status === TRIGGER_CONSTANTS.TRIGGER_STATUS.ACTIVE ? (
                                <PowerSettingsNewIcon fontSize="small" />
                              ) : (
                                <PowerSettingsNewIcon fontSize="small" />
                              )}
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete Trigger">
                            <IconButton
                              size="small"
                              onClick={() => onDelete(trigger.id)}
                              color="error"
                            >
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </Box>
                      </TableCell>
                    </TableRow>
                    {renderExpandedRow(trigger)}
                  </React.Fragment>
                ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
      <TablePagination
        rowsPerPageOptions={[5, 10, 25]}
        component="div"
        count={triggers.length}
        rowsPerPage={rowsPerPage}
        page={page}
        onPageChange={handleChangePage}
        onRowsPerPageChange={handleChangeRowsPerPage}
      />
    </Paper>
  );
}