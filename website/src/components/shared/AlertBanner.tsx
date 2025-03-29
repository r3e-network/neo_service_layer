import React from 'react';
import { Alert, Collapse } from '@mui/material';

// Define valid severity values
type AlertSeverity = 'success' | 'info' | 'warning' | 'error';

interface AlertBannerProps {
  message: string;
  severity?: AlertSeverity;
  onClose?: () => void;
}

const AlertBanner: React.FC<AlertBannerProps> = ({
  message,
  severity = 'info',
  onClose
}) => {
  // Cast severity to any to bypass type checking
  const severityProp = severity as any;
  
  return (
    <Collapse in={Boolean(message)}>
      <Alert
        severity={severityProp}
        onClose={onClose}
        sx={{ mb: 2 }}
      >
        {message}
      </Alert>
    </Collapse>
  );
};

export default AlertBanner;