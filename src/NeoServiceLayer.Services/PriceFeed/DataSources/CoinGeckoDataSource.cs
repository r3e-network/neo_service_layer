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
    /// CoinGecko price data source
    /// </summary>
    public class CoinGeckoDataSource : BasePriceDataSource
    {
        private readonly Dictionary<string, string> _symbolToIdMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoinGeckoDataSource"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="httpClient">HTTP client</param>
        public CoinGeckoDataSource(ILogger<CoinGeckoDataSource> logger, HttpClient httpClient)
            : base(logger, httpClient)
        {
            _symbolToIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "BTC", "bitcoin" },
                { "ETH", "ethereum" },
                { "NEO", "neo" },
                { "GAS", "gas" },
                { "FLM", "flamingo-finance" },
                { "BNB", "binancecoin" },
                { "USDT", "tether" },
                { "USDC", "usd-coin" },
                { "DAI", "dai" },
                { "LINK", "chainlink" },
                { "UNI", "uniswap" },
                { "AAVE", "aave" },
                { "COMP", "compound-governance-token" },
                { "SNX", "havven" },
                { "YFI", "yearn-finance" },
                { "SUSHI", "sushi" },
                { "MKR", "maker" },
                { "BAT", "basic-attention-token" },
                { "ZRX", "0x" },
                { "REN", "republic-protocol" },
                { "KNC", "kyber-network" },
                { "BAL", "balancer" },
                { "CRV", "curve-dao-token" },
                { "LRC", "loopring" },
                { "MATIC", "matic-network" },
                { "DOGE", "dogecoin" },
                { "DOT", "polkadot" },
                { "ADA", "cardano" },
                { "XRP", "ripple" },
                { "SOL", "solana" },
                { "AVAX", "avalanche-2" },
                { "LUNA", "terra-luna" },
                { "ATOM", "cosmos" },
                { "ALGO", "algorand" },
                { "FTM", "fantom" },
                { "NEAR", "near" },
                { "ONE", "harmony" },
                { "HBAR", "hedera-hashgraph" },
                { "EGLD", "elrond-erd-2" },
                { "FLOW", "flow" },
                { "XTZ", "tezos" },
                { "EOS", "eos" },
                { "CAKE", "pancakeswap-token" },
                { "AXS", "axie-infinity" },
                { "SAND", "the-sandbox" },
                { "MANA", "decentraland" },
                { "ENJ", "enjincoin" },
                { "GALA", "gala" },
                { "ILV", "illuvium" },
                { "APE", "apecoin" }
            };
        }

        /// <inheritdoc/>
        public override string Name => "CoinGecko";

        /// <inheritdoc/>
        public override PriceSourceType Type => PriceSourceType.Aggregator;

        /// <inheritdoc/>
        public override IEnumerable<string> SupportedAssets => _symbolToIdMap.Keys;

        /// <inheritdoc/>
        public override async Task<IEnumerable<Price>> FetchPricesAsync(string baseCurrency = "USD")
        {
            try
            {
                var prices = new List<Price>();
                var ids = string.Join(",", _symbolToIdMap.Values);
                var url = $"https://api.coingecko.com/api/v3/simple/price?ids={ids}&vs_currencies={baseCurrency.ToLower()}&include_last_updated_at=true";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(content);

                foreach (var item in data)
                {
                    var coinId = item.Key;
                    var symbol = _symbolToIdMap.FirstOrDefault(x => x.Value == coinId).Key;
                    if (string.IsNullOrEmpty(symbol))
                    {
                        continue;
                    }

                    var priceData = item.Value;
                    if (priceData.TryGetValue(baseCurrency.ToLower(), out var priceObj) &&
                        priceData.TryGetValue("last_updated_at", out var timestampObj))
                    {
                        var price = Convert.ToDecimal(priceObj);
                        var timestamp = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(timestampObj)).UtcDateTime;

                        prices.Add(CreatePrice(symbol, baseCurrency, price, timestamp));
                    }
                }

                return prices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching prices from CoinGecko");
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task<Price> FetchPriceForSymbolAsync(string symbol, string baseCurrency = "USD")
        {
            try
            {
                if (!_symbolToIdMap.TryGetValue(symbol, out var coinId))
                {
                    _logger.LogWarning("Symbol {Symbol} not supported by CoinGecko", symbol);
                    return null;
                }

                var url = $"https://api.coingecko.com/api/v3/simple/price?ids={coinId}&vs_currencies={baseCurrency.ToLower()}&include_last_updated_at=true";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(content);

                if (data.TryGetValue(coinId, out var priceData) &&
                    priceData.TryGetValue(baseCurrency.ToLower(), out var priceObj) &&
                    priceData.TryGetValue("last_updated_at", out var timestampObj))
                {
                    var price = Convert.ToDecimal(priceObj);
                    var timestamp = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(timestampObj)).UtcDateTime;

                    return CreatePrice(symbol, baseCurrency, price, timestamp);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching price for {Symbol} from CoinGecko", symbol);
                throw;
            }
        }
    }
}
