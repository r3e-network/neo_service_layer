import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Typography,
  Box,
  Alert
} from '@mui/material';
import { GasAccount } from '../types/types';

interface GasAccountDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (address: string) => Promise<void>;
  existingAccounts: GasAccount[];
}

export function GasAccountDialog({
  open,
  onClose,
  onSubmit,
  existingAccounts
}: GasAccountDialogProps) {
  const [address, setAddress] = React.useState('');
  const [error, setError] = React.useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = React.useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    // Validate Neo N3 address format
    const neoAddressRegex = /^N[A-HJ-NP-Za-km-z1-9]{33}$/;
    if (!neoAddressRegex.test(address)) {
      setError('Invalid Neo N3 address format');
      return;
    }

    // Check if address already exists
    if (existingAccounts.some(account => account.address === address)) {
      setError('This address already has a gas account');
      return;
    }

    try {
      setIsSubmitting(true);
      await onSubmit(address);
      setAddress('');
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create gas account');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClose = () => {
    setAddress('');
    setError(null);
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>Create Gas Account</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2 }}>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Enter a Neo N3 address to create a new gas account. The address must be in the
              correct format and not already have an existing gas account.
            </Typography>

            <TextField
              autoFocus
              margin="dense"
              label="Neo N3 Address"
              fullWidth
              value={address}
              onChange={(e) => setAddress(e.target.value)}
              error={!!error}
              helperText={error}
              disabled={isSubmitting}
              placeholder="N..."
              sx={{ mt: 2 }}
            />

            <Alert severity="info" sx={{ mt: 2 }}>
              Make sure you have access to this address as you'll need to sign
              transactions to manage gas.
            </Alert>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={!address || isSubmitting}
          >
            {isSubmitting ? 'Creating...' : 'Create Account'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}