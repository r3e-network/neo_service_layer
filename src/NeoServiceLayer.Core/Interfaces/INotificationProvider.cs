using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for notification providers
    /// </summary>
    public interface INotificationProvider
    {
        /// <summary>
        /// Gets the notification channel supported by this provider
        /// </summary>
        NotificationChannel Channel { get; }

        /// <summary>
        /// Sends a notification
        /// </summary>
        /// <param name="notification">Notification to send</param>
        /// <returns>Delivery status and error message if any</returns>
        Task<(NotificationDeliveryStatus Status, string ErrorMessage)> SendAsync(Notification notification);

        /// <summary>
        /// Validates a notification before sending
        /// </summary>
        /// <param name="notification">Notification to validate</param>
        /// <returns>True if the notification is valid, false otherwise</returns>
        bool Validate(Notification notification);

        /// <summary>
        /// Gets the provider name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the provider description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets whether the provider is enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets the provider configuration
        /// </summary>
        Models.NotificationProviderConfiguration Configuration { get; }
    }

    /// <summary>
    /// Interface for email notification provider
    /// </summary>
    public interface IEmailNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// Validates an email address
        /// </summary>
        /// <param name="email">Email address to validate</param>
        /// <returns>True if the email address is valid, false otherwise</returns>
        bool ValidateEmail(string email);
    }

    /// <summary>
    /// Interface for SMS notification provider
    /// </summary>
    public interface ISmsNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// Validates a phone number
        /// </summary>
        /// <param name="phoneNumber">Phone number to validate</param>
        /// <returns>True if the phone number is valid, false otherwise</returns>
        bool ValidatePhoneNumber(string phoneNumber);
    }

    /// <summary>
    /// Interface for push notification provider
    /// </summary>
    public interface IPushNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// Validates a device token
        /// </summary>
        /// <param name="deviceToken">Device token to validate</param>
        /// <returns>True if the device token is valid, false otherwise</returns>
        bool ValidateDeviceToken(string deviceToken);

        /// <summary>
        /// Registers a device token
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="deviceToken">Device token</param>
        /// <param name="platform">Device platform</param>
        /// <returns>True if the device token was registered, false otherwise</returns>
        Task<bool> RegisterDeviceTokenAsync(System.Guid accountId, string deviceToken, string platform);

        /// <summary>
        /// Unregisters a device token
        /// </summary>
        /// <param name="deviceToken">Device token</param>
        /// <returns>True if the device token was unregistered, false otherwise</returns>
        Task<bool> UnregisterDeviceTokenAsync(string deviceToken);
    }

    /// <summary>
    /// Interface for webhook notification provider
    /// </summary>
    public interface IWebhookNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// Validates a webhook URL
        /// </summary>
        /// <param name="webhookUrl">Webhook URL to validate</param>
        /// <returns>True if the webhook URL is valid, false otherwise</returns>
        bool ValidateWebhookUrl(string webhookUrl);
    }


}
