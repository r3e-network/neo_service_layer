// @ts-ignore
import * as React from 'react';
import {
  Box,
  Paper,
  Typography,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Card,
  CardContent,
  LinearProgress
} from '@mui/material';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell
} from 'recharts';
import { ApiUsage, ApiMetrics } from '../types/types';

interface ApiUsageMetricsProps {
  usage: ApiUsage[];
  metrics: ApiMetrics | null;
  onFetchUsage: (options?: {
    startTime?: number;
    endTime?: number;
    endpoint?: string;
  }) => void;
  onFetchMetrics: (options?: {
    interval?: '1m' | '5m' | '1h' | '1d';
    duration?: '1h' | '1d' | '7d' | '30d';
  }) => void;
}

const COLORS = ['#8884d8', '#82ca9d', '#ffc658', '#ff8042'];

export function ApiUsageMetrics({
  usage,
  metrics,
  onFetchUsage,
  onFetchMetrics
}: ApiUsageMetricsProps) {
  const [timeRange, setTimeRange] = React.useState<'1h' | '1d' | '7d' | '30d'>('1d');
  const [interval, setInterval] = React.useState<'1m' | '5m' | '1h' | '1d'>('5m');

  const handleTimeRangeChange = (event: any) => {
    const newRange = event.target.value;
    setTimeRange(newRange);
    onFetchMetrics({ interval, duration: newRange });
  };

  const handleIntervalChange = (event: any) => {
    const newInterval = event.target.value;
    setInterval(newInterval);
    onFetchMetrics({ interval: newInterval, duration: timeRange });
  };

  if (!metrics) return null;

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h6">API Usage & Metrics</Typography>
        <Box display="flex" gap={2}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Time Range</InputLabel>
            <Select
              value={timeRange}
              label="Time Range"
              onChange={handleTimeRangeChange}
            >
              <MenuItem value="1h">Last Hour</MenuItem>
              <MenuItem value="1d">Last 24 Hours</MenuItem>
              <MenuItem value="7d">Last 7 Days</MenuItem>
              <MenuItem value="30d">Last 30 Days</MenuItem>
            </Select>
          </FormControl>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Interval</InputLabel>
            <Select
              value={interval}
              label="Interval"
              onChange={handleIntervalChange}
            >
              <MenuItem value="1m">1 Minute</MenuItem>
              <MenuItem value="5m">5 Minutes</MenuItem>
              <MenuItem value="1h">1 Hour</MenuItem>
              <MenuItem value="1d">1 Day</MenuItem>
            </Select>
          </FormControl>
        </Box>
      </Box>

      <Box sx={{ mb: 4 }}>
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" gutterBottom>
                  Requests per Second
                </Typography>
                <Typography variant="h4">
                  {metrics.requestsPerSecond.toFixed(2)}
                </Typography>
                <LinearProgress
                  variant="determinate"
                  value={Math.min((metrics.requestsPerSecond / 100) * 100, 100)}
                  sx={{ mt: 2 }}
                />
              </CardContent>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" gutterBottom>
                  Average Response Time
                </Typography>
                <Typography variant="h4">
                  {metrics.averageLatency.toFixed(2)} ms
                </Typography>
                <LinearProgress
                  variant="determinate"
                  value={Math.min((metrics.averageLatency / 1000) * 100, 100)}
                  sx={{ mt: 2 }}
                />
              </CardContent>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" gutterBottom>
                  Error Rate
                </Typography>
                <Typography variant="h4">
                  {(metrics.errorRate * 100).toFixed(2)}%
                </Typography>
                <LinearProgress
                  variant="determinate"
                  value={Math.min(metrics.errorRate * 100, 100)}
                  color="error"
                  sx={{ mt: 2 }}
                />
              </CardContent>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <Card>
              <CardContent>
                <Typography color="text.secondary" gutterBottom>
                  Active Connections
                </Typography>
                <Typography variant="h4">{metrics.activeConnections}</Typography>
                <LinearProgress
                  variant="determinate"
                  value={Math.min((metrics.activeConnections / 100) * 100, 100)}
                  color="success"
                  sx={{ mt: 2 }}
                />
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </Box>

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Request Volume Over Time
            </Typography>
            <Box height={300}>
              <ResponsiveContainer width="100%" height="100%">
                {/* @ts-ignore - Recharts component type issue */}
                <LineChart data={metrics.timeseriesData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  {/* @ts-ignore - Recharts component type issue */}
                  <XAxis
                    dataKey="timestamp"
                    tickFormatter={(value) => new Date(value).toLocaleTimeString()}
                  />
                  {/* @ts-ignore - Recharts component type issue */}
                  <YAxis />
                  {/* @ts-ignore - Recharts component type issue */}
                  <Tooltip
                    labelFormatter={(value) => new Date(value).toLocaleString()}
                  />
                  {/* @ts-ignore - Recharts component type issue */}
                  <Line
                    type="monotone"
                    dataKey="requests"
                    stroke="#8884d8"
                    strokeWidth={2}
                  />
                </LineChart>
              </ResponsiveContainer>
            </Box>
          </Paper>
        </Grid>
        <Grid size={{ xs: 12, md: 4 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Endpoint Distribution
            </Typography>
            <Box height={300}>
              <ResponsiveContainer width="100%" height="100%">
                {/* @ts-ignore - Recharts component type issue */}
                <PieChart>
                  {/* @ts-ignore - Recharts component type issue */}
                  <Pie
                    data={Object.entries(metrics.endpointMetrics).map(
                      ([endpoint, data]) => ({
                        name: endpoint,
                        value: data.requests
                      })
                    )}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={80}
                    fill="#8884d8"
                    paddingAngle={5}
                    dataKey="value"
                  >
                    {Object.entries(metrics.endpointMetrics).map((entry, index) => (
                      <Cell
                        key={`cell-${index}`}
                        fill={COLORS[index % COLORS.length]}
                      />
                    ))}
                  </Pie>
                  {/* @ts-ignore - Recharts component type issue */}
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
            </Box>
          </Paper>
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Error Distribution
            </Typography>
            <Box height={300}>
              <ResponsiveContainer width="100%" height="100%">
                {/* @ts-ignore - Recharts component type issue */}
                <BarChart data={metrics.timeseriesData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  {/* @ts-ignore - Recharts component type issue */}
                  <XAxis
                    dataKey="timestamp"
                    tickFormatter={(value) => new Date(value).toLocaleTimeString()}
                  />
                  {/* @ts-ignore - Recharts component type issue */}
                  <YAxis />
                  {/* @ts-ignore - Recharts component type issue */}
                  <Tooltip
                    labelFormatter={(value) => new Date(value).toLocaleString()}
                  />
                  {/* @ts-ignore - Recharts component type issue */}
                  <Bar dataKey="errors" fill="#ff8042" />
                </BarChart>
              </ResponsiveContainer>
            </Box>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
}