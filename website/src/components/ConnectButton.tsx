'use client';

import React from 'react';
import { useWallet } from '@/hooks/useWallet';
import { 
  Button, 
  Menu, 
  MenuItem, 
  Divider, 
  CircularProgress, 
  Box, 
  Typography, 
  Tooltip 
} from '@mui/material';
import { WalletIcon, ArrowRightOnRectangleIcon, ChevronDownIcon } from '@heroicons/react/24/outline';

export default function ConnectButton() {
  const { 
    isConnected, 
    address, 
    balance, 
    network, 
    isLoading, 
    connect, 
    disconnect, 
    isNeoLineInstalled,
    hasCheckedWallet,
    error
  } = useWallet();
  
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  
  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    if (isConnected) {
      setAnchorEl(event.currentTarget);
    } else {
      handleConnect();
    }
  };
  
  const handleClose = () => {
    setAnchorEl(null);
  };
  
  const handleConnect = async () => {
    try {
      console.log('ConnectButton: Initiating wallet connection...');
      
      // Check if NeoLine is detected in browser
      const neoLineExists = typeof window !== 'undefined' && (
        'neo3Dapi' in window || 
        'NEOLine' in window
      );
      
      console.log('ConnectButton: NeoLine objects in window:', {
        neo3Dapi: 'neo3Dapi' in window,
        NEOLine: 'NEOLine' in window,
        instance: window.NEOLine && 'instance' in window.NEOLine
      });
      
      if (!neoLineExists) {
        console.error('ConnectButton: NeoLine not detected in window object');
        alert('NeoLine wallet not detected. Please install the NeoLine extension and refresh the page.');
        return;
      }
      
      await connect();
      console.log('ConnectButton: Connection successful');
    } catch (error) {
      console.error('ConnectButton: Failed to connect:', error);
      // Show a more user-friendly error message
      const errorMessage = error instanceof Error ? error.message : 'Unknown error connecting to wallet';
      alert(`Connection error: ${errorMessage}`);
    }
  };
  
  const handleDisconnect = () => {
    disconnect();
    handleClose();
  };
  
  const formatAddress = (address: string) => {
    return `${address.substring(0, 6)}...${address.substring(address.length - 4)}`;
  };

  // Function to install NeoLine
  const handleInstallNeoLine = () => {
    window.open('https://neoline.io/download.html', '_blank');
  };

  // If we haven't checked for the wallet yet, show a loading button
  if (!hasCheckedWallet) {
    return (
      <Button
        variant="contained"
        color="primary"
        disabled
        startIcon={<CircularProgress size={20} color="inherit" />}
        size="medium"
      >
        Checking wallet...
      </Button>
    );
  }

  if (!isNeoLineInstalled) {
    return (
      <Tooltip title="NeoLine wallet extension is required to interact with the Neo blockchain">
        <Button
          variant="contained"
          color="primary"
          onClick={handleInstallNeoLine}
          startIcon={<WalletIcon className="h-5 w-5" />}
          size="medium"
        >
          Install NeoLine
        </Button>
      </Tooltip>
    );
  }
  
  if (isLoading) {
    return (
      <Button
        variant="contained"
        color="primary"
        disabled
        startIcon={<CircularProgress size={20} color="inherit" />}
        size="medium"
      >
        Connecting...
      </Button>
    );
  }
  
  if (isConnected && address) {
    return (
      <>
        <Button
          id="wallet-button"
          aria-controls={open ? 'wallet-menu' : undefined}
          aria-haspopup="true"
          aria-expanded={open ? 'true' : undefined}
          variant="contained"
          color="primary"
          onClick={handleClick}
          endIcon={<ChevronDownIcon className="h-4 w-4" />}
          size="medium"
        >
          {formatAddress(address)}
        </Button>
        <Menu
          id="wallet-menu"
          anchorEl={anchorEl}
          open={open}
          onClose={handleClose}
          MenuListProps={{
            'aria-labelledby': 'wallet-button',
          }}
        >
          <Box sx={{ px: 2, py: 1 }}>
            <Typography variant="subtitle2" color="text.secondary">
              {network === 'MainNet' ? 'Neo N3 MainNet' : 'Neo N3 TestNet'}
            </Typography>
            <Typography variant="body2" sx={{ wordBreak: 'break-all' }}>
              {address}
            </Typography>
          </Box>
          
          <Divider />
          
          {balance && (
            <Box sx={{ px: 2, py: 1 }}>
              <Typography variant="subtitle2" color="text.secondary">Balance</Typography>
              <Typography variant="body2">{balance.NEO} NEO</Typography>
              <Typography variant="body2">{balance.GAS} GAS</Typography>
            </Box>
          )}
          
          <Divider />
          
          <MenuItem onClick={handleDisconnect}>
            <ArrowRightOnRectangleIcon className="h-5 w-5 mr-2" />
            Disconnect
          </MenuItem>
        </Menu>
      </>
    );
  }
  
  return (
    <Button
      variant="contained"
      color="primary"
      onClick={handleConnect}
      startIcon={<WalletIcon className="h-5 w-5" />}
      size="medium"
    >
      Connect Wallet
    </Button>
  );
} 