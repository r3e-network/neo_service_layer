using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Notification.Providers
{
    /// <summary>
    /// Implementation of the in-app notification provider
    /// </summary>
    public class InAppNotificationProvider : INotificationProvider
    {
        private readonly ILogger<InAppNotificationProvider> _logger;
        private readonly NotificationProviderConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="InAppNotificationProvider"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration</param>
        public InAppNotificationProvider(ILogger<InAppNotificationProvider> logger, IOptions<NotificationProviderConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;
        }

        /// <inheritdoc/>
        public NotificationChannel Channel => NotificationChannel.InApp;

        /// <inheritdoc/>
        public string Name => "InApp";

        /// <inheritdoc/>
        public string Description => "Sends in-app notifications";

        /// <inheritdoc/>
        public bool IsEnabled => _configuration.IsEnabled;

        /// <inheritdoc/>
        public NotificationProviderConfiguration Configuration => _configuration;

        /// <inheritdoc/>
        public Task<(NotificationDeliveryStatus Status, string ErrorMessage)> SendAsync(Core.Models.Notification notification)
        {
            _logger.LogInformation("Sending in-app notification: {Id}", notification.Id);

            try
            {
                // Validate notification
                if (!Validate(notification))
                {
                    return Task.FromResult((NotificationDeliveryStatus.Failed, "Invalid notification"));
                }

                // For in-app notifications, we just mark it as delivered
                // The notification will be retrieved by the client through the API
                return Task.FromResult((NotificationDeliveryStatus.Delivered, (string)null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending in-app notification: {Id}", notification.Id);
                return Task.FromResult((NotificationDeliveryStatus.Failed, ex.Message));
            }
        }

        /// <inheritdoc/>
        public bool Validate(Core.Models.Notification notification)
        {
            if (notification == null)
            {
                return false;
            }

            if (notification.AccountId == Guid.Empty)
            {
                return false;
            }

            if (string.IsNullOrEmpty(notification.Subject))
            {
                return false;
            }

            return true;
        }
    }
}
