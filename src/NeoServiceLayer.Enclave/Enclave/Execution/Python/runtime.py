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
