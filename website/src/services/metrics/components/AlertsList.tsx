import React from 'react';
import {
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Typography,
  Chip,
  Box,
  Paper
} from '@mui/material';
import ErrorIcon from '@mui/icons-material/Error';
import WarningIcon from '@mui/icons-material/Warning';
import { formatDate } from '../utils/formatters';

interface Alert {
  id: string;
  serviceName: string;
  message: string;
  severity: 'critical' | 'warning';
  timestamp: number;
}

interface AlertsListProps {
  alerts: Alert[];
}

export function AlertsList({ alerts }: AlertsListProps) {
  if (alerts.length === 0) {
    return null;
  }

  return (
    <Paper sx={{ mb: 3 }}>
      <Box sx={{ p: 2, bgcolor: 'background.default' }}>
        <Typography variant="h6" gutterBottom>
          Active Alerts
        </Typography>
        <List>
          {alerts.map((alert) => (
            <ListItem
              key={alert.id}
              sx={{
                bgcolor: 'background.paper',
                mb: 1,
                borderRadius: 1,
                border: 1,
                borderColor: alert.severity === 'critical' ? 'error.main' : 'warning.main'
              }}
            >
              <ListItemIcon>
                {alert.severity === 'critical' ? (
                  <ErrorIcon color="error" />
                ) : (
                  <WarningIcon color="warning" />
                )}
              </ListItemIcon>
              <ListItemText
                primary={
                  <Box display="flex" alignItems="center" gap={1}>
                    <Typography variant="subtitle1">
                      {alert.serviceName}
                    </Typography>
                    <Chip
                      label={alert.severity}
                      size="small"
                      color={alert.severity === 'critical' ? 'error' : 'warning'}
                    />
                  </Box>
                }
                secondary={
                  <Box>
                    <Typography variant="body2" color="text.primary">
                      {alert.message}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {formatDate(alert.timestamp)}
                    </Typography>
                  </Box>
                }
              />
            </ListItem>
          ))}
        </List>
      </Box>
    </Paper>
  );
}