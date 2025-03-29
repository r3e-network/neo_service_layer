// @ts-ignore
import * as React from 'react';
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
import BoltIcon from '@mui/icons-material/Bolt';
import TriggerMetricsCard from './TriggerMetricsCard';
import TriggerFilterBar from './TriggerFilterBar';
import TriggerList from './TriggerList';
import TriggerDialog from './TriggerDialog';
import { useTriggers } from '../hooks/useTriggers';
import { Trigger, TriggerFilter } from '../types/types';
import { TRIGGER_CONSTANTS } from '../constants';

export default function TriggerDashboard() {
  const {
    triggers,
    executions,
    metrics,
    loading,
    error,
    createTrigger,
    updateTrigger,
    deleteTrigger,
    toggleTriggerStatus,
    refresh
  } = useTriggers();

  const [filter, setFilter] = React.useState<TriggerFilter>({});
  const [dialogState, setDialogState] = React.useState<{
    open: boolean;
    mode: 'create' | 'edit' | 'view';
    trigger?: Trigger;
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

  const handleCreateTrigger = () => {
    setDialogState({
      open: true,
      mode: 'create'
    });
  };

  const handleViewTrigger = (trigger: Trigger) => {
    setDialogState({
      open: true,
      mode: 'view',
      trigger
    });
  };

  const handleEditTrigger = (trigger: Trigger) => {
    setDialogState({
      open: true,
      mode: 'edit',
      trigger
    });
  };

  const handleDeleteTrigger = async (triggerId: string) => {
    try {
      await deleteTrigger(triggerId);
      setSnackbar({
        open: true,
        message: 'Trigger deleted successfully',
        severity: 'success'
      });
    } catch (error) {
      setSnackbar({
        open: true,
        message: 'Failed to delete trigger',
        severity: 'error'
      });
    }
  };

  const handleToggleTrigger = async (triggerId: string, active: boolean) => {
    try {
      await toggleTriggerStatus(triggerId, active);
      setSnackbar({
        open: true,
        message: `Trigger ${active ? 'activated' : 'deactivated'} successfully`,
        severity: 'success'
      });
    } catch (error) {
      setSnackbar({
        open: true,
        message: `Failed to ${active ? 'activate' : 'deactivate'} trigger`,
        severity: 'error'
      });
    }
  };

  const handleDialogSave = async (triggerData: Partial<Trigger>) => {
    try {
      if (dialogState.mode === 'create') {
        await createTrigger(triggerData);
        setSnackbar({
          open: true,
          message: 'Trigger created successfully',
          severity: 'success'
        });
      } else if (dialogState.mode === 'edit' && dialogState.trigger) {
        await updateTrigger(dialogState.trigger.id, triggerData);
        setSnackbar({
          open: true,
          message: 'Trigger updated successfully',
          severity: 'success'
        });
      }
      setDialogState({ open: false, mode: 'create' });
      refresh();
    } catch (error) {
      setSnackbar({
        open: true,
        message: 'Failed to save trigger',
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

  // Filter triggers based on current filter state
  const filteredTriggers = triggers.filter(trigger => {
    if (filter.search && !trigger.name.toLowerCase().includes(filter.search.toLowerCase())) {
      return false;
    }
    if (filter.type?.length && !filter.type.includes(trigger.condition.type)) {
      return false;
    }
    if (filter.status?.length && !filter.status.includes(trigger.status)) {
      return false;
    }
    if (filter.tags?.length && !filter.tags.some(tag => trigger.tags?.includes(tag))) {
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
            <BoltIcon sx={{ fontSize: 32, color: 'primary.main' }} />
            <Typography variant="h4" component="h1">
              Trigger Management
            </Typography>
          </Box>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleCreateTrigger}
          >
            Create Trigger
          </Button>
        </Box>

        <Grid container spacing={3}>
          <Grid sx={{ gridColumn: { xs: 'span 12', sm: 'span 6', md: 'span 3' } }}>
            <TriggerMetricsCard
              title="Total Triggers"
              value={metrics?.total || 0}
              type="total"
              loading={loading}
            />
          </Grid>
          <Grid sx={{ gridColumn: { xs: 'span 12', sm: 'span 6', md: 'span 3' } }}>
            <TriggerMetricsCard
              title="Active Triggers"
              value={metrics?.active || 0}
              type="active"
              loading={loading}
            />
          </Grid>
          <Grid sx={{ gridColumn: { xs: 'span 12', sm: 'span 6', md: 'span 3' } }}>
            <TriggerMetricsCard
              title="Success Rate"
              value={metrics?.successRate || 0}
              type="success"
              loading={loading}
              isPercentage
            />
          </Grid>
          <Grid sx={{ gridColumn: { xs: 'span 12', sm: 'span 6', md: 'span 3' } }}>
            <TriggerMetricsCard
              title="Executions (24h)"
              value={metrics?.executionsLast24h || 0}
              type="executions"
              loading={loading}
            />
          </Grid>
        </Grid>
      </Box>

      <Paper sx={{ p: 2, mb: 3 }}>
        <TriggerFilterBar
          filter={filter}
          onFilterChange={setFilter}
        />
      </Paper>

      <TriggerList
        triggers={filteredTriggers}
        executions={executions}
        loading={loading}
        onEdit={handleEditTrigger}
        onDelete={handleDeleteTrigger}
        onToggleStatus={handleToggleTrigger}
      />

      <TriggerDialog
        open={dialogState.open}
        mode={dialogState.mode}
        trigger={dialogState.trigger}
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