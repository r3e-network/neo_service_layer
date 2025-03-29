// @ts-ignore
import * as React from 'react';
import {
  Box,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Tooltip,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Typography,
  Alert
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/DeleteOutlined';
import EditIcon from '@mui/icons-material/Edit';
import VisibilityIcon from '@mui/icons-material/Visibility';
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import { ApiKey } from '../types/types';
import { ApiKeyDialog } from './ApiKeyDialog';

interface ApiKeysListProps {
  apiKeys: ApiKey[];
  onCreateKey: () => void;
  onUpdateKey: (
    id: string,
    updates: {
      name?: string;
      allowedEndpoints?: string[];
      allowedIPs?: string[];
      expiresAt?: number;
      rateLimit?: {
        requestsPerMinute: number;
        burstLimit: number;
      };
    }
  ) => Promise<void>;
  onDeleteKey: (id: string) => Promise<void>;
}

export function ApiKeysList({
  apiKeys,
  onCreateKey,
  onUpdateKey,
  onDeleteKey
}: ApiKeysListProps) {
  const [showKey, setShowKey] = React.useState<string | null>(null);
  const [selectedKey, setSelectedKey] = React.useState<ApiKey | null>(null);
  const [showEditDialog, setShowEditDialog] = React.useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = React.useState(false);
  const [copySuccess, setCopySuccess] = React.useState<string | null>(null);

  const handleCopyKey = async (key: string) => {
    try {
      await navigator.clipboard.writeText(key);
      setCopySuccess('API key copied to clipboard');
      setTimeout(() => setCopySuccess(null), 3000);
    } catch (err) {
      console.error('Failed to copy API key:', err);
    }
  };

  const handleEditKey = (key: ApiKey) => {
    setSelectedKey(key);
    setShowEditDialog(true);
  };

  const handleDeleteKey = (key: ApiKey) => {
    setSelectedKey(key);
    setShowDeleteDialog(true);
  };

  const handleConfirmDelete = async () => {
    if (selectedKey) {
      try {
        await onDeleteKey(selectedKey.id);
        setShowDeleteDialog(false);
        setSelectedKey(null);
      } catch (err) {
        console.error('Failed to delete API key:', err);
      }
    }
  };

  const formatDate = (timestamp: number) => {
    return new Date(timestamp).toLocaleString();
  };

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h6">API Keys</Typography>
        <Button
          startIcon={<AddIcon />}
          variant="contained"
          onClick={onCreateKey}
        >
          Create API Key
        </Button>
      </Box>

      {copySuccess && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {copySuccess}
        </Alert>
      )}

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>API Key</TableCell>
              <TableCell>Created</TableCell>
              <TableCell>Last Used</TableCell>
              <TableCell>Expires</TableCell>
              <TableCell>Rate Limit</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {apiKeys.map((key) => (
              <TableRow key={key.id}>
                <TableCell>{key.name}</TableCell>
                <TableCell>
                  <Box display="flex" alignItems="center" gap={1}>
                    <Typography
                      component="span"
                      sx={{
                        fontFamily: 'monospace',
                        letterSpacing: '0.5px'
                      }}
                    >
                      {showKey === key.id ? key.key : '••••••••••••••••'}
                    </Typography>
                    <Tooltip
                      title={showKey === key.id ? 'Hide key' : 'Show key'}
                    >
                      <IconButton
                        size="small"
                        onClick={() =>
                          setShowKey(showKey === key.id ? null : key.id)
                        }
                      >
                        {showKey === key.id ? (
                          <VisibilityOffIcon />
                        ) : (
                          <VisibilityIcon />
                        )}
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Copy key">
                      <IconButton
                        size="small"
                        onClick={() => handleCopyKey(key.key)}
                      >
                        <ContentCopyIcon />
                      </IconButton>
                    </Tooltip>
                  </Box>
                </TableCell>
                <TableCell>{formatDate(key.createdAt)}</TableCell>
                <TableCell>
                  {key.lastUsed ? formatDate(key.lastUsed) : 'Never'}
                </TableCell>
                <TableCell>
                  {key.expiresAt ? (
                    <Chip
                      label={formatDate(key.expiresAt)}
                      color={
                        key.expiresAt < Date.now() ? 'error' : 'default'
                      }
                      size="small"
                    />
                  ) : (
                    'Never'
                  )}
                </TableCell>
                <TableCell>
                  {key.rateLimit?.requestsPerMinute
                    ? `${key.rateLimit.requestsPerMinute}/min`
                    : 'Default'}
                </TableCell>
                <TableCell>
                  <Box display="flex" gap={1}>
                    <Tooltip title="Edit key">
                      <IconButton
                        size="small"
                        onClick={() => handleEditKey(key)}
                      >
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Delete key">
                      <IconButton
                        size="small"
                        onClick={() => handleDeleteKey(key)}
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Tooltip>
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {selectedKey && (
        <>
          <ApiKeyDialog
            open={showEditDialog}
            onClose={() => {
              setShowEditDialog(false);
              setSelectedKey(null);
            }}
            onSubmit={async (name, options) => {
              await onUpdateKey(selectedKey.id, { name, ...options });
              setShowEditDialog(false);
              setSelectedKey(null);
            }}
            apiKey={selectedKey}
          />

          <Dialog
            open={showDeleteDialog}
            onClose={() => {
              setShowDeleteDialog(false);
              setSelectedKey(null);
            }}
          >
            <DialogTitle>Delete API Key</DialogTitle>
            <DialogContent>
              <Typography>
                Are you sure you want to delete the API key "{selectedKey.name}"?
                This action cannot be undone.
              </Typography>
            </DialogContent>
            <DialogActions>
              <Button
                onClick={() => {
                  setShowDeleteDialog(false);
                  setSelectedKey(null);
                }}
              >
                Cancel
              </Button>
              <Button
                onClick={handleConfirmDelete}
                color="error"
                variant="contained"
              >
                Delete
              </Button>
            </DialogActions>
          </Dialog>
        </>
      )}
    </Box>
  );
}