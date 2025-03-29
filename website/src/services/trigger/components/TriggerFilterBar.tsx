// @ts-ignore
import * as React from 'react';
import {
  Box,
  TextField,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  OutlinedInput,
  SelectChangeEvent,
  Grid,
  IconButton,
  Tooltip
} from '@mui/material';
import FilterListIcon from '@mui/icons-material/FilterList';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';
import { TriggerFilter } from '../types/types';
import { TRIGGER_CONSTANTS } from '../constants';

interface TriggerFilterBarProps {
  filter: TriggerFilter;
  onFilterChange: (filter: TriggerFilter) => void;
}

const TriggerFilterBar: React.FC<TriggerFilterBarProps> = ({ filter, onFilterChange }) => {
  const [showFilters, setShowFilters] = React.useState(false);

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newFilter = { ...filter, search: e.target.value };
    onFilterChange(newFilter);
  };

  const handleTypeChange = (event: SelectChangeEvent<string[]>) => {
    const value = event.target.value as string[];
    const newFilter = { ...filter, type: value as any[] };
    onFilterChange(newFilter);
  };

  const handleStatusChange = (event: SelectChangeEvent<string[]>) => {
    const value = event.target.value as string[];
    const newFilter = { ...filter, status: value as any[] };
    onFilterChange(newFilter);
  };

  const handleClearFilters = () => {
    const newFilter = { search: '', type: [], status: [] };
    onFilterChange(newFilter);
  };

  const toggleFilters = () => {
    setShowFilters(!showFilters);
  };

  return (
    <Box sx={{ mb: 3 }}>
      <Box display="flex" gap={1} alignItems="center">
        <TextField
          fullWidth
          placeholder="Search triggers..."
          value={filter.search}
          onChange={handleSearchChange}
          InputProps={{
            startAdornment: <SearchIcon color="action" sx={{ mr: 1 }} />,
            endAdornment: filter.search ? (
              <IconButton size="small" onClick={() => {
                const newFilter = { ...filter, search: '' };
                onFilterChange(newFilter);
              }}>
                <ClearIcon fontSize="small" />
              </IconButton>
            ) : null
          }}
          size="small"
        />
        <Tooltip title="Toggle filters">
          <IconButton onClick={toggleFilters} color={showFilters ? "primary" : "default"}>
            <FilterListIcon />
          </IconButton>
        </Tooltip>
        {((filter.type && filter.type.length > 0) || (filter.status && filter.status.length > 0)) && (
          <Button size="small" onClick={handleClearFilters} sx={{ ml: 1 }}>
            Clear All
          </Button>
        )}
      </Box>

      {showFilters && (
        <Grid container spacing={2} sx={{ mt: 2 }}>
          <Grid sx={{ gridColumn: { xs: 'span 12', sm: 'span 6' } }}>
            <FormControl fullWidth size="small">
              <InputLabel>Trigger Type</InputLabel>
              <Select
                multiple
                value={filter.type as string[]}
                onChange={handleTypeChange}
                input={<OutlinedInput label="Trigger Type" />}
                renderValue={(selected) => (
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                    {(selected as string[]).map((value) => (
                      <Chip key={value} label={value} size="small" />
                    ))}
                  </Box>
                )}
              >
                {Object.values(TRIGGER_CONSTANTS.EVENT_TYPES).map((type) => (
                  <MenuItem key={type} value={type}>
                    {type}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid sx={{ gridColumn: { xs: 'span 12', sm: 'span 6' } }}>
            <FormControl fullWidth size="small">
              <InputLabel>Status</InputLabel>
              <Select
                multiple
                value={filter.status as string[]}
                onChange={handleStatusChange}
                input={<OutlinedInput label="Status" />}
                renderValue={(selected) => (
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                    {(selected as string[]).map((value) => (
                      <Chip key={value} label={value} size="small" />
                    ))}
                  </Box>
                )}
              >
                {Object.values(TRIGGER_CONSTANTS.TRIGGER_STATUS).map((status) => (
                  <MenuItem key={status} value={status}>
                    {status}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      )}
    </Box>
  );
};

export default TriggerFilterBar;
