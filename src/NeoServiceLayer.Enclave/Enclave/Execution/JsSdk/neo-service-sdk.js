/**
 * Neo Service Layer JavaScript SDK
 * This SDK provides JavaScript functions with access to Neo Service Layer capabilities
 */

// Global SDK object
const neoService = {
    /**
     * Price Feed service functions
     */
    priceFeed: {
        /**
         * Fetches price data for a specific symbol
         * @param {string} symbol - Symbol to fetch price for (e.g., "NEO", "GAS")
         * @param {string} baseCurrency - Base currency for the price (default: "USD")
         * @returns {Promise<object>} - Price data
         */
        async getPrice(symbol, baseCurrency = "USD") {
            return await _callNativeFunction("priceFeed.getPrice", { symbol, baseCurrency });
        },

        /**
         * Fetches price data from all configured sources
         * @param {string} baseCurrency - Base currency for the prices (default: "USD")
         * @returns {Promise<Array>} - List of prices
         */
        async getAllPrices(baseCurrency = "USD") {
            return await _callNativeFunction("priceFeed.getAllPrices", { baseCurrency });
        },

        /**
         * Submits price data to the Neo N3 oracle contract
         * @param {object} price - Price data to submit
         * @returns {Promise<string>} - Transaction hash
         */
        async submitToOracle(price) {
            return await _callNativeFunction("priceFeed.submitToOracle", { price });
        }
    },

    /**
     * Secrets service functions
     */
    secrets: {
        /**
         * Gets a secret value by name
         * @param {string} name - Secret name
         * @returns {Promise<string>} - Secret value
         */
        async getSecret(name) {
            return await _callNativeFunction("secrets.getSecret", { name });
        },

        /**
         * Gets a secret value by ID
         * @param {string} id - Secret ID
         * @returns {Promise<string>} - Secret value
         */
        async getSecretById(id) {
            return await _callNativeFunction("secrets.getSecretById", { id });
        }
    },

    /**
     * Blockchain service functions
     */
    blockchain: {
        /**
         * Gets the current block height
         * @returns {Promise<number>} - Current block height
         */
        async getBlockHeight() {
            return await _callNativeFunction("blockchain.getBlockHeight", {});
        },

        /**
         * Gets a block by height
         * @param {number} height - Block height
         * @returns {Promise<object>} - Block data
         */
        async getBlock(height) {
            return await _callNativeFunction("blockchain.getBlock", { height });
        },

        /**
         * Gets a transaction by hash
         * @param {string} txHash - Transaction hash
         * @returns {Promise<object>} - Transaction data
         */
        async getTransaction(txHash) {
            return await _callNativeFunction("blockchain.getTransaction", { txHash });
        },

        /**
         * Gets the balance of an address
         * @param {string} address - Neo address
         * @param {string} assetHash - Asset hash (NEO, GAS, or other NEP-17 token)
         * @returns {Promise<number>} - Balance
         */
        async getBalance(address, assetHash) {
            return await _callNativeFunction("blockchain.getBalance", { address, assetHash });
        },

        /**
         * Invokes a contract read-only method
         * @param {string} scriptHash - Contract script hash
         * @param {string} operation - Contract operation
         * @param {Array} args - Contract arguments
         * @returns {Promise<object>} - Invocation result
         */
        async invokeRead(scriptHash, operation, args = []) {
            return await _callNativeFunction("blockchain.invokeRead", { scriptHash, operation, args });
        },

        /**
         * Invokes a contract method that modifies state
         * @param {string} scriptHash - Contract script hash
         * @param {string} operation - Contract operation
         * @param {Array} args - Contract arguments
         * @returns {Promise<string>} - Transaction hash
         */
        async invokeWrite(scriptHash, operation, args = []) {
            return await _callNativeFunction("blockchain.invokeWrite", { scriptHash, operation, args });
        }
    },

    /**
     * Event service functions
     */
    events: {
        /**
         * Registers a blockchain event subscription
         * @param {string} contractHash - Contract hash to monitor
         * @param {string} eventName - Event name to monitor
         * @param {string} callbackUrl - URL to call when the event occurs
         * @returns {Promise<object>} - Subscription data
         */
        async registerBlockchainEvent(contractHash, eventName, callbackUrl) {
            return await _callNativeFunction("events.registerBlockchainEvent", { contractHash, eventName, callbackUrl });
        },

        /**
         * Registers a time-based event subscription
         * @param {string} name - Event name
         * @param {string} cronExpression - Cron expression for the event schedule
         * @param {string} callbackUrl - URL to call when the event occurs
         * @returns {Promise<object>} - Subscription data
         */
        async registerTimeEvent(name, cronExpression, callbackUrl) {
            return await _callNativeFunction("events.registerTimeEvent", { name, cronExpression, callbackUrl });
        },

        /**
         * Triggers a custom event
         * @param {string} name - Event name
         * @param {string} source - Event source
         * @param {object} data - Event data
         * @returns {Promise<object>} - Event data
         */
        async triggerCustomEvent(name, source, data) {
            return await _callNativeFunction("events.triggerCustomEvent", { name, source, data });
        }
    },

    /**
     * Logging functions
     */
    log: {
        /**
         * Logs an informational message
         * @param {string} message - Message to log
         */
        info(message) {
            _callNativeFunction("log.info", { message });
        },

        /**
         * Logs a warning message
         * @param {string} message - Message to log
         */
        warn(message) {
            _callNativeFunction("log.warn", { message });
        },

        /**
         * Logs an error message
         * @param {string} message - Message to log
         */
        error(message) {
            _callNativeFunction("log.error", { message });
        },

        /**
         * Logs a debug message
         * @param {string} message - Message to log
         */
        debug(message) {
            _callNativeFunction("log.debug", { message });
        }
    },

    /**
     * Storage functions
     */
    storage: {
        /**
         * Gets a value from storage
         * @param {string} key - Storage key
         * @returns {Promise<string>} - Storage value
         */
        async get(key) {
            return await _callNativeFunction("storage.get", { key });
        },

        /**
         * Sets a value in storage
         * @param {string} key - Storage key
         * @param {string} value - Storage value
         * @returns {Promise<boolean>} - Success indicator
         */
        async set(key, value) {
            return await _callNativeFunction("storage.set", { key, value });
        },

        /**
         * Deletes a value from storage
         * @param {string} key - Storage key
         * @returns {Promise<boolean>} - Success indicator
         */
        async delete(key) {
            return await _callNativeFunction("storage.delete", { key });
        }
    }
};

// Internal function to call native functions
async function _callNativeFunction(functionName, args) {
    // This function will be replaced by the actual implementation in NodeJsRuntime
    if (typeof __callNativeFunction === 'function') {
        return await __callNativeFunction(functionName, args);
    } else {
        throw new Error(`Native function ${functionName} is not available`);
    }
}

// Export the SDK
if (typeof module !== 'undefined' && module.exports) {
    module.exports = neoService;
}
