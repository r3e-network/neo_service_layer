'use client';

// @ts-ignore
import * as React from 'react';
import { CodeBlock } from '../../components/CodeBlock';
import { WalletConnect } from '../../components/WalletConnect';
import { ServiceClient } from '../../lib/serviceClient';
import {
  Box,
  Typography,
  Paper,
  Button,
  TextField,
  MenuItem,
  Select,
  InputLabel,
  FormControl,
  Grid,
  Chip,
  CircularProgress,
  Alert,
  Accordion,
  AccordionSummary,
  AccordionDetails
} from '@mui/material';
import { 
  ExpandMore as ExpandMoreIcon,
  PlayArrow as PlayArrowIcon,
  ContentCopy as ContentCopyIcon,
  Check as CheckIcon,
  Code as CodeIcon,
  Settings as SettingsIcon,
  Send as SendIcon,
  Info as InfoIcon
} from '@mui/icons-material';
import { motion } from 'framer-motion';

// Import example code and service definitions
import { exampleCode as defaultExampleCode, exampleSnippets, services, serviceDocumentation } from './playground-data';

export default function PlaygroundPage() {
  // Initialize with the first service and its first endpoint
  const defaultService = services[0];
  const defaultEndpoint = defaultService.endpoints[0];

  const [selectedService, setSelectedService] = React.useState<string>(defaultService.name);
  const [selectedEndpoint, setSelectedEndpoint] = React.useState<string>(defaultEndpoint.value);
  
  const [response, setResponse] = React.useState('');
  const [loading, setLoading] = React.useState(false);
  const [client, setClient] = React.useState<ServiceClient | null>(null);
  // Initialize params based on default endpoint
  const [requestParams, setRequestParams] = React.useState<Record<string, any>>(() => {
      const templates: Record<string, Record<string, any>> = {
        'neo-usd': {},
        'gas-usd': {},
        'btc-usd': {},
        'eth-usd': {},
        'subscribe': {
          symbol: 'NEO/USD',
          interval: '1h',
        },
        'get-history': {
          symbol: 'NEO/USD',
          from: '2023-01-01',
          to: '2023-01-31',
        },
        'create-task': {
          name: 'Price Update Task',
          contract: '',
          method: 'updatePrice',
          schedule: '*/30 * * * *',
          params: [],
        },
        'set-auto-funding': {
          threshold: 100,
          targetBalance: 500,
          maxGasPerTransaction: 10,
        },
        'deploy-function': {
          name: '',
          source: '',
          runtime: 'node16',
          memory: 256,
          timeout: 30,
        },
        'set-secret': {
          key: '',
          value: '',
          description: '',
          expiration: '30d',
          rotationSchedule: '7d',
        },
        'create-trigger': {
          name: '',
          contract: '',
          event: '',
          action: {
            type: 'function',
            name: '',
            params: [],
          },
        },
        'create-transaction': {
          type: 'transfer',
          to: '',
          amount: '',
          asset: 'GAS',
        },
        'sign-transaction': {
          id: '',
        },
        'send-transaction': {
          id: '',
        },
        'get-transaction-status': {
          hash: '',
        },
        'get-transaction': {
          id: '',
        },
        'list-transactions': {
          page: 1,
          pageSize: 10,
          status: '',
        },
        'estimate-fee': {
          type: 'transfer',
          to: '',
          amount: '',
          asset: 'GAS',
        },
      };
      return templates[defaultEndpoint.value] || {};
  });
  const [codeCopied, setCodeCopied] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  
  // Update example code based on selected service and endpoint
  const getExampleCode = React.useCallback(() => {
    if (!selectedService || !selectedEndpoint) return defaultExampleCode;
    
    return exampleSnippets[selectedEndpoint] || defaultExampleCode;
  }, [selectedService, selectedEndpoint]);

  // Get parameter template based on endpoint
  const getParamTemplate = React.useCallback(() => {
    const templates: Record<string, Record<string, any>> = {
      'neo-usd': {},
      'gas-usd': {},
      'btc-usd': {},
      'eth-usd': {},
      'subscribe': {
        symbol: 'NEO/USD',
        interval: '1h',
      },
      'get-history': {
        symbol: 'NEO/USD',
        from: '2023-01-01',
        to: '2023-01-31',
      },
      'create-task': {
        name: 'Price Update Task',
        contract: '',
        method: 'updatePrice',
        schedule: '*/30 * * * *',
        params: [],
      },
      'set-auto-funding': {
        threshold: 100,
        targetBalance: 500,
        maxGasPerTransaction: 10,
      },
      'deploy-function': {
        name: '',
        source: '',
        runtime: 'node16',
        memory: 256,
        timeout: 30,
      },
      'set-secret': {
        key: '',
        value: '',
        description: '',
        expiration: '30d',
        rotationSchedule: '7d',
      },
      'create-trigger': {
        name: '',
        contract: '',
        event: '',
        action: {
          type: 'function',
          name: '',
          params: [],
        },
      },
      'create-transaction': {
        type: 'transfer',
        to: '',
        amount: '',
        asset: 'GAS',
      },
      'sign-transaction': {
        id: '',
      },
      'send-transaction': {
        id: '',
      },
      'get-transaction-status': {
        hash: '',
      },
      'get-transaction': {
        id: '',
      },
      'list-transactions': {
        page: 1,
        pageSize: 10,
        status: '',
      },
      'estimate-fee': {
        type: 'transfer',
        to: '',
        amount: '',
        asset: 'GAS',
      },
    };

    return templates[selectedEndpoint] || {};
  }, [selectedEndpoint]);

  const handleParamChange = (key: string, value: any) => {
    setRequestParams((prev) => ({
      ...prev,
      [key]: value,
    }));
  };

  const handleCopyCode = async () => {
    try {
      await navigator.clipboard.writeText(getExampleCode());
      setCodeCopied(true);
      setTimeout(() => setCodeCopied(false), 2000);
    } catch (err) {
      console.error('Failed to copy code', err);
    }
  };

  const handleExecute = async () => {
    if (!selectedService || !selectedEndpoint || !client) {
      setError('Please connect wallet and select a service and endpoint');
      return;
    }

    setLoading(true);
    setError(null);
    
    try {
      const result = await client.execute(
        selectedService.toLowerCase().replace(' ', '-'),
        selectedEndpoint,
        requestParams
      );
      setResponse(JSON.stringify(result, null, 2));
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      setError(`Request failed: ${errorMessage}`);
      setResponse(JSON.stringify({
        status: 'error',
        message: errorMessage,
      }, null, 2));
    } finally {
      setLoading(false);
    }
  };

  // Get all endpoints for the selected service
  const currentEndpoints = React.useMemo(() => {
    const serviceObj = services.find(service => service.name === selectedService);
    return serviceObj ? serviceObj.endpoints : [];
  }, [selectedService]);

  // Get the currently selected endpoint object
  const currentEndpoint = React.useMemo(() => {
    return currentEndpoints.find(endpoint => endpoint.value === selectedEndpoint);
  }, [currentEndpoints, selectedEndpoint]);

  // Get the description of the selected service
  const serviceDescription = React.useMemo(() => {
    const serviceObj = services.find(service => service.name === selectedService);
    return serviceObj ? serviceObj.description : '';
  }, [selectedService]);

  // Get detailed documentation for the selected service
  const detailedServiceDoc = React.useMemo(() => {
    return selectedService ? serviceDocumentation[selectedService] || '' : '';
  }, [selectedService]);

  return (
    <motion.div 
      initial={{ opacity: 0 }} 
      whileInView={{ opacity: 1 }}
      viewport={{ once: true }}
      transition={{ duration: 0.5 }}
      className="py-16 sm:py-24"
    >
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="text-center mb-12">
          <Typography variant="h3" component="h1" gutterBottom className="font-bold tracking-tight">
            Neo Service Layer Playground
          </Typography>
          <Typography variant="h6" component="p" color="textSecondary" className="max-w-3xl mx-auto">
            Select a service, configure parameters, and execute requests directly. Connect your NeoLine wallet to interact with the network.
          </Typography>
          <Box mt={4} className="flex justify-center">
            <WalletConnect onConnect={setClient} />
          </Box>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 lg:gap-12 items-start">
          
          <div className="space-y-6">
            <Paper elevation={2} className="p-6 rounded-lg">
              <Typography variant="h6" component="h2" gutterBottom>
                1. Select Service & Endpoint
              </Typography>
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6}>
                  <FormControl fullWidth variant="outlined" size="small">
                    <InputLabel id="service-select-label">Service</InputLabel>
                    <Select
                      labelId="service-select-label"
                      value={selectedService}
                      onChange={(e) => {
                        setSelectedService(e.target.value as string);
                        setSelectedEndpoint('');
                        setError(null);
                        setResponse('');
                      }}
                      label="Service"
                    >
                      {services.map((service) => (
                        <MenuItem key={service.name} value={service.name}>
                          {service.name}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <FormControl fullWidth variant="outlined" size="small" disabled={!selectedService}>
                    <InputLabel id="endpoint-select-label">Endpoint</InputLabel>
                    <Select
                      labelId="endpoint-select-label"
                      value={selectedEndpoint}
                      onChange={(e) => setSelectedEndpoint(e.target.value as string)}
                      label="Endpoint"
                    >
                      {currentEndpoints.map((endpoint) => (
                        <MenuItem key={endpoint.value} value={endpoint.value}>
                          {endpoint.name}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
              </Grid>
              {(selectedService || selectedEndpoint) && (
                <Accordion elevation={0} defaultExpanded={false} className="mt-4 bg-gray-50/50 rounded">
                  <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                      <InfoIcon fontSize="small" color="action" sx={{ mr: 1 }} />
                      <Typography variant="subtitle2">
                        {selectedEndpoint ? `${currentEndpoint?.name} Info` : `${selectedService} Info`}
                      </Typography>
                  </AccordionSummary>
                  <AccordionDetails>
                    {selectedEndpoint && currentEndpoint && (
                       <Typography variant="body2" color="textSecondary" paragraph>
                         <strong>Description:</strong> {currentEndpoint.description || 'No description available.'} 
                      </Typography>
                    )}
                    {selectedService && (
                      <Box mt={selectedEndpoint ? 2 : 0}>
                        <Typography variant="subtitle2" component="div" className="mb-1">About {selectedService}</Typography>
                        <Typography variant="body2" color="textSecondary" paragraph>
                          {detailedServiceDoc || serviceDescription}
                        </Typography>
                      </Box>
                    )}
                  </AccordionDetails>
                </Accordion>
              )}
            </Paper>

            {selectedEndpoint && (
              <Paper elevation={2} className="p-6 rounded-lg">
                <Typography variant="h6" component="h2" gutterBottom>
                  2. Configure Parameters
                </Typography>
                 {Object.keys(requestParams).length > 0 ? (
                    <Grid container spacing={2} className="mt-2">
                      {Object.entries(requestParams).map(([key, value]) => (
                        <Grid item xs={12} sm={6} key={key}>
                          <TextField
                            fullWidth
                            size="small"
                            label={key}
                            variant="outlined"
                            value={typeof value === 'object' ? JSON.stringify(value, null, 2) : String(value ?? '')}
                            onChange={(e) => {
                              let newValue: any = e.target.value;
                              if ((typeof value === 'object' || Array.isArray(value)) && value !== null) {
                                try {
                                  newValue = JSON.parse(e.target.value);
                                } catch (parseError) {
                                }
                              }
                              handleParamChange(key, newValue);
                            }}
                            multiline={typeof value === 'object'}
                            rows={typeof value === 'object' ? 4 : 1}
                            InputLabelProps={{ shrink: true }}
                             sx={{ '.MuiInputBase-inputMultiline': { fontFamily: 'monospace' } }}
                          />
                        </Grid>
                      ))}
                    </Grid>
                  ) : (
                    <Alert severity="info" variant="outlined" className="mt-2">
                      This endpoint does not require parameters.
                    </Alert>
                  )}
                <Box mt={4} display="flex" justifyContent="flex-end">
                  <Button
                    variant="contained"
                    color="primary"
                    size="large"
                    startIcon={loading ? <CircularProgress size={20} color="inherit" /> : <PlayArrowIcon />}
                    disabled={loading || !client || !selectedEndpoint}
                    onClick={handleExecute}
                    sx={{ minWidth: '150px' }}
                  >
                    {loading ? 'Executing...' : 'Execute'}
                  </Button>
                </Box>
              </Paper>
            )}
          </div>

          <div className="space-y-6">
             <Paper elevation={2} className="p-6 rounded-lg sticky top-24">
                <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                  <Typography variant="h6" component="h2">
                    Example Code
                  </Typography>
                  <Button
                    size="small"
                    variant="outlined"
                    startIcon={codeCopied ? <CheckIcon fontSize="small"/> : <ContentCopyIcon fontSize="small"/>}
                    onClick={handleCopyCode}
                    disabled={!selectedEndpoint}
                  >
                    {codeCopied ? 'Copied!' : 'Copy'}
                  </Button>
                </Box>
                <CodeBlock 
                  code={getExampleCode()} 
                  language="typescript" 
                  showLineNumbers={false}
                />
             </Paper>

            {(response || error) && (
              <Paper elevation={2} className="p-6 rounded-lg">
                <Typography variant="h6" component="h2" gutterBottom>
                  Response
                </Typography>
                {error && (
                  <Alert severity="error" variant="filled" className="mb-4">
                    {error}
                  </Alert>
                )}
                 <CodeBlock 
                  code={response} 
                  language="json" 
                  showLineNumbers={false}
                 />
              </Paper>
            )}
          </div>

        </div>

        <Box mt={12} textAlign="center">
          <Typography variant="body1" color="textSecondary" gutterBottom>
            Need more help?
          </Typography>
          <Typography variant="body2" color="textSecondary">
            Check out our <a href="/docs" className="text-blue-600 hover:underline">documentation</a> or
            <a href="https://github.com/neo-project/neo-service-layer" target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:underline ml-1">GitHub repository</a> for more examples and guides.
          </Typography>
        </Box>
      </div>
    </motion.div>
  );
}