/**
 * Neo Contract Interaction Function
 * 
 * This function demonstrates how to interact with Neo N3 smart contracts.
 */

// Entry point for the function
async function interactWithContract(params) {
    console.log("Starting contract interaction function");
    
    try {
        // Get contract parameters from input or use defaults
        const scriptHash = params.scriptHash || "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5"; // Example contract
        const operation = params.operation || "balanceOf";
        const args = params.args || [];
        
        console.log(`Interacting with contract: ${scriptHash}, operation: ${operation}`);
        
        // Perform a read-only invocation
        const result = await neoService.blockchain.invokeRead(scriptHash, operation, args);
        console.log(`Contract invocation result: ${JSON.stringify(result)}`);
        
        // If this is a write operation and params.execute is true, execute the transaction
        if (params.execute && params.isWriteOperation) {
            const txHash = await neoService.blockchain.invokeWrite(scriptHash, operation, args);
            console.log(`Transaction executed, hash: ${txHash}`);
            
            // Store the transaction hash
            await neoService.storage.set(`tx_${new Date().toISOString()}`, txHash);
            
            return {
                success: true,
                readResult: result,
                writeResult: {
                    txHash: txHash
                }
            };
        }
        
        // Store the invocation result
        await neoService.storage.set(`invoke_${new Date().toISOString()}`, JSON.stringify(result));
        
        return {
            success: true,
            result: result
        };
    } catch (error) {
        console.error(`Error in contract interaction function: ${error.message}`);
        return {
            success: false,
            error: error.message
        };
    }
}
