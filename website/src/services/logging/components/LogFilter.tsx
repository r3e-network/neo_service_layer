import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Grid,
  Box,
  Typography,
  Chip,
  IconButton
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { LogFilter as LogFilterType, LogLevel, ServiceName } from '../types/types';

interface LogFilterProps {
  open: boolean;
  onClose: () => void;
  filter: LogFilterType;
  onFilterChange: (filter: Partial<LogFilterType>) => void;
}

const LOG_LEVELS: LogLevel[] = ['debug', 'info', 'warn', 'error', 'critical'];
const SERVICES: ServiceName[] = [
  'priceFeed',
  'gasBank',
  'trigger',
  'functions',
  'secrets',
  'api',
  'system'
];

export function LogFilter({
  open,
  onClose,
  filter,
  onFilterChange
}: LogFilterProps) {
  const handleLevelChange = (event: any) => {
    onFilterChange({ level: event.target.value });
  };

  const handleServiceChange = (event: any) => {
    onFilterChange({ service: event.target.value });
  };

  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    onFilterChange({ searchTerm: event.target.value });
  };

  const handleStartDateChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    onFilterChange({ startTime: new Date(event.target.value).getTime() });
  };

  const handleEndDateChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    onFilterChange({ endTime: new Date(event.target.value).getTime() });
  };

  const handleCorrelationIdChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    onFilterChange({ correlationId: event.target.value });
  };

  const handleRequestIdChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    onFilterChange({ requestId: event.target.value });
  };

  const handleClearFilter = () => {
    onFilterChange({
      level: undefined,
      service: undefined,
      startTime: undefined,
      endTime: undefined,
      searchTerm: undefined,
      correlationId: undefined,
      requestId: undefined
    });
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="h6">Filter Logs</Typography>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>
      <DialogContent>
        <Grid container spacing={2} sx={{ mt: 1 }}>
          <Grid item xs={12}>
            <TextField
              fullWidth
              label="Search"
              value={filter.searchTerm || ''}
              onChange={handleSearchChange}
              placeholder="Search in log messages..."
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <FormControl fullWidth>
              <InputLabel>Log Level</InputLabel>
              <Select
                multiple
                value={filter.level || []}
                onChange={handleLevelChange}
                renderValue={(selected) => (
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                    {(selected as LogLevel[]).map((value) => (
                      <Chip key={value} label={value} size="small" />
                    ))}
                  </Box>
                )}
              >
                {LOG_LEVELS.map((level) => (
                  <MenuItem key={level} value={level}>
                    {level}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} md={6}>
            <FormControl fullWidth>
              <InputLabel>Service</InputLabel>
              <Select
                multiple
                value={filter.service || []}
                onChange={handleServiceChange}
                renderValue={(selected) => (
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                    {(selected as ServiceName[]).map((value) => (
                      <Chip key={value} label={value} size="small" />
                    ))}
                  </Box>
                )}
              >
                {SERVICES.map((service) => (
                  <MenuItem key={service} value={service}>
                    {service}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} md={6}>
            <TextField
              fullWidth
              type="datetime-local"
              label="Start Time"
              value={filter.startTime ? new Date(filter.startTime).toISOString().slice(0, 16) : ''}
              onChange={handleStartDateChange}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <TextField
              fullWidth
              type="datetime-local"
              label="End Time"
              value={filter.endTime ? new Date(filter.endTime).toISOString().slice(0, 16) : ''}
              onChange={handleEndDateChange}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <TextField
              fullWidth
              label="Correlation ID"
              value={filter.correlationId || ''}
              onChange={handleCorrelationIdChange}
            />
          </Grid>
          <Grid item xs={12} md={6}>
            <TextField
              fullWidth
              label="Request ID"
              value={filter.requestId || ''}
              onChange={handleRequestIdChange}
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClearFilter} color="inherit">
          Clear Filters
        </Button>
        <Button onClick={onClose}>Cancel</Button>
        <Button onClick={onClose} variant="contained" color="primary">
          Apply Filters
        </Button>
      </DialogActions>
    </Dialog>
  );
}