'use client'; // Keep client directive if potentially adding interactive elements later

import React from 'react';
import { Typography, Paper, Box, List, ListItem, ListItemText, Divider } from '@mui/material';

// Placeholder data (replace with actual API info retrieval later)
const apiServices = [
  { name: 'Price Feed', description: 'Access real-time and historical asset price data.' },
  { name: 'Contract Automation', description: 'Manage automated smart contract execution tasks.' },
  { name: 'Gas Bank', description: 'Handle GAS funding and management for contracts.' },
  { name: 'Transaction', description: 'Create, sign, and broadcast transactions.' },
  { name: 'Functions', description: 'Deploy and manage serverless TEE functions.' },
  { name: 'Secrets', description: 'Securely store and manage sensitive data.' },
  { name: 'Trigger Service', description: 'React to on-chain events automatically.' },
  { name: 'Metrics', description: 'Retrieve service usage and performance metrics.' },
  { name: 'Logging', description: 'Access centralized service logs.' },
];

export default function ApiReferencePage() {
  return (
    <Paper elevation={0} sx={{ p: { xs: 2, sm: 3 } }}> {/* Use elevation 0 for seamless feel in docs */}
      <Typography variant="h4" component="h1" gutterBottom sx={{ fontWeight: 'bold' }}>
        API Reference
      </Typography>
      <Typography variant="body1" color="text.secondary" paragraph>
        Welcome to the Neo Service Layer API reference. Here you will find detailed information about available endpoints, 
        request parameters, response formats, and usage examples for each service.
      </Typography>
      <Divider sx={{ my: 3 }} />
      
      <Typography variant="h5" component="h2" gutterBottom>
        Available Service APIs
      </Typography>
      <Typography variant="body2" color="text.secondary" paragraph>
        Select a service below to view its specific API endpoints and documentation (coming soon).
      </Typography>

      <Box sx={{ mt: 2 }}>
        <List disablePadding>
          {apiServices.map((service, index) => (
            <ListItem key={service.name} disablePadding sx={{ py: 1, borderBottom: index < apiServices.length - 1 ? '1px dashed #e0e0e0' : 'none' }}>
              <ListItemText 
                primary={service.name}
                secondary={service.description}
                primaryTypographyProps={{ fontWeight: 'medium' }}
              />
              {/* Future link: <Button size="small">View Docs</Button> */}
            </ListItem>
          ))}
        </List>
      </Box>
      
      <Box mt={4} bgcolor="action.hover" p={2} borderRadius={1}> 
         <Typography variant="caption" color="text.secondary">
             Note: Detailed endpoint specifications are currently under development and will be published soon. 
             Please refer to the specific service documentation pages under "Getting Started" or "Services" for current usage guides.
         </Typography>
      </Box>
    </Paper>
  );
} 