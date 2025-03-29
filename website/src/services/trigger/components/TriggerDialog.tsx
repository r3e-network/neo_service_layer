// @ts-ignore
import * as React from 'react';
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
  FormHelperText,
  Box,
  Typography,
  Divider,
  Grid,
  IconButton,
  Tooltip,
  Alert
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import InfoIcon from '@mui/icons-material/Info';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import { Trigger, TriggerCondition, TriggerAction, EventType, ValidationError } from '../types/types';
import { validateTrigger, validateTriggerCondition, validateTriggerAction } from '../utils/validation';
import { isValidCronExpression } from '../utils/cronValidation';

interface TriggerDialogProps {
  open: boolean;
  onClose: () => void;
  onSave: (trigger: Trigger) => Promise<void>;
  trigger?: Trigger;
  mode: 'view' | 'create' | 'edit';
}

const defaultCondition: TriggerCondition = {
  type: 'block_height' as EventType,
  parameters: {
    cronExpression: '0 0 * * *'
  }
};

const defaultAction: TriggerAction = {
  type: 'http_request',
  parameters: {
    url: '',
    httpMethod: 'POST',
    headers: {},
    body: '{}'
  }
};

export default function TriggerDialog({
  open,
  onClose,
  onSave,
  trigger,
  mode
}: TriggerDialogProps) {
  const [name, setName] = React.useState('');
  const [description, setDescription] = React.useState('');
  const [condition, setCondition] = React.useState<TriggerCondition>(defaultCondition);
  const [action, setAction] = React.useState<TriggerAction>(defaultAction);
  const [errors, setErrors] = React.useState<{
    name?: string;
    condition?: string;
    action?: string;
    cronExpression?: string;
    headers?: string;
    body?: string;
  }>({});

  React.useEffect(() => {
    if (trigger) {
      setName(trigger.name);
      setDescription(trigger.description || '');
      setCondition(trigger.condition);
      setAction(trigger.action);
    } else {
      resetForm();
    }
  }, [trigger]);

  const resetForm = () => {
    setName('');
    setDescription('');
    setCondition(defaultCondition);
    setAction(defaultAction);
    setErrors({});
  };

  const handleClose = () => {
    resetForm();
    onClose();
  };

  const validateForm = (): boolean => {
    const newErrors: {
      name?: string;
      condition?: string;
      action?: string;
      cronExpression?: string;
      headers?: string;
      body?: string;
    } = {};

    if (!name.trim()) {
      newErrors.name = 'Name is required';
    }

    const conditionErrors = validateTriggerCondition(condition);
    if (conditionErrors && conditionErrors.length > 0) {
      newErrors.condition = conditionErrors.map(err => err.message).join(', ');
    }

    if (condition.type === 'block_height' && condition.parameters.cronExpression) {
      const cronError = !isValidCronExpression(condition.parameters.cronExpression);
      if (cronError) {
        newErrors.cronExpression = 'Invalid cron expression';
      }
    }

    const actionErrors = validateTriggerAction(action);
    if (actionErrors && actionErrors.length > 0) {
      newErrors.action = actionErrors.map(err => err.message).join(', ');
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validateForm()) {
      return;
    }

    const newTrigger: Trigger = {
      id: trigger?.id || `trigger-${Date.now()}`,
      name,
      description,
      owner: trigger?.owner || 'current-user',
      condition,
      action,
      status: trigger?.status || 'active',
      createdAt: trigger?.createdAt || Date.now(),
      updatedAt: Date.now(),
      lastExecutedAt: trigger?.lastExecutedAt,
      executionCount: trigger?.executionCount || 0,
      failureCount: trigger?.failureCount || 0,
      retryCount: trigger?.retryCount || 0,
      timeout: trigger?.timeout || 30000
    };

    await onSave(newTrigger);
    handleClose();
  };

  const renderConditionFields = () => {
    switch (condition.type) {
      case 'block_height':
        return (
          <TextField
            fullWidth
            label="Cron Expression"
            value={condition.parameters.cronExpression}
            onChange={(e) => setCondition({ ...condition, parameters: { ...condition.parameters, cronExpression: e.target.value } })}
            error={!!errors.cronExpression}
            helperText={errors.cronExpression}
            sx={{ mt: 2 }}
          />
        );
      default:
        return null;
    }
  };

  const renderActionFields = () => {
    switch (action.type) {
      case 'http_request':
        return (
          <>
            <TextField
              fullWidth
              label="URL"
              value={action.parameters.url}
              onChange={(e) => setAction({ ...action, parameters: { ...action.parameters, url: e.target.value } })}
              error={!!errors.action}
              helperText={errors.action}
              sx={{ mt: 2 }}
            />
            <FormControl fullWidth sx={{ mt: 2 }}>
              <InputLabel>Method</InputLabel>
              <Select
                value={action.parameters.httpMethod}
                onChange={(e) => setAction({
                  ...action,
                  parameters: { ...action.parameters, httpMethod: e.target.value as 'GET' | 'POST' | 'PUT' | 'DELETE' }
                })}
              >
                <MenuItem value="GET">GET</MenuItem>
                <MenuItem value="POST">POST</MenuItem>
                <MenuItem value="PUT">PUT</MenuItem>
                <MenuItem value="DELETE">DELETE</MenuItem>
              </Select>
            </FormControl>
            <TextField
              fullWidth
              label="Headers (JSON)"
              value={JSON.stringify(action.parameters.headers, null, 2)}
              onChange={(e) => {
                try {
                  const headers = JSON.parse(e.target.value);
                  setAction({ ...action, parameters: { ...action.parameters, headers } });
                  setErrors({ ...errors, headers: '' });
                } catch (err) {
                  setErrors({ ...errors, headers: 'Invalid JSON' });
                }
              }}
              error={!!errors.headers}
              helperText={errors.headers}
              multiline
              rows={3}
              sx={{ mt: 2 }}
            />
            <TextField
              fullWidth
              label="Body (JSON)"
              value={action.parameters.body}
              onChange={(e) => setAction({ ...action, parameters: { ...action.parameters, body: e.target.value } })}
              error={!!errors.body}
              helperText={errors.body}
              multiline
              rows={4}
              sx={{ mt: 2 }}
            />
          </>
        );
      default:
        return null;
    }
  };

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth="md"
      fullWidth
      PaperProps={{
        sx: { minHeight: '60vh' }
      }}
    >
      <DialogTitle>
        <Box display="flex" alignItems="center" justifyContent="space-between">
          <Typography variant="h6">
            {mode === 'edit' ? 'Edit Trigger' : mode === 'create' ? 'Create New Trigger' : 'View Trigger'}
          </Typography>
          <IconButton onClick={handleClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        <Grid container spacing={3}>
          <Grid sx={{ gridColumn: 'span 12' }}>
            <TextField
              fullWidth
              label="Name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              error={!!errors.name}
              helperText={errors.name}
              disabled={mode === 'view'}
            />
          </Grid>
          <Grid sx={{ gridColumn: 'span 12' }}>
            <TextField
              fullWidth
              label="Description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              multiline
              rows={2}
              disabled={mode === 'view'}
            />
          </Grid>

          <Grid sx={{ gridColumn: 'span 12' }}>
            <Typography variant="subtitle1" gutterBottom>
              Trigger Condition
            </Typography>
            <FormControl fullWidth error={!!errors.condition}>
              <InputLabel>Condition Type</InputLabel>
              <Select
                value={condition.type}
                onChange={(e) => setCondition({
                  ...defaultCondition,
                  type: e.target.value as EventType
                })}
                disabled={mode === 'view'}
              >
                <MenuItem value="block_height">Block Height</MenuItem>
              </Select>
              {errors.condition && (
                <FormHelperText>{errors.condition}</FormHelperText>
              )}
            </FormControl>
            {renderConditionFields()}
          </Grid>

          <Grid sx={{ gridColumn: 'span 12' }}>
            <Typography variant="subtitle1" gutterBottom>
              Trigger Action
            </Typography>
            <FormControl fullWidth error={!!errors.action}>
              <InputLabel>Action Type</InputLabel>
              <Select
                value={action.type}
                onChange={(e) => setAction({
                  ...defaultAction,
                  type: e.target.value as 'http_request' | 'contract_call' | 'function_execution'
                })}
                disabled={mode === 'view'}
              >
                <MenuItem value="http_request">HTTP Request</MenuItem>
                <MenuItem value="contract_call">Contract Call</MenuItem>
                <MenuItem value="function_execution">Function Execution</MenuItem>
              </Select>
              {errors.action && (
                <FormHelperText>{errors.action}</FormHelperText>
              )}
            </FormControl>
            {renderActionFields()}
          </Grid>
        </Grid>
      </DialogContent>

      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        {mode !== 'view' && (
          <Button
            onClick={handleSubmit}
            variant="contained"
            color="primary"
          >
            {mode === 'edit' ? 'Save Changes' : 'Create Trigger'}
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
}