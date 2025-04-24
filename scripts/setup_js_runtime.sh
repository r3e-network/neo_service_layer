#!/bin/bash

# Neo Service Layer JavaScript Runtime Setup Script
# This script sets up the JavaScript runtime environment for the function service

# Exit on error
set -e

# Set the base directory to the current directory
BASE_DIR=$(pwd)

echo "Setting up JavaScript runtime environment for Neo Service Layer..."

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "Error: Node.js is not installed. Please install Node.js from https://nodejs.org/"
    exit 1
fi

# Check Node.js version
NODE_VERSION=$(node --version)
echo "Detected Node.js version: $NODE_VERSION"

# Check if npm is installed
if ! command -v npm &> /dev/null; then
    echo "Error: npm is not installed. Please install npm from https://www.npmjs.com/"
    exit 1
fi

# Create JavaScript runtime directory
echo "Creating JavaScript runtime directory..."
mkdir -p "$BASE_DIR/src/NeoServiceLayer.Enclave/Enclave/Execution/JavaScript"

# Create package.json file
echo "Creating package.json file..."
cat > "$BASE_DIR/src/NeoServiceLayer.Enclave/Enclave/Execution/JavaScript/package.json" << EOF
{
  "name": "neo-service-layer-js-runtime",
  "version": "1.0.0",
  "description": "JavaScript runtime for Neo Service Layer",
  "main": "index.js",
  "scripts": {
    "test": "echo \"Error: no test specified\" && exit 1"
  },
  "author": "Neo Service Layer Team",
  "license": "MIT",
  "dependencies": {
    "axios": "^1.3.4",
    "crypto-js": "^4.1.1",
    "dotenv": "^16.0.3",
    "js-yaml": "^4.1.0",
    "lodash": "^4.17.21",
    "moment": "^2.29.4",
    "uuid": "^9.0.0"
  }
}
EOF

# Install JavaScript dependencies
echo "Installing JavaScript dependencies..."
cd "$BASE_DIR/src/NeoServiceLayer.Enclave/Enclave/Execution/JavaScript"
npm install

# Create JavaScript runtime wrapper
echo "Creating JavaScript runtime wrapper..."
cat > "$BASE_DIR/src/NeoServiceLayer.Enclave/Enclave/Execution/JavaScript/runtime.js" << EOF
// Neo Service Layer JavaScript Runtime Wrapper

// Global context for function execution
const neoServiceLayer = {
    // Price feed service
    priceFeed: {
        getPrice: (symbol) => {
            return global.__neo_service_layer_price_feed_get_price(symbol);
        },
        getPrices: (symbols) => {
            return global.__neo_service_layer_price_feed_get_prices(symbols);
        }
    },
    
    // Secrets service
    secrets: {
        get: (key) => {
            return global.__neo_service_layer_secrets_get(key);
        },
        set: (key, value) => {
            return global.__neo_service_layer_secrets_set(key, value);
        },
        delete: (key) => {
            return global.__neo_service_layer_secrets_delete(key);
        }
    },
    
    // Wallet service
    wallet: {
        getAddress: () => {
            return global.__neo_service_layer_wallet_get_address();
        },
        getBalance: (assetId) => {
            return global.__neo_service_layer_wallet_get_balance(assetId);
        },
        sendAsset: (assetId, to, amount) => {
            return global.__neo_service_layer_wallet_send_asset(assetId, to, amount);
        },
        invokeContract: (scriptHash, operation, args) => {
            return global.__neo_service_layer_wallet_invoke_contract(scriptHash, operation, JSON.stringify(args));
        }
    },
    
    // Function service
    functions: {
        invoke: (functionId, params) => {
            return global.__neo_service_layer_functions_invoke(functionId, JSON.stringify(params));
        }
    },
    
    // Storage service
    storage: {
        get: (key) => {
            return global.__neo_service_layer_storage_get(key);
        },
        set: (key, value) => {
            return global.__neo_service_layer_storage_set(key, JSON.stringify(value));
        },
        delete: (key) => {
            return global.__neo_service_layer_storage_delete(key);
        },
        list: (prefix) => {
            return JSON.parse(global.__neo_service_layer_storage_list(prefix));
        }
    },
    
    // HTTP client
    http: {
        get: (url, headers) => {
            return JSON.parse(global.__neo_service_layer_http_get(url, JSON.stringify(headers || {})));
        },
        post: (url, data, headers) => {
            return JSON.parse(global.__neo_service_layer_http_post(url, JSON.stringify(data), JSON.stringify(headers || {})));
        }
    },
    
    // Utilities
    utils: {
        hash: (data, algorithm) => {
            return global.__neo_service_layer_utils_hash(data, algorithm);
        },
        encrypt: (data, key) => {
            return global.__neo_service_layer_utils_encrypt(data, key);
        },
        decrypt: (data, key) => {
            return global.__neo_service_layer_utils_decrypt(data, key);
        },
        base64Encode: (data) => {
            return global.__neo_service_layer_utils_base64_encode(data);
        },
        base64Decode: (data) => {
            return global.__neo_service_layer_utils_base64_decode(data);
        }
    }
};

// Make the Neo Service Layer API available globally
global.priceFeed = neoServiceLayer.priceFeed;
global.secrets = neoServiceLayer.secrets;
global.wallet = neoServiceLayer.wallet;
global.functions = neoServiceLayer.functions;
global.storage = neoServiceLayer.storage;
global.http = neoServiceLayer.http;
global.utils = neoServiceLayer.utils;

// Function execution wrapper
function executeFunction(functionCode, params) {
    try {
        // Create a new function from the code
        const functionObj = new Function('params', functionCode);
        
        // Execute the function with the provided parameters
        const result = functionObj(params);
        
        // Return the result
        return {
            success: true,
            result: result
        };
    } catch (error) {
        // Return the error
        return {
            success: false,
            error: {
                message: error.message,
                stack: error.stack
            }
        };
    }
}

// Event handler execution wrapper
function executeEventHandler(functionCode, event) {
    try {
        // Create a new function from the code
        const functionObj = new Function('event', functionCode);
        
        // Execute the function with the provided event
        const result = functionObj(event);
        
        // Return the result
        return {
            success: true,
            result: result
        };
    } catch (error) {
        // Return the error
        return {
            success: false,
            error: {
                message: error.message,
                stack: error.stack
            }
        };
    }
}

// Export the execution functions
module.exports = {
    executeFunction,
    executeEventHandler
};
EOF

echo "JavaScript runtime environment setup completed successfully!"
