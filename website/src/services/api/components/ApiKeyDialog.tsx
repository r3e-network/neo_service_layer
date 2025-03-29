import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box,
  Typography,
  Chip,
  Alert,
  FormControlLabel,
  Switch,
  Autocomplete
} from '@mui/material';
import { ApiKey, ApiEndpoint } from '../types/types';

interface ApiKeyDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (
    name: string,
    options: {
      allowedEndpoints?: string[];
      allowedIPs?: string[];
      expiresAt?: number;
      rateLimit?: {
        requestsPerMinute: number;
        burstLimit: number;
      };
    }
  ) => Promise<void>;
  apiKey?: ApiKey;
  endpoints?: ApiEndpoint[];
}

export function ApiKeyDialog({
  open,
  onClose,
  onSubmit,
  apiKey,
  endpoints = []
}: ApiKeyDialogProps) {
  const [name, setName] = React.useState(apiKey?.name || '');
  const [selectedEndpoints, setSelectedEndpoints] = React.useState<string[]>(
    apiKey?.allowedEndpoints || []
  );
  const [allowedIPs, setAllowedIPs] = React.useState<string>(
    apiKey?.allowedIPs?.join(', ') || ''
  );
  const [expiresAt, setExpiresAt] = React.useState<string>(
    apiKey?.expiresAt ? new Date(apiKey.expiresAt).toISOString().slice(0, 16) : ''
  );
  const [enableRateLimit, setEnableRateLimit] = React.useState(
    !!apiKey?.rateLimit
  );
  const [requestsPerMinute, setRequestsPerMinute] = React.useState(
    apiKey?.rateLimit?.requestsPerMinute || 60
  );
  const [burstLimit, setBurstLimit] = React.useState(
    apiKey?.rateLimit?.burstLimit || 100
  );
  const [error, setError] = React.useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = React.useState(false);

  React.useEffect(() => {
    if (open) {
      setName(apiKey?.name || '');
      setSelectedEndpoints(apiKey?.allowedEndpoints || []);
      setAllowedIPs(apiKey?.allowedIPs?.join(', ') || '');
      setExpiresAt(apiKey?.expiresAt ? new Date(apiKey.expiresAt).toISOString().slice(0, 16) : '');
      setEnableRateLimit(!!apiKey?.rateLimit);
      setRequestsPerMinute(apiKey?.rateLimit?.requestsPerMinute || 60);
      setBurstLimit(apiKey?.rateLimit?.burstLimit || 100);
      setError(null);
      setIsSubmitting(false);
    }
  }, [open, apiKey]);

  const handleSubmit = async () => {
    if (!name.trim()) {
      setError('API key name is required');
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);

      const options: Parameters<typeof onSubmit>[1] = {
        allowedEndpoints: selectedEndpoints.length > 0 ? selectedEndpoints : undefined,
        allowedIPs: allowedIPs
          ? allowedIPs.split(',').map((ip) => ip.trim())
          : undefined,
        expiresAt: expiresAt ? new Date(expiresAt).getTime() : undefined,
        rateLimit: enableRateLimit
          ? {
              requestsPerMinute,
              burstLimit
            }
          : undefined
      };

      await onSubmit(name, options);
      handleClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save API key');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClose = () => {
    setName('');
    setSelectedEndpoints([]);
    setAllowedIPs('');
    setExpiresAt('');
    setEnableRateLimit(false);
    setRequestsPerMinute(60);
    setBurstLimit(100);
    setError(null);
    setIsSubmitting(false);
    onClose();
  };

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth="sm"
      fullWidth
    >
      <DialogTitle>
        {apiKey ? 'Edit API Key' : 'Create New API Key'}
      </DialogTitle>
      <DialogContent>
        <Box sx={{ pt: 2, display: 'flex', flexDirection: 'column', gap: 3 }}>
          {error && (
            <Alert severity="error" onClose={() => setError(null)}>
              {error}
            </Alert>
          )}

          <TextField
            label="API Key Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            fullWidth
            required
          />

          <Autocomplete
            multiple
            options={endpoints.map((endpoint) => endpoint.path)}
            value={selectedEndpoints}
            onChange={(event, newValue) => {
              setSelectedEndpoints(newValue);
            }}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Allowed Endpoints"
                placeholder="Select endpoints"
              />
            )}
            renderTags={(value, getTagProps) =>
              value.map((option, index) => (
                <Chip
                  variant="outlined"
                  label={option}
                  size="small"
                  {...getTagProps({ index })}
                />
              ))
            }
          />

          <TextField
            label="Allowed IP Addresses"
            value={allowedIPs}
            onChange={(e) => setAllowedIPs(e.target.value)}
            fullWidth
            helperText="Comma-separated list of IP addresses or CIDR ranges"
          />

          <TextField
            label="Expires At"
            type="datetime-local"
            value={expiresAt}
            onChange={(e) => setExpiresAt(e.target.value)}
            fullWidth
            helperText="Leave empty for no expiration"
          />

          <Box>
            <FormControlLabel
              control={
                <Switch
                  checked={enableRateLimit}
                  onChange={(e) => setEnableRateLimit(e.target.checked)}
                />
              }
              label="Enable Rate Limiting"
            />

            {enableRateLimit && (
              <Box sx={{ mt: 2, display: 'flex', gap: 2 }}>
                <TextField
                  label="Requests per Minute"
                  type="number"
                  value={requestsPerMinute}
                  onChange={(e) =>
                    setRequestsPerMinute(parseInt(e.target.value, 10))
                  }
                  inputProps={{ min: 1 }}
                  fullWidth
                />
                <TextField
                  label="Burst Limit"
                  type="number"
                  value={burstLimit}
                  onChange={(e) => setBurstLimit(parseInt(e.target.value, 10))}
                  inputProps={{ min: 1 }}
                  fullWidth
                />
              </Box>
            )}
          </Box>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={isSubmitting}
        >
          {apiKey ? 'Save Changes' : 'Create Key'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}