import React from 'react';
import {
  Box,
  Button,
  Grid,
  Paper,
  Typography,
  Alert,
  Snackbar
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import KeyIcon from '@mui/icons-material/Key';
import SecretMetricsCard from './SecretMetricsCard';
import SecretFilterBar from './SecretFilterBar';
import SecretsList from './SecretsList';
import SecretDialog from './SecretDialog';
import { useSecrets } from '../hooks/useSecrets';
import { Secret, SecretFilter } from '../types/types';

export default function SecretsDashboard() {
  const {
    secrets,
    metrics,
    loading,
    error,
    createSecret,
    updateSecret,
    deleteSecret,
    rotateSecret
  } = useSecrets();

  const [filter, setFilter] = React.useState<SecretFilter>({});
  const [dialogState, setDialogState] = React.useState<{
    open: boolean;
    mode: 'create' | 'edit' | 'view';
    secret?: Secret;
  }>({
    open: false,
    mode: 'create'
  });
  const [snackbar, setSnackbar] = React.useState<{
    open: boolean;
    message: string;
    severity: 'success' | 'error';
  }>({
    open: false,
    message: '',
    severity: 'success'
  });

  const handleCreateSecret = () => {
    setDialogState({
      open: true,
      mode: 'create'
    });
  };

  const handleViewSecret = (secret: Secret) => {
    setDialogState({
      open: true,
      mode: 'view',
      secret
    });
  };

  const handleEditSecret = (secret: Secret) => {
    setDialogState({
      open: true,
      mode: 'edit',
      secret
    });
  };

  const handleDeleteSecret = async (secret: Secret) => {
    try {
      await deleteSecret(secret.id);
      setSnackbar({
        open: true,
        message: 'Secret deleted successfully',
        severity: 'success'
      });
    } catch (error) {
      setSnackbar({
        open: true,
        message: 'Failed to delete secret',
        severity: 'error'
      });
    }
  };

  const handleRotateSecret = async (secret: Secret) => {
    try {
      await rotateSecret(secret.id);
      setSnackbar({
        open: true,
        message: 'Secret rotated successfully',
        severity: 'success'
      });
    } catch (error) {
      setSnackbar({
        open: true,
        message: 'Failed to rotate secret',
        severity: 'error'
      });
    }
  };

  const handleDialogSave = async (secretData: Partial<Secret>) => {
    try {
      if (dialogState.mode === 'create') {
        await createSecret(secretData);
        setSnackbar({
          open: true,
          message: 'Secret created successfully',
          severity: 'success'
        });
      } else if (dialogState.mode === 'edit' && dialogState.secret) {
        await updateSecret(dialogState.secret.id, secretData);
        setSnackbar({
          open: true,
          message: 'Secret updated successfully',
          severity: 'success'
        });
      }
      setDialogState({ open: false, mode: 'create' });
    } catch (error) {
      setSnackbar({
        open: true,
        message: 'Failed to save secret',
        severity: 'error'
      });
    }
  };

  const handleDialogClose = () => {
    setDialogState({ open: false, mode: 'create' });
  };

  const handleSnackbarClose = () => {
    setSnackbar({ ...snackbar, open: false });
  };

  // Filter secrets based on current filter state
  const filteredSecrets = secrets.filter(secret => {
    if (filter.search && !secret.name.toLowerCase().includes(filter.search.toLowerCase())) {
      return false;
    }
    if (filter.type?.length && !filter.type.includes(secret.type)) {
      return false;
    }
    if (filter.rotationStatus && secret.rotationStatus !== filter.rotationStatus) {
      return false;
    }
    if (filter.tags?.length && !filter.tags.some(tag => secret.tags.includes(tag))) {
      return false;
    }
    return true;
  });

  return (
    <Box sx={{ p: 3 }}>
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      <Box sx={{ mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <KeyIcon sx={{ fontSize: 32, color: 'primary.main' }} />
            <Typography variant="h4" component="h1">
              Secrets Management
            </Typography>
          </Box>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleCreateSecret}
          >
            Create Secret
          </Button>
        </Box>

        <Grid container spacing={3}>
          <Grid item xs={12} sm={6} md={3}>
            <SecretMetricsCard
              title="Total Secrets"
              value={metrics.totalSecrets}
              type="total"
              loading={loading}
            />
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <SecretMetricsCard
              title="Needs Rotation"
              value={metrics.needsRotation}
              type="warning"
              loading={loading}
            />
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <SecretMetricsCard
              title="Expired"
              value={metrics.expired}
              type="error"
              loading={loading}
            />
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <SecretMetricsCard
              title="Up to Date"
              value={metrics.upToDate}
              type="success"
              loading={loading}
            />
          </Grid>
        </Grid>
      </Box>

      <Paper sx={{ p: 2, mb: 3 }}>
        <SecretFilterBar
          filter={filter}
          onFilterChange={setFilter}
        />
      </Paper>

      <SecretsList
        secrets={filteredSecrets}
        loading={loading}
        error={error}
        onView={handleViewSecret}
        onEdit={handleEditSecret}
        onDelete={handleDeleteSecret}
        onRotate={handleRotateSecret}
      />

      <SecretDialog
        open={dialogState.open}
        mode={dialogState.mode}
        secret={dialogState.secret}
        onClose={handleDialogClose}
        onSave={handleDialogSave}
      />

      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={handleSnackbarClose}
      >
        <Alert
          onClose={handleSnackbarClose}
          severity={snackbar.severity}
          sx={{ width: '100%' }}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}