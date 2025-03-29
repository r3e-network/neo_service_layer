'use client';

import React from 'react';
import { Container, Typography, Box } from '@mui/material';
import PriceFeedDashboard from './components/PriceFeedDashboard';
import { PRICE_FEED_CONSTANTS } from './constants';

export default function PriceFeedsPage() {
  return (
    <Container maxWidth="xl">
      <Box py={4}>
        <Typography variant="h4" component="h1" gutterBottom>
          Neo N3 Price Feeds
        </Typography>
        <Typography variant="subtitle1" color="text.secondary" paragraph>
          Real-time price data from multiple sources, aggregated and published on-chain
        </Typography>

        {PRICE_FEED_CONSTANTS.SUPPORTED_PAIRS.map((symbol) => (
          <Box key={symbol} mb={4}>
            <Typography variant="h5" gutterBottom>
              {symbol} Price Feed
            </Typography>
            <PriceFeedDashboard symbol={symbol} />
          </Box>
        ))}
      </Box>
    </Container>
  );
}