// Recharts components have TypeScript compatibility issues with React 18 and MUI v7
import React from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer
} from 'recharts';
import { Box, ButtonGroup, Button, Typography } from '@mui/material';
import { useTheme } from '@mui/material/styles';

interface PriceChartProps {
  symbol: string;
}

interface PriceDataPoint {
  timestamp: number;
  price: number;
  confidence: number;
  velocity?: number;
  acceleration?: number;
}

const TIME_RANGES = {
  '1H': 60 * 60 * 1000,
  '24H': 24 * 60 * 60 * 1000,
  '7D': 7 * 24 * 60 * 60 * 1000,
  '30D': 30 * 24 * 60 * 60 * 1000
};

const PriceChart: React.FC<PriceChartProps> = ({ symbol }) => {
  const theme = useTheme();
  const [timeRange, setTimeRange] = React.useState<keyof typeof TIME_RANGES>('24H');
  const [data, setData] = React.useState<PriceDataPoint[]>([]);
  const [showAdvanced, setShowAdvanced] = React.useState(false);

  React.useEffect(() => {
    const fetchHistoricalData = async () => {
      try {
        const response = await fetch(
          `/api/price-feed/${symbol}/history?timeRange=${TIME_RANGES[timeRange]}`
        );
        const historicalData = await response.json();
        setData(historicalData);
      } catch (error) {
        console.error('Failed to fetch historical data:', error);
      }
    };

    fetchHistoricalData();
  }, [symbol, timeRange]);

  return (
    <Box sx={{ width: '100%', height: 400 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Typography variant="h6">Price History</Typography>
        <Box>
          <ButtonGroup size="small" aria-label="time range selection">
            {Object.keys(TIME_RANGES).map((range) => (
              <Button
                key={range}
                onClick={() => setTimeRange(range as keyof typeof TIME_RANGES)}
                variant={timeRange === range ? 'contained' : 'outlined'}
              >
                {range}
              </Button>
            ))}
          </ButtonGroup>
          <Button
            sx={{ ml: 2 }}
            size="small"
            onClick={() => setShowAdvanced(!showAdvanced)}
          >
            {showAdvanced ? 'Hide Advanced' : 'Show Advanced'}
          </Button>
        </Box>
      </Box>

      <ResponsiveContainer width="100%" height={350}>
        {/* @ts-ignore - Recharts component compatibility */}
        <LineChart
          data={data}
          margin={{
            top: 5,
            right: 30,
            left: 20,
            bottom: 5
          }}
        >
          {/* @ts-ignore - Recharts component compatibility */}
          <CartesianGrid strokeDasharray="3 3" />
          {/* @ts-ignore - Recharts component compatibility */}
          <XAxis
            dataKey="timestamp"
            tickFormatter={(timestamp) => new Date(timestamp).toLocaleTimeString()}
          />
          {/* @ts-ignore - Recharts component compatibility */}
          <YAxis yAxisId="price" domain={['auto', 'auto']} />
          {showAdvanced && (
            /* @ts-ignore - Recharts component compatibility */
            <YAxis
              yAxisId="metrics"
              orientation="right"
              domain={[-1, 1]}
            />
          )}
          {/* @ts-ignore - Recharts component compatibility */}
          <Tooltip
            formatter={(value: number, name: string) => [
              name === 'price'
                ? `$${value.toFixed(2)}`
                : value.toFixed(4),
              name.charAt(0).toUpperCase() + name.slice(1)
            ]}
            labelFormatter={(timestamp) => new Date(timestamp).toLocaleString()}
          />
          {/* @ts-ignore - Recharts component compatibility */}
          <Legend />
          {/* @ts-ignore - Recharts component compatibility */}
          <Line
            yAxisId="price"
            type="monotone"
            dataKey="price"
            stroke={theme.palette.primary.main}
            dot={false}
          />
          {/* @ts-ignore - Recharts component compatibility */}
          <Line
            yAxisId="price"
            type="monotone"
            dataKey="confidence"
            stroke={theme.palette.success.main}
            dot={false}
            opacity={0.5}
          />
          {showAdvanced && (
            <>
              {/* @ts-ignore - Recharts component compatibility */}
              <Line
                yAxisId="metrics"
                type="monotone"
                dataKey="velocity"
                stroke={theme.palette.warning.main}
                dot={false}
              />
              {/* @ts-ignore - Recharts component compatibility */}
              <Line
                yAxisId="metrics"
                type="monotone"
                dataKey="acceleration"
                stroke={theme.palette.error.main}
                dot={false}
              />
            </>
          )}
        </LineChart>
      </ResponsiveContainer>
    </Box>
  );
};

export default PriceChart;