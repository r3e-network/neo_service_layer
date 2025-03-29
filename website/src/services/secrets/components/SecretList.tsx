import React from 'react';
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Tooltip,
  Chip,
  TablePagination,
  Skeleton,
  Typography,
  Alert
} from '@mui/material';
import VisibilityIcon from '@mui/icons-material/Visibility';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import RefreshIcon from '@mui/icons-material/Refresh';
import LockIcon from '@mui/icons-material/Lock';
import LockOpenIcon from '@mui/icons-material/LockOpen';
import { formatDate } from '../utils/formatters';
import { Secret } from '../types/types';
import { SECRETS_CONSTANTS } from '../constants';

interface SecretListProps {
  secrets: Secret[];
  loading: boolean;
  error?: string;
  onView: (secret: Secret) => void;
  onEdit: (secret: Secret) => void;
  onDelete: (secret: Secret) => void;
  onRotate: (secret: Secret) => void;
}

export default function SecretList({
  secrets,
  loading,
  error,
  onView,
  onEdit,
  onDelete,
  onRotate
}: SecretListProps) {
  const [page, setPage] = React.useState(0);
  const [rowsPerPage, setRowsPerPage] = React.useState(10);

  const handleChangePage = (event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error}
      </Alert>
    );
  }

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
      
      if (expiredDate < now) {
        return 'expired';
      }
      return 'needsRotation';
    }
    
    return 'upToDate';
  };

  const needsRotation = (secret: Secret) => {
    return getRotationStatus(secret) === 'needsRotation';
  };

  const getRotationStatusColor = (status: string) => {
    switch (status) {
      case 'upToDate':
        return 'success';
      case 'needsRotation':
        return 'warning';
      case 'expired':
        return 'error';
      default:
        return 'default';
    }
  };

  const LoadingSkeleton = () => (
    <TableRow>
      <TableCell><Skeleton variant="text" /></TableCell>
      <TableCell><Skeleton variant="text" /></TableCell>
      <TableCell><Skeleton variant="text" /></TableCell>
      <TableCell><Skeleton variant="text" /></TableCell>
      <TableCell><Skeleton variant="text" /></TableCell>
      <TableCell><Skeleton variant="text" /></TableCell>
      <TableCell><Skeleton variant="text" /></TableCell>
    </TableRow>
  );

  return (
    <Paper sx={{ width: '100%', mb: 2 }}>
      <TableContainer>
        <Table size="medium">
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Tags</TableCell>
              <TableCell>Last Updated</TableCell>
              <TableCell>Rotation Status</TableCell>
              <TableCell>Access Level</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              Array.from(Array(3)).map((_, index) => (
                <LoadingSkeleton key={index} />
              ))
            ) : secrets.length === 0 ? (
              <TableRow>
                <TableCell 
                  align="center"
                  sx={{ gridColumn: 'span 7' }}
                >
                  <Box sx={{ py: 3 }}>
                    <LockIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
                    <Typography variant="h6" color="text.secondary">
                      No secrets found
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Create a new secret to get started
                    </Typography>
                  </Box>
                </TableCell>
              </TableRow>
            ) : (
              secrets
                .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
                .map((secret) => (
                  <TableRow key={secret.id} hover>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        {secret.encryptedValue ? <LockIcon fontSize="small" /> : <LockOpenIcon fontSize="small" />}
                        {secret.name}
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={secret.type.replace(/_/g, ' ')}
                        size="small"
                        color="primary"
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                        {secret.tags && secret.tags.map((tag) => (
                          <Chip
                            key={tag}
                            label={tag}
                            size="small"
                            variant="outlined"
                          />
                        ))}
                      </Box>
                    </TableCell>
                    <TableCell>{formatDate(new Date(secret.updatedAt).toISOString())}</TableCell>
                    <TableCell>
                      <Chip
                        label={getRotationStatus(secret)}
                        size="small"
                        color={getRotationStatusColor(getRotationStatus(secret)) as any}
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label="Owner"
                        size="small"
                        variant="outlined"
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
                      </Box>
                    </TableCell>
                  </TableRow>
                ))
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
    </Paper>
  );
}