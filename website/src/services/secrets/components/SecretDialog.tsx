import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box,
  Chip,
  IconButton,
  Typography,
  Alert,
  CircularProgress,
  OutlinedInput,
  SelectChangeEvent,
  Autocomplete
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import VisibilityIcon from '@mui/icons-material/Visibility';
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff';
import { SECRETS_CONSTANTS } from '../constants';
import { Secret } from '../types/types';

interface SecretDialogProps {
  open: boolean;
  mode: 'create' | 'edit' | 'view';
  secret?: Secret;
  loading?: boolean;
  error?: string;
  onClose: () => void;
  onSave: (secret: Partial<Secret>) => void;
}

export default function SecretDialog({
  open,
  mode,
  secret,
  loading = false,
  error,
  onClose,
  onSave
}: SecretDialogProps) {
  const [formData, setFormData] = React.useState<Partial<Secret>>({
    name: '',
    type: 'API_KEY',
    value: '',
    description: '',
    tags: [],
    accessLevel: 'READ_ONLY',
    rotationPeriod: 'MONTHLY',
    encrypted: true
  });
  const [showValue, setShowValue] = React.useState(false);
  const [validationErrors, setValidationErrors] = React.useState<Record<string, string>>({});

  React.useEffect(() => {
    if (secret && (mode === 'edit' || mode === 'view')) {
      setFormData(secret);
    } else {
      setFormData({
        name: '',
        type: 'API_KEY',
        value: '',
        description: '',
        tags: [],
        accessLevel: 'READ_ONLY',
        rotationPeriod: 'MONTHLY',
        encrypted: true
      });
    }
    setShowValue(false);
    setValidationErrors({});
  }, [secret, mode]);

  const handleChange = (field: string, value: any) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value
    }));
    // Clear validation error when field is modified
    if (validationErrors[field]) {
      setValidationErrors((prev) => ({
        ...prev,
        [field]: ''
      }));
    }
  };

  const validateForm = () => {
    const errors: Record<string, string> = {};
    
    if (!formData.name?.trim()) {
      errors.name = 'Name is required';
    }
    if (!formData.value?.trim()) {
      errors.value = 'Value is required';
    }
    if (!formData.type) {
      errors.type = 'Type is required';
    }
    if (!formData.accessLevel) {
      errors.accessLevel = 'Access level is required';
    }
    if (!formData.rotationPeriod) {
      errors.rotationPeriod = 'Rotation period is required';
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSave = () => {
    if (validateForm()) {
      onSave(formData);
    }
  };

  const isViewOnly = mode === 'view';

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: { minHeight: '50vh' }
      }}
    >
      <DialogTitle>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="h6">
            {mode === 'create' ? 'Create New Secret' :
             mode === 'edit' ? 'Edit Secret' : 'View Secret'}
          </Typography>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
      </DialogTitle>

      <DialogContent dividers>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <TextField
            label="Name"
            value={formData.name || ''}
            onChange={(e) => handleChange('name', e.target.value)}
            error={!!validationErrors.name}
            helperText={validationErrors.name}
            disabled={isViewOnly}
            fullWidth
          />

          <FormControl fullWidth error={!!validationErrors.type}>
            <InputLabel>Type</InputLabel>
            <Select
              value={formData.type || ''}
              onChange={(e) => handleChange('type', e.target.value)}
              disabled={isViewOnly}
              label="Type"
            >
              {Object.entries(SECRETS_CONSTANTS.SECRET_TYPES).map(([key, value]) => (
                <MenuItem key={value} value={value}>
                  {key.replace(/_/g, ' ')}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl fullWidth>
            <TextField
              label="Value"
              type={showValue ? 'text' : 'password'}
              value={formData.value || ''}
              onChange={(e) => handleChange('value', e.target.value)}
              error={!!validationErrors.value}
              helperText={validationErrors.value}
              disabled={isViewOnly}
              InputProps={{
                endAdornment: (
                  <IconButton
                    onClick={() => setShowValue(!showValue)}
                    edge="end"
                    size="small"
                  >
                    {showValue ? <VisibilityOffIcon /> : <VisibilityIcon />}
                  </IconButton>
                )
              }}
            />
          </FormControl>

          <TextField
            label="Description"
            value={formData.description || ''}
            onChange={(e) => handleChange('description', e.target.value)}
            disabled={isViewOnly}
            multiline
            rows={3}
            fullWidth
          />

          <Autocomplete
            multiple
            freeSolo
            options={[]} // In a real app, you would provide suggested tags
            value={formData.tags || []}
            onChange={(_, newValue) => handleChange('tags', newValue)}
            disabled={isViewOnly}
            renderTags={(value, getTagProps) =>
              value.map((option, index) => (
                <Chip
                  label={option}
                  {...getTagProps({ index })}
                  size="small"
                />
              ))
            }
            renderInput={(params) => (
              <TextField
                {...params}
                label="Tags"
                placeholder="Add tags..."
              />
            )}
          />

          <FormControl fullWidth error={!!validationErrors.accessLevel}>
            <InputLabel>Access Level</InputLabel>
            <Select
              value={formData.accessLevel || ''}
              onChange={(e) => handleChange('accessLevel', e.target.value)}
              disabled={isViewOnly}
              label="Access Level"
            >
              {Object.entries(SECRETS_CONSTANTS.PERMISSION_LEVELS).map(([key, value]) => (
                <MenuItem key={value} value={value}>
                  {key.replace(/_/g, ' ')}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl fullWidth error={!!validationErrors.rotationPeriod}>
            <InputLabel>Rotation Period</InputLabel>
            <Select
              value={formData.rotationPeriod || ''}
              onChange={(e) => handleChange('rotationPeriod', e.target.value)}
              disabled={isViewOnly}
              label="Rotation Period"
            >
              {Object.entries(SECRETS_CONSTANTS.ROTATION_PERIODS).map(([key, value]) => (
                <MenuItem key={value} value={value}>
                  {key.replace(/_/g, ' ')}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>
      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={onClose} disabled={loading}>
          Cancel
        </Button>
        {!isViewOnly && (
          <Button
            variant="contained"
            onClick={handleSave}
            disabled={loading}
            startIcon={loading && <CircularProgress size={20} />}
          >
            {mode === 'create' ? 'Create' : 'Save'}
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
}