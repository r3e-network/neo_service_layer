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
  Typography,
  Alert,
  Divider,
  Chip
} from '@mui/material';
import { ProgrammingLanguage, RuntimeEnvironment } from '../types/types';

interface FunctionDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (
    name: string,
    code: string,
    language: string,
    config: any
  ) => Promise<void>;
}

const TEMPLATES: Record<ProgrammingLanguage, string> = {
  'javascript': `// Function parameters are available in the 'event' object
module.exports = async function(event) {
  // Your code here
  return {
    message: 'Hello from JavaScript!'
  };
};`,
  'typescript': `interface Event {
  // Define your event type here
}

export default async function(event: Event) {
  // Your code here
  return {
    message: 'Hello from TypeScript!'
  };
}`,
  'python': `def handler(event):
    # Your code here
    return {
        'message': 'Hello from Python!'
    }`,
  'go': `package main

import (
	"encoding/json"
)

type Event struct {
	// Define your event type here
}

type Response struct {
	Message string \`json:"message"\`
}

func Handler(event Event) (Response, error) {
	// Your code here
	return Response{
		Message: "Hello from Go!",
	}, nil
}`,
  'rust': `use serde::{Deserialize, Serialize};

#[derive(Deserialize)]
struct Event {
    // Define your event type here
}

#[derive(Serialize)]
struct Response {
    message: String,
}

fn handler(event: Event) -> Response {
    // Your code here
    Response {
        message: String::from("Hello from Rust!"),
    }
}`
};

// Define runtime environments
const NODE_16 = { name: 'Node.js', version: '16.x', features: ['JavaScript', 'TypeScript'], memoryLimit: 256, timeoutSeconds: 60, concurrency: 10 };
const NODE_18 = { name: 'Node.js', version: '18.x', features: ['JavaScript', 'TypeScript'], memoryLimit: 256, timeoutSeconds: 60, concurrency: 10 };
const PYTHON_3_9 = { name: 'Python', version: '3.9', features: ['Python'], memoryLimit: 256, timeoutSeconds: 60, concurrency: 10 };
const PYTHON_3_11 = { name: 'Python', version: '3.11', features: ['Python'], memoryLimit: 256, timeoutSeconds: 60, concurrency: 10 };
const GO_1_17 = { name: 'Go', version: '1.17', features: ['Go'], memoryLimit: 256, timeoutSeconds: 60, concurrency: 10 };
const GO_1_20 = { name: 'Go', version: '1.20', features: ['Go'], memoryLimit: 256, timeoutSeconds: 60, concurrency: 10 };
const RUST_1_68 = { name: 'Rust', version: '1.68', features: ['Rust'], memoryLimit: 256, timeoutSeconds: 60, concurrency: 10 };

export function FunctionDialog({
  open,
  onClose,
  onSubmit
}: FunctionDialogProps) {
  const [name, setName] = React.useState('');
  const [description, setDescription] = React.useState('');
  const [language, setLanguage] = React.useState<ProgrammingLanguage>('javascript');
  const [runtime, setRuntime] = React.useState<RuntimeEnvironment>(NODE_16);
  const [memory, setMemory] = React.useState(128);
  const [timeout, setTimeout] = React.useState(30);
  const [error, setError] = React.useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = React.useState(false);
  const [code, setCode] = React.useState(TEMPLATES['javascript']);

  const handleSubmit = async () => {
    if (!name.trim()) {
      setError('Function name is required');
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      await onSubmit(
        name,
        code,
        language,
        {
          description,
          runtime,
          memory,
          timeout
        }
      );
      handleClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create function');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClose = () => {
    setName('');
    setDescription('');
    setLanguage('javascript');
    setRuntime(NODE_16);
    setMemory(128);
    setTimeout(30);
    setError(null);
    setIsSubmitting(false);
    onClose();
  };

  const getAvailableRuntimes = (lang: ProgrammingLanguage): RuntimeEnvironment[] => {
    switch (lang) {
      case 'javascript':
      case 'typescript':
        return [NODE_16, NODE_18];
      case 'python':
        return [PYTHON_3_9, PYTHON_3_11];
      case 'go':
        return [GO_1_17, GO_1_20];
      case 'rust':
        return [RUST_1_68];
      default:
        return [NODE_16];
    }
  };

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth="sm"
      fullWidth
    >
      <DialogTitle>Create New Function</DialogTitle>
      <DialogContent>
        <Box sx={{ pt: 2, display: 'flex', flexDirection: 'column', gap: 3 }}>
          {error && (
            <Alert severity="error" onClose={() => setError(null)}>
              {error}
            </Alert>
          )}

          <TextField
            label="Function Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            fullWidth
            required
          />

          <TextField
            label="Description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            fullWidth
            multiline
            rows={2}
          />

          <FormControl fullWidth>
            <InputLabel>Language</InputLabel>
            <Select
              value={language}
              label="Language"
              onChange={(e) => {
                const newLang = e.target.value as ProgrammingLanguage;
                setLanguage(newLang);
                // Reset runtime when language changes
                setRuntime(getAvailableRuntimes(newLang)[0]);
                setCode(TEMPLATES[newLang]);
              }}
            >
              <MenuItem value="javascript">JavaScript</MenuItem>
              <MenuItem value="typescript">TypeScript</MenuItem>
              <MenuItem value="python">Python</MenuItem>
              <MenuItem value="go">Go</MenuItem>
              <MenuItem value="rust">Rust</MenuItem>
            </Select>
          </FormControl>

          <FormControl fullWidth>
            <InputLabel>Runtime</InputLabel>
            <Select
              value={JSON.stringify(runtime)}
              label="Runtime"
              onChange={(e) => setRuntime(JSON.parse(e.target.value as string))}
            >
              {getAvailableRuntimes(language).map((rt) => (
                <MenuItem key={`${rt.name}-${rt.version}`} value={JSON.stringify(rt)}>
                  {rt.name} {rt.version}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              label="Memory (MB)"
              type="number"
              value={memory}
              onChange={(e) => setMemory(parseInt(e.target.value, 10))}
              inputProps={{ min: 128, max: 1024, step: 64 }}
              fullWidth
            />
            <TextField
              label="Timeout (seconds)"
              type="number"
              value={timeout}
              onChange={(e) => setTimeout(parseInt(e.target.value, 10))}
              inputProps={{ min: 1, max: 300, step: 1 }}
              fullWidth
            />
          </Box>

          <Divider sx={{ my: 2 }} />

          <Typography variant="subtitle2" color="text.secondary">
            Template Preview
          </Typography>
          <Box
            sx={{
              bgcolor: 'grey.100',
              p: 2,
              borderRadius: 1,
              fontFamily: 'monospace',
              whiteSpace: 'pre-wrap'
            }}
          >
            {code}
          </Box>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={isSubmitting}
        >
          Create
        </Button>
      </DialogActions>
    </Dialog>
  );
}