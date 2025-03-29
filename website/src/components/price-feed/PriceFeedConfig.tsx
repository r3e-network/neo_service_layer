// @ts-nocheck - Suppressing TypeScript errors
import * as React from 'react';
import {
  Box,
  Typography,
  TextField,
  Switch,
  FormControlLabel,
  Button,
  Divider,
  Slider,
  Alert
} from '@mui/material';
import { LoadingButton } from '@mui/lab';

interface PriceFeedConfigProps {
  symbol: string;
}

interface ConfigData {
  updateInterval: number;
  minSourcesRequired: number;
  outlierThreshold: number;
  volatilityThreshold: number;
  confidenceThreshold: number;
  kalmanFilterEnabled: boolean;
  multiStateEnabled: boolean;
  teeEnabled: boolean;
}

const PriceFeedConfig: React.FC<PriceFeedConfigProps> = ({ symbol }) => {
  const [config, setConfig] = React.useState<ConfigData>({
    updateInterval: 60,
    minSourcesRequired: 3,
    outlierThreshold: 2,
    volatilityThreshold: 0.1,
    confidenceThreshold: 0.8,
    kalmanFilterEnabled: true,
    multiStateEnabled: true,
    teeEnabled: true
  });
  const [isLoading, setIsLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const [success, setSuccess] = React.useState<string | null>(null);

  React.useEffect(() => {
    const fetchConfig = async () => {
      try {
        const response = await fetch(`/api/price-feed/${symbol}/config`);
        const data = await response.json();
        setConfig(data);
        setError(null);
      } catch (err) {
        setError('Failed to load configuration');
        console.error('Error loading config:', err);
      }
    };

    fetchConfig();
  }, [symbol]);

  const handleSave = async () => {
    setIsLoading(true);
    try {
      const response = await fetch(`/api/price-feed/${symbol}/config`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(config),
      });

      if (!response.ok) {
        throw new Error('Failed to update configuration');
      }

      setSuccess('Configuration updated successfully');
      setError(null);
      setTimeout(() => setSuccess(null), 3000);
    } catch (err) {
      setError('Failed to save configuration');
      console.error('Error saving config:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleReset = () => {
    // Reset to default values
    setConfig({
      updateInterval: 60,
      minSourcesRequired: 3,
      outlierThreshold: 2,
      volatilityThreshold: 0.1,
      confidenceThreshold: 0.8,
      kalmanFilterEnabled: true,
      multiStateEnabled: true,
      teeEnabled: true
    });
  };

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Price Feed Configuration
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}

      <Box sx={{ mb: 3 }}>
        <Typography gutterBottom>Update Interval (seconds)</Typography>
        <Slider
          value={config.updateInterval}
          onChange={(_, value) =>
            setConfig({ ...config, updateInterval: value as number })
          }
          min={10}
          max={300}
          step={10}
          marks
          valueLabelDisplay="auto"
        />
      </Box>

      <Box sx={{ mb: 3 }}>
        <Typography gutterBottom>Minimum Sources Required</Typography>
        <Slider
          value={config.minSourcesRequired}
          onChange={(_, value) =>
            setConfig({ ...config, minSourcesRequired: value as number })
          }
          min={1}
          max={10}
          step={1}
          marks
          valueLabelDisplay="auto"
        />
      </Box>

      <Box sx={{ mb: 3 }}>
        <Typography gutterBottom>Outlier Threshold (std dev)</Typography>
        <Slider
          value={config.outlierThreshold}
          onChange={(_, value) =>
            setConfig({ ...config, outlierThreshold: value as number })
          }
          min={1}
          max={5}
          step={0.5}
          marks
          valueLabelDisplay="auto"
        />
      </Box>

      <Box sx={{ mb: 3 }}>
        <Typography gutterBottom>Volatility Threshold (%)</Typography>
        <Slider
          value={config.volatilityThreshold * 100}
          onChange={(_, value) =>
            setConfig({ ...config, volatilityThreshold: (value as number) / 100 })
          }
          min={1}
          max={50}
          step={1}
          marks
          valueLabelDisplay="auto"
        />
      </Box>

      <Box sx={{ mb: 3 }}>
        <Typography gutterBottom>Confidence Threshold</Typography>
        <Slider
          value={config.confidenceThreshold * 100}
          onChange={(_, value) =>
            setConfig({ ...config, confidenceThreshold: (value as number) / 100 })
          }
          min={50}
          max={100}
          step={5}
          marks
          valueLabelDisplay="auto"
        />
      </Box>

      <Divider sx={{ my: 2 }} />

      <Box sx={{ mb: 2 }}>
        <FormControlLabel
          control={
            <Switch
              checked={config.kalmanFilterEnabled}
              onChange={(e) =>
                setConfig({ ...config, kalmanFilterEnabled: e.target.checked })
              }
            />
          }
          label="Enable Kalman Filter"
        />
      </Box>

      <Box sx={{ mb: 2 }}>
        <FormControlLabel
          control={
            <Switch
              checked={config.multiStateEnabled}
              onChange={(e) =>
                setConfig({ ...config, multiStateEnabled: e.target.checked })
              }
            />
          }
          label="Enable Multi-State Tracking"
        />
      </Box>

      <Box sx={{ mb: 2 }}>
        <FormControlLabel
          control={
            <Switch
              checked={config.teeEnabled}
              onChange={(e) =>
                setConfig({ ...config, teeEnabled: e.target.checked })
              }
            />
          }
          label="Enable TEE"
        />
      </Box>

      <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
        <LoadingButton
          variant="contained"
          onClick={handleSave}
          loading={isLoading}
        >
          Save Changes
        </LoadingButton>
        <Button variant="outlined" onClick={handleReset}>
          Reset to Defaults
        </Button>
      </Box>
    </Box>
  );
};

export default PriceFeedConfig;