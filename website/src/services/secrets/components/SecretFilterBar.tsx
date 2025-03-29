import React from 'react';
import {
  Box,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  IconButton,
  Button,
  OutlinedInput,
  SelectChangeEvent
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';
import { SECRETS_CONSTANTS } from '../constants';
import { SecretFilter } from '../types/types';

interface SecretFilterBarProps {
  filter: SecretFilter;
  onFilterChange: (filter: SecretFilter) => void;
}

export default function SecretFilterBar({
  filter,
  onFilterChange
}: SecretFilterBarProps) {
  const [searchText, setSearchText] = React.useState(filter.search || '');

  const handleTypeChange = (event: SelectChangeEvent<string[]>) => {
    const value = event.target.value as string[];
    onFilterChange({
      ...filter,
      type: value as any[]
    });
  };

  const handleRotationStatusChange = (event: SelectChangeEvent<string>) => {
    const value = event.target.value;
    onFilterChange({
      ...filter,
      rotationStatus: value as any
    });
  };

  const handleTagsChange = (event: SelectChangeEvent<string[]>) => {
    const value = event.target.value as string[];
    onFilterChange({
      ...filter,
      tags: value
    });
  };

  const handleSearch = () => {
    onFilterChange({
      ...filter,
      search: searchText
    });
  };

  const handleClear = () => {
    setSearchText('');
    onFilterChange({});
  };

  return (
    <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
      <TextField
        size="small"
        placeholder="Search secrets..."
        value={searchText}
        onChange={(e) => setSearchText(e.target.value)}
        onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
        InputProps={{
          startAdornment: (
            <IconButton size="small" onClick={handleSearch}>
              <SearchIcon />
            </IconButton>
          ),
          endAdornment: searchText && (
            <IconButton size="small" onClick={() => setSearchText('')}>
              <ClearIcon />
            </IconButton>
          )
        }}
        sx={{ minWidth: 200 }}
      />

      <FormControl size="small" sx={{ minWidth: 200 }}>
        <InputLabel>Secret Types</InputLabel>
        <Select
          multiple
          value={filter.type || []}
          onChange={handleTypeChange}
          input={<OutlinedInput label="Secret Types" />}
          renderValue={(selected) => (
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
              {selected.map((value) => (
                <Chip
                  key={value}
                  label={value.replace(/_/g, ' ')}
                  size="small"
                />
              ))}
            </Box>
          )}
        >
          {Object.entries(SECRETS_CONSTANTS.SECRET_TYPES).map(([key, value]) => (
            <MenuItem key={key} value={value}>
              {key.replace(/_/g, ' ')}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      <FormControl size="small" sx={{ minWidth: 150 }}>
        <InputLabel>Rotation Status</InputLabel>
        <Select
          value={filter.rotationStatus || ''}
          onChange={handleRotationStatusChange}
          label="Rotation Status"
        >
          <MenuItem value="">All</MenuItem>
          <MenuItem value="upToDate">Up to Date</MenuItem>
          <MenuItem value="needsRotation">Needs Rotation</MenuItem>
          <MenuItem value="expired">Expired</MenuItem>
        </Select>
      </FormControl>

      <FormControl size="small" sx={{ minWidth: 200 }}>
        <InputLabel>Tags</InputLabel>
        <Select
          multiple
          value={filter.tags || []}
          onChange={handleTagsChange}
          input={<OutlinedInput label="Tags" />}
          renderValue={(selected) => (
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
              {selected.map((value) => (
                <Chip key={value} label={value} size="small" />
              ))}
            </Box>
          )}
        >
          {/* In a real implementation, you would fetch available tags from the backend */}
          <MenuItem value="production">Production</MenuItem>
          <MenuItem value="staging">Staging</MenuItem>
          <MenuItem value="development">Development</MenuItem>
          <MenuItem value="api">API</MenuItem>
          <MenuItem value="database">Database</MenuItem>
        </Select>
      </FormControl>

      <Button
        variant="outlined"
        size="small"
        onClick={handleClear}
        startIcon={<ClearIcon />}
      >
        Clear Filters
      </Button>
    </Box>
  );
}