'use client';

import React from 'react';
import { Box } from '@mui/material';
import SecretsDashboard from './components/SecretsDashboard';
import { SecretsProvider } from './context/SecretsContext';

export default function SecretsPage() {
  return (
    <Box>
      <SecretsProvider>
        <SecretsDashboard />
      </SecretsProvider>
    </Box>
  );
}