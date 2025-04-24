#!/bin/bash

# Neo Service Layer Docker Setup Script
# This script sets up and runs the complete Neo Service Layer using Docker

# Exit on error
set -e

# Set the base directory to the current directory
BASE_DIR=$(pwd)

echo "Setting up Neo Service Layer using Docker..."

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "Error: Docker is not installed. Please install Docker from https://www.docker.com/get-started"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "Error: Docker Compose is not installed. Please install Docker Compose from https://docs.docker.com/compose/install/"
    exit 1
fi

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "Error: Docker is not running. Please start Docker and try again."
    exit 1
fi

# Create necessary directories for JavaScript runtime
echo "Creating JavaScript runtime directory..."
mkdir -p "$BASE_DIR/src/NeoServiceLayer.Enclave/Enclave/Execution/JavaScript"

# Create package.json file for JavaScript runtime
echo "Creating package.json file for JavaScript runtime..."
cat > "$BASE_DIR/src/NeoServiceLayer.Enclave/Enclave/Execution/JavaScript/package.json" << EOF
{
  "name": "neo-service-layer-js-runtime",
  "version": "1.0.0",
  "description": "JavaScript runtime for Neo Service Layer",
  "main": "runtime.js",
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

# Create necessary directories for Python runtime
echo "Creating Python runtime directory..."
mkdir -p "$BASE_DIR/src/NeoServiceLayer.Enclave/Enclave/Execution/Python"

# Create requirements.txt file for Python runtime
echo "Creating requirements.txt file for Python runtime..."
cat > "$BASE_DIR/src/NeoServiceLayer.Enclave/Enclave/Execution/Python/requirements.txt" << EOF
requests==2.28.2
pyyaml==6.0
python-dotenv==1.0.0
cryptography==39.0.2
EOF

# Create Python runtime wrapper
echo "Creating Python runtime wrapper..."
cat > "$BASE_DIR/src/NeoServiceLayer.Enclave/Enclave/Execution/Python/runtime.py" << EOF
# Neo Service Layer Python Runtime Wrapper

import json
import sys
import traceback

# Neo Service Layer API
class PriceFeed:
    def get_price(self, symbol):
        return __neo_service_layer_price_feed_get_price(symbol)

    def get_prices(self, symbols):
        return __neo_service_layer_price_feed_get_prices(symbols)

class Secrets:
    def get(self, key):
        return __neo_service_layer_secrets_get(key)

    def set(self, key, value):
        return __neo_service_layer_secrets_set(key, value)

    def delete(self, key):
        return __neo_service_layer_secrets_delete(key)

class Wallet:
    def get_address(self):
        return __neo_service_layer_wallet_get_address()

    def get_balance(self, asset_id):
        return __neo_service_layer_wallet_get_balance(asset_id)

    def send_asset(self, asset_id, to, amount):
        return __neo_service_layer_wallet_send_asset(asset_id, to, amount)

    def invoke_contract(self, script_hash, operation, args):
        return __neo_service_layer_wallet_invoke_contract(script_hash, operation, json.dumps(args))

class Functions:
    def invoke(self, function_id, params):
        return __neo_service_layer_functions_invoke(function_id, json.dumps(params))

class Storage:
    def get(self, key):
        return __neo_service_layer_storage_get(key)

    def set(self, key, value):
        return __neo_service_layer_storage_set(key, json.dumps(value))

    def delete(self, key):
        return __neo_service_layer_storage_delete(key)

    def list(self, prefix):
        return json.loads(__neo_service_layer_storage_list(prefix))

class Http:
    def get(self, url, headers=None):
        if headers is None:
            headers = {}
        return json.loads(__neo_service_layer_http_get(url, json.dumps(headers)))

    def post(self, url, data, headers=None):
        if headers is None:
            headers = {}
        return json.loads(__neo_service_layer_http_post(url, json.dumps(data), json.dumps(headers)))

class Utils:
    def hash(self, data, algorithm):
        return __neo_service_layer_utils_hash(data, algorithm)

    def encrypt(self, data, key):
        return __neo_service_layer_utils_encrypt(data, key)

    def decrypt(self, data, key):
        return __neo_service_layer_utils_decrypt(data, key)

    def base64_encode(self, data):
        return __neo_service_layer_utils_base64_encode(data)

    def base64_decode(self, data):
        return __neo_service_layer_utils_base64_decode(data)

# Create global instances
price_feed = PriceFeed()
secrets = Secrets()
wallet = Wallet()
functions = Functions()
storage = Storage()
http = Http()
utils = Utils()

def execute_function(function_code, params):
    try:
        # Create a new function from the code
        namespace = {
            'price_feed': price_feed,
            'secrets': secrets,
            'wallet': wallet,
            'functions': functions,
            'storage': storage,
            'http': http,
            'utils': utils,
            'params': params
        }

        # Execute the function
        exec(function_code, namespace)

        # Get the result
        if 'result' in namespace:
            result = namespace['result']
        else:
            result = None

        # Return the result
        return {
            'success': True,
            'result': result
        }
    except Exception as e:
        # Return the error
        return {
            'success': False,
            'error': {
                'message': str(e),
                'stack': traceback.format_exc()
            }
        }

def execute_event_handler(function_code, event):
    try:
        # Create a new function from the code
        namespace = {
            'price_feed': price_feed,
            'secrets': secrets,
            'wallet': wallet,
            'functions': functions,
            'storage': storage,
            'http': http,
            'utils': utils,
            'event': event
        }

        # Execute the function
        exec(function_code, namespace)

        # Get the result
        if 'result' in namespace:
            result = namespace['result']
        else:
            result = None

        # Return the result
        return {
            'success': True,
            'result': result
        }
    except Exception as e:
        # Return the error
        return {
            'success': False,
            'error': {
                'message': str(e),
                'stack': traceback.format_exc()
            }
        }
EOF

# Create necessary directories for .NET runtime
echo "Creating .NET runtime directory..."
mkdir -p "$BASE_DIR/src/NeoServiceLayer.Enclave/Enclave/Execution/DotNet"

# Create .NET runtime wrapper
echo "Creating .NET runtime wrapper..."
cat > "$BASE_DIR/src/NeoServiceLayer.Enclave/Enclave/Execution/DotNet/Runtime.cs" << EOF
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace NeoServiceLayer.Enclave.Execution.DotNet
{
    public class Runtime
    {
        // Neo Service Layer API
        public class PriceFeed
        {
            public string GetPrice(string symbol)
            {
                return __neo_service_layer_price_feed_get_price(symbol);
            }

            public string GetPrices(string[] symbols)
            {
                return __neo_service_layer_price_feed_get_prices(symbols);
            }
        }

        public class Secrets
        {
            public string Get(string key)
            {
                return __neo_service_layer_secrets_get(key);
            }

            public string Set(string key, string value)
            {
                return __neo_service_layer_secrets_set(key, value);
            }

            public string Delete(string key)
            {
                return __neo_service_layer_secrets_delete(key);
            }
        }

        public class Wallet
        {
            public string GetAddress()
            {
                return __neo_service_layer_wallet_get_address();
            }

            public string GetBalance(string assetId)
            {
                return __neo_service_layer_wallet_get_balance(assetId);
            }

            public string SendAsset(string assetId, string to, decimal amount)
            {
                return __neo_service_layer_wallet_send_asset(assetId, to, amount.ToString());
            }

            public string InvokeContract(string scriptHash, string operation, object[] args)
            {
                return __neo_service_layer_wallet_invoke_contract(scriptHash, operation, JsonSerializer.Serialize(args));
            }
        }

        public class Functions
        {
            public string Invoke(string functionId, object parameters)
            {
                return __neo_service_layer_functions_invoke(functionId, JsonSerializer.Serialize(parameters));
            }
        }

        public class Storage
        {
            public string Get(string key)
            {
                return __neo_service_layer_storage_get(key);
            }

            public string Set(string key, object value)
            {
                return __neo_service_layer_storage_set(key, JsonSerializer.Serialize(value));
            }

            public string Delete(string key)
            {
                return __neo_service_layer_storage_delete(key);
            }

            public string[] List(string prefix)
            {
                return JsonSerializer.Deserialize<string[]>(__neo_service_layer_storage_list(prefix));
            }
        }

        public class Http
        {
            public T Get<T>(string url, Dictionary<string, string> headers = null)
            {
                headers = headers ?? new Dictionary<string, string>();
                string response = __neo_service_layer_http_get(url, JsonSerializer.Serialize(headers));
                return JsonSerializer.Deserialize<T>(response);
            }

            public T Post<T>(string url, object data, Dictionary<string, string> headers = null)
            {
                headers = headers ?? new Dictionary<string, string>();
                string response = __neo_service_layer_http_post(url, JsonSerializer.Serialize(data), JsonSerializer.Serialize(headers));
                return JsonSerializer.Deserialize<T>(response);
            }
        }

        public class Utils
        {
            public string Hash(string data, string algorithm)
            {
                return __neo_service_layer_utils_hash(data, algorithm);
            }

            public string Encrypt(string data, string key)
            {
                return __neo_service_layer_utils_encrypt(data, key);
            }

            public string Decrypt(string data, string key)
            {
                return __neo_service_layer_utils_decrypt(data, key);
            }

            public string Base64Encode(string data)
            {
                return __neo_service_layer_utils_base64_encode(data);
            }

            public string Base64Decode(string data)
            {
                return __neo_service_layer_utils_base64_decode(data);
            }
        }

        // Global instances
        public static readonly PriceFeed priceFeed = new PriceFeed();
        public static readonly Secrets secrets = new Secrets();
        public static readonly Wallet wallet = new Wallet();
        public static readonly Functions functions = new Functions();
        public static readonly Storage storage = new Storage();
        public static readonly Http http = new Http();
        public static readonly Utils utils = new Utils();

        // Function execution wrapper
        public static async Task<string> ExecuteFunction(string functionCode, string parametersJson)
        {
            try
            {
                // Create script options
                var options = ScriptOptions.Default
                    .AddReferences(typeof(System.Console).Assembly)
                    .AddReferences(typeof(System.Text.Json.JsonSerializer).Assembly)
                    .AddImports("System", "System.Collections.Generic", "System.Linq", "System.Text.Json");

                // Parse parameters
                var parameters = JsonSerializer.Deserialize<object>(parametersJson);

                // Create globals
                var globals = new Globals
                {
                    priceFeed = priceFeed,
                    secrets = secrets,
                    wallet = wallet,
                    functions = functions,
                    storage = storage,
                    http = http,
                    utils = utils,
                    parameters = parameters
                };

                // Execute the function
                var result = await CSharpScript.EvaluateAsync<object>(functionCode, options, globals);

                // Return the result
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    result = result
                });
            }
            catch (Exception ex)
            {
                // Return the error
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = new
                    {
                        message = ex.Message,
                        stack = ex.StackTrace
                    }
                });
            }
        }

        // Event handler execution wrapper
        public static async Task<string> ExecuteEventHandler(string functionCode, string eventJson)
        {
            try
            {
                // Create script options
                var options = ScriptOptions.Default
                    .AddReferences(typeof(System.Console).Assembly)
                    .AddReferences(typeof(System.Text.Json.JsonSerializer).Assembly)
                    .AddImports("System", "System.Collections.Generic", "System.Linq", "System.Text.Json");

                // Parse event
                var eventObj = JsonSerializer.Deserialize<object>(eventJson);

                // Create globals
                var globals = new Globals
                {
                    priceFeed = priceFeed,
                    secrets = secrets,
                    wallet = wallet,
                    functions = functions,
                    storage = storage,
                    http = http,
                    utils = utils,
                    eventObj = eventObj
                };

                // Execute the function
                var result = await CSharpScript.EvaluateAsync<object>(functionCode, options, globals);

                // Return the result
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    result = result
                });
            }
            catch (Exception ex)
            {
                // Return the error
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = new
                    {
                        message = ex.Message,
                        stack = ex.StackTrace
                    }
                });
            }
        }

        // Globals class for script execution
        public class Globals
        {
            public PriceFeed priceFeed { get; set; }
            public Secrets secrets { get; set; }
            public Wallet wallet { get; set; }
            public Functions functions { get; set; }
            public Storage storage { get; set; }
            public Http http { get; set; }
            public Utils utils { get; set; }
            public object parameters { get; set; }
            public object eventObj { get; set; }
        }

        // Native function declarations
        private static string __neo_service_layer_price_feed_get_price(string symbol) => throw new NotImplementedException();
        private static string __neo_service_layer_price_feed_get_prices(string[] symbols) => throw new NotImplementedException();
        private static string __neo_service_layer_secrets_get(string key) => throw new NotImplementedException();
        private static string __neo_service_layer_secrets_set(string key, string value) => throw new NotImplementedException();
        private static string __neo_service_layer_secrets_delete(string key) => throw new NotImplementedException();
        private static string __neo_service_layer_wallet_get_address() => throw new NotImplementedException();
        private static string __neo_service_layer_wallet_get_balance(string assetId) => throw new NotImplementedException();
        private static string __neo_service_layer_wallet_send_asset(string assetId, string to, string amount) => throw new NotImplementedException();
        private static string __neo_service_layer_wallet_invoke_contract(string scriptHash, string operation, string args) => throw new NotImplementedException();
        private static string __neo_service_layer_functions_invoke(string functionId, string parameters) => throw new NotImplementedException();
        private static string __neo_service_layer_storage_get(string key) => throw new NotImplementedException();
        private static string __neo_service_layer_storage_set(string key, string value) => throw new NotImplementedException();
        private static string __neo_service_layer_storage_delete(string key) => throw new NotImplementedException();
        private static string __neo_service_layer_storage_list(string prefix) => throw new NotImplementedException();
        private static string __neo_service_layer_http_get(string url, string headers) => throw new NotImplementedException();
        private static string __neo_service_layer_http_post(string url, string data, string headers) => throw new NotImplementedException();
        private static string __neo_service_layer_utils_hash(string data, string algorithm) => throw new NotImplementedException();
        private static string __neo_service_layer_utils_encrypt(string data, string key) => throw new NotImplementedException();
        private static string __neo_service_layer_utils_decrypt(string data, string key) => throw new NotImplementedException();
        private static string __neo_service_layer_utils_base64_encode(string data) => throw new NotImplementedException();
        private static string __neo_service_layer_utils_base64_decode(string data) => throw new NotImplementedException();
    }
}
EOF

# Create .NET runtime project file
echo "Creating .NET runtime project file..."
cat > "$BASE_DIR/src/NeoServiceLayer.Enclave/Enclave/Execution/DotNet/DotNetRuntime.csproj" << EOF
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp.Scripting" Version="4.5.0" />
  </ItemGroup>

</Project>
EOF

# Update the init-mongodb.sh script to use mongosh instead of mongo
echo "Updating init-mongodb.sh script..."
cat > "$BASE_DIR/init-mongodb.sh" << EOF
#!/bin/bash

echo "Initializing MongoDB..."

# Wait for MongoDB to be ready
until mongosh --host mongodb --eval "print(\"waited for connection\")"
do
    echo "Waiting for MongoDB to be ready..."
    sleep 2
done

# Create the database and collections
mongosh --host mongodb <<EOFMONGO
use neo_service_layer;

// Create collections
db.createCollection("accounts");
db.createCollection("wallets");
db.createCollection("secrets");
db.createCollection("functions");
db.createCollection("price_feeds");
db.createCollection("migrations");

// Create indexes
db.accounts.createIndex({ "email": 1 }, { unique: true });
db.wallets.createIndex({ "accountId": 1 });
db.secrets.createIndex({ "accountId": 1 });
db.functions.createIndex({ "accountId": 1 });
db.price_feeds.createIndex({ "symbol": 1 });
db.migrations.createIndex({ "version": 1 }, { unique: true });

// Insert initial migration records
db.migrations.insertOne({ "version": 1, "name": "InitialSchema", "appliedAt": new Date() });
db.migrations.insertOne({ "version": 2, "name": "AddIndexes", "appliedAt": new Date() });

print("MongoDB initialization completed successfully!");
EOFMONGO

echo "MongoDB initialization completed!"

# Make the init-mongodb.sh script executable
chmod +x "$BASE_DIR/init-mongodb.sh"

# Build and start the Docker containers
echo "Building and starting Docker containers..."
docker-compose build
docker-compose up -d

echo "Neo Service Layer Docker setup completed successfully!"
echo ""
echo "The following services are now running:"
echo "  - API: http://localhost:8080"
echo "  - Swagger UI: http://localhost:8081"
echo "  - Grafana: http://localhost:3000"
echo "  - Prometheus: http://localhost:9090"
echo "  - MailHog: http://localhost:8025"
echo ""
echo "You can view the logs with:"
echo "  docker-compose logs -f"
echo ""
echo "To stop the services, run:"
echo "  docker-compose down"
