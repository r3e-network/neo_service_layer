using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Services.Analytics.Repositories
{
    /// <summary>
    /// Implementation of the report repository
    /// </summary>
    public class ReportRepository : IReportRepository
    {
        private readonly ILogger<ReportRepository> _logger;
        private readonly IDatabaseService _databaseService;
        private const string ReportsCollectionName = "analytics_reports";
        private const string ReportExecutionsCollectionName = "analytics_report_executions";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public ReportRepository(ILogger<ReportRepository> logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <inheritdoc/>
        public async Task<Report> CreateAsync(Report report)
        {
            _logger.LogInformation("Creating report: {Name} for account: {AccountId}", report.Name, report.AccountId);

            try
            {
                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(ReportsCollectionName))
                {
                    await _databaseService.CreateCollectionAsync(ReportsCollectionName);
                }

                // Set default values
                if (report.Id == Guid.Empty)
                {
                    report.Id = Guid.NewGuid();
                }

                report.CreatedAt = DateTime.UtcNow;
                report.UpdatedAt = DateTime.UtcNow;
                report.Status = ReportStatus.Active;

                // Calculate next run time if scheduled
                if (report.Schedule != null && report.Schedule.IsScheduled)
                {
                    report.NextRunAt = CalculateNextRunTime(report.Schedule);
                }

                // Create report
                return await _databaseService.CreateAsync(ReportsCollectionName, report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report: {Name} for account: {AccountId}", report.Name, report.AccountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Report> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting report: {Id}", id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportsCollectionName))
                {
                    return null;
                }

                // Get report
                return await _databaseService.GetByIdAsync<Report, Guid>(ReportsCollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Report>> GetByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting reports for account: {AccountId}", accountId);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportsCollectionName))
                {
                    return Enumerable.Empty<Report>();
                }

                // Get reports
                return await _databaseService.GetByFilterAsync<Report>(
                    ReportsCollectionName,
                    r => r.AccountId == accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Report>> GetByUserAsync(Guid userId)
        {
            _logger.LogInformation("Getting reports for user: {UserId}", userId);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportsCollectionName))
                {
                    return Enumerable.Empty<Report>();
                }

                // Get reports
                return await _databaseService.GetByFilterAsync<Report>(
                    ReportsCollectionName,
                    r => r.CreatedBy == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports for user: {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Report>> GetByStatusAsync(ReportStatus status)
        {
            _logger.LogInformation("Getting reports with status: {Status}", status);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportsCollectionName))
                {
                    return Enumerable.Empty<Report>();
                }

                // Get reports
                return await _databaseService.GetByFilterAsync<Report>(
                    ReportsCollectionName,
                    r => r.Status == status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports with status: {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Report>> GetDueForExecutionAsync()
        {
            _logger.LogInformation("Getting reports due for execution");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportsCollectionName))
                {
                    return Enumerable.Empty<Report>();
                }

                // Get reports
                var now = DateTime.UtcNow;
                return await _databaseService.GetByFilterAsync<Report>(
                    ReportsCollectionName,
                    r => r.Status == ReportStatus.Active &&
                         r.Schedule != null &&
                         r.Schedule.IsScheduled &&
                         r.NextRunAt.HasValue &&
                         r.NextRunAt.Value <= now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports due for execution");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Report> UpdateAsync(Report report)
        {
            _logger.LogInformation("Updating report: {Id}", report.Id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportsCollectionName))
                {
                    throw new InvalidOperationException("Reports collection does not exist");
                }

                // Update timestamp
                report.UpdatedAt = DateTime.UtcNow;

                // Calculate next run time if scheduled
                if (report.Schedule != null && report.Schedule.IsScheduled && report.Status == ReportStatus.Active)
                {
                    report.NextRunAt = CalculateNextRunTime(report.Schedule);
                }
                else
                {
                    report.NextRunAt = null;
                }

                // Update report
                return await _databaseService.UpdateAsync<Report, Guid>(ReportsCollectionName, report.Id, report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report: {Id}", report.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting report: {Id}", id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportsCollectionName))
                {
                    return false;
                }

                // Delete report
                return await _databaseService.DeleteAsync<Report, Guid>(ReportsCollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ReportExecution> CreateExecutionAsync(ReportExecution execution)
        {
            _logger.LogInformation("Creating report execution for report: {ReportId}", execution.ReportId);

            try
            {
                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(ReportExecutionsCollectionName))
                {
                    await _databaseService.CreateCollectionAsync(ReportExecutionsCollectionName);
                }

                // Set default values
                if (execution.Id == Guid.Empty)
                {
                    execution.Id = Guid.NewGuid();
                }

                if (execution.StartTime == default)
                {
                    execution.StartTime = DateTime.UtcNow;
                }

                if (execution.Status == 0)
                {
                    execution.Status = ReportExecutionStatus.Pending;
                }

                // Create execution
                return await _databaseService.CreateAsync(ReportExecutionsCollectionName, execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report execution for report: {ReportId}", execution.ReportId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ReportExecution> GetExecutionByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting report execution: {Id}", id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportExecutionsCollectionName))
                {
                    return null;
                }

                // Get execution
                return await _databaseService.GetByIdAsync<ReportExecution, Guid>(ReportExecutionsCollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report execution: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ReportExecution>> GetExecutionsByReportAsync(Guid reportId)
        {
            _logger.LogInformation("Getting executions for report: {ReportId}", reportId);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportExecutionsCollectionName))
                {
                    return Enumerable.Empty<ReportExecution>();
                }

                // Get executions
                return await _databaseService.GetByFilterAsync<ReportExecution>(
                    ReportExecutionsCollectionName,
                    e => e.ReportId == reportId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting executions for report: {ReportId}", reportId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ReportExecution>> GetExecutionsByStatusAsync(ReportExecutionStatus status)
        {
            _logger.LogInformation("Getting report executions with status: {Status}", status);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportExecutionsCollectionName))
                {
                    return Enumerable.Empty<ReportExecution>();
                }

                // Get executions
                return await _databaseService.GetByFilterAsync<ReportExecution>(
                    ReportExecutionsCollectionName,
                    e => e.Status == status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report executions with status: {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ReportExecution> UpdateExecutionAsync(ReportExecution execution)
        {
            _logger.LogInformation("Updating report execution: {Id}", execution.Id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportExecutionsCollectionName))
                {
                    throw new InvalidOperationException("Report executions collection does not exist");
                }

                // Update execution
                return await _databaseService.UpdateAsync<ReportExecution, Guid>(ReportExecutionsCollectionName, execution.Id, execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report execution: {Id}", execution.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountAsync()
        {
            _logger.LogInformation("Getting report count");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportsCollectionName))
                {
                    return 0;
                }

                // Get count
                var reports = await _databaseService.GetAllAsync<Report>(ReportsCollectionName);
                return reports.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report count");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetExecutionsCountLast24HoursAsync()
        {
            _logger.LogInformation("Getting report executions count for last 24 hours");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(ReportExecutionsCollectionName))
                {
                    return 0;
                }

                // Get count
                var startTime = DateTime.UtcNow.AddHours(-24);
                var executions = await _databaseService.GetByFilterAsync<ReportExecution>(
                    ReportExecutionsCollectionName,
                    e => e.StartTime >= startTime);

                return executions.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report executions count for last 24 hours");
                throw;
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
    }
}
