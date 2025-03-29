import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  CircularProgress,
  useTheme
} from '@mui/material';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import WarningIcon from '@mui/icons-material/Warning';
import PeopleIcon from '@mui/icons-material/People';
import TimelineIcon from '@mui/icons-material/Timeline';

interface SecretMetricsCardProps {
  title: string;
  value: number;
  type: 'number' | 'warning' | 'users' | 'activity' | 'error' | 'success' | 'total';
  loading?: boolean;
}

export default function SecretMetricsCard({
  title,
  value,
  type,
  loading = false
}: SecretMetricsCardProps) {
  const theme = useTheme();

  const getIcon = () => {
    switch (type) {
      case 'number':
        return <TrendingUpIcon sx={{ color: theme.palette.primary.main }} />;
      case 'warning':
        return <WarningIcon sx={{ color: theme.palette.warning.main }} />;
      case 'users':
        return <PeopleIcon sx={{ color: theme.palette.info.main }} />;
      case 'activity':
        return <TimelineIcon sx={{ color: theme.palette.success.main }} />;
    }
  };

  const getColor = () => {
    switch (type) {
      case 'number':
        return theme.palette.primary.main;
      case 'warning':
        return theme.palette.warning.main;
      case 'users':
        return theme.palette.info.main;
      case 'activity':
        return theme.palette.success.main;
    }
  };

  const formatValue = () => {
    if (type === 'activity') {
      return value.toLocaleString(undefined, { maximumFractionDigits: 1 });
    }
    return value.toLocaleString();
  };

  return (
    <Card>
      <CardContent>
        <Box display="flex" alignItems="center" mb={2}>
          {getIcon()}
          <Typography
            variant="subtitle2"
            color="textSecondary"
            sx={{ ml: 1 }}
          >
            {title}
          </Typography>
        </Box>

        {loading ? (
          <Box display="flex" justifyContent="center">
            <CircularProgress size={24} />
          </Box>
        ) : (
          <Typography
            variant="h4"
            component="div"
            sx={{ color: getColor() }}
          >
            {formatValue()}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
}