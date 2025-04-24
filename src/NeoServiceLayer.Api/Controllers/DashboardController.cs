using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Api.Controllers
{
    /// <summary>
    /// Controller for dashboard operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IAnalyticsService _analyticsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="analyticsService">Analytics service</param>
        public DashboardController(ILogger<DashboardController> logger, IAnalyticsService analyticsService)
        {
            _logger = logger;
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Gets dashboards for the authenticated user
        /// </summary>
        /// <returns>List of dashboards</returns>
        [HttpGet]
        public async Task<IActionResult> GetDashboards()
        {
            _logger.LogInformation("Getting dashboards for authenticated user");

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                var dashboards = await _analyticsService.GetDashboardsByAccountAsync(accountId);
                return Ok(dashboards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboards for authenticated user");
                return StatusCode(500, new { Message = "An error occurred while getting dashboards" });
            }
        }

        /// <summary>
        /// Gets a dashboard by ID
        /// </summary>
        /// <param name="id">Dashboard ID</param>
        /// <returns>Dashboard</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDashboard(Guid id)
        {
            _logger.LogInformation("Getting dashboard: {Id}", id);

            try
            {
                var dashboard = await _analyticsService.GetDashboardAsync(id);
                if (dashboard == null)
                {
                    return NotFound(new { Message = "Dashboard not found" });
                }

                // Check if the dashboard belongs to the authenticated user
                var accountId = GetAccountId();
                if (dashboard.AccountId != accountId && !dashboard.IsPublic && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while getting the dashboard" });
            }
        }

        /// <summary>
        /// Creates a dashboard
        /// </summary>
        /// <param name="dashboard">Dashboard to create</param>
        /// <returns>Created dashboard</returns>
        [HttpPost]
        public async Task<IActionResult> CreateDashboard([FromBody] Dashboard dashboard)
        {
            _logger.LogInformation("Creating dashboard: {Name}", dashboard.Name);

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
                dashboard.AccountId = accountId;
                dashboard.CreatedBy = userId;

                var createdDashboard = await _analyticsService.CreateDashboardAsync(dashboard);
                return CreatedAtAction(nameof(GetDashboard), new { id = createdDashboard.Id }, createdDashboard);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid dashboard data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dashboard: {Name}", dashboard.Name);
                return StatusCode(500, new { Message = "An error occurred while creating the dashboard" });
            }
        }

        /// <summary>
        /// Updates a dashboard
        /// </summary>
        /// <param name="id">Dashboard ID</param>
        /// <param name="dashboard">Updated dashboard</param>
        /// <returns>Updated dashboard</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDashboard(Guid id, [FromBody] Dashboard dashboard)
        {
            _logger.LogInformation("Updating dashboard: {Id}", id);

            try
            {
                // Check if the dashboard exists
                var existingDashboard = await _analyticsService.GetDashboardAsync(id);
                if (existingDashboard == null)
                {
                    return NotFound(new { Message = "Dashboard not found" });
                }

                // Check if the dashboard belongs to the authenticated user
                var accountId = GetAccountId();
                if (existingDashboard.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Update dashboard
                dashboard.Id = id;
                dashboard.AccountId = existingDashboard.AccountId;
                dashboard.CreatedBy = existingDashboard.CreatedBy;
                dashboard.CreatedAt = existingDashboard.CreatedAt;

                var updatedDashboard = await _analyticsService.UpdateDashboardAsync(dashboard);
                return Ok(updatedDashboard);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid dashboard data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dashboard: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while updating the dashboard" });
            }
        }

        /// <summary>
        /// Deletes a dashboard
        /// </summary>
        /// <param name="id">Dashboard ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDashboard(Guid id)
        {
            _logger.LogInformation("Deleting dashboard: {Id}", id);

            try
            {
                // Check if the dashboard exists
                var existingDashboard = await _analyticsService.GetDashboardAsync(id);
                if (existingDashboard == null)
                {
                    return NotFound(new { Message = "Dashboard not found" });
                }

                // Check if the dashboard belongs to the authenticated user
                var accountId = GetAccountId();
                if (existingDashboard.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Delete dashboard
                var success = await _analyticsService.DeleteDashboardAsync(id);
                if (success)
                {
                    return Ok(new { Message = "Dashboard deleted successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to delete dashboard" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dashboard: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while deleting the dashboard" });
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
