import React from 'react';
import {
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Chip,
  Typography,
  Box,
  Paper,
  Tooltip,
  Stack
} from '@mui/material';
import AccountBalance from '@mui/icons-material/AccountBalance';
import Warning from '@mui/icons-material/Warning';
import { GasAccount } from '../types/types';
import { formatGas, formatDate } from '../utils/formatters';

interface GasAccountListProps {
  accounts: GasAccount[];
  selectedAddress?: string;
  onSelectAccount: (address: string) => void;
}

export function GasAccountList({
  accounts,
  selectedAddress,
  onSelectAccount
}: GasAccountListProps) {
  const getStatusColor = (status: string): 'success' | 'warning' | 'error' | 'default' => {
    switch (status) {
      case 'active':
        return 'success';
      case 'low':
        return 'warning';
      case 'depleted':
      case 'locked':
        return 'error';
      default:
        return 'default';
    }
  };

  return (
    <List>
      {accounts.map((account) => (
        <ListItem
          key={account.id}
          onClick={() => onSelectAccount(account.address)}
          sx={{
            cursor: 'pointer',
            '&:hover': {
              backgroundColor: 'action.hover'
            },
            mb: 1,
            borderRadius: 1,
            border: 1,
            borderColor: 'divider',
            bgcolor: account.address === selectedAddress ? 'action.selected' : 'transparent'
          }}
        >
          <ListItemText
            primary={
              <Box display="flex" alignItems="center" gap={1}>
                <AccountBalance color="primary" />
                <Typography variant="subtitle1">
                  {account.address}
                </Typography>
                <Chip
                  label={account.status}
                  size="small"
                  color={getStatusColor(account.status)}
                />
              </Box>
            }
            secondary={
              <Box mt={1}>
                <Stack direction="row" spacing={2}>
                  <Box flex={1}>
                    <Typography variant="body2" color="text.secondary">
                      Balance
                    </Typography>
                    <Typography variant="h6">
                      {formatGas(account.balance)}
                    </Typography>
                  </Box>
                  <Box flex={1}>
                    <Typography variant="body2" color="text.secondary">
                      Reserved
                    </Typography>
                    <Typography variant="h6">
                      {formatGas(account.reserved)}
                    </Typography>
                  </Box>
                  <Box flex={1}>
                    <Typography variant="body2" color="text.secondary">
                      Available
                    </Typography>
                    <Typography variant="h6">
                      {formatGas(account.available)}
                    </Typography>
                  </Box>
                </Stack>
                <Typography variant="caption" color="text.secondary">
                  Last Updated: {formatDate(new Date(account.lastUpdated))}
                </Typography>
              </Box>
            }
          />
          {account.status === 'low' && (
            <ListItemSecondaryAction>
              <Tooltip title="Gas balance is low">
                <IconButton edge="end" color="warning">
                  <Warning />
                </IconButton>
              </Tooltip>
            </ListItemSecondaryAction>
          )}
        </ListItem>
      ))}
    </List>
  );
}