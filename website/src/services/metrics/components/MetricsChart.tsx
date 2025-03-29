import React from 'react';
import { Card, CardContent, Typography, Box, CircularProgress } from '@mui/material';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  ChartOptions
} from 'chart.js';
import { formatDate } from '../utils/formatters';
import { ServiceMetric } from '../types/types';
import { useMetrics } from '../hooks/useMetrics';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

interface MetricsChartProps {
  title: string;
  serviceName: string;
  metricName: string;
  timeRange: number;
  unit?: string;
}

export function MetricsChart({
  title,
  serviceName,
  metricName,
  timeRange
}: MetricsChartProps) {
  const { getServiceMetricHistory } = useMetrics();
  const [loading, setLoading] = React.useState(true);
  const [data, setData] = React.useState<ServiceMetric[]>([]);

  React.useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      const history = await getServiceMetricHistory(serviceName, metricName, timeRange);
      setData(history);
      setLoading(false);
    };

    fetchData();
    const interval = setInterval(fetchData, 60000); // Refresh every minute
    
    return () => clearInterval(interval);
  }, [serviceName, metricName, timeRange, getServiceMetricHistory]);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" height={300}>
        <CircularProgress />
      </Box>
    );
  }

  const chartData = {
    datasets: [
      {
        label: title,
        data: data.map(point => ({
          x: point.timestamp,
          y: point.value
        })),
        borderColor: 'rgb(75, 192, 192)',
        tension: 0.1
      }
    ]
  };

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top' as const
      },
      title: {
        display: false
      }
    },
    scales: {
      x: {
        type: 'time' as const,
        time: {
          unit: timeRange === 3600 ? 'minute' : timeRange === 86400 ? 'hour' : 'day'
        },
        title: {
          display: true,
          text: 'Time'
        }
      },
      y: {
        beginAtZero: true,
        title: {
          display: true,
          text: title
        }
      }
    }
  };

  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          {title}
        </Typography>
        <Box height={300}>
          <Line data={chartData} options={options} />
        </Box>
      </CardContent>
    </Card>
  );
}