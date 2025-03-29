import React from 'react';
import {
  List,
  ListItem,
  ListItemText,
  Chip,
  Box,
  Typography,
  TablePagination,
  Paper
} from '@mui/material';
import { LogEntry, LogFilter, LogLevel } from '../types/types';
import { formatDate } from '../utils/formatters';

interface LogListProps {
  logs: LogEntry[];
  onLogClick: (log: LogEntry) => void;
  filter: LogFilter;
  totalLogs: number;
  onFilterChange: (filter: Partial<LogFilter>) => void;
}

export function LogList({
  logs,
  onLogClick,
  filter,
  totalLogs,
  onFilterChange
}: LogListProps) {
  const handlePageChange = (event: unknown, newPage: number) => {
    onFilterChange({
      offset: newPage * (filter.limit || 25)
    });
  };

  const handleRowsPerPageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    onFilterChange({
      limit: parseInt(event.target.value, 10),
      offset: 0
    });
  };

  const getLevelColor = (level: LogLevel) => {
    switch (level) {
      case 'debug':
        return 'info';
      case 'info':
        return 'success';
      case 'warn':
        return 'warning';
      case 'error':
      case 'critical':
        return 'error';
      default:
        return 'default';
    }
  };

  return (
    <Paper sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <List sx={{ flex: 1, overflow: 'auto' }}>
        {logs.map((log) => (
          <ListItem
            key={log.id}
            onClick={() => onLogClick(log)}
            sx={{
              cursor: 'pointer',
              '&:hover': {
                backgroundColor: 'action.hover'
              },
              borderBottom: '1px solid',
              borderColor: 'divider'
            }}
          >
            <ListItemText
              primary={
                <Box display="flex" alignItems="center" gap={1}>
                  <Chip
                    label={log.level}
                    size="small"
                    color={getLevelColor(log.level)}
                  />
                  <Typography
                    component="span"
                    variant="body2"
                    color="text.secondary"
                  >
                    {formatDate(log.timestamp)}
                  </Typography>
                  <Chip
                    label={log.service}
                    size="small"
                    variant="outlined"
                  />
                  {log.correlationId && (
                    <Typography
                      component="span"
                      variant="body2"
                      color="text.secondary"
                    >
                      CID: {log.correlationId.slice(0, 8)}
                    </Typography>
                  )}
                </Box>
              }
              secondary={
                <Typography
                  variant="body1"
                  sx={{
                    mt: 0.5,
                    fontFamily: log.level === 'error' || log.level === 'critical'
                      ? 'monospace'
                      : 'inherit'
                  }}
                >
                  {log.message}
                </Typography>
              }
            />
          </ListItem>
        ))}
      </List>
      <TablePagination
        component="div"
        count={totalLogs}
        page={Math.floor((filter.offset || 0) / (filter.limit || 25))}
        onPageChange={handlePageChange}
        rowsPerPage={filter.limit || 25}
        onRowsPerPageChange={handleRowsPerPageChange}
        rowsPerPageOptions={[10, 25, 50, 100]}
      />
    </Paper>
  );
}