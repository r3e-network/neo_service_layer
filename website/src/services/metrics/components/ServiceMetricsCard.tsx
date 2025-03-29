import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Grid,
  Box,
  Divider,
  Accordion,
  AccordionSummary,
  AccordionDetails
} from '@mui/material';
import SpeedIcon from '@mui/icons-material/Speed';
import TimerIcon from '@mui/icons-material/Timer';
import ErrorIcon from '@mui/icons-material/Error';
import PeopleIcon from '@mui/icons-material/People';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { ServiceMetrics, ServiceHealth } from '../types/types';
import { formatNumber, formatPercentage } from '../utils/formatters';

export interface ServiceMetricsCardProps {
  serviceName: string;
  metrics: ServiceMetrics;
  health?: ServiceHealth;
}

export function ServiceMetricsCard({ serviceName, metrics, health }: ServiceMetricsCardProps) {
  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          {serviceName}
        </Typography>
        
        {health && (
          <Box sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
            <Box 
              sx={{ 
                width: 10, 
                height: 10, 
                borderRadius: '50%', 
                bgcolor: health.status === 'healthy' ? 'success.main' : 
                         health.status === 'degraded' ? 'warning.main' : 'error.main'
              }} 
            />
            <Typography variant="body2" color="text.secondary">
              Status: {health.status.charAt(0).toUpperCase() + health.status.slice(1)}
            </Typography>
          </Box>
        )}
        
        <Divider sx={{ my: 2 }} />
        
        <Accordion defaultExpanded>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box display="flex" alignItems="center" gap={1}>
              <SpeedIcon color="primary" />
              <Typography>API Metrics</Typography>
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Requests/Min</Typography>
                <Typography variant="h6">{formatNumber(metrics.api.requestsPerMinute)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Avg. Latency</Typography>
                <Typography variant="h6">{metrics.api.averageLatency.toFixed(2)} ms</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Error Rate</Typography>
                <Typography variant="h6">{formatPercentage(metrics.api.errorRate)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Active Connections</Typography>
                <Typography variant="h6">{formatNumber(metrics.api.activeConnections)}</Typography>
              </Grid>
            </Grid>
          </AccordionDetails>
        </Accordion>
        
        <Accordion>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box display="flex" alignItems="center" gap={1}>
              <TimerIcon color="primary" />
              <Typography>Functions</Typography>
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Total Functions</Typography>
                <Typography variant="h6">{formatNumber(metrics.functions.totalFunctions)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Active Functions</Typography>
                <Typography variant="h6">{formatNumber(metrics.functions.activeFunctions)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Executions/Min</Typography>
                <Typography variant="h6">{formatNumber(metrics.functions.executionsPerMinute)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Avg. Execution Time</Typography>
                <Typography variant="h6">{metrics.functions.averageExecutionTime.toFixed(2)} ms</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Failure Rate</Typography>
                <Typography variant="h6">{formatPercentage(metrics.functions.failureRate)}</Typography>
              </Grid>
            </Grid>
          </AccordionDetails>
        </Accordion>
        
        <Accordion>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box display="flex" alignItems="center" gap={1}>
              <ErrorIcon color="primary" />
              <Typography>Triggers</Typography>
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Total Triggers</Typography>
                <Typography variant="h6">{formatNumber(metrics.trigger.totalTriggers)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Active Triggers</Typography>
                <Typography variant="h6">{formatNumber(metrics.trigger.activeTriggers)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Executions/Min</Typography>
                <Typography variant="h6">{formatNumber(metrics.trigger.executionsPerMinute)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Avg. Execution Time</Typography>
                <Typography variant="h6">{metrics.trigger.averageExecutionTime.toFixed(2)} ms</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="body2" color="text.secondary">Failure Rate</Typography>
                <Typography variant="h6">{formatPercentage(metrics.trigger.failureRate)}</Typography>
              </Grid>
            </Grid>
          </AccordionDetails>
        </Accordion>
      </CardContent>
    </Card>
  );
}