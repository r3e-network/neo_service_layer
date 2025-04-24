using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Notification.Providers
{
    /// <summary>
    /// Implementation of the SMS notification provider
    /// </summary>
    public class SmsNotificationProvider : ISmsNotificationProvider
    {
        private readonly ILogger<SmsNotificationProvider> _logger;
        private readonly NotificationProviderConfiguration _configuration;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsNotificationProvider"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration</param>
        public SmsNotificationProvider(ILogger<SmsNotificationProvider> logger, IOptions<NotificationProviderConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;
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
        public NotificationChannel Channel => NotificationChannel.SMS;

        /// <inheritdoc/>
        public string Name => "SMS";

        /// <inheritdoc/>
        public string Description => "Sends notifications via SMS";

        /// <inheritdoc/>
        public bool IsEnabled => _configuration.IsEnabled;

        /// <inheritdoc/>
        public NotificationProviderConfiguration Configuration => _configuration;

        /// <inheritdoc/>
        public async Task<(NotificationDeliveryStatus Status, string ErrorMessage)> SendAsync(Core.Models.Notification notification)
        {
            _logger.LogInformation("Sending SMS notification: {Id}", notification.Id);

            try
            {
                // Validate notification
                if (!Validate(notification))
                {
                    return (NotificationDeliveryStatus.Failed, "Invalid notification");
                }

                // Get recipient phone number
                if (!notification.Data.TryGetValue("PhoneNumber", out var phoneNumberObj) || phoneNumberObj == null)
                {
                    return (NotificationDeliveryStatus.Failed, "Recipient phone number not specified");
                }

                var phoneNumber = phoneNumberObj.ToString();
                if (!ValidatePhoneNumber(phoneNumber))
                {
                    return (NotificationDeliveryStatus.Failed, $"Invalid phone number: {phoneNumber}");
                }

                // Get API URL and key
                _configuration.Options.TryGetValue("ApiUrl", out var apiUrl);
                _configuration.Options.TryGetValue("ApiKey", out var apiKey);

                if (string.IsNullOrEmpty(apiUrl))
                {
                    return (NotificationDeliveryStatus.Failed, "SMS API URL not configured");
                }

                // Prepare message
                var message = notification.Content;
                if (message.Length > 160)
                {
                    message = message.Substring(0, 157) + "...";
                }

                // Prepare request
                var requestData = new
                {
                    to = phoneNumber,
                    message = message
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
                    return (NotificationDeliveryStatus.Failed, $"SMS API error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS notification: {Id}", notification.Id);
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

            if (string.IsNullOrEmpty(notification.Content))
            {
                return false;
            }

            if (!notification.Data.TryGetValue("PhoneNumber", out var phoneNumberObj) || phoneNumberObj == null)
            {
                return false;
            }

            var phoneNumber = phoneNumberObj.ToString();
            return ValidatePhoneNumber(phoneNumber);
        }

        /// <inheritdoc/>
        public bool ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return false;
            }

            // Simple phone number validation (international format)
            var regex = new Regex(@"^\+[1-9]\d{1,14}$");
            return regex.IsMatch(phoneNumber);
        }
    }
}
