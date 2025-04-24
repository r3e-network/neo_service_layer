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
        // Get configuration from parameters or use defaults
        const symbols = params.symbols || ["NEO", "GAS", "BTC", "ETH"];
        const baseCurrency = params.baseCurrency || "USD";
        
        console.log(`Processing prices for symbols: ${symbols.join(", ")} in ${baseCurrency}`);
        
        // Fetch prices for each symbol
        const prices = [];
        for (const symbol of symbols) {
            try {
                const price = await neoService.priceFeed.getPrice(symbol, baseCurrency);
                console.log(`Fetched price for ${symbol}: ${JSON.stringify(price)}`);
                prices.push(price);
            } catch (error) {
                console.error(`Error fetching price for ${symbol}: ${error.message}`);
            }
        }
        
        // Submit prices to oracle
        const txHashes = [];
        for (const price of prices) {
            try {
                const txHash = await neoService.priceFeed.submitToOracle(price);
                console.log(`Submitted price for ${price.symbol} to oracle, tx: ${txHash}`);
                txHashes.push({ symbol: price.symbol, txHash });
            } catch (error) {
                console.error(`Error submitting price for ${price.symbol} to oracle: ${error.message}`);
            }
        }
        
        // Store the result in function storage
        await neoService.storage.set("last_execution", new Date().toISOString());
        await neoService.storage.set("last_prices", JSON.stringify(prices));
        await neoService.storage.set("last_tx_hashes", JSON.stringify(txHashes));
        
        return {
            success: true,
            processedSymbols: symbols,
            prices: prices,
            transactions: txHashes
        };
    } catch (error) {
        console.error(`Error in price feed oracle function: ${error.message}`);
        return {
            success: false,
            error: error.message
        };
    }
}
