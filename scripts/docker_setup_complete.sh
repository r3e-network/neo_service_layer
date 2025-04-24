#!/bin/bash

# Neo Service Layer Complete Docker Setup Script
# This script sets up and runs all services using Docker

# Exit on error
set -e

# Set the base directory to the current directory
BASE_DIR=$(pwd)

echo "Setting up Neo Service Layer using Docker (Complete Version)..."

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

# Create necessary directories
echo "Creating necessary directories..."
mkdir -p src/NeoServiceLayer.Enclave/Enclave/Execution/JavaScript
mkdir -p src/NeoServiceLayer.Enclave/Enclave/Execution/Python
mkdir -p src/NeoServiceLayer.Enclave/Enclave/Execution/DotNet
mkdir -p Templates

# Create JavaScript runtime files
echo "Creating JavaScript runtime files..."
cat > src/NeoServiceLayer.Enclave/Enclave/Execution/JavaScript/package.json << 'EOF'
{
  "name": "neo-service-layer-js-runtime",
  "version": "1.0.0",
  "description": "JavaScript runtime for Neo Service Layer",
  "main": "runtime.js",
  "dependencies": {
    "axios": "^0.21.1",
    "moment": "^2.29.1",
    "lodash": "^4.17.21"
  }
}
EOF

cat > src/NeoServiceLayer.Enclave/Enclave/Execution/JavaScript/runtime.js << 'EOF'
// Neo Service Layer JavaScript Runtime
const vm = require('vm');
const fs = require('fs');
const path = require('path');

// Create a sandbox for function execution
function createSandbox(functionCode, params, context) {
    // Create a sandbox with Neo Service Layer APIs
    const sandbox = {
        console: console,
        setTimeout: setTimeout,
        clearTimeout: clearTimeout,
        setInterval: setInterval,
        clearInterval: clearInterval,
        Buffer: Buffer,
        process: {
            env: context.environmentVariables || {}
        },
        neoService: {
            // Storage API
            storage: {
                get: async (key) => {
                    console.log(`Getting value for key: ${key}`);
                    return context.storage[key];
                },
                set: async (key, value) => {
                    console.log(`Setting value for key: ${key}`);
                    context.storage[key] = value;
                    return true;
                },
                delete: async (key) => {
                    console.log(`Deleting key: ${key}`);
                    delete context.storage[key];
                    return true;
                }
            },
            // Secrets API
            secrets: {
                get: async (secretName) => {
                    console.log(`Getting secret: ${secretName}`);
                    return context.secrets[secretName];
                }
            },
            // Blockchain API
            blockchain: {
                invokeRead: async (scriptHash, operation, args) => {
                    console.log(`Invoking read operation: ${operation} on contract: ${scriptHash}`);
                    return { result: "simulated-result" };
                },
                invokeWrite: async (scriptHash, operation, args) => {
                    console.log(`Invoking write operation: ${operation} on contract: ${scriptHash}`);
                    return "simulated-tx-hash";
                }
            },
            // Price Feed API
            priceFeed: {
                getPrice: async (symbol, baseCurrency = "USD") => {
                    console.log(`Getting price for ${symbol} in ${baseCurrency}`);
                    return { price: 100.0, timestamp: Date.now() };
                },
                getPriceHistory: async (symbol, baseCurrency = "USD", period = "1d") => {
                    console.log(`Getting price history for ${symbol} in ${baseCurrency} for period ${period}`);
                    return [
                        { price: 100.0, timestamp: Date.now() - 3600000 },
                        { price: 101.0, timestamp: Date.now() - 1800000 },
                        { price: 102.0, timestamp: Date.now() }
                    ];
                }
            },
            // Event API
            events: {
                register: async (contractHash, eventName, callbackFunction) => {
                    console.log(`Registering for event: ${eventName} on contract: ${contractHash}`);
                    return "simulated-subscription-id";
                },
                unregister: async (subscriptionId) => {
                    console.log(`Unregistering subscription: ${subscriptionId}`);
                    return true;
                }
            }
        },
        params: params || {}
    };
    
    return sandbox;
}

// Execute a function
async function executeFunction(functionCode, entryPoint, params, context) {
    try {
        // Create sandbox
        const sandbox = createSandbox(functionCode, params, context);
        
        // Create context
        const vmContext = vm.createContext(sandbox);
        
        // Execute function code
        vm.runInContext(functionCode, vmContext);
        
        // Call entry point
        if (typeof vmContext[entryPoint] !== 'function') {
            throw new Error(`Entry point ${entryPoint} is not a function`);
        }
        
        // Execute the function
        const result = await vmContext[entryPoint](params);
        return result;
    } catch (error) {
        console.error(`Error executing function: ${error.message}`);
        throw error;
    }
}

module.exports = {
    executeFunction
};
EOF

# Create Python runtime files
echo "Creating Python runtime files..."
cat > src/NeoServiceLayer.Enclave/Enclave/Execution/Python/requirements.txt << 'EOF'
requests==2.25.1
pandas==1.2.4
numpy==1.20.2
EOF

cat > src/NeoServiceLayer.Enclave/Enclave/Execution/Python/runtime.py << 'EOF'
# Neo Service Layer Python Runtime
import sys
import json
import traceback
from types import ModuleType
from typing import Any, Dict, List, Optional

# Create a sandbox for function execution
def create_sandbox(function_code: str, params: Dict[str, Any], context: Dict[str, Any]) -> Dict[str, Any]:
    # Create a sandbox with Neo Service Layer APIs
    sandbox = {
        "print": print,
        "params": params or {},
        "neo_service": {
            # Storage API
            "storage": {
                "get": lambda key: context.get("storage", {}).get(key),
                "set": lambda key, value: context.get("storage", {}).update({key: value}) or True,
                "delete": lambda key: context.get("storage", {}).pop(key, None) is not None
            },
            # Secrets API
            "secrets": {
                "get": lambda secret_name: context.get("secrets", {}).get(secret_name)
            },
            # Blockchain API
            "blockchain": {
                "invoke_read": lambda script_hash, operation, args: {"result": "simulated-result"},
                "invoke_write": lambda script_hash, operation, args: "simulated-tx-hash"
            },
            # Price Feed API
            "price_feed": {
                "get_price": lambda symbol, base_currency="USD": {"price": 100.0, "timestamp": 1625097600000},
                "get_price_history": lambda symbol, base_currency="USD", period="1d": [
                    {"price": 100.0, "timestamp": 1625097600000 - 3600000},
                    {"price": 101.0, "timestamp": 1625097600000 - 1800000},
                    {"price": 102.0, "timestamp": 1625097600000}
                ]
            },
            # Event API
            "events": {
                "register": lambda contract_hash, event_name, callback_function: "simulated-subscription-id",
                "unregister": lambda subscription_id: True
            }
        }
    }
    
    return sandbox

# Execute a function
def execute_function(function_code: str, entry_point: str, params: Dict[str, Any], context: Dict[str, Any]) -> Any:
    try:
        # Create sandbox
        sandbox = create_sandbox(function_code, params, context)
        
        # Create a module for the function
        module = ModuleType("user_function")
        
        # Set sandbox variables in the module
        for key, value in sandbox.items():
            setattr(module, key, value)
        
        # Execute function code
        exec(function_code, module.__dict__)
        
        # Call entry point
        if not hasattr(module, entry_point) or not callable(getattr(module, entry_point)):
            raise Exception(f"Entry point {entry_point} is not a function")
        
        # Execute the function
        result = getattr(module, entry_point)(params)
        return result
    except Exception as e:
        print(f"Error executing function: {str(e)}")
        traceback.print_exc()
        raise e

# Main function for CLI usage
def main():
    if len(sys.argv) < 4:
        print("Usage: python runtime.py <function_file> <entry_point> <params_json> [<context_json>]")
        sys.exit(1)
    
    function_file = sys.argv[1]
    entry_point = sys.argv[2]
    params_json = sys.argv[3]
    context_json = sys.argv[4] if len(sys.argv) > 4 else "{}"
    
    with open(function_file, "r") as f:
        function_code = f.read()
    
    params = json.loads(params_json)
    context = json.loads(context_json)
    
    result = execute_function(function_code, entry_point, params, context)
    print(json.dumps(result))

if __name__ == "__main__":
    main()
EOF

# Create .NET runtime files
echo "Creating .NET runtime files..."
cat > src/NeoServiceLayer.Enclave/Enclave/Execution/DotNet/Runtime.cs << 'EOF'
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.Loader;

namespace NeoServiceLayer.Enclave.Execution.DotNet
{
    /// <summary>
    /// .NET runtime for executing C# functions
    /// </summary>
    public class Runtime
    {
        /// <summary>
        /// Executes a C# function
        /// </summary>
        /// <param name="functionCode">Function code</param>
        /// <param name="entryPoint">Entry point method</param>
        /// <param name="parameters">Function parameters</param>
        /// <param name="context">Execution context</param>
        /// <returns>Function result</returns>
        public async Task<object> ExecuteFunctionAsync(string functionCode, string entryPoint, Dictionary<string, object> parameters, Dictionary<string, object> context)
        {
            try
            {
                // Compile the function
                var assembly = CompileFunction(functionCode);
                
                // Find the entry point method
                var entryPointParts = entryPoint.Split('.');
                var className = entryPointParts.Length > 1 ? entryPointParts[0] : "UserFunction";
                var methodName = entryPointParts.Length > 1 ? entryPointParts[1] : entryPoint;
                
                var type = assembly.GetType(className);
                if (type == null)
                {
                    throw new Exception($"Class {className} not found");
                }
                
                var method = type.GetMethod(methodName);
                if (method == null)
                {
                    throw new Exception($"Method {methodName} not found in class {className}");
                }
                
                // Create an instance of the class
                var instance = Activator.CreateInstance(type);
                
                // Set context properties
                var contextProperty = type.GetProperty("Context");
                if (contextProperty != null)
                {
                    contextProperty.SetValue(instance, context);
                }
                
                // Call the method
                var result = method.Invoke(instance, new object[] { parameters });
                
                // Handle async methods
                if (result is Task task)
                {
                    await task;
                    var resultProperty = task.GetType().GetProperty("Result");
                    if (resultProperty != null)
                    {
                        result = resultProperty.GetValue(task);
                    }
                    else
                    {
                        result = null;
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error executing function: {ex.Message}");
                throw;
            }
        }
        
        private Assembly CompileFunction(string functionCode)
        {
            // Parse the function code
            var syntaxTree = CSharpSyntaxTree.ParseText(functionCode);
            
            // Create references to necessary assemblies
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location)
            };
            
            // Create compilation
            var compilation = CSharpCompilation.Create(
                "UserFunction",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            
            // Emit the assembly
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                
                if (!result.Success)
                {
                    var errors = new List<string>();
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        if (diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            errors.Add(diagnostic.ToString());
                        }
                    }
                    
                    throw new Exception($"Compilation failed: {string.Join(Environment.NewLine, errors)}");
                }
                
                ms.Seek(0, SeekOrigin.Begin);
                
                // Load the assembly
                var assemblyLoadContext = new AssemblyLoadContext("UserFunction", true);
                return assemblyLoadContext.LoadFromStream(ms);
            }
        }
    }
}
EOF

cat > src/NeoServiceLayer.Enclave/Enclave/Execution/DotNet/DotNetRuntime.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.0.1" />
    <PackageReference Include="System.Runtime.Loader" Version="4.3.0" />
  </ItemGroup>

</Project>
EOF

# Create template files
echo "Creating template files..."
cat > Templates/price-feed-oracle.js << 'EOF'
/**
 * Neo Price Feed Oracle Function
 * 
 * This function fetches price data from multiple sources, processes it,
 * and submits it to the Neo N3 blockchain oracle.
 */

// Entry point for the function
async function processPrices(params) {
    console.log("Starting price feed oracle function");
    
    try {
        // Get parameters or use defaults
        const symbols = params.symbols || process.env.DEFAULT_SYMBOLS || "NEO,GAS,BTC,ETH";
        const baseCurrency = params.baseCurrency || process.env.DEFAULT_BASE_CURRENCY || "USD";
        
        // Split symbols into an array
        const symbolArray = symbols.split(",").map(s => s.trim());
        
        console.log(`Processing prices for symbols: ${symbolArray.join(", ")} in ${baseCurrency}`);
        
        // Fetch prices for each symbol
        const priceData = {};
        for (const symbol of symbolArray) {
            const price = await neoService.priceFeed.getPrice(symbol, baseCurrency);
            priceData[symbol] = price;
            console.log(`Fetched price for ${symbol}: ${price.price} ${baseCurrency}`);
        }
        
        // Get the oracle API key
        const oracleApiKey = await neoService.secrets.get("oracle_api_key");
        if (!oracleApiKey) {
            throw new Error("Oracle API key not found");
        }
        
        // Submit prices to the blockchain oracle
        // In a real implementation, this would make an API call to the oracle service
        console.log(`Submitting prices to blockchain oracle with API key: ${oracleApiKey.substring(0, 3)}...`);
        
        // Store the last update timestamp
        await neoService.storage.set("last_price_update", new Date().toISOString());
        await neoService.storage.set("last_price_data", JSON.stringify(priceData));
        
        return {
            success: true,
            timestamp: new Date().toISOString(),
            priceData: priceData
        };
    } catch (error) {
        console.error(`Error in price feed oracle function: ${error.message}`);
        return {
            success: false,
            error: error.message
        };
    }
}
EOF

cat > Templates/blockchain-event-handler.js << 'EOF'
/**
 * Neo Blockchain Event Handler Function
 * 
 * This function handles blockchain events and performs actions based on the event data.
 */

// Entry point for the function
async function handleEvent(params) {
    console.log("Starting blockchain event handler function");
    
    try {
        // Get event data from parameters
        const { contractHash, eventName, eventData } = params;
        
        if (!contractHash || !eventName) {
            throw new Error("Missing required parameters: contractHash and eventName");
        }
        
        console.log(`Processing event: ${eventName} from contract: ${contractHash}`);
        
        // Log the event if logging is enabled
        const loggingEnabled = process.env.EVENT_LOGGING_ENABLED === "true";
        if (loggingEnabled) {
            console.log(`Event data: ${JSON.stringify(eventData)}`);
            
            // Store the event in the function's storage
            const eventKey = `event_${contractHash}_${eventName}_${Date.now()}`;
            await neoService.storage.set(eventKey, JSON.stringify({
                contractHash,
                eventName,
                eventData,
                timestamp: new Date().toISOString()
            }));
        }
        
        // Process different event types
        switch (eventName) {
            case "Transfer":
                return await handleTransferEvent(contractHash, eventData);
            case "Mint":
                return await handleMintEvent(contractHash, eventData);
            case "Burn":
                return await handleBurnEvent(contractHash, eventData);
            default:
                console.log(`No specific handler for event type: ${eventName}`);
                return {
                    success: true,
                    message: `Event ${eventName} processed with default handler`,
                    data: eventData
                };
        }
    } catch (error) {
        console.error(`Error in blockchain event handler function: ${error.message}`);
        return {
            success: false,
            error: error.message
        };
    }
}

// Handler for Transfer events
async function handleTransferEvent(contractHash, eventData) {
    console.log(`Handling Transfer event from contract: ${contractHash}`);
    
    // Extract transfer details
    const { from, to, amount } = eventData;
    
    // Get notification API key for sending alerts
    const notificationApiKey = await neoService.secrets.get("notification_api_key");
    
    // In a real implementation, you would send notifications or trigger other actions
    console.log(`Would send transfer notification with API key: ${notificationApiKey.substring(0, 3)}...`);
    
    return {
        success: true,
        message: "Transfer event processed successfully",
        transfer: {
            from,
            to,
            amount
        }
    };
}

// Handler for Mint events
async function handleMintEvent(contractHash, eventData) {
    console.log(`Handling Mint event from contract: ${contractHash}`);
    
    // Extract mint details
    const { to, amount } = eventData;
    
    return {
        success: true,
        message: "Mint event processed successfully",
        mint: {
            to,
            amount
        }
    };
}

// Handler for Burn events
async function handleBurnEvent(contractHash, eventData) {
    console.log(`Handling Burn event from contract: ${contractHash}`);
    
    // Extract burn details
    const { from, amount } = eventData;
    
    return {
        success: true,
        message: "Burn event processed successfully",
        burn: {
            from,
            amount
        }
    };
}
EOF

# Make the init-mongodb.sh script executable
chmod +x "$BASE_DIR/init-mongodb.sh"

# Build and start the Docker containers
echo "Building and starting Docker containers..."
docker-compose build
docker-compose up -d

echo "Neo Service Layer Docker setup (Complete Version) completed successfully!"
echo ""
echo "The following services are now running:"
echo "  - API: http://localhost:8080"
echo "  - Swagger UI: http://localhost:8081"
echo "  - MongoDB: localhost:27017"
echo "  - Redis: localhost:6379"
echo "  - MailHog: http://localhost:8025"
echo "  - Prometheus: http://localhost:9090"
echo "  - Grafana: http://localhost:3000"
echo ""
echo "You can view the logs with:"
echo "  docker-compose logs -f"
echo ""
echo "To stop the services, run:"
echo "  docker-compose down"
