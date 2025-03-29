import React from 'react';
import {
  Box,
  TextField,
  Switch,
  FormControlLabel,
  Button,
  FormHelperText,
  CircularProgress,
  Grid,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from '@mui/material';
import { PriceFeedConfigProps, ValidationError } from '../types/types';

export default function PriceFeedConfig({
  config,
  onConfigUpdate,
  onConfigReset,
  validationErrors,
  isLoading,
  className
}: PriceFeedConfigProps) {
  const getFieldError = (field: string): string => {
    const error = validationErrors.find((err) => err.field === field);
    return error ? error.message : '';
  };

  const handleNumberChange = (field: string) => (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const value = parseFloat(event.target.value);
    if (!isNaN(value)) {
      // Handle nested properties
      if (field.includes('.')) {
        const [parent, child] = field.split('.');
        const parentKey = parent as keyof typeof config;
        const parentValue = config[parentKey];
        
        if (parentValue && typeof parentValue === 'object') {
          onConfigUpdate({ 
            [parentKey]: { 
              ...(parentValue as Record<string, any>), 
              [child]: value 
            } 
          });
        }
      } else {
        onConfigUpdate({ [field as keyof typeof config]: value });
      }
    }
  };

  const handleSwitchChange = (field: keyof typeof config) => (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    onConfigUpdate({ [field]: event.target.checked });
  };

  const handleMethodChange = (event: React.ChangeEvent<{ value: unknown }>) => {
    onConfigUpdate({ updateMethod: event.target.value as 'auto' | 'manual' });
  };

  if (isLoading) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight={200}
        className={className}
      >
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box component="form" className={className}>
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <TextField
            fullWidth
            label="Heartbeat Interval (ms)"
            type="number"
            value={config.heartbeatInterval}
            onChange={handleNumberChange('heartbeatInterval')}
            error={!!getFieldError('heartbeatInterval')}
            helperText={getFieldError('heartbeatInterval')}
            InputProps={{
              endAdornment: <InputAdornment position="end">ms</InputAdornment>
            }}
          />
        </Grid>

        <Grid item xs={12}>
          <TextField
            fullWidth
            label="Deviation Threshold"
            type="number"
            value={config.deviationThreshold}
            onChange={handleNumberChange('deviationThreshold')}
            error={!!getFieldError('deviationThreshold')}
            helperText={getFieldError('deviationThreshold')}
            InputProps={{
              endAdornment: <InputAdornment position="end">%</InputAdornment>
            }}
          />
        </Grid>

        <Grid item xs={12}>
          <TextField
            fullWidth
            label="Minimum Source Count"
            type="number"
            value={config.minSourceCount}
            onChange={handleNumberChange('minSourceCount')}
            error={!!getFieldError('minSourceCount')}
            helperText={getFieldError('minSourceCount')}
          />
        </Grid>

        <Grid item xs={12}>
          <FormControl fullWidth>
            <InputLabel>Update Method</InputLabel>
            <Select
              value={config.updateMethod}
              onChange={handleMethodChange}
              label="Update Method"
            >
              <MenuItem value="auto">Automatic</MenuItem>
              <MenuItem value="manual">Manual</MenuItem>
            </Select>
          </FormControl>
        </Grid>

        {config.alertThresholds && (
          <>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Price Deviation Alert Threshold"
                type="number"
                value={config.alertThresholds.priceDeviation}
                onChange={handleNumberChange('alertThresholds.priceDeviation')}
                error={!!getFieldError('alertThresholds.priceDeviation')}
                helperText={getFieldError('alertThresholds.priceDeviation')}
                InputProps={{
                  endAdornment: <InputAdornment position="end">%</InputAdornment>
                }}
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Source Latency Alert Threshold"
                type="number"
                value={config.alertThresholds.sourceLatency}
                onChange={handleNumberChange('alertThresholds.sourceLatency')}
                error={!!getFieldError('alertThresholds.sourceLatency')}
                helperText={getFieldError('alertThresholds.sourceLatency')}
                InputProps={{
                  endAdornment: <InputAdornment position="end">ms</InputAdornment>
                }}
              />
            </Grid>
          </>
        )}

        <Grid item xs={12}>
          <FormControlLabel
            control={
              <Switch
                checked={config.isActive}
                onChange={handleSwitchChange('isActive')}
                color="primary"
              />
            }
            label="Active"
          />
        </Grid>

        <Grid item xs={12}>
          <Box display="flex" justifyContent="space-between">
            <Button
              variant="outlined"
              onClick={onConfigReset}
              disabled={isLoading}
            >
              Reset to Defaults
            </Button>
            <Button
              variant="contained"
              color="primary"
              onClick={() => onConfigUpdate(config)}
              disabled={isLoading || validationErrors.length > 0}
            >
              Save Changes
            </Button>
          </Box>
        </Grid>
      </Grid>
    </Box>
  );
}