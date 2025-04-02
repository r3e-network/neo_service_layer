// Proper MUI v7 theme configuration
'use client';

import { createTheme } from '@mui/material/styles';

// Create a theme instance
export const lightTheme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#00E599', // Neo green
      light: '#4CFFDF',
      dark: '#00A366',
      contrastText: '#000000',
    },
    secondary: {
      main: '#00AEFF', // Bright blue for better contrast
      light: '#7CD9FF',
      dark: '#0077B3',
      contrastText: '#000000',
    },
    error: {
      main: '#FF3D71',
    },
    warning: {
      main: '#FFAA00',
    },
    info: {
      main: '#0095FF',
    },
    success: {
      main: '#00D68F',
    },
    background: {
      default: '#F7F9FC', // Subtle light background
      paper: '#FFFFFF',
    },
    text: {
      primary: '#2E3A59',
      secondary: '#8F9BB3',
    },
  },
  typography: {
    fontFamily: '"Inter", "Helvetica", "Arial", sans-serif',
    h1: {
      fontWeight: 800,
      letterSpacing: '-0.025em',
    },
    h2: {
      fontWeight: 700,
      letterSpacing: '-0.025em',
    },
    h3: {
      fontWeight: 600,
      letterSpacing: '-0.015em',
    },
    h4: {
      fontWeight: 600,
    },
    button: {
      textTransform: 'none',
      fontWeight: 600,
    },
    body1: {
      lineHeight: 1.7,
    },
  },
  shape: {
    borderRadius: 12,
  },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: {
          scrollbarWidth: 'thin',
          '&::-webkit-scrollbar': {
            width: '8px',
            height: '8px',
          },
          '&::-webkit-scrollbar-track': {
            background: '#f1f1f1',
            borderRadius: '4px',
          },
          '&::-webkit-scrollbar-thumb': {
            background: '#bbc4d5',
            borderRadius: '4px',
          },
          '&::-webkit-scrollbar-thumb:hover': {
            background: '#8f9bb3',
          },
        },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        root: {
          borderBottom: '1px solid rgba(0, 0, 0, 0.08)',
          padding: '16px',
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: '10px',
          fontWeight: 600,
          padding: '10px 20px',
          boxShadow: 'none',
          '&:hover': {
            boxShadow: '0px 4px 8px rgba(0, 0, 0, 0.1)',
            transform: 'translateY(-2px)',
            transition: 'all 0.2s ease-in-out',
          },
        },
        contained: {
          '&.MuiButton-containedPrimary': {
            background: 'linear-gradient(135deg, #00E599 0%, #00D1FF 100%)',
          },
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          borderRadius: '16px',
          boxShadow: '0px 8px 24px rgba(0, 0, 0, 0.06)',
        },
        elevation1: {
          boxShadow: '0px 4px 12px rgba(0, 0, 0, 0.04)',
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: '16px',
          overflow: 'hidden',
          transition: 'transform 0.3s ease, box-shadow 0.3s ease',
          '&:hover': {
            transform: 'translateY(-5px)',
            boxShadow: '0px 12px 30px rgba(0, 0, 0, 0.08)',
          },
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: '8px',
          fontWeight: 500,
        },
      },
    },
  },
});

// Create dark theme variant
export const darkTheme = createTheme({
  ...lightTheme,
  palette: {
    ...lightTheme.palette,
    mode: 'dark',
    primary: {
      main: '#00E599', // Keep Neo green
      light: '#4CFFDF',
      dark: '#00A366',
      contrastText: '#FFFFFF',
    },
    secondary: {
      main: '#00AEFF', // Bright blue
      light: '#7CD9FF',
      dark: '#0077B3',
      contrastText: '#FFFFFF',
    },
    background: {
      default: '#121a29', // Dark blue-gray
      paper: '#1a2332',
    },
    text: {
      primary: '#FFFFFF',
      secondary: '#B4BDC9',
    },
  },
  components: {
    ...lightTheme.components,
    MuiCssBaseline: {
      styleOverrides: {
        body: {
          scrollbarWidth: 'thin',
          '&::-webkit-scrollbar': {
            width: '8px',
            height: '8px',
          },
          '&::-webkit-scrollbar-track': {
            background: '#1a2332',
            borderRadius: '4px',
          },
          '&::-webkit-scrollbar-thumb': {
            background: '#3a4a61',
            borderRadius: '4px',
          },
          '&::-webkit-scrollbar-thumb:hover': {
            background: '#4d6282',
          },
        },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        root: {
          borderBottom: '1px solid rgba(255, 255, 255, 0.08)',
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        contained: {
          '&.MuiButton-containedPrimary': {
            background: 'linear-gradient(135deg, #00E599 0%, #00D1FF 100%)',
          },
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
        },
      },
    },
  },
});
