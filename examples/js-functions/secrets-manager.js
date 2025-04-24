/**
 * Neo Secrets Manager Function
 * 
 * This function demonstrates how to use the Secrets service to securely
 * access and manage sensitive information.
 */

// Entry point for the function
async function manageSecrets(params) {
    console.log("Starting secrets manager function");
    
    try {
        // Get operation from parameters
        const operation = params.operation || "get";
        
        switch (operation) {
            case "get":
                return await getSecret(params);
            case "list":
                return await listSecrets(params);
            case "use":
                return await useSecretForApi(params);
            default:
                throw new Error(`Unknown operation: ${operation}`);
        }
    } catch (error) {
        console.error(`Error in secrets manager function: ${error.message}`);
        return {
            success: false,
            error: error.message
        };
    }
}

// Get a secret by name
async function getSecret(params) {
    const secretName = params.name;
    if (!secretName) {
        throw new Error("Secret name is required");
    }
    
    console.log(`Getting secret: ${secretName}`);
    
    const secretValue = await neoService.secrets.getSecret(secretName);
    
    // Never log the actual secret value in production!
    console.log(`Retrieved secret: ${secretName}`);
    
    return {
        success: true,
        name: secretName,
        // Return only metadata, not the actual value
        hasValue: !!secretValue
    };
}

// List available secrets (names only)
async function listSecrets(params) {
    console.log("Listing available secrets");
    
    // In a real implementation, you would get this from a repository
    // For this example, we'll just return a list of secret names that we know exist
    const secretNames = [
        "api_key",
        "database_password",
        "jwt_secret",
        "notification_api_key"
    ];
    
    return {
        success: true,
        secrets: secretNames.map(name => ({ name }))
    };
}

// Use a secret to make an API call
async function useSecretForApi(params) {
    const apiName = params.api || "example";
    const secretName = `${apiName}_api_key`;
    
    console.log(`Using secret for API call: ${apiName}`);
    
    // Get the API key from secrets
    const apiKey = await neoService.secrets.getSecret(secretName);
    if (!apiKey) {
        throw new Error(`API key not found for: ${apiName}`);
    }
    
    // In a real implementation, you would use the API key to make an API call
    // For this example, we'll just simulate a successful API call
    console.log(`Making API call to ${apiName} with API key: ${apiKey.substring(0, 3)}...`);
    
    // Store the API call timestamp
    await neoService.storage.set(`api_call_${apiName}`, new Date().toISOString());
    
    return {
        success: true,
        api: apiName,
        message: "API call successful"
    };
}
