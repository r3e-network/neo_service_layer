using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models.Analytics;
using NeoServiceLayer.Services.Analytics.Repositories;

namespace NeoServiceLayer.Services.Analytics
{
    /// <summary>
    /// Implementation of the analytics service
    /// </summary>
    public class AnalyticsService : IAnalyticsService, IDisposable
    {
        private readonly ILogger<AnalyticsService> _logger;
        private readonly IMetricRepository _metricRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IReportRepository _reportRepository;
        private readonly IAlertRepository _alertRepository;
        private readonly INotificationService _notificationService;
        private readonly AnalyticsConfiguration _configuration;

        private Timer _metricCollectionTimer;
        private Timer _alertEvaluationTimer;
        private Timer _reportExecutionTimer;
        private Timer _dataRetentionTimer;

        private readonly SemaphoreSlim _metricCollectionSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _alertEvaluationSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _reportExecutionSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _dataRetentionSemaphore = new SemaphoreSlim(1, 1);

        private readonly List<AnalyticsEvent> _eventBatch = new List<AnalyticsEvent>();
        private readonly SemaphoreSlim _eventBatchSemaphore = new SemaphoreSlim(1, 1);
        private Timer _eventFlushTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="metricRepository">Metric repository</param>
        /// <param name="eventRepository">Event repository</param>
        /// <param name="dashboardRepository">Dashboard repository</param>
        /// <param name="reportRepository">Report repository</param>
        /// <param name="alertRepository">Alert repository</param>
        /// <param name="notificationService">Notification service</param>
        /// <param name="configuration">Configuration</param>
        public AnalyticsService(
            ILogger<AnalyticsService> logger,
            IMetricRepository metricRepository,
            IEventRepository eventRepository,
            IDashboardRepository dashboardRepository,
            IReportRepository reportRepository,
            IAlertRepository alertRepository,
            INotificationService notificationService,
            IOptions<AnalyticsConfiguration> configuration)
        {
            _logger = logger;
            _metricRepository = metricRepository;
            _eventRepository = eventRepository;
            _dashboardRepository = dashboardRepository;
            _reportRepository = reportRepository;
            _alertRepository = alertRepository;
            _notificationService = notificationService;
            _configuration = configuration.Value;

            // Start timers if enabled
            if (_configuration.Enabled)
            {
                StartTimers();
            }
        }

        /// <inheritdoc/>
        public async Task<AnalyticsEvent> TrackEventAsync(AnalyticsEvent @event)
        {
            if (!_configuration.Enabled)
            {
                return @event;
            }

            _logger.LogDebug("Tracking event: {Name}, category: {Category}", @event.Name, @event.Category);

            try
            {
                // Set default values
                if (@event.Id == Guid.Empty)
                {
                    @event.Id = Guid.NewGuid();
                }

                if (@event.Timestamp == default)
                {
                    @event.Timestamp = DateTime.UtcNow;
                }

                // Add to batch
                await _eventBatchSemaphore.WaitAsync();
                try
                {
                    _eventBatch.Add(@event);

                    // Flush if batch size reached
                    if (_eventBatch.Count >= _configuration.EventBatchSize)
                    {
                        await FlushEventBatchAsync();
                    }
                }
                finally
                {
                    _eventBatchSemaphore.Release();
                }

                return @event;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking event: {Name}, category: {Category}", @event.Name, @event.Category);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<MetricDataPoint> TrackMetricAsync(Guid metricId, double value, Dictionary<string, string> dimensions = null)
        {
            if (!_configuration.Enabled)
            {
                return new MetricDataPoint
                {
                    Id = Guid.NewGuid(),
                    MetricId = metricId,
                    Timestamp = DateTime.UtcNow,
                    Value = value,
                    Dimensions = dimensions ?? new Dictionary<string, string>()
                };
            }

            _logger.LogDebug("Tracking metric: {MetricId}, value: {Value}", metricId, value);

            try
            {
                // Create data point
                var dataPoint = new MetricDataPoint
                {
                    Id = Guid.NewGuid(),
                    MetricId = metricId,
                    Timestamp = DateTime.UtcNow,
                    Value = value,
                    Dimensions = dimensions ?? new Dictionary<string, string>()
                };

                // Save data point
                return await _metricRepository.CreateDataPointAsync(dataPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking metric: {MetricId}, value: {Value}", metricId, value);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Metric> CreateMetricAsync(Metric metric)
        {
            _logger.LogInformation("Creating metric: {Name}", metric.Name);

            try
            {
                // Validate metric
                if (string.IsNullOrEmpty(metric.Name))
                {
                    throw new ArgumentException("Metric name is required");
                }

                // Set default values
                if (metric.Id == Guid.Empty)
                {
                    metric.Id = Guid.NewGuid();
                }

                if (string.IsNullOrEmpty(metric.Category))
                {
                    metric.Category = "Custom";
                }

                if (string.IsNullOrEmpty(metric.Unit))
                {
                    metric.Unit = "Count";
                }

                if (metric.Type == 0)
                {
                    metric.Type = MetricType.Gauge;
                }

                metric.CreatedAt = DateTime.UtcNow;
                metric.UpdatedAt = DateTime.UtcNow;

                // Create metric
                return await _metricRepository.CreateAsync(metric);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating metric: {Name}", metric.Name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Metric> GetMetricAsync(Guid id)
        {
            _logger.LogInformation("Getting metric: {Id}", id);

            try
            {
                return await _metricRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metric: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Metric>> GetMetricsByCategoryAsync(string category)
        {
            _logger.LogInformation("Getting metrics by category: {Category}", category);

            try
            {
                return await _metricRepository.GetByCategoryAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics by category: {Category}", category);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Metric>> GetAllMetricsAsync()
        {
            _logger.LogInformation("Getting all metrics");

            try
            {
                return await _metricRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all metrics");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Metric> UpdateMetricAsync(Metric metric)
        {
            _logger.LogInformation("Updating metric: {Id}", metric.Id);

            try
            {
                // Validate metric
                if (string.IsNullOrEmpty(metric.Name))
                {
                    throw new ArgumentException("Metric name is required");
                }

                // Update timestamp
                metric.UpdatedAt = DateTime.UtcNow;

                // Update metric
                return await _metricRepository.UpdateAsync(metric);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating metric: {Id}", metric.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteMetricAsync(Guid id)
        {
            _logger.LogInformation("Deleting metric: {Id}", id);

            try
            {
                return await _metricRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting metric: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MetricDataPoint>> GetMetricDataPointsAsync(Guid metricId, DateTime startTime, DateTime endTime, Dictionary<string, string> dimensions = null)
        {
            _logger.LogInformation("Getting data points for metric: {MetricId}, start: {StartTime}, end: {EndTime}", metricId, startTime, endTime);

            try
            {
                return await _metricRepository.GetDataPointsAsync(metricId, startTime, endTime, dimensions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data points for metric: {MetricId}", metricId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<MetricAggregation> GetMetricAggregationAsync(Guid metricId, DateTime startTime, DateTime endTime, AggregationPeriod period, AggregationFunction function, List<string> groupBy = null)
        {
            _logger.LogInformation("Getting aggregation for metric: {MetricId}, start: {StartTime}, end: {EndTime}, period: {Period}, function: {Function}",
                metricId, startTime, endTime, period, function);

            try
            {
                return await _metricRepository.GetAggregationAsync(metricId, startTime, endTime, period, function, groupBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting aggregation for metric: {MetricId}", metricId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AnalyticsEvent>> GetEventsAsync(EventFilter filter)
        {
            _logger.LogInformation("Getting events by filter");

            try
            {
                // Flush event batch to ensure all events are persisted
                await FlushEventBatchAsync();

                return await _eventRepository.GetByFilterAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events by filter");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetEventCountAsync(EventFilter filter)
        {
            _logger.LogInformation("Getting event count by filter");

            try
            {
                // Flush event batch to ensure all events are persisted
                await FlushEventBatchAsync();

                return await _eventRepository.GetCountByFilterAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event count by filter");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Dashboard> CreateDashboardAsync(Dashboard dashboard)
        {
            _logger.LogInformation("Creating dashboard: {Name} for account: {AccountId}", dashboard.Name, dashboard.AccountId);

            try
            {
                // Validate dashboard
                if (string.IsNullOrEmpty(dashboard.Name))
                {
                    throw new ArgumentException("Dashboard name is required");
                }

                if (dashboard.AccountId == Guid.Empty)
                {
                    throw new ArgumentException("Account ID is required");
                }

                if (dashboard.CreatedBy == Guid.Empty)
                {
                    throw new ArgumentException("Created by user ID is required");
                }

                // Set default values
                if (dashboard.Id == Guid.Empty)
                {
                    dashboard.Id = Guid.NewGuid();
                }

                dashboard.CreatedAt = DateTime.UtcNow;
                dashboard.UpdatedAt = DateTime.UtcNow;

                if (dashboard.Layout == null)
                {
                    dashboard.Layout = new DashboardLayout();
                }

                if (dashboard.RefreshIntervalSeconds <= 0)
                {
                    dashboard.RefreshIntervalSeconds = _configuration.DefaultDashboardRefreshIntervalSeconds;
                }

                // Create dashboard
                return await _dashboardRepository.CreateAsync(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dashboard: {Name} for account: {AccountId}", dashboard.Name, dashboard.AccountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Dashboard> GetDashboardAsync(Guid id)
        {
            _logger.LogInformation("Getting dashboard: {Id}", id);

            try
            {
                return await _dashboardRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Dashboard>> GetDashboardsByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting dashboards for account: {AccountId}", accountId);

            try
            {
                return await _dashboardRepository.GetByAccountAsync(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboards for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Dashboard> UpdateDashboardAsync(Dashboard dashboard)
        {
            _logger.LogInformation("Updating dashboard: {Id}", dashboard.Id);

            try
            {
                // Validate dashboard
                if (string.IsNullOrEmpty(dashboard.Name))
                {
                    throw new ArgumentException("Dashboard name is required");
                }

                // Update timestamp
                dashboard.UpdatedAt = DateTime.UtcNow;

                // Update dashboard
                return await _dashboardRepository.UpdateAsync(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dashboard: {Id}", dashboard.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteDashboardAsync(Guid id)
        {
            _logger.LogInformation("Deleting dashboard: {Id}", id);

            try
            {
                return await _dashboardRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dashboard: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Report> CreateReportAsync(Report report)
        {
            _logger.LogInformation("Creating report: {Name} for account: {AccountId}", report.Name, report.AccountId);

            try
            {
                // Validate report
                if (string.IsNullOrEmpty(report.Name))
                {
                    throw new ArgumentException("Report name is required");
                }

                if (report.AccountId == Guid.Empty)
                {
                    throw new ArgumentException("Account ID is required");
                }

                if (report.CreatedBy == Guid.Empty)
                {
                    throw new ArgumentException("Created by user ID is required");
                }

                // Set default values
                if (report.Id == Guid.Empty)
                {
                    report.Id = Guid.NewGuid();
                }

                report.CreatedAt = DateTime.UtcNow;
                report.UpdatedAt = DateTime.UtcNow;
                report.Status = ReportStatus.Active;

                if (report.Type == 0)
                {
                    report.Type = ReportType.Dashboard;
                }

                if (report.Format == 0)
                {
                    report.Format = ReportFormat.PDF;
                }

                // Create report
                return await _reportRepository.CreateAsync(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report: {Name} for account: {AccountId}", report.Name, report.AccountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Report> GetReportAsync(Guid id)
        {
            _logger.LogInformation("Getting report: {Id}", id);

            try
            {
                return await _reportRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Report>> GetReportsByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting reports for account: {AccountId}", accountId);

            try
            {
                return await _reportRepository.GetByAccountAsync(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Report> UpdateReportAsync(Report report)
        {
            _logger.LogInformation("Updating report: {Id}", report.Id);

            try
            {
                // Validate report
                if (string.IsNullOrEmpty(report.Name))
                {
                    throw new ArgumentException("Report name is required");
                }

                // Update timestamp
                report.UpdatedAt = DateTime.UtcNow;

                // Update report
                return await _reportRepository.UpdateAsync(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report: {Id}", report.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteReportAsync(Guid id)
        {
            _logger.LogInformation("Deleting report: {Id}", id);

            try
            {
                return await _reportRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ReportExecution> ExecuteReportAsync(Guid reportId, Dictionary<string, object> parameters = null)
        {
            _logger.LogInformation("Executing report: {ReportId}", reportId);

            try
            {
                // Get report
                var report = await _reportRepository.GetByIdAsync(reportId);
                if (report == null)
                {
                    throw new ArgumentException($"Report not found: {reportId}");
                }

                // Create execution
                var execution = new ReportExecution
                {
                    Id = Guid.NewGuid(),
                    ReportId = reportId,
                    StartTime = DateTime.UtcNow,
                    Status = ReportExecutionStatus.Running,
                    Parameters = parameters ?? new Dictionary<string, object>()
                };

                execution = await _reportRepository.CreateExecutionAsync(execution);

                // Execute report asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ExecuteReportInternalAsync(report, execution);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing report: {ReportId}", reportId);

                        // Update execution status
                        execution.Status = ReportExecutionStatus.Failed;
                        execution.EndTime = DateTime.UtcNow;
                        execution.ErrorMessage = ex.Message;
                        await _reportRepository.UpdateExecutionAsync(execution);
                    }
                });

                return execution;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing report: {ReportId}", reportId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ReportExecution> GetReportExecutionAsync(Guid id)
        {
            _logger.LogInformation("Getting report execution: {Id}", id);

            try
            {
                return await _reportRepository.GetExecutionByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report execution: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ReportExecution>> GetReportExecutionsByReportAsync(Guid reportId)
        {
            _logger.LogInformation("Getting executions for report: {ReportId}", reportId);

            try
            {
                return await _reportRepository.GetExecutionsByReportAsync(reportId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting executions for report: {ReportId}", reportId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Alert> CreateAlertAsync(Alert alert)
        {
            _logger.LogInformation("Creating alert: {Name} for account: {AccountId}", alert.Name, alert.AccountId);

            try
            {
                // Validate alert
                if (string.IsNullOrEmpty(alert.Name))
                {
                    throw new ArgumentException("Alert name is required");
                }

                if (alert.AccountId == Guid.Empty)
                {
                    throw new ArgumentException("Account ID is required");
                }

                if (alert.CreatedBy == Guid.Empty)
                {
                    throw new ArgumentException("Created by user ID is required");
                }

                if (alert.Condition == null)
                {
                    throw new ArgumentException("Alert condition is required");
                }

                // Set default values
                if (alert.Id == Guid.Empty)
                {
                    alert.Id = Guid.NewGuid();
                }

                alert.CreatedAt = DateTime.UtcNow;
                alert.UpdatedAt = DateTime.UtcNow;
                alert.Status = AlertStatus.OK;

                if (alert.EvaluationFrequencySeconds <= 0)
                {
                    alert.EvaluationFrequencySeconds = _configuration.AlertEvaluationIntervalSeconds;
                }

                if (alert.EvaluationWindowSeconds <= 0)
                {
                    alert.EvaluationWindowSeconds = 300; // 5 minutes
                }

                if (alert.Notification == null)
                {
                    alert.Notification = new AlertNotification
                    {
                        Channels = _configuration.DefaultAlertChannels.ToList(),
                        SendRecoveryNotification = true,
                        IncludeAlertData = true,
                        IncludeEvaluationDetails = true,
                        MinIntervalSeconds = 300 // 5 minutes
                    };
                }

                // Create alert
                return await _alertRepository.CreateAsync(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert: {Name} for account: {AccountId}", alert.Name, alert.AccountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Alert> GetAlertAsync(Guid id)
        {
            _logger.LogInformation("Getting alert: {Id}", id);

            try
            {
                return await _alertRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Alert>> GetAlertsByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting alerts for account: {AccountId}", accountId);

            try
            {
                return await _alertRepository.GetByAccountAsync(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Alert> UpdateAlertAsync(Alert alert)
        {
            _logger.LogInformation("Updating alert: {Id}", alert.Id);

            try
            {
                // Validate alert
                if (string.IsNullOrEmpty(alert.Name))
                {
                    throw new ArgumentException("Alert name is required");
                }

                if (alert.Condition == null)
                {
                    throw new ArgumentException("Alert condition is required");
                }

                // Update timestamp
                alert.UpdatedAt = DateTime.UtcNow;

                // Update alert
                return await _alertRepository.UpdateAsync(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating alert: {Id}", alert.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAlertAsync(Guid id)
        {
            _logger.LogInformation("Deleting alert: {Id}", id);

            try
            {
                return await _alertRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting alert: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Alert> SilenceAlertAsync(Guid id, int duration)
        {
            _logger.LogInformation("Silencing alert: {Id} for {Duration} seconds", id, duration);

            try
            {
                // Get alert
                var alert = await _alertRepository.GetByIdAsync(id);
                if (alert == null)
                {
                    throw new ArgumentException($"Alert not found: {id}");
                }

                // Silence alert
                alert.SilencedUntil = DateTime.UtcNow.AddSeconds(duration);
                alert.Status = AlertStatus.Silenced;
                alert.UpdatedAt = DateTime.UtcNow;

                // Update alert
                return await _alertRepository.UpdateAsync(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error silencing alert: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Alert> UnsilenceAlertAsync(Guid id)
        {
            _logger.LogInformation("Unsilencing alert: {Id}", id);

            try
            {
                // Get alert
                var alert = await _alertRepository.GetByIdAsync(id);
                if (alert == null)
                {
                    throw new ArgumentException($"Alert not found: {id}");
                }

                // Unsilence alert
                alert.SilencedUntil = null;
                alert.Status = AlertStatus.OK;
                alert.UpdatedAt = DateTime.UtcNow;

                // Update alert
                return await _alertRepository.UpdateAsync(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsilencing alert: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AlertEvent>> GetAlertEventsAsync(Guid alertId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Getting events for alert: {AlertId}, start: {StartTime}, end: {EndTime}", alertId, startTime, endTime);

            try
            {
                return await _alertRepository.GetEventsByAlertAsync(alertId, startTime, endTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for alert: {AlertId}", alertId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<object> ExecuteQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            _logger.LogInformation("Executing query: {Query}", query);

            try
            {
                // This is a placeholder for a query execution engine
                // In a real implementation, this would parse and execute the query against the analytics data
                // For now, we'll just return a dummy result
                await Task.Delay(100); // Simulate query execution

                return new { Message = "Query execution not implemented" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query: {Query}", query);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<AnalyticsStatistics> GetStatisticsAsync()
        {
            _logger.LogInformation("Getting analytics statistics");

            try
            {
                var totalMetrics = await _metricRepository.GetMetricsCountAsync();
                var totalEvents = await _eventRepository.GetCountAsync();
                var totalDashboards = await _dashboardRepository.GetCountAsync();
                var totalReports = await _reportRepository.GetCountAsync();
                var totalAlerts = await _alertRepository.GetCountAsync();
                var eventsLast24Hours = await _eventRepository.GetCountLast24HoursAsync();
                var dataPointsLast24Hours = await _metricRepository.GetDataPointsCountLast24HoursAsync();
                var reportExecutionsLast24Hours = await _reportRepository.GetExecutionsCountLast24HoursAsync();
                var alertEventsLast24Hours = await _alertRepository.GetEventsCountLast24HoursAsync();
                var storageUsageBytes = await _metricRepository.GetStorageUsageAsync() + await _eventRepository.GetStorageUsageAsync();

                return new AnalyticsStatistics
                {
                    TotalMetrics = totalMetrics,
                    TotalEvents = totalEvents,
                    TotalDashboards = totalDashboards,
                    TotalReports = totalReports,
                    TotalAlerts = totalAlerts,
                    EventsLast24Hours = eventsLast24Hours,
                    DataPointsLast24Hours = dataPointsLast24Hours,
                    ReportExecutionsLast24Hours = reportExecutionsLast24Hours,
                    AlertEventsLast24Hours = alertEventsLast24Hours,
                    StorageUsageBytes = storageUsageBytes,
                    LastUpdateTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics statistics");
                throw;
            }
        }

        /// <summary>
        /// Executes a report
        /// </summary>
        /// <param name="report">Report to execute</param>
        /// <param name="execution">Report execution</param>
        private async Task ExecuteReportInternalAsync(Report report, ReportExecution execution)
        {
            _logger.LogInformation("Executing report internally: {ReportId}", report.Id);

            try
            {
                // Update report last run time
                report.LastRunAt = DateTime.UtcNow;
                if (report.Schedule != null && report.Schedule.IsScheduled)
                {
                    report.NextRunAt = CalculateNextRunTime(report.Schedule);
                }
                await _reportRepository.UpdateAsync(report);

                // Execute report based on type
                switch (report.Type)
                {
                    case ReportType.Dashboard:
                        await ExecuteDashboardReportAsync(report, execution);
                        break;
                    case ReportType.Query:
                        await ExecuteQueryReportAsync(report, execution);
                        break;
                    case ReportType.Custom:
                        await ExecuteCustomReportAsync(report, execution);
                        break;
                    default:
                        throw new NotSupportedException($"Report type not supported: {report.Type}");
                }

                // Update execution status
                execution.Status = ReportExecutionStatus.Completed;
                execution.EndTime = DateTime.UtcNow;
                await _reportRepository.UpdateExecutionAsync(execution);

                // Deliver report
                if (report.Delivery != null && report.Delivery.Methods.Any())
                {
                    await DeliverReportAsync(report, execution);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing report internally: {ReportId}", report.Id);

                // Update execution status
                execution.Status = ReportExecutionStatus.Failed;
                execution.EndTime = DateTime.UtcNow;
                execution.ErrorMessage = ex.Message;
                await _reportRepository.UpdateExecutionAsync(execution);
            }
        }

        /// <summary>
        /// Executes a dashboard report
        /// </summary>
        /// <param name="report">Report to execute</param>
        /// <param name="execution">Report execution</param>
        private async Task ExecuteDashboardReportAsync(Report report, ReportExecution execution)
        {
            _logger.LogInformation("Executing dashboard report: {ReportId}", report.Id);

            // Get dashboard
            if (!report.DashboardId.HasValue)
            {
                throw new InvalidOperationException("Dashboard ID is required for dashboard reports");
            }

            var dashboard = await _dashboardRepository.GetByIdAsync(report.DashboardId.Value);
            if (dashboard == null)
            {
                throw new InvalidOperationException($"Dashboard not found: {report.DashboardId}");
            }

            // Generate report
            // In a real implementation, this would render the dashboard to the specified format
            // For now, we'll just set a dummy output file URL
            execution.OutputFileUrl = $"https://example.com/reports/{execution.Id}.{report.Format.ToString().ToLower()}";

            await Task.Delay(1000); // Simulate report generation
        }

        /// <summary>
        /// Executes a query report
        /// </summary>
        /// <param name="report">Report to execute</param>
        /// <param name="execution">Report execution</param>
        private async Task ExecuteQueryReportAsync(Report report, ReportExecution execution)
        {
            _logger.LogInformation("Executing query report: {ReportId}", report.Id);

            // Execute query
            if (string.IsNullOrEmpty(report.Query))
            {
                throw new InvalidOperationException("Query is required for query reports");
            }

            // In a real implementation, this would execute the query and generate the report
            // For now, we'll just set a dummy output file URL
            execution.OutputFileUrl = $"https://example.com/reports/{execution.Id}.{report.Format.ToString().ToLower()}";

            await Task.Delay(1000); // Simulate report generation
        }

        /// <summary>
        /// Executes a custom report
        /// </summary>
        /// <param name="report">Report to execute</param>
        /// <param name="execution">Report execution</param>
        private async Task ExecuteCustomReportAsync(Report report, ReportExecution execution)
        {
            _logger.LogInformation("Executing custom report: {ReportId}", report.Id);

            // In a real implementation, this would execute custom report logic
            // For now, we'll just set a dummy output file URL
            execution.OutputFileUrl = $"https://example.com/reports/{execution.Id}.{report.Format.ToString().ToLower()}";

            await Task.Delay(1000); // Simulate report generation
        }

        /// <summary>
        /// Delivers a report
        /// </summary>
        /// <param name="report">Report</param>
        /// <param name="execution">Report execution</param>
        private async Task DeliverReportAsync(Report report, ReportExecution execution)
        {
            _logger.LogInformation("Delivering report: {ReportId}", report.Id);

            try
            {
                // Initialize delivery status
                execution.DeliveryStatus = new Dictionary<string, DeliveryStatus>();

                // Deliver via each method
                foreach (var method in report.Delivery.Methods)
                {
                    switch (method)
                    {
                        case DeliveryMethod.Email:
                            await DeliverReportByEmailAsync(report, execution);
                            break;
                        case DeliveryMethod.Webhook:
                            await DeliverReportByWebhookAsync(report, execution);
                            break;
                        case DeliveryMethod.Storage:
                            await DeliverReportToStorageAsync(report, execution);
                            break;
                        default:
                            _logger.LogWarning("Unsupported delivery method: {Method}", method);
                            break;
                    }
                }

                // Update execution
                await _reportRepository.UpdateExecutionAsync(execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering report: {ReportId}", report.Id);
            }
        }

        /// <summary>
        /// Delivers a report by email
        /// </summary>
        /// <param name="report">Report</param>
        /// <param name="execution">Report execution</param>
        private async Task DeliverReportByEmailAsync(Report report, ReportExecution execution)
        {
            _logger.LogInformation("Delivering report by email: {ReportId}", report.Id);

            try
            {
                // Check if there are recipients
                if (report.Delivery.EmailRecipients == null || !report.Delivery.EmailRecipients.Any())
                {
                    _logger.LogWarning("No email recipients specified for report: {ReportId}", report.Id);
                    return;
                }

                // In a real implementation, this would send an email with the report
                // For now, we'll just update the delivery status
                foreach (var recipient in report.Delivery.EmailRecipients)
                {
                    execution.DeliveryStatus[$"Email:{recipient}"] = new DeliveryStatus
                    {
                        Method = DeliveryMethod.Email,
                        Status = DeliveryStatusType.Sent,
                        Timestamp = DateTime.UtcNow,
                        Recipient = recipient
                    };
                }

                await Task.Delay(100); // Simulate email delivery
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering report by email: {ReportId}", report.Id);

                // Update delivery status
                execution.DeliveryStatus["Email"] = new DeliveryStatus
                {
                    Method = DeliveryMethod.Email,
                    Status = DeliveryStatusType.Failed,
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Delivers a report by webhook
        /// </summary>
        /// <param name="report">Report</param>
        /// <param name="execution">Report execution</param>
        private async Task DeliverReportByWebhookAsync(Report report, ReportExecution execution)
        {
            _logger.LogInformation("Delivering report by webhook: {ReportId}", report.Id);

            try
            {
                // Check if there are webhook URLs
                if (report.Delivery.WebhookUrls == null || !report.Delivery.WebhookUrls.Any())
                {
                    _logger.LogWarning("No webhook URLs specified for report: {ReportId}", report.Id);
                    return;
                }

                // In a real implementation, this would send a webhook request with the report
                // For now, we'll just update the delivery status
                foreach (var webhookUrl in report.Delivery.WebhookUrls)
                {
                    execution.DeliveryStatus[$"Webhook:{webhookUrl}"] = new DeliveryStatus
                    {
                        Method = DeliveryMethod.Webhook,
                        Status = DeliveryStatusType.Sent,
                        Timestamp = DateTime.UtcNow,
                        Recipient = webhookUrl
                    };
                }

                await Task.Delay(100); // Simulate webhook delivery
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering report by webhook: {ReportId}", report.Id);

                // Update delivery status
                execution.DeliveryStatus["Webhook"] = new DeliveryStatus
                {
                    Method = DeliveryMethod.Webhook,
                    Status = DeliveryStatusType.Failed,
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Delivers a report to storage
        /// </summary>
        /// <param name="report">Report</param>
        /// <param name="execution">Report execution</param>
        private async Task DeliverReportToStorageAsync(Report report, ReportExecution execution)
        {
            _logger.LogInformation("Delivering report to storage: {ReportId}", report.Id);

            try
            {
                // In a real implementation, this would save the report to storage
                // For now, we'll just update the delivery status
                execution.DeliveryStatus["Storage"] = new DeliveryStatus
                {
                    Method = DeliveryMethod.Storage,
                    Status = DeliveryStatusType.Sent,
                    Timestamp = DateTime.UtcNow,
                    Recipient = report.Delivery.StoragePath ?? $"reports/{report.AccountId}/{report.Id}"
                };

                await Task.Delay(100); // Simulate storage delivery
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering report to storage: {ReportId}", report.Id);

                // Update delivery status
                execution.DeliveryStatus["Storage"] = new DeliveryStatus
                {
                    Method = DeliveryMethod.Storage,
                    Status = DeliveryStatusType.Failed,
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Calculates the next run time for a report schedule
        /// </summary>
        /// <param name="schedule">Report schedule</param>
        /// <returns>Next run time</returns>
        private DateTime CalculateNextRunTime(ReportSchedule schedule)
        {
            if (!schedule.IsScheduled)
            {
                return DateTime.MaxValue;
            }

            var now = DateTime.UtcNow;
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(schedule.TimeZone);
            var nowInTimeZone = TimeZoneInfo.ConvertTimeFromUtc(now, timeZone);

            // Check if schedule is active
            if (schedule.StartDate.HasValue && schedule.StartDate.Value > nowInTimeZone)
            {
                return TimeZoneInfo.ConvertTimeToUtc(schedule.StartDate.Value.Add(schedule.TimeOfDay), timeZone);
            }

            if (schedule.EndDate.HasValue && schedule.EndDate.Value < nowInTimeZone)
            {
                return DateTime.MaxValue;
            }

            // Calculate next run time based on frequency
            DateTime nextRun;
            switch (schedule.Frequency)
            {
                case ScheduleFrequency.Hourly:
                    nextRun = nowInTimeZone.AddHours(schedule.Interval);
                    nextRun = new DateTime(nextRun.Year, nextRun.Month, nextRun.Day, nextRun.Hour, 0, 0);
                    break;

                case ScheduleFrequency.Daily:
                    nextRun = nowInTimeZone.Date.Add(schedule.TimeOfDay);
                    if (nextRun <= nowInTimeZone)
                    {
                        nextRun = nextRun.AddDays(schedule.Interval);
                    }
                    break;

                case ScheduleFrequency.Weekly:
                    nextRun = nowInTimeZone.Date.Add(schedule.TimeOfDay);
                    if (!schedule.DaysOfWeek.Any())
                    {
                        // Default to Monday if no days specified
                        schedule.DaysOfWeek.Add(DayOfWeek.Monday);
                    }

                    // Find the next day of the week
                    var daysToAdd = 1;
                    while (!schedule.DaysOfWeek.Contains(nextRun.DayOfWeek) || nextRun <= nowInTimeZone)
                    {
                        nextRun = nextRun.AddDays(1);
                        daysToAdd++;

                        // Add interval weeks if we've gone through all days of the week
                        if (daysToAdd > 7)
                        {
                            nextRun = nextRun.AddDays(7 * (schedule.Interval - 1));
                            daysToAdd = 1;
                        }
                    }
                    break;

                case ScheduleFrequency.Monthly:
                    nextRun = nowInTimeZone.Date.Add(schedule.TimeOfDay);
                    if (!schedule.DaysOfMonth.Any())
                    {
                        // Default to 1st day if no days specified
                        schedule.DaysOfMonth.Add(1);
                    }

                    // Find the next day of the month
                    var currentMonth = nowInTimeZone.Month;
                    var currentYear = nowInTimeZone.Year;
                    var found = false;

                    for (var i = 0; i < 12 * schedule.Interval && !found; i++)
                    {
                        var month = currentMonth + i;
                        var year = currentYear + (month - 1) / 12;
                        month = ((month - 1) % 12) + 1;

                        foreach (var day in schedule.DaysOfMonth.OrderBy(d => d))
                        {
                            var daysInMonth = DateTime.DaysInMonth(year, month);
                            var dayToUse = Math.Min(day, daysInMonth);
                            var candidate = new DateTime(year, month, dayToUse).Add(schedule.TimeOfDay);

                            if (candidate > nowInTimeZone)
                            {
                                nextRun = candidate;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        nextRun = nowInTimeZone.AddMonths(schedule.Interval);
                    }
                    break;

                case ScheduleFrequency.Custom:
                    // For custom schedules, just add the interval in days
                    nextRun = nowInTimeZone.Date.Add(schedule.TimeOfDay);
                    if (nextRun <= nowInTimeZone)
                    {
                        nextRun = nextRun.AddDays(schedule.Interval);
                    }
                    break;

                default:
                    nextRun = nowInTimeZone.Date.Add(schedule.TimeOfDay);
                    if (nextRun <= nowInTimeZone)
                    {
                        nextRun = nextRun.AddDays(1);
                    }
                    break;
            }

            // Convert back to UTC
            return TimeZoneInfo.ConvertTimeToUtc(nextRun, timeZone);
        }

        /// <summary>
        /// Evaluates alerts
        /// </summary>
        private async Task EvaluateAlertsAsync()
        {
            if (!await _alertEvaluationSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                _logger.LogInformation("Evaluating alerts");

                // Get alerts due for evaluation
                var alerts = await _alertRepository.GetDueForEvaluationAsync();
                foreach (var alert in alerts)
                {
                    try
                    {
                        await EvaluateAlertAsync(alert);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error evaluating alert: {AlertId}", alert.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating alerts");
            }
            finally
            {
                _alertEvaluationSemaphore.Release();
            }
        }

        /// <summary>
        /// Evaluates an alert
        /// </summary>
        /// <param name="alert">Alert to evaluate</param>
        private async Task EvaluateAlertAsync(Alert alert)
        {
            _logger.LogInformation("Evaluating alert: {AlertId}", alert.Id);

            try
            {
                // Update last evaluation time
                alert.LastEvaluationAt = DateTime.UtcNow;

                // Evaluate alert based on condition type
                bool isTriggered = false;
                double value = 0;
                double threshold = alert.Condition.Threshold;
                string message = null;

                switch (alert.Condition.Type)
                {
                    case ConditionType.Threshold:
                        (isTriggered, value, message) = await EvaluateThresholdConditionAsync(alert);
                        break;
                    case ConditionType.Change:
                        (isTriggered, value, message) = await EvaluateChangeConditionAsync(alert);
                        break;
                    case ConditionType.Anomaly:
                        (isTriggered, value, message) = await EvaluateAnomalyConditionAsync(alert);
                        break;
                    case ConditionType.NoData:
                        (isTriggered, value, message) = await EvaluateNoDataConditionAsync(alert);
                        break;
                    default:
                        throw new NotSupportedException($"Alert condition type not supported: {alert.Condition.Type}");
                }

                // Update alert status
                var previousStatus = alert.Status;
                if (isTriggered)
                {
                    alert.Status = AlertStatus.Alerting;
                }
                else if (alert.Status == AlertStatus.Alerting)
                {
                    alert.Status = AlertStatus.OK;
                }

                // Create alert event if status changed
                if (alert.Status != previousStatus)
                {
                    var eventType = alert.Status == AlertStatus.Alerting
                        ? AlertEventType.Triggered
                        : AlertEventType.Resolved;

                    var alertEvent = new AlertEvent
                    {
                        Id = Guid.NewGuid(),
                        AlertId = alert.Id,
                        Timestamp = DateTime.UtcNow,
                        Type = eventType,
                        Severity = alert.Condition.Severity,
                        Value = value,
                        Threshold = threshold,
                        Message = message ?? $"Alert {(eventType == AlertEventType.Triggered ? "triggered" : "resolved")}: {alert.Name}"
                    };

                    await _alertRepository.CreateEventAsync(alertEvent);

                    // Send notification
                    if (alert.Notification != null && alert.Notification.Channels.Any())
                    {
                        if (eventType == AlertEventType.Triggered ||
                            (eventType == AlertEventType.Resolved && alert.Notification.SendRecoveryNotification))
                        {
                            await SendAlertNotificationAsync(alert, alertEvent);
                        }
                    }
                }

                // Update alert
                await _alertRepository.UpdateAsync(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating alert: {AlertId}", alert.Id);

                // Update alert status
                alert.Status = AlertStatus.Error;
                alert.LastEvaluationAt = DateTime.UtcNow;
                await _alertRepository.UpdateAsync(alert);

                // Create error event
                var alertEvent = new AlertEvent
                {
                    Id = Guid.NewGuid(),
                    AlertId = alert.Id,
                    Timestamp = DateTime.UtcNow,
                    Type = AlertEventType.Error,
                    Severity = AlertSeverity.Error,
                    Message = $"Error evaluating alert: {ex.Message}"
                };

                await _alertRepository.CreateEventAsync(alertEvent);
            }
        }

        /// <summary>
        /// Evaluates a threshold condition
        /// </summary>
        /// <param name="alert">Alert</param>
        /// <returns>Tuple of (isTriggered, value, message)</returns>
        private async Task<(bool, double, string)> EvaluateThresholdConditionAsync(Alert alert)
        {
            _logger.LogInformation("Evaluating threshold condition for alert: {AlertId}", alert.Id);

            if (!alert.MetricId.HasValue)
            {
                throw new InvalidOperationException("Metric ID is required for threshold conditions");
            }

            // Get metric data
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddSeconds(-alert.EvaluationWindowSeconds);
            var aggregation = await _metricRepository.GetAggregationAsync(
                alert.MetricId.Value,
                startTime,
                endTime,
                _configuration.DefaultAggregationPeriod,
                alert.Condition.AggregationFunction,
                alert.Condition.GroupBy);

            if (aggregation.Values.Count == 0)
            {
                return (false, 0, "No data available");
            }

            // Check if condition is met
            bool isTriggered = false;
            double value = 0;
            string message = null;

            foreach (var aggregatedValue in aggregation.Values)
            {
                value = aggregatedValue.Value;
                isTriggered = EvaluateCondition(value, alert.Condition.Operator, alert.Condition.Threshold);

                if (isTriggered)
                {
                    var dimensionsStr = string.Join(", ", aggregatedValue.Dimensions.Select(d => $"{d.Key}={d.Value}"));
                    message = $"Alert triggered: {alert.Name}. Value: {value} {GetOperatorString(alert.Condition.Operator)} {alert.Condition.Threshold}";
                    if (!string.IsNullOrEmpty(dimensionsStr))
                    {
                        message += $" for {dimensionsStr}";
                    }
                    break;
                }
            }

            return (isTriggered, value, message);
        }

        /// <summary>
        /// Evaluates a change condition
        /// </summary>
        /// <param name="alert">Alert</param>
        /// <returns>Tuple of (isTriggered, value, message)</returns>
        private async Task<(bool, double, string)> EvaluateChangeConditionAsync(Alert alert)
        {
            _logger.LogInformation("Evaluating change condition for alert: {AlertId}", alert.Id);

            if (!alert.MetricId.HasValue)
            {
                throw new InvalidOperationException("Metric ID is required for change conditions");
            }

            // Get metric data for current and previous periods
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddSeconds(-alert.EvaluationWindowSeconds * 2);
            var aggregation = await _metricRepository.GetAggregationAsync(
                alert.MetricId.Value,
                startTime,
                endTime,
                _configuration.DefaultAggregationPeriod,
                alert.Condition.AggregationFunction,
                alert.Condition.GroupBy);

            if (aggregation.Values.Count < 2)
            {
                return (false, 0, "Insufficient data available");
            }

            // Group values by dimensions
            var valuesByDimensions = aggregation.Values
                .GroupBy(v => string.Join(",", v.Dimensions.OrderBy(d => d.Key).Select(d => $"{d.Key}={d.Value}")))
                .ToDictionary(g => g.Key, g => g.OrderBy(v => v.Timestamp).ToList());

            // Check if condition is met
            bool isTriggered = false;
            double value = 0;
            string message = null;

            foreach (var group in valuesByDimensions)
            {
                if (group.Value.Count < 2)
                {
                    continue;
                }

                var previousValue = group.Value[0].Value;
                var currentValue = group.Value[group.Value.Count - 1].Value;
                var changeValue = currentValue - previousValue;
                var changePercent = previousValue != 0 ? (changeValue / Math.Abs(previousValue)) * 100 : 0;

                // Use percent change or absolute change based on comparison value
                value = alert.Condition.ComparisonValue == "percent" ? changePercent : changeValue;
                isTriggered = EvaluateCondition(value, alert.Condition.Operator, alert.Condition.Threshold);

                if (isTriggered)
                {
                    message = $"Alert triggered: {alert.Name}. Change: {value:F2}{(alert.Condition.ComparisonValue == "percent" ? "%" : "")} {GetOperatorString(alert.Condition.Operator)} {alert.Condition.Threshold}{(alert.Condition.ComparisonValue == "percent" ? "%" : "")}";
                    if (!string.IsNullOrEmpty(group.Key))
                    {
                        message += $" for {group.Key}";
                    }
                    break;
                }
            }

            return (isTriggered, value, message);
        }

        /// <summary>
        /// Evaluates an anomaly condition
        /// </summary>
        /// <param name="alert">Alert</param>
        /// <returns>Tuple of (isTriggered, value, message)</returns>
        private async Task<(bool, double, string)> EvaluateAnomalyConditionAsync(Alert alert)
        {
            _logger.LogInformation("Evaluating anomaly condition for alert: {AlertId}", alert.Id);

            // This is a placeholder for anomaly detection
            // In a real implementation, this would use more sophisticated anomaly detection algorithms
            return (false, 0, "Anomaly detection not implemented");
        }

        /// <summary>
        /// Evaluates a no data condition
        /// </summary>
        /// <param name="alert">Alert</param>
        /// <returns>Tuple of (isTriggered, value, message)</returns>
        private async Task<(bool, double, string)> EvaluateNoDataConditionAsync(Alert alert)
        {
            _logger.LogInformation("Evaluating no data condition for alert: {AlertId}", alert.Id);

            if (!alert.MetricId.HasValue)
            {
                throw new InvalidOperationException("Metric ID is required for no data conditions");
            }

            // Get metric data
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddSeconds(-alert.EvaluationWindowSeconds);
            var dataPoints = await _metricRepository.GetDataPointsAsync(alert.MetricId.Value, startTime, endTime);

            // Check if there is no data
            bool isTriggered = !dataPoints.Any();
            double value = isTriggered ? 0 : 1;
            string message = isTriggered ? $"Alert triggered: {alert.Name}. No data received for {alert.EvaluationWindowSeconds} seconds" : null;

            return (isTriggered, value, message);
        }

        /// <summary>
        /// Evaluates a condition
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="operator">Operator</param>
        /// <param name="threshold">Threshold</param>
        /// <returns>True if condition is met, false otherwise</returns>
        private bool EvaluateCondition(double value, ConditionOperator @operator, double threshold)
        {
            switch (@operator)
            {
                case ConditionOperator.GreaterThan:
                    return value > threshold;
                case ConditionOperator.GreaterThanOrEqual:
                    return value >= threshold;
                case ConditionOperator.LessThan:
                    return value < threshold;
                case ConditionOperator.LessThanOrEqual:
                    return value <= threshold;
                case ConditionOperator.Equal:
                    return Math.Abs(value - threshold) < 0.0001; // Use epsilon for floating point comparison
                case ConditionOperator.NotEqual:
                    return Math.Abs(value - threshold) >= 0.0001; // Use epsilon for floating point comparison
                case ConditionOperator.OutsideRange:
                    // For outside range, threshold is interpreted as a range (e.g. "5:10")
                    var rangeOutside = threshold.ToString().Split(':');
                    if (rangeOutside.Length == 2 && double.TryParse(rangeOutside[0], out var minOutside) && double.TryParse(rangeOutside[1], out var maxOutside))
                    {
                        return value < minOutside || value > maxOutside;
                    }
                    return false;
                case ConditionOperator.InsideRange:
                    // For inside range, threshold is interpreted as a range (e.g. "5:10")
                    var rangeInside = threshold.ToString().Split(':');
                    if (rangeInside.Length == 2 && double.TryParse(rangeInside[0], out var minInside) && double.TryParse(rangeInside[1], out var maxInside))
                    {
                        return value >= minInside && value <= maxInside;
                    }
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets a string representation of an operator
        /// </summary>
        /// <param name="operator">Operator</param>
        /// <returns>String representation</returns>
        private string GetOperatorString(ConditionOperator @operator)
        {
            switch (@operator)
            {
                case ConditionOperator.GreaterThan:
                    return ">";
                case ConditionOperator.GreaterThanOrEqual:
                    return ">=";
                case ConditionOperator.LessThan:
                    return "<";
                case ConditionOperator.LessThanOrEqual:
                    return "<=";
                case ConditionOperator.Equal:
                    return "=";
                case ConditionOperator.NotEqual:
                    return "!=";
                case ConditionOperator.OutsideRange:
                    return "outside";
                case ConditionOperator.InsideRange:
                    return "inside";
                default:
                    return @operator.ToString();
            }
        }

        /// <summary>
        /// Sends an alert notification
        /// </summary>
        /// <param name="alert">Alert</param>
        /// <param name="alertEvent">Alert event</param>
        private async Task SendAlertNotificationAsync(Alert alert, AlertEvent alertEvent)
        {
            _logger.LogInformation("Sending alert notification for alert: {AlertId}, event: {EventType}", alert.Id, alertEvent.Type);

            try
            {
                // Check if notification is configured
                if (alert.Notification == null || !alert.Notification.Channels.Any())
                {
                    _logger.LogWarning("No notification channels configured for alert: {AlertId}", alert.Id);
                    return;
                }

                // Check if notification should be throttled
                var lastNotificationTime = alertEvent.NotificationStatus.Values
                    .Where(s => s == Core.Enums.NotificationStatus.Sent)
                    .Select(s => DateTime.UtcNow) // Use current time as a fallback
                    .DefaultIfEmpty(DateTime.MinValue)
                    .Max();

                if ((DateTime.UtcNow - lastNotificationTime).TotalSeconds < alert.Notification.MinIntervalSeconds)
                {
                    _logger.LogInformation("Throttling notification for alert: {AlertId}", alert.Id);
                    return;
                }

                // Prepare notification data
                var subject = alert.Notification.SubjectTemplate;
                if (string.IsNullOrEmpty(subject))
                {
                    subject = $"Alert {alertEvent.Type}: {alert.Name}";
                }

                var message = alert.Notification.MessageTemplate;
                if (string.IsNullOrEmpty(message))
                {
                    message = alertEvent.Message;
                }

                var data = new Dictionary<string, object>
                {
                    { "AlertId", alert.Id },
                    { "AlertName", alert.Name },
                    { "EventType", alertEvent.Type },
                    { "Severity", alertEvent.Severity },
                    { "Timestamp", alertEvent.Timestamp },
                    { "Value", alertEvent.Value },
                    { "Threshold", alertEvent.Threshold },
                    { "Message", alertEvent.Message },
                    { "Dimensions", alertEvent.Dimensions }
                };

                // Send notification through each channel
                foreach (var channel in alert.Notification.Channels)
                {
                    try
                    {
                        // Create notification
                        var notification = new Core.Models.Notification
                        {
                            AccountId = alert.AccountId,
                            Type = Core.Models.NotificationType.Event,
                            Priority = GetNotificationPriority(alertEvent.Severity),
                            Subject = subject,
                            Content = message,
                            Data = data,
                            Channels = new List<Core.Models.NotificationChannel> { (Core.Models.NotificationChannel)channel }
                        };

                        // Add channel-specific data
                        switch (channel)
                        {
                            case Core.Models.NotificationChannel.Email:
                                if (alert.Notification.EmailRecipients != null && alert.Notification.EmailRecipients.Any())
                                {
                                    notification.Data["EmailRecipients"] = alert.Notification.EmailRecipients;
                                }
                                break;
                            case Core.Models.NotificationChannel.Webhook:
                                if (alert.Notification.WebhookUrls != null && alert.Notification.WebhookUrls.Any())
                                {
                                    notification.Data["WebhookUrl"] = alert.Notification.WebhookUrls.First();
                                }
                                break;
                        }

                        // Send notification
                        var sentNotification = await _notificationService.SendNotificationAsync(notification);

                        // Update alert event notification status
                        alertEvent.NotificationStatus[(Core.Models.NotificationChannel)channel] = Core.Enums.NotificationStatus.Sent;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending alert notification through channel {Channel} for alert: {AlertId}", channel, alert.Id);
                        alertEvent.NotificationStatus[(Core.Models.NotificationChannel)channel] = Core.Enums.NotificationStatus.Failed;
                    }
                }

                // Update alert event
                await _alertRepository.CreateEventAsync(alertEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending alert notification for alert: {AlertId}", alert.Id);
            }
        }

        /// <summary>
        /// Gets the notification priority based on alert severity
        /// </summary>
        /// <param name="severity">Alert severity</param>
        /// <returns>Notification priority</returns>
        private Core.Models.NotificationPriority GetNotificationPriority(AlertSeverity severity)
        {
            switch (severity)
            {
                case AlertSeverity.Critical:
                    return Core.Models.NotificationPriority.Critical;
                case AlertSeverity.Error:
                    return Core.Models.NotificationPriority.High;
                case AlertSeverity.Warning:
                    return Core.Models.NotificationPriority.Normal;
                case AlertSeverity.Info:
                    return Core.Models.NotificationPriority.Low;
                default:
                    return Core.Models.NotificationPriority.Normal;
            }
        }

        /// <summary>
        /// Starts the timers
        /// </summary>
        private void StartTimers()
        {
            _logger.LogInformation("Starting analytics timers");

            // Start metric collection timer
            _metricCollectionTimer = new Timer(
                async _ => await CollectMetricsAsync(),
                null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(_configuration.MetricCollectionIntervalSeconds));

            // Start alert evaluation timer
            _alertEvaluationTimer = new Timer(
                async _ => await EvaluateAlertsAsync(),
                null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(_configuration.AlertEvaluationIntervalSeconds));

            // Start report execution timer
            _reportExecutionTimer = new Timer(
                async _ => await ExecuteReportsAsync(),
                null,
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(_configuration.ReportExecutionIntervalSeconds));

            // Start data retention timer
            _dataRetentionTimer = new Timer(
                async _ => await EnforceDataRetentionAsync(),
                null,
                TimeSpan.FromHours(1),
                TimeSpan.FromHours(24));

            // Start event flush timer
            _eventFlushTimer = new Timer(
                async _ => await FlushEventBatchAsync(),
                null,
                TimeSpan.FromSeconds(_configuration.EventFlushIntervalSeconds),
                TimeSpan.FromSeconds(_configuration.EventFlushIntervalSeconds));
        }

        /// <summary>
        /// Collects metrics
        /// </summary>
        private async Task CollectMetricsAsync()
        {
            if (!await _metricCollectionSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                _logger.LogInformation("Collecting metrics");

                // In a real implementation, this would collect system metrics
                // For now, we'll just log a message
                _logger.LogInformation("Metrics collection not implemented");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting metrics");
            }
            finally
            {
                _metricCollectionSemaphore.Release();
            }
        }

        /// <summary>
        /// Executes reports
        /// </summary>
        private async Task ExecuteReportsAsync()
        {
            if (!await _reportExecutionSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                _logger.LogInformation("Executing reports");

                // Get reports due for execution
                var reports = await _reportRepository.GetDueForExecutionAsync();
                foreach (var report in reports)
                {
                    try
                    {
                        // Create execution
                        var execution = new ReportExecution
                        {
                            Id = Guid.NewGuid(),
                            ReportId = report.Id,
                            StartTime = DateTime.UtcNow,
                            Status = ReportExecutionStatus.Running,
                            Parameters = new Dictionary<string, object>()
                        };

                        execution = await _reportRepository.CreateExecutionAsync(execution);

                        // Execute report
                        await ExecuteReportInternalAsync(report, execution);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing report: {ReportId}", report.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing reports");
            }
            finally
            {
                _reportExecutionSemaphore.Release();
            }
        }

        /// <summary>
        /// Enforces data retention
        /// </summary>
        private async Task EnforceDataRetentionAsync()
        {
            if (!await _dataRetentionSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                _logger.LogInformation("Enforcing data retention");

                // Delete old events
                var retentionDate = DateTime.UtcNow.AddDays(-_configuration.RetentionDays);
                var deletedCount = await _eventRepository.DeleteOlderThanAsync(retentionDate);
                _logger.LogInformation("Deleted {Count} old events", deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enforcing data retention");
            }
            finally
            {
                _dataRetentionSemaphore.Release();
            }
        }

        /// <summary>
        /// Flushes the event batch
        /// </summary>
        private async Task FlushEventBatchAsync()
        {
            if (_eventBatch.Count == 0)
            {
                return;
            }

            await _eventBatchSemaphore.WaitAsync();
            try
            {
                _logger.LogInformation("Flushing event batch: {Count} events", _eventBatch.Count);

                // Create a copy of the batch
                var events = _eventBatch.ToList();
                _eventBatch.Clear();

                // Save events
                foreach (var @event in events)
                {
                    try
                    {
                        await _eventRepository.CreateAsync(@event);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving event: {EventName}", @event.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flushing event batch");
            }
            finally
            {
                _eventBatchSemaphore.Release();
            }
        }

        /// <summary>
        /// Disposes the service
        /// </summary>
        public void Dispose()
        {
            _logger.LogInformation("Disposing analytics service");

            // Dispose timers
            _metricCollectionTimer?.Dispose();
            _alertEvaluationTimer?.Dispose();
            _reportExecutionTimer?.Dispose();
            _dataRetentionTimer?.Dispose();
            _eventFlushTimer?.Dispose();

            // Dispose semaphores
            _metricCollectionSemaphore?.Dispose();
            _alertEvaluationSemaphore?.Dispose();
            _reportExecutionSemaphore?.Dispose();
            _dataRetentionSemaphore?.Dispose();
            _eventBatchSemaphore?.Dispose();
        }
    }
}