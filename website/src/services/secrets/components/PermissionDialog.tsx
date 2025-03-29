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
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Alert
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import { SECRETS_CONSTANTS } from '../constants';
import { SecretPermission } from '../types/types';
import { useSecrets } from '../hooks/useSecrets';

interface PermissionDialogProps {
  open: boolean;
  onClose: () => void;
  secretId: string;
  permissions: SecretPermission[];
}

export default function PermissionDialog({
  open,
  onClose,
  secretId,
  permissions
}: PermissionDialogProps) {
  const [newUserId, setNewUserId] = React.useState('');
  const [newPermissionLevel, setNewPermissionLevel] = React.useState('READ');
  const [error, setError] = React.useState<string | null>(null);
  const [loading, setLoading] = React.useState(false);

  const { updatePermission, deletePermission } = useSecrets();

  const handleAddPermission = async () => {
    if (!newUserId.trim()) {
      setError('User ID is required');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      await updatePermission(secretId, newUserId, newPermissionLevel as any);
      setNewUserId('');
      // Refresh permissions would happen via the parent component
    } catch (err) {
      setError('Failed to add permission');
      console.error('Failed to add permission:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleDeletePermission = async (userId: string) => {
    setLoading(true);
    setError(null);

    try {
      await deletePermission(secretId, userId);
      // Refresh permissions would happen via the parent component
    } catch (err) {
      setError('Failed to delete permission');
      console.error('Failed to delete permission:', err);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (timestamp: number) => {
    return new Date(timestamp).toLocaleString();
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="md"
      fullWidth
    >
      <DialogTitle>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="h6">Manage Permissions</Typography>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        <Box sx={{ mb: 3 }}>
          <Typography variant="subtitle1" gutterBottom>
            Add New Permission
          </Typography>
          <Box display="flex" gap={2}>
            <TextField
              label="User ID"
              value={newUserId}
              onChange={(e) => setNewUserId(e.target.value)}
              fullWidth
              size="small"
            />
            <FormControl sx={{ minWidth: 200 }} size="small">
              <InputLabel>Permission Level</InputLabel>
              <Select
                value={newPermissionLevel}
                onChange={(e) => setNewPermissionLevel(e.target.value)}
                label="Permission Level"
              >
                {Object.entries(SECRETS_CONSTANTS.PERMISSION_LEVELS).map(([key, value]) => (
                  <MenuItem key={value} value={value}>
                    {key.replace(/_/g, ' ')}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleAddPermission}
              disabled={loading}
            >
              Add
            </Button>
          </Box>
        </Box>

        <Typography variant="subtitle1" gutterBottom>
          Current Permissions
        </Typography>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>User ID</TableCell>
                <TableCell>Permission Level</TableCell>
                <TableCell>Granted At</TableCell>
                <TableCell>Granted By</TableCell>
                <TableCell>Expires</TableCell>
                <TableCell>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {permissions.length === 0 ? (
                <TableRow>
                  <TableCell sx={{ gridColumn: 'span 6' }} align="center">
                    No permissions found
                  </TableCell>
                </TableRow>
              ) : (
                permissions.map((permission) => (
                  <TableRow key={`${permission.secretId}-${permission.userId}`}>
                    <TableCell>{permission.userId}</TableCell>
                    <TableCell>
                      <Chip
                        label={permission.level}
                        color={
                          permission.level === 'ADMIN'
                            ? 'error'
                            : permission.level === 'WRITE'
                            ? 'warning'
                            : 'primary'
                        }
                        size="small"
                      />
                    </TableCell>
                    <TableCell>{formatDate(permission.grantedAt)}</TableCell>
                    <TableCell>{permission.grantedBy}</TableCell>
                    <TableCell>
                      {permission.expiresAt
                        ? formatDate(permission.expiresAt)
                        : 'Never'}
                    </TableCell>
                    <TableCell>
                      <IconButton
                        size="small"
                        onClick={() => handleDeletePermission(permission.userId)}
                        disabled={loading}
                      >
                        <DeleteIcon fontSize="small" color="error" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}
