import React from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Chip,
  Tooltip,
  Menu,
  MenuItem,
  Typography,
  Box,
  TablePagination,
  Paper,
  Skeleton,
  Alert
} from '@mui/material';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import RefreshIcon from '@mui/icons-material/Refresh';
import LockIcon from '@mui/icons-material/Lock';
import LockOpenIcon from '@mui/icons-material/LockOpen';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import HistoryIcon from '@mui/icons-material/History';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { useAuth } from '@/hooks/useAuth';
import { Secret, SecretPermission } from '../types/types';
import { SECRETS_CONSTANTS } from '../constants';
import SecretDialog from './SecretDialog';
import PermissionDialog from './PermissionDialog';
import ConfirmDialog from './ConfirmDialog';

interface SecretsListProps {
  secrets: Secret[];
  loading: boolean;
  error?: string;
  onView: (secret: Secret) => void;
  onEdit: (secret: Secret) => void;
  onDelete: (secret: Secret) => void;
  onRotate: (secret: Secret) => void;
}

const formatDate = (timestamp: string | number): string => {
  return new Date(timestamp).toLocaleString();
};

export default function SecretsList({
  secrets,
  loading,
  error,
  onView,
  onEdit,
  onDelete,
  onRotate
}: SecretsListProps) {
  const [page, setPage] = React.useState(0);
  const [rowsPerPage, setRowsPerPage] = React.useState(10);
  const [selectedSecret, setSelectedSecret] = React.useState<Secret | null>(null);
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const [isPermissionDialogOpen, setPermissionDialogOpen] = React.useState(false);

  const { isAuthenticated, user } = useAuth();

  const handleChangePage = (event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>, secret: Secret) => {
    setSelectedSecret(secret);
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handlePermissions = () => {
    setPermissionDialogOpen(true);
    handleMenuClose();
  };

  const getRotationStatus = (secret: Secret) => {
    if (!secret.rotationPeriod) {
      return 'upToDate';
    }
    
    const lastRotated = new Date(secret.lastRotatedAt);
    const now = new Date();
    const rotationDueDate = new Date(lastRotated);
    rotationDueDate.setMilliseconds(rotationDueDate.getMilliseconds() + secret.rotationPeriod);
    
    if (rotationDueDate < now) {
      // If more than twice the rotation period has passed, consider it expired
      const expiredDate = new Date(lastRotated);
      expiredDate.setMilliseconds(expiredDate.getMilliseconds() + (secret.rotationPeriod * 2));
      
      return expiredDate < now ? 'expired' : 'needsRotation';
    }
    
    return 'upToDate';
  };

  const needsRotation = (secret: Secret) => {
    const status = getRotationStatus(secret);
    return status === 'needsRotation' || status === 'expired';
  };

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error}
      </Alert>
    );
  }

  return (
    <Paper sx={{ width: '100%', overflow: 'hidden' }}>
      <TableContainer>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Created</TableCell>
              <TableCell>Last Rotated</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              Array.from(new Array(5)).map((_, index) => (
                <TableRow key={`skeleton-${index}`}>
                  <TableCell><Skeleton variant="text" width={120} /></TableCell>
                  <TableCell><Skeleton variant="text" width={80} /></TableCell>
                  <TableCell><Skeleton variant="text" width={100} /></TableCell>
                  <TableCell><Skeleton variant="text" width={100} /></TableCell>
                  <TableCell><Skeleton variant="text" width={80} /></TableCell>
                  <TableCell align="right"><Skeleton variant="text" width={120} /></TableCell>
                </TableRow>
              ))
            ) : secrets.length === 0 ? (
              <TableRow>
                <TableCell 
                  align="center"
                  sx={{ gridColumn: 'span 6' }}
                >
                  <Typography variant="body1" sx={{ py: 2 }}>
                    No secrets found
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              secrets
                .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
                .map((secret) => {
                  const status = getRotationStatus(secret);
                  
                  return (
                    <TableRow key={secret.id}>
                      <TableCell>{secret.name}</TableCell>
                      <TableCell>
                        <Chip
                          label={secret.type}
                          size="small"
                          color="primary"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>{formatDate(secret.createdAt)}</TableCell>
                      <TableCell>
                        {secret.lastRotatedAt ? formatDate(secret.lastRotatedAt) : 'Never'}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={
                            status === 'upToDate'
                              ? 'Up to date'
                              : status === 'needsRotation'
                              ? 'Needs rotation'
                              : 'Expired'
                          }
                          size="small"
                          color={
                            status === 'upToDate'
                              ? 'success'
                              : status === 'needsRotation'
                              ? 'warning'
                              : 'error'
                          }
                          icon={
                            status === 'upToDate' ? (
                              <LockIcon fontSize="small" />
                            ) : (
                              <LockOpenIcon fontSize="small" />
                            )
                          }
                        />
                      </TableCell>
                      <TableCell align="right">
                        <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                          <Tooltip title="View Secret">
                            <IconButton size="small" onClick={() => onView(secret)}>
                              <VisibilityIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Edit Secret">
                            <IconButton size="small" onClick={() => onEdit(secret)}>
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Rotate Secret">
                            <IconButton
                              size="small"
                              onClick={() => onRotate(secret)}
                              disabled={!needsRotation(secret)}
                            >
                              <RefreshIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete Secret">
                            <IconButton
                              size="small"
                              onClick={() => onDelete(secret)}
                              color="error"
                            >
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="More Options">
                            <IconButton
                              size="small"
                              onClick={(e) => handleMenuOpen(e, secret)}
                            >
                              <MoreVertIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </Box>
                      </TableCell>
                    </TableRow>
                  );
                })
            )}
          </TableBody>
        </Table>
      </TableContainer>
      <TablePagination
        rowsPerPageOptions={[5, 10, 25]}
        component="div"
        count={secrets.length}
        rowsPerPage={rowsPerPage}
        page={page}
        onPageChange={handleChangePage}
        onRowsPerPageChange={handleChangeRowsPerPage}
      />

      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={() => {
          if (selectedSecret) onView(selectedSecret);
          handleMenuClose();
        }}>
          <VisibilityIcon fontSize="small" sx={{ mr: 1 }} />
          View
        </MenuItem>
        <MenuItem onClick={() => {
          if (selectedSecret) onEdit(selectedSecret);
          handleMenuClose();
        }}>
          <EditIcon fontSize="small" sx={{ mr: 1 }} />
          Edit
        </MenuItem>
        <MenuItem onClick={handlePermissions}>
          <LockOpenIcon fontSize="small" sx={{ mr: 1 }} />
          Permissions
        </MenuItem>
        <MenuItem onClick={() => {
          if (selectedSecret) onRotate(selectedSecret);
          handleMenuClose();
        }}>
          <RefreshIcon fontSize="small" sx={{ mr: 1 }} />
          Rotate
        </MenuItem>
        <MenuItem onClick={() => {
          if (selectedSecret) onDelete(selectedSecret);
          handleMenuClose();
        }}>
          <DeleteIcon fontSize="small" sx={{ mr: 1 }} color="error" />
          Delete
        </MenuItem>
      </Menu>

      {selectedSecret && (
        <PermissionDialog
          open={isPermissionDialogOpen}
          onClose={() => setPermissionDialogOpen(false)}
          secretId={selectedSecret.id}
          permissions={[]} // This will need to be updated to get permissions from props or API
        />
      )}
    </Paper>
  );
}