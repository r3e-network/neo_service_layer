import React from 'react';
import {
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Paper,
  Typography,
  Tooltip,
  Chip,
  Box
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';
import PendingIcon from '@mui/icons-material/Pending';
import { UserFunction, FunctionStatus } from '../types/types';

interface FunctionsListProps {
  functions: UserFunction[];
  selectedId?: string;
  onSelect: (id: string) => void;
  onDelete: (id: string) => void;
}

export function FunctionsList({
  functions,
  selectedId,
  onSelect,
  onDelete
}: FunctionsListProps) {
  const handleDelete = (event: React.MouseEvent, id: string) => {
    event.stopPropagation();
    onDelete(id);
  };

  const getStatusColor = (status: FunctionStatus): 'success' | 'error' | 'warning' | 'default' => {
    switch (status) {
      case 'deployed':
        return 'success';
      case 'failed':
        return 'error';
      case 'deprecated':
        return 'warning';
      case 'disabled':
        return 'error';
      default:
        return 'default';
    }
  };

  const getStatusIcon = (status: FunctionStatus) => {
    switch (status) {
      case 'deployed':
        return <CheckCircleIcon fontSize="small" />;
      case 'failed':
        return <ErrorIcon fontSize="small" />;
      case 'draft':
        return <PendingIcon fontSize="small" />;
      default:
        return null;
    }
  };

  if (!functions.length) {
    return (
      <Paper sx={{ p: 3, textAlign: 'center' }}>
        <Typography color="text.secondary">
          No functions found
        </Typography>
      </Paper>
    );
  }

  return (
    <Paper>
      <List sx={{ width: '100%', bgcolor: 'background.paper' }}>
        {functions.map((func) => (
          <ListItem
            key={func.id}
            onClick={() => onSelect(func.id)}
            sx={{ 
              '&:hover': { backgroundColor: 'action.hover' },
              ...(selectedId === func.id && { backgroundColor: 'action.selected' })
            }}
          >
            <ListItemText
              primary={
                <Box display="flex" alignItems="center" gap={1}>
                  <Typography
                    component="span"
                    variant="body1"
                    sx={{
                      fontWeight: selectedId === func.id ? 'bold' : 'normal',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap'
                    }}
                  >
                    {func.name}
                  </Typography>
                  <Chip
                    size="small"
                    label={func.status.toLowerCase()}
                    color={getStatusColor(func.status)}
                    sx={{ ml: 1 }}
                  />
                  {getStatusIcon(func.status)}
                </Box>
              }
              secondary={
                <Typography
                  component="span"
                  variant="body2"
                  color="text.secondary"
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1
                  }}
                >
                  <Chip
                    size="small"
                    label={func.language}
                    variant="outlined"
                  />
                  {func.lastExecuted && (
                    <span>Last run: {new Date(func.lastExecuted).toLocaleDateString()}</span>
                  )}
                </Typography>
              }
            />
            <ListItemSecondaryAction>
              <Tooltip title="Delete function">
                <IconButton
                  edge="end"
                  aria-label="delete"
                  onClick={(e) => handleDelete(e, func.id)}
                  size="small"
                >
                  <DeleteIcon />
                </IconButton>
              </Tooltip>
            </ListItemSecondaryAction>
          </ListItem>
        ))}
      </List>
    </Paper>
  );
}