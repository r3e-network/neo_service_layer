using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models.Analytics
{
    /// <summary>
    /// Represents a report in the analytics system
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Gets or sets the unique identifier for the report
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the report
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the report
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the account ID that owns this report
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the user ID that created this report
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the type of the report
        /// </summary>
        public ReportType Type { get; set; }

        /// <summary>
        /// Gets or sets the format of the report
        /// </summary>
        public ReportFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the dashboard ID for the report
        /// </summary>
        public Guid? DashboardId { get; set; }

        /// <summary>
        /// Gets or sets the query for the report
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the parameters for the report
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the schedule for the report
        /// </summary>
        public ReportSchedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the delivery options for the report
        /// </summary>
        public ReportDelivery Delivery { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the report
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last run timestamp
        /// </summary>
        public DateTime? LastRunAt { get; set; }

        /// <summary>
        /// Gets or sets the next run timestamp
        /// </summary>
        public DateTime? NextRunAt { get; set; }

        /// <summary>
        /// Gets or sets the status of the report
        /// </summary>
        public ReportStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the error message if the report failed
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Represents a report execution
    /// </summary>
    public class ReportExecution
    {
        /// <summary>
        /// Gets or sets the unique identifier for the execution
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the report ID
        /// </summary>
        public Guid ReportId { get; set; }

        /// <summary>
        /// Gets or sets the user ID that triggered the execution
        /// </summary>
        public Guid? TriggeredBy { get; set; }

        /// <summary>
        /// Gets or sets the start timestamp
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end timestamp
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the status of the execution
        /// </summary>
        public ReportExecutionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the parameters used for the execution
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the output file URL
        /// </summary>
        public string OutputFileUrl { get; set; }

        /// <summary>
        /// Gets or sets the error message if the execution failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the delivery status
        /// </summary>
        public Dictionary<string, DeliveryStatus> DeliveryStatus { get; set; } = new Dictionary<string, DeliveryStatus>();
    }

    /// <summary>
    /// Represents the schedule for a report
    /// </summary>
    public class ReportSchedule
    {
        /// <summary>
        /// Gets or sets whether the report is scheduled
        /// </summary>
        public bool IsScheduled { get; set; }

        /// <summary>
        /// Gets or sets the frequency of the schedule
        /// </summary>
        public ScheduleFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets the interval of the schedule
        /// </summary>
        public int Interval { get; set; } = 1;

        /// <summary>
        /// Gets or sets the days of the week for the schedule
        /// </summary>
        public List<DayOfWeek> DaysOfWeek { get; set; } = new List<DayOfWeek>();

        /// <summary>
        /// Gets or sets the days of the month for the schedule
        /// </summary>
        public List<int> DaysOfMonth { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the months for the schedule
        /// </summary>
        public List<int> Months { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the time of day for the schedule
        /// </summary>
        public TimeSpan TimeOfDay { get; set; }

        /// <summary>
        /// Gets or sets the time zone for the schedule
        /// </summary>
        public string TimeZone { get; set; } = "UTC";

        /// <summary>
        /// Gets or sets the start date for the schedule
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date for the schedule
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Represents the delivery options for a report
    /// </summary>
    public class ReportDelivery
    {
        /// <summary>
        /// Gets or sets the delivery methods
        /// </summary>
        public List<DeliveryMethod> Methods { get; set; } = new List<DeliveryMethod>();

        /// <summary>
        /// Gets or sets the email recipients
        /// </summary>
        public List<string> EmailRecipients { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the email subject
        /// </summary>
        public string EmailSubject { get; set; }

        /// <summary>
        /// Gets or sets the email message
        /// </summary>
        public string EmailMessage { get; set; }

        /// <summary>
        /// Gets or sets the webhook URLs
        /// </summary>
        public List<string> WebhookUrls { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the storage path
        /// </summary>
        public string StoragePath { get; set; }

        /// <summary>
        /// Gets or sets whether to include the report data in the delivery
        /// </summary>
        public bool IncludeReportData { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include the report file in the delivery
        /// </summary>
        public bool IncludeReportFile { get; set; } = true;
    }

    /// <summary>
    /// Represents the delivery status for a report execution
    /// </summary>
    public class DeliveryStatus
    {
        /// <summary>
        /// Gets or sets the delivery method
        /// </summary>
        public DeliveryMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public DeliveryStatusType Status { get; set; }

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the recipient
        /// </summary>
        public string Recipient { get; set; }
    }

    /// <summary>
    /// Represents the type of a report
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// Dashboard report
        /// </summary>
        Dashboard,

        /// <summary>
        /// Query report
        /// </summary>
        Query,

        /// <summary>
        /// Custom report
        /// </summary>
        Custom
    }

    /// <summary>
    /// Represents the format of a report
    /// </summary>
    public enum ReportFormat
    {
        /// <summary>
        /// PDF format
        /// </summary>
        PDF,

        /// <summary>
        /// CSV format
        /// </summary>
        CSV,

        /// <summary>
        /// Excel format
        /// </summary>
        Excel,

        /// <summary>
        /// JSON format
        /// </summary>
        JSON,

        /// <summary>
        /// HTML format
        /// </summary>
        HTML
    }

    /// <summary>
    /// Represents the status of a report
    /// </summary>
    public enum ReportStatus
    {
        /// <summary>
        /// Active status
        /// </summary>
        Active,

        /// <summary>
        /// Paused status
        /// </summary>
        Paused,

        /// <summary>
        /// Failed status
        /// </summary>
        Failed,

        /// <summary>
        /// Deleted status
        /// </summary>
        Deleted
    }

    /// <summary>
    /// Represents the status of a report execution
    /// </summary>
    public enum ReportExecutionStatus
    {
        /// <summary>
        /// Pending status
        /// </summary>
        Pending,

        /// <summary>
        /// Running status
        /// </summary>
        Running,

        /// <summary>
        /// Completed status
        /// </summary>
        Completed,

        /// <summary>
        /// Failed status
        /// </summary>
        Failed,

        /// <summary>
        /// Cancelled status
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// Represents the frequency of a schedule
    /// </summary>
    public enum ScheduleFrequency
    {
        /// <summary>
        /// Hourly frequency
        /// </summary>
        Hourly,

        /// <summary>
        /// Daily frequency
        /// </summary>
        Daily,

        /// <summary>
        /// Weekly frequency
        /// </summary>
        Weekly,

        /// <summary>
        /// Monthly frequency
        /// </summary>
        Monthly,

        /// <summary>
        /// Custom frequency
        /// </summary>
        Custom
    }

    /// <summary>
    /// Represents the delivery method for a report
    /// </summary>
    public enum DeliveryMethod
    {
        /// <summary>
        /// Email delivery
        /// </summary>
        Email,

        /// <summary>
        /// Webhook delivery
        /// </summary>
        Webhook,

        /// <summary>
        /// Storage delivery
        /// </summary>
        Storage
    }

    /// <summary>
    /// Represents the status type for a delivery
    /// </summary>
    public enum DeliveryStatusType
    {
        /// <summary>
        /// Pending status
        /// </summary>
        Pending,

        /// <summary>
        /// Sent status
        /// </summary>
        Sent,

        /// <summary>
        /// Failed status
        /// </summary>
        Failed
    }
}
