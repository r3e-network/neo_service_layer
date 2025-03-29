import React from 'react';
import { useRouter } from 'next/router';
import { Box, Container, Typography, Paper } from '@mui/material';
import PriceFeedDashboard from '../../src/services/price-feeds/components/PriceFeedDashboard';

const PriceFeedPage: React.FC = () => {
  const router = useRouter();
  const { symbol } = router.query;

  if (!symbol || typeof symbol !== 'string') {
    return (
      <Container maxWidth="lg">
        <Box sx={{ py: 4 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h4" component="h1" gutterBottom>
              Invalid Symbol
            </Typography>
            <Typography>
              Please provide a valid trading pair symbol.
            </Typography>
          </Paper>
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box sx={{ py: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Price Feed: {symbol.toUpperCase()}
        </Typography>
        <PriceFeedDashboard symbol={symbol} />
      </Box>
    </Container>
  );
};

export default PriceFeedPage;