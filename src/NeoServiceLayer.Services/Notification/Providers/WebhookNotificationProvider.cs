using System;
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
    /// Implementation of the webhook notification provider
    /// </summary>
    public class WebhookNotificationProvider : IWebhookNotificationProvider
    {
        private readonly ILogger<WebhookNotificationProvider> _logger;
        private readonly NotificationProviderConfiguration _configuration;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookNotificationProvider"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration</param>
        public WebhookNotificationProvider(ILogger<WebhookNotificationProvider> logger, IOptions<NotificationProviderConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _httpClient = new HttpClient();

            // Configure HTTP client
            _configuration.Options.TryGetValue("Timeout", out var timeoutStr);
            if (int.TryParse(timeoutStr, out var timeout) && timeout > 0)
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
            }
            else
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(30);
            }
        }

        /// <inheritdoc/>
        public NotificationChannel Channel => NotificationChannel.Webhook;

        /// <inheritdoc/>
        public string Name => "Webhook";

        /// <inheritdoc/>
        public string Description => "Sends notifications via webhooks";

        /// <inheritdoc/>
        public bool IsEnabled => _configuration.IsEnabled;

        /// <inheritdoc/>
        public NotificationProviderConfiguration Configuration => _configuration;

        /// <inheritdoc/>
        public async Task<(NotificationDeliveryStatus Status, string ErrorMessage)> SendAsync(Core.Models.Notification notification)
        {
            _logger.LogInformation("Sending webhook notification: {Id}", notification.Id);

            try
            {
                // Validate notification
                if (!Validate(notification))
                {
                    return (NotificationDeliveryStatus.Failed, "Invalid notification");
                }

                // Get webhook URL
                if (!notification.Data.TryGetValue("WebhookUrl", out var webhookUrlObj) || webhookUrlObj == null)
                {
                    return (NotificationDeliveryStatus.Failed, "Webhook URL not specified");
                }

                var webhookUrl = webhookUrlObj.ToString();
                if (!ValidateWebhookUrl(webhookUrl))
                {
                    return (NotificationDeliveryStatus.Failed, $"Invalid webhook URL: {webhookUrl}");
                }

                // Prepare payload
                var payload = new
                {
                    id = notification.Id,
                    type = notification.Type.ToString(),
                    subject = notification.Subject,
                    content = notification.Content,
                    data = notification.Data,
                    timestamp = DateTime.UtcNow
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // Get headers
                if (notification.Data.TryGetValue("WebhookHeaders", out var headersObj) && headersObj is Dictionary<string, string> headers)
                {
                    foreach (var header in headers)
                    {
                        content.Headers.Add(header.Key, header.Value);
                    }
                }

                // Send request
                var response = await _httpClient.PostAsync(webhookUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return (NotificationDeliveryStatus.Delivered, null);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (NotificationDeliveryStatus.Failed, $"Webhook error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending webhook notification: {Id}", notification.Id);
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

            if (!notification.Data.TryGetValue("WebhookUrl", out var webhookUrlObj) || webhookUrlObj == null)
            {
                return false;
            }

            var webhookUrl = webhookUrlObj.ToString();
            return ValidateWebhookUrl(webhookUrl);
        }

        /// <inheritdoc/>
        public bool ValidateWebhookUrl(string webhookUrl)
        {
            if (string.IsNullOrEmpty(webhookUrl))
            {
                return false;
            }

            // Validate URL
            if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uri))
            {
                return false;
            }

            // Only allow HTTP and HTTPS
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
    }
}
