import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  FormControlLabel,
  Switch,
  Box,
  Typography,
  Slider,
  Grid,
  Alert,
  InputAdornment
} from '@mui/material';
import { GasSettings } from '../types/types';
import { formatGas } from '../utils/formatters';

interface GasSettingsDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (settings: Partial<GasSettings>) => Promise<void>;
  currentSettings: GasSettings;
}

export function GasSettingsDialog({
  open,
  onClose,
  onSubmit,
  currentSettings
}: GasSettingsDialogProps) {
  const [settings, setSettings] = React.useState<GasSettings>(currentSettings);
  const [error, setError] = React.useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = React.useState(false);

  React.useEffect(() => {
    setSettings(currentSettings);
  }, [currentSettings]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    try {
      setIsSubmitting(true);
      await onSubmit(settings);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update settings');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClose = () => {
    setSettings(currentSettings);
    setError(null);
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>Gas Bank Settings</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2 }}>
            <Typography variant="h6" gutterBottom>
              Account Settings
            </Typography>
            
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <TextField
                  label="Minimum Balance"
                  type="number"
                  fullWidth
                  value={settings.minimumBalance}
                  onChange={(e) =>
                    setSettings({
                      ...settings,
                      minimumBalance: Number(e.target.value)
                    })
                  }
                  InputProps={{
                    endAdornment: <InputAdornment position="end">GAS</InputAdornment>
                  }}
                />
                <Typography variant="caption" color="text.secondary">
                  Minimum balance to maintain in each account
                </Typography>
              </Grid>

              <Grid item xs={12} md={6}>
                <TextField
                  label="Low Balance Threshold"
                  type="number"
                  fullWidth
                  value={settings.lowBalanceThreshold}
                  onChange={(e) =>
                    setSettings({
                      ...settings,
                      lowBalanceThreshold: Number(e.target.value)
                    })
                  }
                  InputProps={{
                    endAdornment: <InputAdornment position="end">GAS</InputAdornment>
                  }}
                />
                <Typography variant="caption" color="text.secondary">
                  Balance threshold for low balance warnings
                </Typography>
              </Grid>

              <Grid item xs={12}>
                <Typography gutterBottom>
                  Maximum Reservation Duration (hours)
                </Typography>
                <Slider
                  value={settings.maxReservationDuration / 3600000} // Convert from ms to hours
                  onChange={(_, value) =>
                    setSettings({
                      ...settings,
                      maxReservationDuration: (value as number) * 3600000
                    })
                  }
                  min={1}
                  max={72}
                  marks={[
                    { value: 1, label: '1h' },
                    { value: 24, label: '24h' },
                    { value: 48, label: '48h' },
                    { value: 72, label: '72h' }
                  ]}
                />
              </Grid>

              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.autoRefill}
                      onChange={(e) =>
                        setSettings({
                          ...settings,
                          autoRefill: e.target.checked
                        })
                      }
                    />
                  }
                  label="Auto-refill accounts when balance is low"
                />
              </Grid>

              {settings.autoRefill && (
                <Grid item xs={12} md={6}>
                  <TextField
                    label="Auto-refill Amount"
                    type="number"
                    fullWidth
                    value={settings.autoRefillAmount}
                    onChange={(e) =>
                      setSettings({
                        ...settings,
                        autoRefillAmount: Number(e.target.value)
                      })
                    }
                    InputProps={{
                      endAdornment: <InputAdornment position="end">GAS</InputAdornment>
                    }}
                  />
                  <Typography variant="caption" color="text.secondary">
                    Amount to add when auto-refilling
                  </Typography>
                </Grid>
              )}
            </Grid>

            <Typography variant="h6" gutterBottom sx={{ mt: 4 }}>
              Notification Settings
            </Typography>

            <Grid container spacing={3}>
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.emailNotifications}
                      onChange={(e) =>
                        setSettings({
                          ...settings,
                          emailNotifications: e.target.checked
                        })
                      }
                    />
                  }
                  label="Enable email notifications"
                />
              </Grid>

              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.slackNotifications}
                      onChange={(e) =>
                        setSettings({
                          ...settings,
                          slackNotifications: e.target.checked
                        })
                      }
                    />
                  }
                  label="Enable Slack notifications"
                />
              </Grid>
            </Grid>

            {error && (
              <Alert severity="error" sx={{ mt: 2 }}>
                {error}
              </Alert>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isSubmitting}
          >
            {isSubmitting ? 'Saving...' : 'Save Settings'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}