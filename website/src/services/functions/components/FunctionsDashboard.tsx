// @ts-ignore
import * as React from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Alert,
  Tab,
  Button,
  IconButton,
  Tooltip,
  Card,
  CardContent,
  CircularProgress
} from '@mui/material';
import { TabContext, TabList, TabPanel as MuiTabPanel } from '@mui/lab';
import AddIcon from '@mui/icons-material/Add';
import RefreshIcon from '@mui/icons-material/Refresh';
import CodeIcon from '@mui/icons-material/Code';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import SettingsIcon from '@mui/icons-material/Settings';
import { useFunctions } from '../hooks/useFunctions';
import { FunctionsList } from './FunctionsList';
import { FunctionEditor } from './FunctionEditor';
import { FunctionExecutions } from './FunctionExecutions';
import { FunctionMetrics } from './FunctionMetrics';
import { FunctionDialog } from './FunctionDialog';
import { FunctionPermissionsDialog } from './FunctionPermissionsDialog';

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
      id={`function-tabpanel-${index}`}
      aria-labelledby={`function-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

export default function FunctionsDashboard() {
  const [selectedTab, setSelectedTab] = React.useState(0);
  const [showNewFunctionDialog, setShowNewFunctionDialog] = React.useState(false);
  const [showPermissionsDialog, setShowPermissionsDialog] = React.useState(false);
  const [selectedFunctionId, setSelectedFunctionId] = React.useState<string | undefined>();

  const {
    functions,
    selectedFunction,
    executions,
    totalExecutions,
    loading,
    error,
    createFunction,
    updateFunction,
    deleteFunction,
    deployFunction,
    executeFunction,
    updatePermissions,
    fetchExecutions
  } = useFunctions(selectedFunctionId);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setSelectedTab(newValue);
  };

  const handleFunctionSelect = (id: string) => {
    setSelectedFunctionId(id);
    fetchExecutions(id);
  };

  const handleCreateFunction = async (
    name: string,
    code: string,
    language: string,
    config: any
  ) => {
    try {
      await createFunction(name, code, language, config);
      setShowNewFunctionDialog(false);
    } catch (err) {
      console.error('Failed to create function:', err);
    }
  };

  const handleDeploy = async () => {
    if (selectedFunction) {
      try {
        await deployFunction(selectedFunction.id);
      } catch (err) {
        console.error('Failed to deploy function:', err);
      }
    }
  };

  const handleExecute = async () => {
    if (selectedFunction) {
      try {
        await executeFunction(selectedFunction.id);
      } catch (err) {
        console.error('Failed to execute function:', err);
      }
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

      <Box sx={{ flexGrow: 1, mt: 4 }}>
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 4 }}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Your Functions
                </Typography>
                <FunctionsList
                  functions={functions}
                  onSelect={handleFunctionSelect}
                  onDelete={deleteFunction}
                  selectedId={selectedFunction?.id}
                />
              </CardContent>
            </Card>
          </Grid>
          <Grid size={{ xs: 12, md: 8 }}>
            {selectedFunction ? (
              <TabContext value={selectedTab.toString()}>
                <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                  <TabList onChange={(event, newValue) => setSelectedTab(parseInt(newValue, 10))}>
                    <Tab label="Editor" value="0" />
                    <Tab label="Executions" value="1" />
                    <Tab label="Metrics" value="2" />
                  </TabList>
                </Box>
                <MuiTabPanel value="0">
                  <FunctionEditor
                    function={selectedFunction}
                    onUpdate={async (id, updates) => {
                      await updateFunction(id, updates);
                    }}
                  />
                </MuiTabPanel>
                <MuiTabPanel value="1">
                  <FunctionExecutions
                    executions={executions}
                    totalExecutions={totalExecutions}
                    functionId={selectedFunction.id}
                    onFetchExecutions={fetchExecutions}
                    onRefresh={() => fetchExecutions(selectedFunction.id)}
                  />
                </MuiTabPanel>
                <MuiTabPanel value="2">
                  <FunctionMetrics metrics={selectedFunction.metrics} />
                </MuiTabPanel>
              </TabContext>
            ) : (
              <Card>
                <CardContent>
                  <Typography variant="body1" color="text.secondary" align="center">
                    Select a function or create a new one to get started
                  </Typography>
                </CardContent>
              </Card>
            )}
          </Grid>
        </Grid>
      </Box>

      <FunctionDialog
        open={showNewFunctionDialog}
        onClose={() => setShowNewFunctionDialog(false)}
        onSubmit={async (name, code, language, config) => {
          await createFunction(name, code, language, config);
          setShowNewFunctionDialog(false);
        }}
      />

      {selectedFunction && (
        <FunctionPermissionsDialog
          open={showPermissionsDialog}
          onClose={() => setShowPermissionsDialog(false)}
          function={selectedFunction}
          onUpdate={async (id, permissions) => {
            await updatePermissions(id, permissions);
            setShowPermissionsDialog(false);
          }}
        />
      )}
    </Box>
  );
}