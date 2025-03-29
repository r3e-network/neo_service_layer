import React from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Typography,
  Box,
  Chip
} from '@mui/material';
import { useTheme } from '@mui/material/styles';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';

interface PriceSource {
  name: string;
  price: number;
  weight: number;
  timestamp: string;
}

interface PriceSourceListProps {
  sources: PriceSource[];
}

const PriceSourceList: React.FC<PriceSourceListProps> = ({ sources }) => {
  const theme = useTheme();

  const getTimeDiff = (timestamp: string) => {
    const diff = Date.now() - new Date(timestamp).getTime();
    if (diff < 1000) return 'just now';
    if (diff < 60000) return `${Math.floor(diff / 1000)}s ago`;
    if (diff < 3600000) return `${Math.floor(diff / 60000)}m ago`;
    return `${Math.floor(diff / 3600000)}h ago`;
  };

  const getWeightColor = (weight: number) => {
    if (weight >= 0.8) return theme.palette.success.main;
    if (weight >= 0.5) return theme.palette.warning.main;
    return theme.palette.error.main;
  };

  const calculateDeviation = (price: number) => {
    const avgPrice = sources.reduce((sum, src) => sum + src.price, 0) / sources.length;
    return ((price - avgPrice) / avgPrice) * 100;
  };

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Price Sources
      </Typography>
      <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
        <Table stickyHeader size="small">
          <TableHead>
            <TableRow>
              <TableCell>Source</TableCell>
              <TableCell align="right">Price</TableCell>
              <TableCell align="right">Deviation</TableCell>
              <TableCell align="right">Weight</TableCell>
              <TableCell align="right">Last Update</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {sources.map((source) => {
              const deviation = calculateDeviation(source.price);
              return (
                <TableRow key={source.name}>
                  <TableCell component="th" scope="row">
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      {source.name}
                      {Math.abs(deviation) > 5 && (
                        <Chip
                          size="small"
                          label="Outlier"
                          color="warning"
                          sx={{ height: 20 }}
                        />
                      )}
                    </Box>
                  </TableCell>
                  <TableCell align="right">
                    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', gap: 1 }}>
                      ${source.price.toFixed(2)}
                      {deviation > 0 ? (
                        <TrendingUpIcon sx={{ color: theme.palette.success.main, fontSize: 16 }} />
                      ) : (
                        <TrendingDownIcon sx={{ color: theme.palette.error.main, fontSize: 16 }} />
                      )}
                    </Box>
                  </TableCell>
                  <TableCell
                    align="right"
                    sx={{
                      color: Math.abs(deviation) > 5
                        ? theme.palette.error.main
                        : Math.abs(deviation) > 2
                        ? theme.palette.warning.main
                        : theme.palette.success.main
                    }}
                  >
                    {deviation > 0 ? '+' : ''}{deviation.toFixed(2)}%
                  </TableCell>
                  <TableCell align="right">
                    <Box
                      sx={{
                        width: '100%',
                        height: 4,
                        bgcolor: theme.palette.grey[200],
                        borderRadius: 2,
                        position: 'relative',
                        overflow: 'hidden'
                      }}
                    >
                      <Box
                        sx={{
                          position: 'absolute',
                          left: 0,
                          top: 0,
                          height: '100%',
                          width: `${source.weight * 100}%`,
                          bgcolor: getWeightColor(source.weight),
                          borderRadius: 2
                        }}
                      />
                    </Box>
                    <Typography variant="caption" sx={{ color: getWeightColor(source.weight) }}>
                      {(source.weight * 100).toFixed(1)}%
                    </Typography>
                  </TableCell>
                  <TableCell align="right">
                    <Typography variant="caption" color="textSecondary">
                      {getTimeDiff(source.timestamp)}
                    </Typography>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};

export default PriceSourceList;