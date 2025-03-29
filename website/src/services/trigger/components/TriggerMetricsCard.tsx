import React from 'react';
import { Box, Paper, Typography, CircularProgress, Tooltip } from '@mui/material';
import BoltIcon from '@mui/icons-material/Bolt';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import SpeedIcon from '@mui/icons-material/Speed';
import ScheduleIcon from '@mui/icons-material/Schedule';
import { formatNumber, formatPercent } from '../../../utils/formatters';

interface TriggerMetricsCardProps {
  title: string;
  value: number;
  type: 'total' | 'active' | 'success' | 'executions';
  loading?: boolean;
  isPercentage?: boolean;
}

const TriggerMetricsCard: React.FC<TriggerMetricsCardProps> = ({ 
  title, 
  value, 
  type, 
  loading = false,
  isPercentage = false
}) => {
  if (loading) {
    return (
      <Paper elevation={2} sx={{ p: 2, height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <CircularProgress size={24} />
      </Paper>
    );
  }

  // Select icon based on type
  const getIcon = () => {
    switch (type) {
      case 'total':
        return <BoltIcon sx={{ color: 'primary.main' }} />;
      case 'active':
        return <CheckCircleIcon sx={{ color: 'success.main' }} />;
      case 'success':
        return <SpeedIcon sx={{ color: 'info.main' }} />;
      case 'executions':
        return <ScheduleIcon sx={{ color: 'secondary.main' }} />;
      default:
        return <BoltIcon sx={{ color: 'primary.main' }} />;
    }
  };

  // Format value based on type
  const formattedValue = isPercentage ? formatPercent(value) : formatNumber(value);

  return (
    <Paper 
      elevation={2} 
      sx={{ 
        p: 2, 
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'space-between'
      }}
    >
      <Typography variant="subtitle2" color="text.secondary" gutterBottom>
        {title}
      </Typography>
      
      <Box display="flex" alignItems="center" gap={1} mt={1}>
        {getIcon()}
        <Typography variant="h4" component="div" fontWeight="medium">
          {formattedValue}
        </Typography>
      </Box>
    </Paper>
  );
};

export default TriggerMetricsCard;
