using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Api.Controllers
{
    /// <summary>
    /// Controller for report operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;
        private readonly IAnalyticsService _analyticsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="analyticsService">Analytics service</param>
        public ReportController(ILogger<ReportController> logger, IAnalyticsService analyticsService)
        {
            _logger = logger;
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Gets reports for the authenticated user
        /// </summary>
        /// <returns>List of reports</returns>
        [HttpGet]
        public async Task<IActionResult> GetReports()
        {
            _logger.LogInformation("Getting reports for authenticated user");

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                var reports = await _analyticsService.GetReportsByAccountAsync(accountId);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports for authenticated user");
                return StatusCode(500, new { Message = "An error occurred while getting reports" });
            }
        }

        /// <summary>
        /// Gets a report by ID
        /// </summary>
        /// <param name="id">Report ID</param>
        /// <returns>Report</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReport(Guid id)
        {
            _logger.LogInformation("Getting report: {Id}", id);

            try
            {
                var report = await _analyticsService.GetReportAsync(id);
                if (report == null)
                {
                    return NotFound(new { Message = "Report not found" });
                }

                // Check if the report belongs to the authenticated user
                var accountId = GetAccountId();
                if (report.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while getting the report" });
            }
        }

        /// <summary>
        /// Creates a report
        /// </summary>
        /// <param name="report">Report to create</param>
        /// <returns>Created report</returns>
        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] Report report)
        {
            _logger.LogInformation("Creating report: {Name}", report.Name);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                var userId = GetUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid user ID" });
                }

                // Set account ID and user ID
                report.AccountId = accountId;
                report.CreatedBy = userId;

                var createdReport = await _analyticsService.CreateReportAsync(report);
                return CreatedAtAction(nameof(GetReport), new { id = createdReport.Id }, createdReport);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid report data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report: {Name}", report.Name);
                return StatusCode(500, new { Message = "An error occurred while creating the report" });
            }
        }

        /// <summary>
        /// Updates a report
        /// </summary>
        /// <param name="id">Report ID</param>
        /// <param name="report">Updated report</param>
        /// <returns>Updated report</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReport(Guid id, [FromBody] Report report)
        {
            _logger.LogInformation("Updating report: {Id}", id);

            try
            {
                // Check if the report exists
                var existingReport = await _analyticsService.GetReportAsync(id);
                if (existingReport == null)
                {
                    return NotFound(new { Message = "Report not found" });
                }

                // Check if the report belongs to the authenticated user
                var accountId = GetAccountId();
                if (existingReport.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Update report
                report.Id = id;
                report.AccountId = existingReport.AccountId;
                report.CreatedBy = existingReport.CreatedBy;
                report.CreatedAt = existingReport.CreatedAt;

                var updatedReport = await _analyticsService.UpdateReportAsync(report);
                return Ok(updatedReport);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid report data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while updating the report" });
            }
        }

        /// <summary>
        /// Deletes a report
        /// </summary>
        /// <param name="id">Report ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(Guid id)
        {
            _logger.LogInformation("Deleting report: {Id}", id);

            try
            {
                // Check if the report exists
                var existingReport = await _analyticsService.GetReportAsync(id);
                if (existingReport == null)
                {
                    return NotFound(new { Message = "Report not found" });
                }

                // Check if the report belongs to the authenticated user
                var accountId = GetAccountId();
                if (existingReport.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Delete report
                var success = await _analyticsService.DeleteReportAsync(id);
                if (success)
                {
                    return Ok(new { Message = "Report deleted successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to delete report" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while deleting the report" });
            }
        }

        /// <summary>
        /// Executes a report
        /// </summary>
        /// <param name="id">Report ID</param>
        /// <param name="parameters">Report parameters</param>
        /// <returns>Report execution</returns>
        [HttpPost("{id}/execute")]
        public async Task<IActionResult> ExecuteReport(Guid id, [FromBody] Dictionary<string, object> parameters = null)
        {
            _logger.LogInformation("Executing report: {Id}", id);

            try
            {
                // Check if the report exists
                var existingReport = await _analyticsService.GetReportAsync(id);
                if (existingReport == null)
                {
                    return NotFound(new { Message = "Report not found" });
                }

                // Check if the report belongs to the authenticated user
                var accountId = GetAccountId();
                if (existingReport.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Execute report
                var execution = await _analyticsService.ExecuteReportAsync(id, parameters);
                return Ok(execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing report: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while executing the report" });
            }
        }

        /// <summary>
        /// Gets report executions
        /// </summary>
        /// <param name="id">Report ID</param>
        /// <returns>List of report executions</returns>
        [HttpGet("{id}/executions")]
        public async Task<IActionResult> GetReportExecutions(Guid id)
        {
            _logger.LogInformation("Getting executions for report: {Id}", id);

            try
            {
                // Check if the report exists
                var existingReport = await _analyticsService.GetReportAsync(id);
                if (existingReport == null)
                {
                    return NotFound(new { Message = "Report not found" });
                }

                // Check if the report belongs to the authenticated user
                var accountId = GetAccountId();
                if (existingReport.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Get executions
                var executions = await _analyticsService.GetReportExecutionsByReportAsync(id);
                return Ok(executions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting executions for report: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while getting report executions" });
            }
        }

        /// <summary>
        /// Gets a report execution
        /// </summary>
        /// <param name="id">Report ID</param>
        /// <param name="executionId">Execution ID</param>
        /// <returns>Report execution</returns>
        [HttpGet("{id}/executions/{executionId}")]
        public async Task<IActionResult> GetReportExecution(Guid id, Guid executionId)
        {
            _logger.LogInformation("Getting execution: {ExecutionId} for report: {Id}", executionId, id);

            try
            {
                // Check if the report exists
                var existingReport = await _analyticsService.GetReportAsync(id);
                if (existingReport == null)
                {
                    return NotFound(new { Message = "Report not found" });
                }

                // Check if the report belongs to the authenticated user
                var accountId = GetAccountId();
                if (existingReport.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Get execution
                var execution = await _analyticsService.GetReportExecutionAsync(executionId);
                if (execution == null || execution.ReportId != id)
                {
                    return NotFound(new { Message = "Execution not found" });
                }

                return Ok(execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution: {ExecutionId} for report: {Id}", executionId, id);
                return StatusCode(500, new { Message = "An error occurred while getting the report execution" });
            }
        }

        /// <summary>
        /// Gets the account ID from the authenticated user
        /// </summary>
        /// <returns>Account ID</returns>
        private Guid GetAccountId()
        {
            var accountIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !Guid.TryParse(accountIdClaim, out var accountId))
            {
                return Guid.Empty;
            }

            return accountId;
        }

        /// <summary>
        /// Gets the user ID from the authenticated user
        /// </summary>
        /// <returns>User ID</returns>
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Guid.Empty;
            }

            return userId;
        }
    }
}
