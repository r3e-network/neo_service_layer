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
    /// Controller for alert operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlertController : ControllerBase
    {
        private readonly ILogger<AlertController> _logger;
        private readonly IAnalyticsService _analyticsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="analyticsService">Analytics service</param>
        public AlertController(ILogger<AlertController> logger, IAnalyticsService analyticsService)
        {
            _logger = logger;
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Gets alerts for the authenticated user
        /// </summary>
        /// <returns>List of alerts</returns>
        [HttpGet]
        public async Task<IActionResult> GetAlerts()
        {
            _logger.LogInformation("Getting alerts for authenticated user");

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                var alerts = await _analyticsService.GetAlertsByAccountAsync(accountId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts for authenticated user");
                return StatusCode(500, new { Message = "An error occurred while getting alerts" });
            }
        }

        /// <summary>
        /// Gets an alert by ID
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <returns>Alert</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlert(Guid id)
        {
            _logger.LogInformation("Getting alert: {Id}", id);

            try
            {
                var alert = await _analyticsService.GetAlertAsync(id);
                if (alert == null)
                {
                    return NotFound(new { Message = "Alert not found" });
                }

                // Check if the alert belongs to the authenticated user
                var accountId = GetAccountId();
                if (alert.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while getting the alert" });
            }
        }

        /// <summary>
        /// Creates an alert
        /// </summary>
        /// <param name="alert">Alert to create</param>
        /// <returns>Created alert</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAlert([FromBody] Alert alert)
        {
            _logger.LogInformation("Creating alert: {Name}", alert.Name);

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
                alert.AccountId = accountId;
                alert.CreatedBy = userId;

                var createdAlert = await _analyticsService.CreateAlertAsync(alert);
                return CreatedAtAction(nameof(GetAlert), new { id = createdAlert.Id }, createdAlert);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid alert data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert: {Name}", alert.Name);
                return StatusCode(500, new { Message = "An error occurred while creating the alert" });
            }
        }

        /// <summary>
        /// Updates an alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <param name="alert">Updated alert</param>
        /// <returns>Updated alert</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlert(Guid id, [FromBody] Alert alert)
        {
            _logger.LogInformation("Updating alert: {Id}", id);

            try
            {
                // Check if the alert exists
                var existingAlert = await _analyticsService.GetAlertAsync(id);
                if (existingAlert == null)
                {
                    return NotFound(new { Message = "Alert not found" });
                }

                // Check if the alert belongs to the authenticated user
                var accountId = GetAccountId();
                if (existingAlert.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Update alert
                alert.Id = id;
                alert.AccountId = existingAlert.AccountId;
                alert.CreatedBy = existingAlert.CreatedBy;
                alert.CreatedAt = existingAlert.CreatedAt;

                var updatedAlert = await _analyticsService.UpdateAlertAsync(alert);
                return Ok(updatedAlert);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid alert data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating alert: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while updating the alert" });
            }
        }

        /// <summary>
        /// Deletes an alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlert(Guid id)
        {
            _logger.LogInformation("Deleting alert: {Id}", id);

            try
            {
                // Check if the alert exists
                var existingAlert = await _analyticsService.GetAlertAsync(id);
                if (existingAlert == null)
                {
                    return NotFound(new { Message = "Alert not found" });
                }

                // Check if the alert belongs to the authenticated user
                var accountId = GetAccountId();
                if (existingAlert.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Delete alert
                var success = await _analyticsService.DeleteAlertAsync(id);
                if (success)
                {
                    return Ok(new { Message = "Alert deleted successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to delete alert" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting alert: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while deleting the alert" });
            }
        }

        /// <summary>
        /// Silences an alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <param name="duration">Silence duration in seconds</param>
        /// <returns>Silenced alert</returns>
        [HttpPost("{id}/silence")]
        public async Task<IActionResult> SilenceAlert(Guid id, [FromQuery] int duration = 3600)
        {
            _logger.LogInformation("Silencing alert: {Id} for {Duration} seconds", id, duration);

            try
            {
                // Check if the alert exists
                var existingAlert = await _analyticsService.GetAlertAsync(id);
                if (existingAlert == null)
                {
                    return NotFound(new { Message = "Alert not found" });
                }

                // Check if the alert belongs to the authenticated user
                var accountId = GetAccountId();
                if (existingAlert.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Silence alert
                var silencedAlert = await _analyticsService.SilenceAlertAsync(id, duration);
                return Ok(silencedAlert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error silencing alert: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while silencing the alert" });
            }
        }

        /// <summary>
        /// Unsilences an alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <returns>Unsilenced alert</returns>
        [HttpPost("{id}/unsilence")]
        public async Task<IActionResult> UnsilenceAlert(Guid id)
        {
            _logger.LogInformation("Unsilencing alert: {Id}", id);

            try
            {
                // Check if the alert exists
                var existingAlert = await _analyticsService.GetAlertAsync(id);
                if (existingAlert == null)
                {
                    return NotFound(new { Message = "Alert not found" });
                }

                // Check if the alert belongs to the authenticated user
                var accountId = GetAccountId();
                if (existingAlert.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Unsilence alert
                var unsilencedAlert = await _analyticsService.UnsilenceAlertAsync(id);
                return Ok(unsilencedAlert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsilencing alert: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while unsilencing the alert" });
            }
        }

        /// <summary>
        /// Gets alert events
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>List of alert events</returns>
        [HttpGet("{id}/events")]
        public async Task<IActionResult> GetAlertEvents(Guid id, [FromQuery] DateTime? startTime = null, [FromQuery] DateTime? endTime = null)
        {
            _logger.LogInformation("Getting events for alert: {Id}", id);

            try
            {
                // Check if the alert exists
                var existingAlert = await _analyticsService.GetAlertAsync(id);
                if (existingAlert == null)
                {
                    return NotFound(new { Message = "Alert not found" });
                }

                // Check if the alert belongs to the authenticated user
                var accountId = GetAccountId();
                if (existingAlert.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Set default time range if not provided
                if (!startTime.HasValue)
                {
                    startTime = DateTime.UtcNow.AddDays(-7);
                }

                if (!endTime.HasValue)
                {
                    endTime = DateTime.UtcNow;
                }

                // Get events
                var events = await _analyticsService.GetAlertEventsAsync(id, startTime.Value, endTime.Value);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for alert: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while getting alert events" });
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
