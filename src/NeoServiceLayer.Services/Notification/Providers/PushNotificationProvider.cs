using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Notification.Providers
{
    /// <summary>
    /// Implementation of the push notification provider
    /// </summary>
    public class PushNotificationProvider : IPushNotificationProvider
    {
        private readonly ILogger<PushNotificationProvider> _logger;
        private readonly NotificationProviderConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IDatabaseService _databaseService;
        private const string DeviceTokenCollectionName = "device_tokens";

        /// <summary>
        /// Initializes a new instance of the <see cref="PushNotificationProvider"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="databaseService">Database service</param>
        public PushNotificationProvider(
            ILogger<PushNotificationProvider> logger,
            IOptions<NotificationProviderConfiguration> configuration,
            IDatabaseService databaseService)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _databaseService = databaseService;
            _httpClient = new HttpClient();

            // Configure HTTP client
            _configuration.Options.TryGetValue("ApiUrl", out var apiUrl);
            _configuration.Options.TryGetValue("ApiKey", out var apiKey);

            if (!string.IsNullOrEmpty(apiUrl))
            {
                _httpClient.BaseAddress = new Uri(apiUrl);
            }

            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            }
        }

        /// <inheritdoc/>
        public NotificationChannel Channel => NotificationChannel.Push;

        /// <inheritdoc/>
        public string Name => "Push";

        /// <inheritdoc/>
        public string Description => "Sends notifications via push notifications";

        /// <inheritdoc/>
        public bool IsEnabled => _configuration.IsEnabled;

        /// <inheritdoc/>
        public NotificationProviderConfiguration Configuration => _configuration;

        /// <inheritdoc/>
        public async Task<(NotificationDeliveryStatus Status, string ErrorMessage)> SendAsync(Core.Models.Notification notification)
        {
            _logger.LogInformation("Sending push notification: {Id}", notification.Id);

            try
            {
                // Validate notification
                if (!Validate(notification))
                {
                    return (NotificationDeliveryStatus.Failed, "Invalid notification");
                }

                // Get device tokens
                if (!notification.Data.TryGetValue("DeviceTokens", out var deviceTokensObj) || deviceTokensObj == null)
                {
                    return (NotificationDeliveryStatus.Failed, "Device tokens not specified");
                }

                List<string> deviceTokens;
                if (deviceTokensObj is List<string> tokenList)
                {
                    deviceTokens = tokenList;
                }
                else if (deviceTokensObj is string[] tokenArray)
                {
                    deviceTokens = new List<string>(tokenArray);
                }
                else if (deviceTokensObj is string singleToken)
                {
                    deviceTokens = new List<string> { singleToken };
                }
                else
                {
                    return (NotificationDeliveryStatus.Failed, "Invalid device tokens format");
                }

                if (deviceTokens.Count == 0)
                {
                    return (NotificationDeliveryStatus.Failed, "No device tokens specified");
                }

                // Validate device tokens
                var validTokens = new List<string>();
                foreach (var token in deviceTokens)
                {
                    if (ValidateDeviceToken(token))
                    {
                        validTokens.Add(token);
                    }
                }

                if (validTokens.Count == 0)
                {
                    return (NotificationDeliveryStatus.Failed, "No valid device tokens");
                }

                // Get API URL and key
                _configuration.Options.TryGetValue("ApiUrl", out var apiUrl);
                _configuration.Options.TryGetValue("ApiKey", out var apiKey);

                if (string.IsNullOrEmpty(apiUrl))
                {
                    return (NotificationDeliveryStatus.Failed, "Push notification API URL not configured");
                }

                // Prepare request
                var requestData = new
                {
                    tokens = validTokens,
                    notification = new
                    {
                        title = notification.Subject,
                        body = notification.Content,
                        data = notification.Data
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                // Send request
                var response = await _httpClient.PostAsync("", content);
                if (response.IsSuccessStatusCode)
                {
                    return (NotificationDeliveryStatus.Sent, null);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (NotificationDeliveryStatus.Failed, $"Push notification API error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification: {Id}", notification.Id);
                return (NotificationDeliveryStatus.Failed, ex.Message);
            }
        }

        /// <inheritdoc/>
        public bool Validate(Core.Models.Notification notification)
        {
            if (notification == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(notification.Subject))
            {
                return false;
            }

            if (string.IsNullOrEmpty(notification.Content))
            {
                return false;
            }

            if (!notification.Data.TryGetValue("DeviceTokens", out var deviceTokensObj) || deviceTokensObj == null)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool ValidateDeviceToken(string deviceToken)
        {
            if (string.IsNullOrEmpty(deviceToken))
            {
                return false;
            }

            // Simple validation - token should be at least 32 characters
            return deviceToken.Length >= 32;
        }

        /// <inheritdoc/>
        public async Task<bool> RegisterDeviceTokenAsync(Guid accountId, string deviceToken, string platform)
        {
            _logger.LogInformation("Registering device token for account: {AccountId}", accountId);

            try
            {
                if (!ValidateDeviceToken(deviceToken))
                {
                    return false;
                }

                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(DeviceTokenCollectionName))
                {
                    await _databaseService.CreateCollectionAsync(DeviceTokenCollectionName);
                }

                // Check if token already exists
                var existingTokens = await _databaseService.GetByFilterAsync<DeviceTokenRegistration>(
                    DeviceTokenCollectionName,
                    t => t.DeviceToken == deviceToken);

                var existingToken = existingTokens.FirstOrDefault();
                if (existingToken != null)
                {
                    // Update existing token
                    existingToken.AccountId = accountId;
                    existingToken.Platform = platform;
                    existingToken.UpdatedAt = DateTime.UtcNow;
                    await _databaseService.UpdateAsync<DeviceTokenRegistration, Guid>(DeviceTokenCollectionName, existingToken.Id, existingToken);
                }
                else
                {
                    // Create new token
                    var registration = new DeviceTokenRegistration
                    {
                        Id = Guid.NewGuid(),
                        AccountId = accountId,
                        DeviceToken = deviceToken,
                        Platform = platform,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _databaseService.CreateAsync(DeviceTokenCollectionName, registration);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device token for account: {AccountId}", accountId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UnregisterDeviceTokenAsync(string deviceToken)
        {
            _logger.LogInformation("Unregistering device token");

            try
            {
                if (!ValidateDeviceToken(deviceToken))
                {
                    return false;
                }

                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(DeviceTokenCollectionName))
                {
                    return false;
                }

                // Find token
                var existingTokens = await _databaseService.GetByFilterAsync<DeviceTokenRegistration>(
                    DeviceTokenCollectionName,
                    t => t.DeviceToken == deviceToken);

                var existingToken = existingTokens.FirstOrDefault();
                if (existingToken != null)
                {
                    // Delete token
                    return await _databaseService.DeleteAsync<DeviceTokenRegistration, Guid>(DeviceTokenCollectionName, existingToken.Id);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering device token");
                return false;
            }
        }

        /// <summary>
        /// Device token registration
        /// </summary>
        private class DeviceTokenRegistration
        {
            /// <summary>
            /// Gets or sets the unique identifier for the registration
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the device token
            /// </summary>
            public string DeviceToken { get; set; }

            /// <summary>
            /// Gets or sets the device platform
            /// </summary>
            public string Platform { get; set; }

            /// <summary>
            /// Gets or sets the creation timestamp
            /// </summary>
            public DateTime CreatedAt { get; set; }

            /// <summary>
            /// Gets or sets the last update timestamp
            /// </summary>
            public DateTime UpdatedAt { get; set; }
        }
    }
}
