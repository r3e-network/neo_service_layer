import React from 'react';
import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend
} from 'recharts';
import { Box, ToggleButton, ToggleButtonGroup, CircularProgress } from '@mui/material';
import { formatCurrency } from '@/utils/formatters';

const TIME_RANGES = ['1h', '24h', '7d', '30d'];

interface PriceChartProps {
  data: Array<{ timestamp: number; price: number }>;
  onTimeRangeChange: (timeRange: string) => void;
  isLoading?: boolean;
  className?: string;
}

export default function PriceChart({
  data,
  onTimeRangeChange,
  isLoading = false,
  className
}: PriceChartProps) {
  const [timeRange, setTimeRange] = React.useState('24h');

  const handleTimeRangeChange = (
    _event: React.MouseEvent<HTMLElement>,
    newTimeRange: string | null
  ) => {
    if (newTimeRange) {
      setTimeRange(newTimeRange);
      onTimeRangeChange(newTimeRange);
    }
  };

  const formatPrice = (value: number) => {
    return value.toLocaleString('en-US', { minimumFractionDigits: 2 });
  };

  const formatTimestamp = (timestamp: number) => {
    const date = new Date(timestamp);
    return date.toLocaleTimeString();
  };

  if (isLoading) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight={400}
        className={className}
      >
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box className={className}>
      <Box display="flex" justifyContent="flex-end" mb={2}>
        <ToggleButtonGroup
          value={timeRange}
          exclusive
          onChange={handleTimeRangeChange}
          size="small"
        >
          {TIME_RANGES.map((range) => (
            <ToggleButton key={range} value={range}>
              {range}
            </ToggleButton>
          ))}
        </ToggleButtonGroup>
      </Box>

      <ResponsiveContainer width="100%" height={400}>
        <LineChart data={data} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="timestamp"
            tickFormatter={formatTimestamp}
          />
          <YAxis
            domain={['auto', 'auto']}
            tickFormatter={formatPrice}
          />
          <Tooltip
            formatter={(value: number) => formatPrice(value)}
            labelFormatter={formatTimestamp}
          />
          <Legend />
          <Line
            type="monotone"
            dataKey="price"
            stroke="#2196f3"
            dot={false}
            name="Price"
          />
          <Line
            type="monotone"
            dataKey="volume"
            stroke="#4caf50"
            dot={false}
            name="Volume"
            yAxisId={1}
          />
        </LineChart>
      </ResponsiveContainer>
    </Box>
  );
}