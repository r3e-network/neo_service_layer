// @ts-ignore
import * as React from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CardActions,
  Typography,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Grid,
  Chip
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import { UserFunction, FunctionTrigger, TriggerType, TriggerConfig } from '../types/types';
import { useFunctions } from '../hooks/useFunctions';

interface FunctionTriggersProps {
  function: UserFunction;
}

interface TriggerDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (trigger: Omit<FunctionTrigger, 'id'>) => void;
  trigger?: FunctionTrigger;
}

function TriggerDialog({
  open,
  onClose,
  onSubmit,
  trigger
}: TriggerDialogProps) {
  const [name, setName] = React.useState(trigger?.name || '');
  const [triggerType, setTriggerType] = React.useState<TriggerType>(trigger?.type || 'schedule');
  const [cronExpression, setCronExpression] = React.useState(trigger?.config?.schedule?.cron || '');
  const [httpMethod, setHttpMethod] = React.useState(trigger?.config?.http?.method || 'GET');
  const [httpPath, setHttpPath] = React.useState(trigger?.config?.http?.path || '');
  const [eventName, setEventName] = React.useState(trigger?.config?.event?.eventName || '');
  const [contractMethod, setContractMethod] = React.useState(trigger?.config?.contract?.method || '');
  const [oracleDataSource, setOracleDataSource] = React.useState(trigger?.config?.oracle?.dataSource || '');

  const handleSubmit = () => {
    const newTrigger: Omit<FunctionTrigger, 'id'> = {
      name,
      type: triggerType,
      enabled: true,
      config: getTriggerConfig(),
    };

    onSubmit(newTrigger);
    onClose();
  };

  const getTriggerConfig = (): TriggerConfig => {
    switch (triggerType) {
      case 'schedule':
        return {
          schedule: {
            cron: cronExpression,
            timezone: 'UTC'
          }
        };
      case 'http':
        return {
          http: {
            method: httpMethod,
            path: httpPath,
            auth: true
          }
        };
      case 'event':
        return {
          event: {
            contractHash: 'default-contract-hash',
            eventName,
            filters: {}
          }
        };
      case 'contract':
        return {
          contract: {
            contractHash: 'default-contract-hash',
            method: contractMethod,
            parameters: [],
            frequency: 60
          }
        };
      case 'oracle':
        return {
          oracle: {
            dataSource: oracleDataSource,
            query: '',
            frequency: 60
          }
        };
      default:
        return {
          schedule: {
            cron: cronExpression,
            timezone: 'UTC'
          }
        };
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {trigger ? 'Edit Trigger' : 'New Trigger'}
      </DialogTitle>
      <DialogContent>
        <Box sx={{ pt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
          <TextField
            label="Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            fullWidth
          />
          <FormControl fullWidth>
            <InputLabel>Type</InputLabel>
            <Select
              value={triggerType}
              label="Type"
              onChange={(e) => setTriggerType(e.target.value as TriggerType)}
            >
              <MenuItem value="schedule">Schedule</MenuItem>
              <MenuItem value="http">HTTP</MenuItem>
              <MenuItem value="event">Event</MenuItem>
              <MenuItem value="contract">Contract</MenuItem>
              <MenuItem value="oracle">Oracle</MenuItem>
            </Select>
          </FormControl>
          {triggerType === 'schedule' && (
            <TextField
              label="Cron Expression"
              value={cronExpression}
              onChange={(e) => setCronExpression(e.target.value)}
              fullWidth
              helperText="Example: */5 * * * * (every 5 minutes)"
            />
          )}
          {triggerType === 'http' && (
            <Box>
              <FormControl fullWidth>
                <InputLabel>Method</InputLabel>
                <Select
                  value={httpMethod}
                  label="Method"
                  onChange={(e) => setHttpMethod(e.target.value)}
                >
                  <MenuItem value="GET">GET</MenuItem>
                  <MenuItem value="POST">POST</MenuItem>
                  <MenuItem value="PUT">PUT</MenuItem>
                  <MenuItem value="DELETE">DELETE</MenuItem>
                </Select>
              </FormControl>
              <TextField
                label="Path"
                value={httpPath}
                onChange={(e) => setHttpPath(e.target.value)}
                fullWidth
              />
            </Box>
          )}
          {triggerType === 'event' && (
            <TextField
              label="Event Name"
              value={eventName}
              onChange={(e) => setEventName(e.target.value)}
              fullWidth
            />
          )}
          {triggerType === 'contract' && (
            <TextField
              label="Method"
              value={contractMethod}
              onChange={(e) => setContractMethod(e.target.value)}
              fullWidth
            />
          )}
          {triggerType === 'oracle' && (
            <TextField
              label="Data Source"
              value={oracleDataSource}
              onChange={(e) => setOracleDataSource(e.target.value)}
              fullWidth
            />
          )}
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={!name}
        >
          {trigger ? 'Update' : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export function FunctionTriggers({ function: func }: FunctionTriggersProps) {
  const [showDialog, setShowDialog] = React.useState(false);
  const [selectedTrigger, setSelectedTrigger] = React.useState<FunctionTrigger | undefined>();
  const { createTrigger, updateTrigger, deleteTrigger } = useFunctions();

  const handleCreateTrigger = async (trigger: Omit<FunctionTrigger, 'id'>) => {
    try {
      await createTrigger(func.id, trigger);
    } catch (err) {
      console.error('Failed to create trigger:', err);
    }
  };

  const handleUpdateTrigger = async (trigger: Omit<FunctionTrigger, 'id'>) => {
    if (selectedTrigger) {
      try {
        await updateTrigger(func.id, selectedTrigger.id, trigger);
      } catch (err) {
        console.error('Failed to update trigger:', err);
      }
    }
  };

  const handleDeleteTrigger = async (triggerId: string) => {
    try {
      await deleteTrigger(func.id, triggerId);
    } catch (err) {
      console.error('Failed to delete trigger:', err);
    }
  };

  const handleEditTrigger = (trigger: FunctionTrigger) => {
    setSelectedTrigger(trigger);
    setShowDialog(true);
  };

  const handleCloseDialog = () => {
    setSelectedTrigger(undefined);
    setShowDialog(false);
  };

  const getTriggerDescription = (trigger: FunctionTrigger): string => {
    switch (trigger.type) {
      case 'schedule':
        return trigger.config.schedule ? `Schedule: ${trigger.config.schedule.cron}` : '';
      case 'http':
        return trigger.config.http ? `HTTP: ${trigger.config.http.method} ${trigger.config.http.path}` : '';
      case 'event':
        return trigger.config.event ? `Event: ${trigger.config.event.eventName}` : '';
      case 'contract':
        return trigger.config.contract ? `Contract: ${trigger.config.contract.method}` : '';
      case 'oracle':
        return trigger.config.oracle ? `Oracle: ${trigger.config.oracle.dataSource}` : '';
      default:
        return '';
    }
  };

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h6">
          Triggers
        </Typography>
        <Button
          startIcon={<AddIcon />}
          variant="contained"
          onClick={() => setShowDialog(true)}
        >
          Add Trigger
        </Button>
      </Box>

      <Grid container spacing={3}>
        {func.triggers?.map((trigger) => (
          <Grid key={trigger.id} size={4}>
            <Card>
              <CardContent>
                <Box display="flex" alignItems="center" gap={1} mb={1}>
                  <Typography variant="h6" component="div">
                    {trigger.name}
                  </Typography>
                  <Chip
                    label={trigger.type}
                    size="small"
                    color={trigger.type === 'schedule' ? 'primary' : 'secondary'}
                  />
                </Box>
                <Typography color="text.secondary" gutterBottom>
                  {getTriggerDescription(trigger)}
                </Typography>
              </CardContent>
              <CardActions>
                <IconButton
                  size="small"
                  onClick={() => handleEditTrigger(trigger)}
                >
                  <EditIcon />
                </IconButton>
                <IconButton
                  size="small"
                  onClick={() => handleDeleteTrigger(trigger.id)}
                >
                  <DeleteIcon />
                </IconButton>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Grid>

      <TriggerDialog
        open={showDialog}
        onClose={handleCloseDialog}
        onSubmit={selectedTrigger ? handleUpdateTrigger : handleCreateTrigger}
        trigger={selectedTrigger}
      />
    </Box>
  );
}