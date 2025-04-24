using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Configuration for notifications
    /// </summary>
    public class NotificationConfiguration
    {
        /// <summary>
        /// Gets or sets the processing interval in seconds
        /// </summary>
        public int ProcessingIntervalSeconds { get; set; } = 15;

        /// <summary>
        /// Gets or sets the maximum batch size for processing notifications
        /// </summary>
        public int MaxBatchSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the default maximum retry count
        /// </summary>
        public int DefaultMaxRetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the retry interval in seconds
        /// </summary>
        public int RetryIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the providers
        /// </summary>
        public List<NotificationProviderConfiguration> Providers { get; set; } = new List<NotificationProviderConfiguration>();

        /// <summary>
        /// Gets or sets the default channels
        /// </summary>
        public List<NotificationChannel> DefaultChannels { get; set; } = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.InApp };

        /// <summary>
        /// Gets or sets the default sender email
        /// </summary>
        public string DefaultSenderEmail { get; set; } = "noreply@neoservicelayer.com";

        /// <summary>
        /// Gets or sets the default sender name
        /// </summary>
        public string DefaultSenderName { get; set; } = "Neo Service Layer";

        /// <summary>
        /// Gets or sets the SMTP host
        /// </summary>
        public string SmtpHost { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the SMTP port
        /// </summary>
        public int SmtpPort { get; set; } = 25;

        /// <summary>
        /// Gets or sets the SMTP username
        /// </summary>
        public string SmtpUsername { get; set; }

        /// <summary>
        /// Gets or sets the SMTP password
        /// </summary>
        public string SmtpPassword { get; set; }

        /// <summary>
        /// Gets or sets whether to enable SSL for SMTP
        /// </summary>
        public bool SmtpEnableSsl { get; set; } = false;

        /// <summary>
        /// Gets or sets the SMS API URL
        /// </summary>
        public string SmsApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the SMS API key
        /// </summary>
        public string SmsApiKey { get; set; }

        /// <summary>
        /// Gets or sets the push notification API URL
        /// </summary>
        public string PushApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the push notification API key
        /// </summary>
        public string PushApiKey { get; set; }

        /// <summary>
        /// Gets or sets the webhook timeout in seconds
        /// </summary>
        public int WebhookTimeoutSeconds { get; set; } = 30;
    }
}
