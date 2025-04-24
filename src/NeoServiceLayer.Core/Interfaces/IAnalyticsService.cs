using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for analytics service
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Tracks an event
        /// </summary>
        /// <param name="event">Event to track</param>
        /// <returns>The tracked event</returns>
        Task<AnalyticsEvent> TrackEventAsync(AnalyticsEvent @event);

        /// <summary>
        /// Tracks a metric
        /// </summary>
        /// <param name="metricId">Metric ID</param>
        /// <param name="value">Metric value</param>
        /// <param name="dimensions">Metric dimensions</param>
        /// <returns>The tracked metric data point</returns>
        Task<MetricDataPoint> TrackMetricAsync(Guid metricId, double value, Dictionary<string, string> dimensions = null);

        /// <summary>
        /// Creates a metric
        /// </summary>
        /// <param name="metric">Metric to create</param>
        /// <returns>The created metric</returns>
        Task<Metric> CreateMetricAsync(Metric metric);

        /// <summary>
        /// Gets a metric by ID
        /// </summary>
        /// <param name="id">Metric ID</param>
        /// <returns>The metric if found, null otherwise</returns>
        Task<Metric> GetMetricAsync(Guid id);

        /// <summary>
        /// Gets metrics by category
        /// </summary>
        /// <param name="category">Category</param>
        /// <returns>List of metrics in the category</returns>
        Task<IEnumerable<Metric>> GetMetricsByCategoryAsync(string category);

        /// <summary>
        /// Gets all metrics
        /// </summary>
        /// <returns>List of all metrics</returns>
        Task<IEnumerable<Metric>> GetAllMetricsAsync();

        /// <summary>
        /// Updates a metric
        /// </summary>
        /// <param name="metric">Metric to update</param>
        /// <returns>The updated metric</returns>
        Task<Metric> UpdateMetricAsync(Metric metric);

        /// <summary>
        /// Deletes a metric
        /// </summary>
        /// <param name="id">Metric ID</param>
        /// <returns>True if the metric was deleted, false otherwise</returns>
        Task<bool> DeleteMetricAsync(Guid id);

        /// <summary>
        /// Gets metric data points
        /// </summary>
        /// <param name="metricId">Metric ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="dimensions">Dimensions to filter by</param>
        /// <returns>List of metric data points</returns>
        Task<IEnumerable<MetricDataPoint>> GetMetricDataPointsAsync(Guid metricId, DateTime startTime, DateTime endTime, Dictionary<string, string> dimensions = null);

        /// <summary>
        /// Gets metric aggregation
        /// </summary>
        /// <param name="metricId">Metric ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="period">Aggregation period</param>
        /// <param name="function">Aggregation function</param>
        /// <param name="groupBy">Dimensions to group by</param>
        /// <returns>Metric aggregation</returns>
        Task<MetricAggregation> GetMetricAggregationAsync(Guid metricId, DateTime startTime, DateTime endTime, AggregationPeriod period, AggregationFunction function, List<string> groupBy = null);

        /// <summary>
        /// Gets events
        /// </summary>
        /// <param name="filter">Event filter</param>
        /// <returns>List of events matching the filter</returns>
        Task<IEnumerable<AnalyticsEvent>> GetEventsAsync(EventFilter filter);

        /// <summary>
        /// Gets event count
        /// </summary>
        /// <param name="filter">Event filter</param>
        /// <returns>Count of events matching the filter</returns>
        Task<int> GetEventCountAsync(EventFilter filter);

        /// <summary>
        /// Creates a dashboard
        /// </summary>
        /// <param name="dashboard">Dashboard to create</param>
        /// <returns>The created dashboard</returns>
        Task<Dashboard> CreateDashboardAsync(Dashboard dashboard);

        /// <summary>
        /// Gets a dashboard by ID
        /// </summary>
        /// <param name="id">Dashboard ID</param>
        /// <returns>The dashboard if found, null otherwise</returns>
        Task<Dashboard> GetDashboardAsync(Guid id);

        /// <summary>
        /// Gets dashboards by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of dashboards for the account</returns>
        Task<IEnumerable<Dashboard>> GetDashboardsByAccountAsync(Guid accountId);

        /// <summary>
        /// Updates a dashboard
        /// </summary>
        /// <param name="dashboard">Dashboard to update</param>
        /// <returns>The updated dashboard</returns>
        Task<Dashboard> UpdateDashboardAsync(Dashboard dashboard);

        /// <summary>
        /// Deletes a dashboard
        /// </summary>
        /// <param name="id">Dashboard ID</param>
        /// <returns>True if the dashboard was deleted, false otherwise</returns>
        Task<bool> DeleteDashboardAsync(Guid id);

        /// <summary>
        /// Creates a report
        /// </summary>
        /// <param name="report">Report to create</param>
        /// <returns>The created report</returns>
        Task<Report> CreateReportAsync(Report report);

        /// <summary>
        /// Gets a report by ID
        /// </summary>
        /// <param name="id">Report ID</param>
        /// <returns>The report if found, null otherwise</returns>
        Task<Report> GetReportAsync(Guid id);

        /// <summary>
        /// Gets reports by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of reports for the account</returns>
        Task<IEnumerable<Report>> GetReportsByAccountAsync(Guid accountId);

        /// <summary>
        /// Updates a report
        /// </summary>
        /// <param name="report">Report to update</param>
        /// <returns>The updated report</returns>
        Task<Report> UpdateReportAsync(Report report);

        /// <summary>
        /// Deletes a report
        /// </summary>
        /// <param name="id">Report ID</param>
        /// <returns>True if the report was deleted, false otherwise</returns>
        Task<bool> DeleteReportAsync(Guid id);

        /// <summary>
        /// Executes a report
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <param name="parameters">Report parameters</param>
        /// <returns>The report execution</returns>
        Task<ReportExecution> ExecuteReportAsync(Guid reportId, Dictionary<string, object> parameters = null);

        /// <summary>
        /// Gets a report execution by ID
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>The report execution if found, null otherwise</returns>
        Task<ReportExecution> GetReportExecutionAsync(Guid id);

        /// <summary>
        /// Gets report executions by report ID
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <returns>List of report executions for the report</returns>
        Task<IEnumerable<ReportExecution>> GetReportExecutionsByReportAsync(Guid reportId);

        /// <summary>
        /// Creates an alert
        /// </summary>
        /// <param name="alert">Alert to create</param>
        /// <returns>The created alert</returns>
        Task<Alert> CreateAlertAsync(Alert alert);

        /// <summary>
        /// Gets an alert by ID
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <returns>The alert if found, null otherwise</returns>
        Task<Alert> GetAlertAsync(Guid id);

        /// <summary>
        /// Gets alerts by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of alerts for the account</returns>
        Task<IEnumerable<Alert>> GetAlertsByAccountAsync(Guid accountId);

        /// <summary>
        /// Updates an alert
        /// </summary>
        /// <param name="alert">Alert to update</param>
        /// <returns>The updated alert</returns>
        Task<Alert> UpdateAlertAsync(Alert alert);

        /// <summary>
        /// Deletes an alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <returns>True if the alert was deleted, false otherwise</returns>
        Task<bool> DeleteAlertAsync(Guid id);

        /// <summary>
        /// Silences an alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <param name="duration">Silence duration in seconds</param>
        /// <returns>The silenced alert</returns>
        Task<Alert> SilenceAlertAsync(Guid id, int duration);

        /// <summary>
        /// Unsilences an alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <returns>The unsilenced alert</returns>
        Task<Alert> UnsilenceAlertAsync(Guid id);

        /// <summary>
        /// Gets alert events
        /// </summary>
        /// <param name="alertId">Alert ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>List of alert events</returns>
        Task<IEnumerable<AlertEvent>> GetAlertEventsAsync(Guid alertId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Executes a query
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <param name="parameters">Query parameters</param>
        /// <returns>Query result</returns>
        Task<object> ExecuteQueryAsync(string query, Dictionary<string, object> parameters = null);

        /// <summary>
        /// Gets analytics statistics
        /// </summary>
        /// <returns>Analytics statistics</returns>
        Task<AnalyticsStatistics> GetStatisticsAsync();
    }

    /// <summary>
    /// Analytics statistics
    /// </summary>
    public class AnalyticsStatistics
    {
        /// <summary>
        /// Gets or sets the total number of metrics
        /// </summary>
        public int TotalMetrics { get; set; }

        /// <summary>
        /// Gets or sets the total number of events
        /// </summary>
        public int TotalEvents { get; set; }

        /// <summary>
        /// Gets or sets the total number of dashboards
        /// </summary>
        public int TotalDashboards { get; set; }

        /// <summary>
        /// Gets or sets the total number of reports
        /// </summary>
        public int TotalReports { get; set; }

        /// <summary>
        /// Gets or sets the total number of alerts
        /// </summary>
        public int TotalAlerts { get; set; }

        /// <summary>
        /// Gets or sets the events in the last 24 hours
        /// </summary>
        public int EventsLast24Hours { get; set; }

        /// <summary>
        /// Gets or sets the data points in the last 24 hours
        /// </summary>
        public int DataPointsLast24Hours { get; set; }

        /// <summary>
        /// Gets or sets the report executions in the last 24 hours
        /// </summary>
        public int ReportExecutionsLast24Hours { get; set; }

        /// <summary>
        /// Gets or sets the alert events in the last 24 hours
        /// </summary>
        public int AlertEventsLast24Hours { get; set; }

        /// <summary>
        /// Gets or sets the storage usage in bytes
        /// </summary>
        public long StorageUsageBytes { get; set; }

        /// <summary>
        /// Gets or sets the last update time
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
    }
}
