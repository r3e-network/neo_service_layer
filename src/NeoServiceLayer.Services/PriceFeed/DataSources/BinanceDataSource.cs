using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.PriceFeed.DataSources
{
    /// <summary>
    /// Binance price data source
    /// </summary>
    public class BinanceDataSource : BasePriceDataSource
    {
        private readonly HashSet<string> _supportedAssets;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinanceDataSource"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="httpClient">HTTP client</param>
        public BinanceDataSource(ILogger<BinanceDataSource> logger, HttpClient httpClient)
            : base(logger, httpClient)
        {
            _supportedAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "BTC", "ETH", "NEO", "GAS", "FLM", "BNB", "USDT", "USDC", "DAI", "LINK",
                "UNI", "AAVE", "COMP", "SNX", "YFI", "SUSHI", "MKR", "BAT", "ZRX", "REN",
                "KNC", "BAL", "CRV", "LRC", "MATIC", "DOGE", "DOT", "ADA", "XRP", "SOL",
                "AVAX", "LUNA", "ATOM", "ALGO", "FTM", "NEAR", "ONE", "HBAR", "EGLD", "FLOW",
                "XTZ", "EOS", "CAKE", "AXS", "SAND", "MANA", "ENJ", "GALA", "ILV", "APE"
            };
        }

        /// <inheritdoc/>
        public override string Name => "Binance";

        /// <inheritdoc/>
        public override PriceSourceType Type => PriceSourceType.Exchange;

        /// <inheritdoc/>
        public override IEnumerable<string> SupportedAssets => _supportedAssets;

        /// <inheritdoc/>
        public override async Task<IEnumerable<Price>> FetchPricesAsync(string baseCurrency = "USD")
        {
            try
            {
                var prices = new List<Price>();
                var url = "https://api.binance.com/api/v3/ticker/price";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<List<BinanceTickerPrice>>(content);

                // Get timestamp
                var timestamp = DateTime.UtcNow;

                // Filter and convert to Price objects
                foreach (var ticker in data)
                {
                    var symbol = ParseSymbol(ticker.Symbol, baseCurrency);
                    if (symbol != null && _supportedAssets.Contains(symbol))
                    {
                        prices.Add(CreatePrice(symbol, baseCurrency, ticker.Price, timestamp));
                    }
                }

                return prices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching prices from Binance");
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task<Price> FetchPriceForSymbolAsync(string symbol, string baseCurrency = "USD")
        {
            try
            {
                if (!_supportedAssets.Contains(symbol))
                {
                    _logger.LogWarning("Symbol {Symbol} not supported by Binance", symbol);
                    return null;
                }

                var marketSymbol = GetMarketSymbol(symbol, baseCurrency);
                var url = $"https://api.binance.com/api/v3/ticker/price?symbol={marketSymbol}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<BinanceTickerPrice>(content);

                return CreatePrice(symbol, baseCurrency, data.Price, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching price for {Symbol} from Binance", symbol);
                throw;
            }
        }

        /// <summary>
        /// Parses a Binance symbol into a standard symbol
        /// </summary>
        /// <param name="binanceSymbol">Binance symbol (e.g., BTCUSDT)</param>
        /// <param name="baseCurrency">Base currency</param>
        /// <returns>Standard symbol</returns>
        private string ParseSymbol(string binanceSymbol, string baseCurrency)
        {
            // Handle USDT as USD for consistency
            var baseSymbol = baseCurrency == "USD" ? "USDT" : baseCurrency;

            // Check if the symbol ends with the base currency
            if (binanceSymbol.EndsWith(baseSymbol, StringComparison.OrdinalIgnoreCase))
            {
                var symbol = binanceSymbol.Substring(0, binanceSymbol.Length - baseSymbol.Length);
                if (_supportedAssets.Contains(symbol))
                {
                    return symbol;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the market symbol for Binance
        /// </summary>
        /// <param name="symbol">Standard symbol</param>
        /// <param name="baseCurrency">Base currency</param>
        /// <returns>Binance market symbol</returns>
        private string GetMarketSymbol(string symbol, string baseCurrency)
        {
            // Handle USD as USDT for Binance
            var baseSymbol = baseCurrency == "USD" ? "USDT" : baseCurrency;
            return $"{symbol}{baseSymbol}";
        }

        /// <summary>
        /// Binance ticker price model
        /// </summary>
        private class BinanceTickerPrice
        {
            /// <summary>
            /// Gets or sets the symbol
            /// </summary>
            public string Symbol { get; set; }

            /// <summary>
            /// Gets or sets the price
            /// </summary>
            public decimal Price { get; set; }
        }
    }
}
