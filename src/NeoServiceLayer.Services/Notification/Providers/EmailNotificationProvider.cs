using System;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Notification.Providers
{
    /// <summary>
    /// Implementation of the email notification provider
    /// </summary>
    public class EmailNotificationProvider : IEmailNotificationProvider
    {
        private readonly ILogger<EmailNotificationProvider> _logger;
        private readonly NotificationProviderConfiguration _configuration;
        private readonly SmtpClient _smtpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailNotificationProvider"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration</param>
        public EmailNotificationProvider(ILogger<EmailNotificationProvider> logger, IOptions<NotificationProviderConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;

            // Configure SMTP client
            _configuration.Options.TryGetValue("SmtpHost", out var host);
            _configuration.Options.TryGetValue("SmtpPort", out var portStr);
            _configuration.Options.TryGetValue("SmtpUsername", out var username);
            _configuration.Options.TryGetValue("SmtpPassword", out var password);
            _configuration.Options.TryGetValue("SmtpEnableSsl", out var enableSslStr);

            if (string.IsNullOrEmpty(host))
            {
                host = "localhost";
            }

            if (!int.TryParse(portStr, out var port))
            {
                port = 25;
            }

            bool.TryParse(enableSslStr, out var enableSsl);

            _smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                _smtpClient.Credentials = new NetworkCredential(username, password);
            }
        }

        /// <inheritdoc/>
        public NotificationChannel Channel => NotificationChannel.Email;

        /// <inheritdoc/>
        public string Name => "Email";

        /// <inheritdoc/>
        public string Description => "Sends notifications via email";

        /// <inheritdoc/>
        public bool IsEnabled => _configuration.IsEnabled;

        /// <inheritdoc/>
        public NotificationProviderConfiguration Configuration => _configuration;

        /// <inheritdoc/>
        public async Task<(NotificationDeliveryStatus Status, string ErrorMessage)> SendAsync(Core.Models.Notification notification)
        {
            _logger.LogInformation("Sending email notification: {Id}", notification.Id);

            try
            {
                // Validate notification
                if (!Validate(notification))
                {
                    return (NotificationDeliveryStatus.Failed, "Invalid notification");
                }

                // Get recipient email
                if (!notification.Data.TryGetValue("Email", out var emailObj) || emailObj == null)
                {
                    return (NotificationDeliveryStatus.Failed, "Recipient email not specified");
                }

                var email = emailObj.ToString();
                if (!ValidateEmail(email))
                {
                    return (NotificationDeliveryStatus.Failed, $"Invalid email address: {email}");
                }

                // Get sender email
                _configuration.Options.TryGetValue("SenderEmail", out var senderEmail);
                _configuration.Options.TryGetValue("SenderName", out var senderName);

                if (string.IsNullOrEmpty(senderEmail))
                {
                    senderEmail = "noreply@neoservicelayer.com";
                }

                if (string.IsNullOrEmpty(senderName))
                {
                    senderName = "Neo Service Layer";
                }

                // Create message
                var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = notification.Subject,
                    Body = notification.Content,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(email));

                // Send message
                await _smtpClient.SendMailAsync(message);

                return (NotificationDeliveryStatus.Sent, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email notification: {Id}", notification.Id);
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

            if (!notification.Data.TryGetValue("Email", out var emailObj) || emailObj == null)
            {
                return false;
            }

            var email = emailObj.ToString();
            return ValidateEmail(email);
        }

        /// <inheritdoc/>
        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            // Simple email validation
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
    }
}
