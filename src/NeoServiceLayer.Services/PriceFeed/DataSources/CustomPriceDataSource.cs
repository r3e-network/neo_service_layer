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
    /// Custom price data source implementation
    /// </summary>
    public class CustomPriceDataSource : BasePriceDataSource
    {
        private new readonly ILogger<CustomPriceDataSource> _logger;
        private new readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomPriceDataSource"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="httpClient">HTTP client</param>
        public CustomPriceDataSource(ILogger<CustomPriceDataSource> logger, HttpClient httpClient)
            : base(logger, httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <inheritdoc/>
        public override string Name => "Custom";

        /// <inheritdoc/>
        public override PriceSourceType Type => PriceSourceType.Custom;

        /// <inheritdoc/>
        public override IEnumerable<string> SupportedAssets => new[] { "BTC", "ETH", "NEO", "GAS" };

        /// <inheritdoc/>
        public override async Task<IEnumerable<Price>> FetchPricesAsync(string baseCurrency = "USD")
        {
            var prices = new List<Price>();
            foreach (var asset in SupportedAssets)
            {
                try
                {
                    var price = await FetchPriceForSymbolAsync(asset, baseCurrency);
                    if (price != null)
                    {
                        prices.Add(price);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching price for {Symbol}/{BaseCurrency} from Custom API", asset, baseCurrency);
                }
            }
            return prices;
        }

        /// <inheritdoc/>
        public override async Task<Price> FetchPriceForSymbolAsync(string symbol, string baseCurrency = "USD")
        {
            _logger.LogInformation("Fetching price for {Symbol}/{BaseCurrency} from Custom API", symbol, baseCurrency);

            try
            {
                // Build request URL with query parameters
                var requestUrl = "https://api.example.com/price";
                var queryParams = new Dictionary<string, string>();

                // Add symbol and base currency to query parameters if not already present
                if (!queryParams.ContainsKey("symbol"))
                {
                    queryParams["symbol"] = symbol;
                }

                if (!queryParams.ContainsKey("base"))
                {
                    queryParams["base"] = baseCurrency;
                }

                // Add query parameters to URL
                if (queryParams.Count > 0)
                {
                    var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
                    requestUrl = requestUrl + (requestUrl.Contains("?") ? "&" : "?") + queryString;
                }

                // Create request message
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                // Add headers
                request.Headers.Add("User-Agent", "NeoServiceLayer/1.0");

                // Add request body for POST requests if needed
                // request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

                // Send request
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Parse response
                var content = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(content);

                // Extract price using JSON path
                var priceElement = GetJsonElementByPath(jsonDocument.RootElement, "price");
                if (!priceElement.HasValue)
                {
                    throw new InvalidOperationException("Price not found in response");
                }

                var price = priceElement.Value.GetDecimal();

                // Use current time as timestamp
                DateTime timestamp = DateTime.UtcNow;

                return CreatePrice(symbol, baseCurrency, price, timestamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching price for {Symbol}/{BaseCurrency} from Custom API", symbol, baseCurrency);
                throw;
            }
        }

        /// <summary>
        /// Gets a JSON element by path
        /// </summary>
        /// <param name="root">Root element</param>
        /// <param name="path">JSON path (e.g., "data.price")</param>
        /// <returns>JSON element if found, null otherwise</returns>
        private JsonElement? GetJsonElementByPath(JsonElement root, string path)
        {
            var parts = path.Split('.');
            var current = root;

            foreach (var part in parts)
            {
                if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(part, out var property))
                {
                    current = property;
                }
                else if (current.ValueKind == JsonValueKind.Array && int.TryParse(part, out var index) &&
                         index >= 0 && index < current.GetArrayLength())
                {
                    current = current[index];
                }
                else
                {
                    return null;
                }
            }

            return current;
        }
    }
}
