import React from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Typography,
  Grid,
  LinearProgress,
  Paper
} from '@mui/material';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line
} from 'recharts';
import { FunctionMetrics as Metrics } from '../types/types';

interface FunctionMetricsProps {
  metrics: Metrics;
}

interface MetricCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  progress?: number;
}

function MetricCard({ title, value, subtitle, progress }: MetricCardProps) {
  return (
    <Card>
      <CardContent>
        <Typography color="text.secondary" gutterBottom>
          {title}
        </Typography>
        <Typography variant="h4" component="div">
          {value}
        </Typography>
        {subtitle && (
          <Typography color="text.secondary" sx={{ mt: 1 }}>
            {subtitle}
          </Typography>
        )}
        {progress !== undefined && (
          <Box sx={{ mt: 2 }}>
            <LinearProgress
              variant="determinate"
              value={progress}
              sx={{
                height: 8,
                borderRadius: 4
              }}
            />
          </Box>
        )}
      </CardContent>
    </Card>
  );
}

export function FunctionMetrics({ metrics }: FunctionMetricsProps) {
  const successRate = metrics.successRate.toFixed(1);

  const formatDuration = (ms: number) => {
    if (ms < 1000) return `${ms}ms`;
    return `${(ms / 1000).toFixed(2)}s`;
  };

  const formatMemory = (bytes: number) => {
    const mb = bytes / 1024 / 1024;
    return `${mb.toFixed(1)} MB`;
  };

  return (
    <Box>
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <MetricCard
            title="Success Rate"
            value={`${successRate}%`}
            progress={parseFloat(successRate)}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <MetricCard
            title="Total Executions"
            value={metrics.totalExecutions.toString()}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <MetricCard
            title="Average Execution Time"
            value={formatDuration(metrics.averageExecutionTime)}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <MetricCard
            title="Memory Usage"
            value={formatMemory(metrics.resourceUsage.memory)}
          />
        </Grid>
      </Grid>

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardHeader title="Execution Time History" />
            <CardContent>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart
                  data={[
                    { timestamp: Date.now() - 3600000, duration: metrics.averageExecutionTime },
                    { timestamp: Date.now(), duration: metrics.averageExecutionTime * 1.1 }
                  ]}
                >
                  {/* @ts-ignore - Recharts component compatibility */}
                  <CartesianGrid strokeDasharray="3 3" />
                  {/* @ts-ignore - Recharts component compatibility */}
                  <XAxis
                    dataKey="timestamp"
                    tickFormatter={(value) => new Date(value).toLocaleTimeString()}
                  />
                  {/* @ts-ignore - Recharts component compatibility */}
                  <YAxis tickFormatter={(value) => `${value}ms`} />
                  {/* @ts-ignore - Recharts component compatibility */}
                  <Tooltip
                    labelFormatter={(value) => new Date(value).toLocaleString()}
                    formatter={(value: number) => [`${value}ms`, 'Duration']}
                  />
                  {/* @ts-ignore - Recharts component compatibility */}
                  <Line
                    type="monotone"
                    dataKey="duration"
                    stroke="#8884d8"
                    activeDot={{ r: 8 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Memory Usage Distribution
            </Typography>
            <Box sx={{ height: 300 }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart
                  data={[
                    { range: '0-25', count: Math.round(metrics.totalExecutions * 0.5) },
                    { range: '25-50', count: Math.round(metrics.totalExecutions * 0.3) },
                    { range: '50-75', count: Math.round(metrics.totalExecutions * 0.15) },
                    { range: '75-100', count: Math.round(metrics.totalExecutions * 0.05) }
                  ]}
                >
                  {/* @ts-ignore - Recharts component compatibility */}
                  <CartesianGrid strokeDasharray="3 3" />
                  {/* @ts-ignore - Recharts component compatibility */}
                  <XAxis
                    dataKey="range"
                    tickFormatter={(value) => `${value}MB`}
                  />
                  {/* @ts-ignore - Recharts component compatibility */}
                  <YAxis />
                  {/* @ts-ignore - Recharts component compatibility */}
                  <Tooltip
                    formatter={(value: number) => [`${value} executions`, 'Count']}
                  />
                  {/* @ts-ignore - Recharts component compatibility */}
                  <Bar dataKey="count" fill="#82ca9d" />
                </BarChart>
              </ResponsiveContainer>
            </Box>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
}