using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Services.Analytics.Repositories
{
    /// <summary>
    /// Interface for report repository
    /// </summary>
    public interface IReportRepository
    {
        /// <summary>
        /// Creates a new report
        /// </summary>
        /// <param name="report">Report to create</param>
        /// <returns>The created report</returns>
        Task<Report> CreateAsync(Report report);

        /// <summary>
        /// Gets a report by ID
        /// </summary>
        /// <param name="id">Report ID</param>
        /// <returns>The report if found, null otherwise</returns>
        Task<Report> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets reports by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of reports for the account</returns>
        Task<IEnumerable<Report>> GetByAccountAsync(Guid accountId);

        /// <summary>
        /// Gets reports by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of reports created by the user</returns>
        Task<IEnumerable<Report>> GetByUserAsync(Guid userId);

        /// <summary>
        /// Gets reports by status
        /// </summary>
        /// <param name="status">Report status</param>
        /// <returns>List of reports with the specified status</returns>
        Task<IEnumerable<Report>> GetByStatusAsync(ReportStatus status);

        /// <summary>
        /// Gets reports due for execution
        /// </summary>
        /// <returns>List of reports due for execution</returns>
        Task<IEnumerable<Report>> GetDueForExecutionAsync();

        /// <summary>
        /// Updates a report
        /// </summary>
        /// <param name="report">Report to update</param>
        /// <returns>The updated report</returns>
        Task<Report> UpdateAsync(Report report);

        /// <summary>
        /// Deletes a report
        /// </summary>
        /// <param name="id">Report ID</param>
        /// <returns>True if the report was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Creates a report execution
        /// </summary>
        /// <param name="execution">Execution to create</param>
        /// <returns>The created execution</returns>
        Task<ReportExecution> CreateExecutionAsync(ReportExecution execution);

        /// <summary>
        /// Gets a report execution by ID
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>The execution if found, null otherwise</returns>
        Task<ReportExecution> GetExecutionByIdAsync(Guid id);

        /// <summary>
        /// Gets report executions by report ID
        /// </summary>
        /// <param name="reportId">Report ID</param>
        /// <returns>List of executions for the report</returns>
        Task<IEnumerable<ReportExecution>> GetExecutionsByReportAsync(Guid reportId);

        /// <summary>
        /// Gets report executions by status
        /// </summary>
        /// <param name="status">Execution status</param>
        /// <returns>List of executions with the specified status</returns>
        Task<IEnumerable<ReportExecution>> GetExecutionsByStatusAsync(ReportExecutionStatus status);

        /// <summary>
        /// Updates a report execution
        /// </summary>
        /// <param name="execution">Execution to update</param>
        /// <returns>The updated execution</returns>
        Task<ReportExecution> UpdateExecutionAsync(ReportExecution execution);

        /// <summary>
        /// Gets the count of reports
        /// </summary>
        /// <returns>Count of reports</returns>
        Task<int> GetCountAsync();

        /// <summary>
        /// Gets the count of report executions in the last 24 hours
        /// </summary>
        /// <returns>Count of report executions in the last 24 hours</returns>
        Task<int> GetExecutionsCountLast24HoursAsync();
    }
}
