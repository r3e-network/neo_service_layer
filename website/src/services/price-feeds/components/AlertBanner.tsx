import React from 'react';
import { Alert, AlertTitle, Collapse } from '@mui/material';
import { AlertBannerProps } from '../types/types';

export default function AlertBanner({
  title,
  message,
  severity,
  onClose,
  className
}: AlertBannerProps) {
  return (
    <Collapse in={true} className={className}>
      <Alert
        severity={severity}
        onClose={onClose}
        sx={{ mb: 2 }}
      >
        {title && <AlertTitle>{title}</AlertTitle>}
        {message}
      </Alert>
    </Collapse>
  );
}