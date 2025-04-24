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
