// @ts-ignore
import * as React from 'react';
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  Chip,
  IconButton,
  Collapse,
  TextField,
  InputAdornment
} from '@mui/material';
import KeyboardArrowDown from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUp from '@mui/icons-material/KeyboardArrowUp';
import Search from '@mui/icons-material/Search';
import Lock from '@mui/icons-material/Lock';
import LockOpen from '@mui/icons-material/LockOpen';
import { ApiEndpoint } from '../types/types';

interface ApiEndpointsListProps {
  endpoints: ApiEndpoint[];
}

export function ApiEndpointsList({ endpoints }: ApiEndpointsListProps) {
  const [expandedEndpoint, setExpandedEndpoint] = React.useState<string | null>(null);
  const [searchQuery, setSearchQuery] = React.useState('');

  const filteredEndpoints = endpoints.filter((endpoint) => {
    const searchLower = searchQuery.toLowerCase();
    return (
      endpoint.path.toLowerCase().includes(searchLower) ||
      endpoint.method.toLowerCase().includes(searchLower) ||
      endpoint.description.toLowerCase().includes(searchLower)
    );
  });

  const getMethodColor = (method: string): 'success' | 'info' | 'warning' | 'error' => {
    switch (method.toUpperCase()) {
      case 'GET':
        return 'success';
      case 'POST':
        return 'info';
      case 'PUT':
        return 'warning';
      case 'DELETE':
        return 'error';
      default:
        return 'warning';
    }
  };

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h6">API Endpoints</Typography>
        <TextField
          placeholder="Search endpoints..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          size="small"
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search />
              </InputAdornment>
            )
          }}
          sx={{ width: 300 }}
        />
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell padding="checkbox" />
              <TableCell>Method</TableCell>
              <TableCell>Path</TableCell>
              <TableCell>Authentication</TableCell>
              <TableCell>Rate Limit</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredEndpoints.map((endpoint) => (
              <React.Fragment key={endpoint.id}>
                <TableRow
                  hover
                  sx={{
                    '&:last-child td, &:last-child th': { border: 0 },
                    cursor: 'pointer'
                  }}
                  onClick={() =>
                    setExpandedEndpoint(
                      expandedEndpoint === endpoint.id ? null : endpoint.id
                    )
                  }
                >
                  <TableCell padding="checkbox">
                    <IconButton size="small">
                      {expandedEndpoint === endpoint.id ? (
                        <KeyboardArrowUp />
                      ) : (
                        <KeyboardArrowDown />
                      )}
                    </IconButton>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={endpoint.method}
                      color={getMethodColor(endpoint.method)}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>
                    <Typography
                      component="code"
                      sx={{
                        bgcolor: 'grey.100',
                        p: 0.5,
                        borderRadius: 1,
                        fontFamily: 'monospace'
                      }}
                    >
                      {endpoint.path}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    {endpoint.requiresAuth ? (
                      <Chip
                        icon={<Lock />}
                        label="Required"
                        color="warning"
                        size="small"
                      />
                    ) : (
                      <Chip
                        icon={<LockOpen />}
                        label="Public"
                        variant="outlined"
                        size="small"
                      />
                    )}
                  </TableCell>
                  <TableCell>
                    {endpoint.rateLimit ? (
                      `${endpoint.rateLimit.requestsPerMinute}/min (burst: ${endpoint.rateLimit.burstLimit})`
                    ) : (
                      'Default'
                    )}
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell
                    style={{ paddingBottom: 0, paddingTop: 0 }}
                    colSpan={5}
                  >
                    <Collapse
                      in={expandedEndpoint === endpoint.id}
                      timeout="auto"
                      unmountOnExit
                    >
                      <Box sx={{ margin: 2 }}>
                        <Typography variant="subtitle2" gutterBottom>
                          Description
                        </Typography>
                        <Typography
                          variant="body2"
                          color="text.secondary"
                          sx={{ mb: 2 }}
                        >
                          {endpoint.description}
                        </Typography>

                        <Typography variant="subtitle2" gutterBottom>
                          Example Request
                        </Typography>
                        <Paper
                          sx={{
                            p: 2,
                            bgcolor: 'grey.900',
                            color: 'grey.100',
                            fontFamily: 'monospace',
                            fontSize: '0.875rem'
                          }}
                        >
                          <Box component="pre" sx={{ m: 0 }}>
                            {endpoint.method} {endpoint.path}
                            {endpoint.requiresAuth && `
X-Neo-Timestamp: ${Date.now()}
X-Neo-Signature: <signature>
X-Neo-PublicKey: <public-key>
X-Neo-Salt: <salt>`}
                          </Box>
                        </Paper>
                      </Box>
                    </Collapse>
                  </TableCell>
                </TableRow>
              </React.Fragment>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}