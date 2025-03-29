import React from 'react';
import {
  Box,
  Paper,
  Typography,
  Grid,
  CircularProgress,
  Alert,
  Tabs,
  Tab,
  Button,
  Tooltip,
  IconButton,
  Stack,
  Divider
} from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import SettingsIcon from '@mui/icons-material/Settings';
import AddIcon from '@mui/icons-material/Add';
import { useGasBank } from '../hooks/useGasBank';
import { GasAccountList } from './GasAccountList';
import { GasTransactionList } from './GasTransactionList';
import { GasReservationList } from './GasReservationList';
import GasMetricsCard from './GasMetricsCard';
import { GasAccountDialog } from './GasAccountDialog';
import { GasSettingsDialog } from './GasSettingsDialog';
import { formatGas } from '../utils/formatters';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`gas-tabpanel-${index}`}
      aria-labelledby={`gas-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

export default function GasBankDashboard() {
  const [selectedTab, setSelectedTab] = React.useState(0);
  const [showAccountDialog, setShowAccountDialog] = React.useState(false);
  const [showSettingsDialog, setShowSettingsDialog] = React.useState(false);
  const [selectedAddress, setSelectedAddress] = React.useState<string | undefined>();

  const {
    accounts,
    selectedAccount,
    transactions,
    totalTransactions,
    reservations,
    metrics,
    settings,
    loading,
    error,
    createAccount,
    fetchTransactions,
    updateSettings
  } = useGasBank(selectedAddress);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setSelectedTab(newValue);
  };

  const handleAccountSelect = (address: string) => {
    setSelectedAddress(address);
    fetchTransactions(address);
  };

  const handleAddAccount = async (address: string) => {
    try {
      await createAccount(address);
      setShowAccountDialog(false);
    } catch (err) {
      console.error('Failed to create account:', err);
    }
  };

  const handleUpdateSettings = async (newSettings: any) => {
    try {
      await updateSettings(newSettings);
      setShowSettingsDialog(false);
    } catch (err) {
      console.error('Failed to update settings:', err);
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ height: '100%' }}>
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error.message}
        </Alert>
      )}

      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1">
          Gas Bank
        </Typography>
        <Box display="flex" gap={1}>
          <Button
            startIcon={<AddIcon />}
            variant="contained"
            onClick={() => setShowAccountDialog(true)}
          >
            New Account
          </Button>
          <Tooltip title="Settings">
            <IconButton onClick={() => setShowSettingsDialog(true)}>
              <SettingsIcon />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {metrics && (
        <Box mb={3}>
          <GasMetricsCard metrics={metrics} />
        </Box>
      )}

      <Paper sx={{ mb: 3 }}>
        <Tabs
          value={selectedTab}
          onChange={handleTabChange}
          aria-label="gas bank tabs"
        >
          <Tab label="Accounts" id="gas-tab-0" />
          <Tab
            label="Transactions"
            id="gas-tab-1"
            disabled={!selectedAddress}
          />
          <Tab
            label="Reservations"
            id="gas-tab-2"
            disabled={!selectedAddress}
          />
        </Tabs>

        <TabPanel value={selectedTab} index={0}>
          <GasAccountList
            accounts={accounts}
            selectedAddress={selectedAddress}
            onSelectAccount={handleAccountSelect}
          />
        </TabPanel>

        <TabPanel value={selectedTab} index={1}>
          {selectedAccount && (
            <GasTransactionList
              transactions={transactions}
              totalTransactions={totalTransactions}
              onFetchTransactions={fetchTransactions}
              address={selectedAccount.address}
            />
          )}
        </TabPanel>

        <TabPanel value={selectedTab} index={2}>
          {selectedAccount && (
            <GasReservationList
              reservations={reservations}
              account={selectedAccount}
            />
          )}
        </TabPanel>
      </Paper>

      <GasAccountDialog
        open={showAccountDialog}
        onClose={() => setShowAccountDialog(false)}
        onSubmit={handleAddAccount}
        existingAccounts={accounts}
      />

      <GasSettingsDialog
        open={showSettingsDialog}
        onClose={() => setShowSettingsDialog(false)}
        onSubmit={handleUpdateSettings}
        currentSettings={settings}
      />
    </Box>
  );
}