import React from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Tooltip,
  CircularProgress,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Stack,
  Chip,
  TextField,
  FormControl,
  InputLabel,
  Select,
  Alert,
  Grid,
  Toolbar
} from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import FilterListIcon from '@mui/icons-material/FilterList';
import DownloadIcon from '@mui/icons-material/Download';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import StopIcon from '@mui/icons-material/Stop';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';
import { LogEntry, LogFilter, LogStats as LogStatsType, LogLevel } from '../types/types';
import { LogFilter as LogFilterComponent } from './LogFilter';
import { LogList } from './LogList';
import { LogStats } from './LogStats';
import { useLogs } from '../hooks/useLogs';
import { formatDate } from '../utils/formatters';

export default function LogViewer() {
  const {
    logs,
    totalLogs,
    loading,
    error,
    stats,
    filter,
    updateFilter,
    fetchLogs,
    fetchStats,
    exportLogs,
    startLogStream,
    stopLogStream
  } = useLogs();

  const [isStreaming, setIsStreaming] = React.useState(false);
  const [showFilter, setShowFilter] = React.useState(false);
  const [selectedLog, setSelectedLog] = React.useState<LogEntry | null>(null);

  const handleRefresh = () => {
    fetchLogs();
    fetchStats();
  };

  const handleStreamToggle = () => {
    if (isStreaming) {
      stopLogStream();
    } else {
      startLogStream({
        level: filter.level,
        service: filter.service,
        correlationId: filter.correlationId,
        requestId: filter.requestId
      });
    }
    setIsStreaming(!isStreaming);
  };

  const handleExport = async (format: 'json' | 'csv') => {
    await exportLogs(format);
  };

  const handleLogClick = (log: LogEntry) => {
    setSelectedLog(log);
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
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Paper sx={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flex: 1 }}>
            Logs
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Tooltip title="Toggle Live Stream">
              <IconButton onClick={handleStreamToggle} color={isStreaming ? 'primary' : 'default'}>
                {isStreaming ? <StopIcon /> : <PlayArrowIcon />}
              </IconButton>
            </Tooltip>
            <Tooltip title="Refresh">
              <IconButton onClick={handleRefresh} disabled={isStreaming}>
                <RefreshIcon />
              </IconButton>
            </Tooltip>
            <Tooltip title="Filter">
              <IconButton onClick={() => setShowFilter(true)}>
                <FilterListIcon />
              </IconButton>
            </Tooltip>
            <Button
              startIcon={<DownloadIcon />}
              onClick={() => handleExport('json')}
              variant="outlined"
              size="small"
            >
              Export
            </Button>
          </Box>
        </Toolbar>

        {error && (
          <Alert severity="error" sx={{ mx: 2 }}>
            {error.message}
          </Alert>
        )}

        <Box sx={{ p: 2, display: 'flex', gap: 2 }}>
          <LogStats stats={stats} />
        </Box>

        <Box sx={{ flex: 1, overflow: 'auto' }}>
          {loading ? (
            <Box display="flex" justifyContent="center" p={4}>
              <CircularProgress />
            </Box>
          ) : (
            <LogList
              logs={logs}
              onLogClick={handleLogClick}
              filter={filter}
              totalLogs={totalLogs}
              onFilterChange={updateFilter}
            />
          )}
        </Box>
      </Paper>

      <LogFilterComponent
        open={showFilter}
        onClose={() => setShowFilter(false)}
        filter={filter}
        onFilterChange={updateFilter}
      />

      <Dialog
        open={!!selectedLog}
        onClose={() => setSelectedLog(null)}
        maxWidth="md"
        fullWidth
      >
        {selectedLog && (
          <>
            <DialogTitle>
              <Box display="flex" alignItems="center" gap={1}>
                <Chip
                  label={selectedLog.level}
                  size="small"
                  color={getLevelColor(selectedLog.level)}
                />
                <Typography variant="subtitle1">
                  {selectedLog.service}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {formatDate(selectedLog.timestamp)}
                </Typography>
              </Box>
            </DialogTitle>
            <DialogContent>
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <Typography variant="body1">
                    {selectedLog.message}
                  </Typography>
                </Grid>
                {selectedLog.correlationId && (
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="text.secondary">
                      Correlation ID: {selectedLog.correlationId}
                    </Typography>
                  </Grid>
                )}
                {selectedLog.requestId && (
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="text.secondary">
                      Request ID: {selectedLog.requestId}
                    </Typography>
                  </Grid>
                )}
                {selectedLog.stackTrace && (
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" gutterBottom>
                      Stack Trace:
                    </Typography>
                    <Paper
                      variant="outlined"
                      sx={{
                        p: 1,
                        backgroundColor: 'grey.100',
                        fontFamily: 'monospace',
                        whiteSpace: 'pre-wrap',
                        overflow: 'auto',
                        maxHeight: 200
                      }}
                    >
                      {selectedLog.stackTrace}
                    </Paper>
                  </Grid>
                )}
                {Object.entries(selectedLog.metadata).length > 0 && (
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" gutterBottom>
                      Metadata:
                    </Typography>
                    <Paper
                      variant="outlined"
                      sx={{
                        p: 1,
                        backgroundColor: 'grey.100',
                        fontFamily: 'monospace',
                        whiteSpace: 'pre-wrap'
                      }}
                    >
                      {JSON.stringify(selectedLog.metadata, null, 2)}
                    </Paper>
                  </Grid>
                )}
              </Grid>
            </DialogContent>
            <DialogActions>
              <Button onClick={() => setSelectedLog(null)}>Close</Button>
            </DialogActions>
          </>
        )}
      </Dialog>
    </Box>
  );
}