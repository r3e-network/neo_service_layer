import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControlLabel,
  Switch,
  TextField,
  Box,
  Typography,
  Chip,
  IconButton,
  Alert,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import { FunctionPermissions, UserFunction } from '../types/types';

interface FunctionPermissionsDialogProps {
  open: boolean;
  onClose: () => void;
  function: UserFunction;
  onUpdate: (id: string, permissions: Partial<FunctionPermissions>) => Promise<void>;
}

export function FunctionPermissionsDialog({
  open,
  onClose,
  function: func,
  onUpdate
}: FunctionPermissionsDialogProps) {
  const [permissions, setPermissions] = React.useState<FunctionPermissions>({
    allowedContracts: func.permissions?.allowedContracts || [],
    allowedAPIs: func.permissions?.allowedAPIs || [],
    allowedSecrets: func.permissions?.allowedSecrets || [],
    maxGasPerExecution: func.permissions?.maxGasPerExecution || 10,
    roles: func.permissions?.roles || []
  });
  const [newRole, setNewRole] = React.useState('');
  const [newContract, setNewContract] = React.useState('');
  const [newAPI, setNewAPI] = React.useState('');
  const [newSecret, setNewSecret] = React.useState('');
  const [error, setError] = React.useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = React.useState(false);

  React.useEffect(() => {
    setPermissions({
      allowedContracts: func.permissions?.allowedContracts || [],
      allowedAPIs: func.permissions?.allowedAPIs || [],
      allowedSecrets: func.permissions?.allowedSecrets || [],
      maxGasPerExecution: func.permissions?.maxGasPerExecution || 10,
      roles: func.permissions?.roles || []
    });
  }, [func]);

  const handleSubmit = async () => {
    try {
      setIsSubmitting(true);
      setError(null);
      await onUpdate(func.id, permissions);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update permissions');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleAddRole = () => {
    if (newRole && !permissions.roles.includes(newRole)) {
      setPermissions({
        ...permissions,
        roles: [...permissions.roles, newRole]
      });
      setNewRole('');
    }
  };

  const handleRemoveRole = (role: string) => {
    setPermissions({
      ...permissions,
      roles: permissions.roles.filter(r => r !== role)
    });
  };

  const handleMaxGasChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(event.target.value, 10);
    setPermissions({
      ...permissions,
      maxGasPerExecution: isNaN(value) ? 0 : value
    });
  };

  const handleToggleAPIAccess = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.checked) {
      setPermissions({
        ...permissions,
        allowedAPIs: [...permissions.allowedAPIs, 'storage']
      });
    } else {
      setPermissions({
        ...permissions,
        allowedAPIs: permissions.allowedAPIs.filter(api => api !== 'storage')
      });
    }
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
    >
      <DialogTitle>Function Permissions</DialogTitle>
      <DialogContent>
        <Box sx={{ pt: 2, display: 'flex', flexDirection: 'column', gap: 3 }}>
          {error && (
            <Alert severity="error" onClose={() => setError(null)}>
              {error}
            </Alert>
          )}

          <Box>
            <Typography variant="subtitle2" gutterBottom>
              Access Control
            </Typography>
            <FormControlLabel
              control={
                <Switch
                  checked={permissions.allowedAPIs.includes('storage')}
                  onChange={handleToggleAPIAccess}
                />
              }
              label="Storage Access"
            />
          </Box>

          <Box>
            <Typography variant="subtitle1" gutterBottom>
              Roles
            </Typography>
            <Box display="flex" alignItems="center" mb={2}>
              <TextField
                value={newRole}
                onChange={(e) => setNewRole(e.target.value)}
                placeholder="Add a role"
                size="small"
                fullWidth
              />
              <IconButton color="primary" onClick={handleAddRole}>
                <AddIcon />
              </IconButton>
            </Box>
            <List>
              {permissions.roles.map((role, index) => (
                <ListItem key={index}>
                  <ListItemText primary={role} />
                  <ListItemSecondaryAction>
                    <IconButton edge="end" onClick={() => handleRemoveRole(role)}>
                      <DeleteIcon />
                    </IconButton>
                  </ListItemSecondaryAction>
                </ListItem>
              ))}
            </List>
          </Box>

          <Box>
            <Typography variant="subtitle1" gutterBottom>
              Gas Limit
            </Typography>
            <TextField
              type="number"
              label="Max Gas Per Execution"
              value={permissions.maxGasPerExecution}
              onChange={handleMaxGasChange}
              fullWidth
              margin="normal"
            />
          </Box>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={isSubmitting}
        >
          Save Changes
        </Button>
      </DialogActions>
    </Dialog>
  );
}