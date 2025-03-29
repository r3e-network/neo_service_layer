import React from 'react';
import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
  Paper,
  Divider
} from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import { UserFunction, ProgrammingLanguage } from '../types/types';
import MonacoEditor from '@monaco-editor/react';

interface FunctionEditorProps {
  function: UserFunction;
  onUpdate: (id: string, updates: Partial<UserFunction>) => Promise<void>;
}

export function FunctionEditor({ function: func, onUpdate }: FunctionEditorProps) {
  const [code, setCode] = React.useState(func.code);
  const [name, setName] = React.useState(func.name);
  const [language, setLanguage] = React.useState(func.language);
  const [description, setDescription] = React.useState(func.description || '');
  const [isModified, setIsModified] = React.useState(false);

  React.useEffect(() => {
    setCode(func.code);
    setName(func.name);
    setLanguage(func.language);
    setDescription(func.description || '');
    setIsModified(false);
  }, [func]);

  const handleSave = async () => {
    try {
      await onUpdate(func.id, {
        name,
        code,
        language,
        description
      });
      setIsModified(false);
    } catch (err) {
      console.error('Failed to update function:', err);
    }
  };

  const handleEditorChange = (value: string | undefined) => {
    if (value !== undefined) {
      setCode(value);
      setIsModified(true);
    }
  };

  const getEditorLanguage = (lang: ProgrammingLanguage): string => {
    switch (lang) {
      case 'javascript':
        return 'javascript';
      case 'typescript':
        return 'typescript';
      case 'python':
        return 'python';
      case 'go':
        return 'go';
      case 'rust':
        return 'rust';
      default:
        return 'plaintext';
    }
  };

  return (
    <Box>
      <Paper sx={{ p: 3, mb: 3 }}>
        <Box display="flex" gap={2} mb={3}>
          <TextField
            label="Function Name"
            value={name}
            onChange={(e) => {
              setName(e.target.value);
              setIsModified(true);
            }}
            fullWidth
          />
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel id="language-select-label">Language</InputLabel>
            <Select
              labelId="language-select-label"
              value={language}
              label="Language"
              onChange={(e) => {
                setLanguage(e.target.value as ProgrammingLanguage);
                setIsModified(true);
              }}
            >
              <MenuItem value="javascript">JavaScript</MenuItem>
              <MenuItem value="typescript">TypeScript</MenuItem>
              <MenuItem value="python">Python</MenuItem>
              <MenuItem value="go">Go</MenuItem>
              <MenuItem value="rust">Rust</MenuItem>
            </Select>
          </FormControl>
        </Box>

        <TextField
          label="Description"
          value={description}
          onChange={(e) => {
            setDescription(e.target.value);
            setIsModified(true);
          }}
          fullWidth
          multiline
          rows={2}
          sx={{ mb: 3 }}
        />

        <Box display="flex" justifyContent="flex-end">
          <Button
            startIcon={<SaveIcon />}
            variant="contained"
            onClick={handleSave}
            disabled={!isModified}
          >
            Save Changes
          </Button>
        </Box>
      </Paper>

      <Paper sx={{ height: '60vh' }}>
        <MonacoEditor
          height="100%"
          language={getEditorLanguage(language)}
          value={code}
          onChange={handleEditorChange}
          theme="vs-dark"
          options={{
            minimap: { enabled: true },
            scrollBeyondLastLine: false,
            fontSize: 14,
            wordWrap: 'on',
            automaticLayout: true
          }}
        />
      </Paper>
    </Box>
  );
}