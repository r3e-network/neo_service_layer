using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Api.Controllers
{
    /// <summary>
    /// Controller for event monitoring operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventMonitoringController : ControllerBase
    {
        private readonly ILogger<EventMonitoringController> _logger;
        private readonly IEventMonitoringService _eventMonitoringService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventMonitoringController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="eventMonitoringService">Event monitoring service</param>
        public EventMonitoringController(ILogger<EventMonitoringController> logger, IEventMonitoringService eventMonitoringService)
        {
            _logger = logger;
            _eventMonitoringService = eventMonitoringService;
        }

        /// <summary>
        /// Gets the monitoring status
        /// </summary>
        /// <returns>Monitoring status</returns>
        [HttpGet("status")]
        public async Task<IActionResult> GetMonitoringStatus()
        {
            _logger.LogInformation("Getting monitoring status");

            try
            {
                var isMonitoring = await _eventMonitoringService.GetMonitoringStatusAsync();
                var currentBlockHeight = await _eventMonitoringService.GetCurrentBlockHeightAsync();
                var statistics = await _eventMonitoringService.GetStatisticsAsync();

                return Ok(new
                {
                    IsMonitoring = isMonitoring,
                    CurrentBlockHeight = currentBlockHeight,
                    Statistics = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monitoring status");
                return StatusCode(500, new { Message = "An error occurred while getting monitoring status" });
            }
        }

        /// <summary>
        /// Starts monitoring
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("start")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> StartMonitoring()
        {
            _logger.LogInformation("Starting monitoring");

            try
            {
                var success = await _eventMonitoringService.StartMonitoringAsync();
                if (success)
                {
                    return Ok(new { Message = "Monitoring started successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to start monitoring" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting monitoring");
                return StatusCode(500, new { Message = "An error occurred while starting monitoring" });
            }
        }

        /// <summary>
        /// Stops monitoring
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("stop")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> StopMonitoring()
        {
            _logger.LogInformation("Stopping monitoring");

            try
            {
                var success = await _eventMonitoringService.StopMonitoringAsync();
                if (success)
                {
                    return Ok(new { Message = "Monitoring stopped successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to stop monitoring" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping monitoring");
                return StatusCode(500, new { Message = "An error occurred while stopping monitoring" });
            }
        }

        /// <summary>
        /// Creates a new subscription
        /// </summary>
        /// <param name="subscription">Subscription to create</param>
        /// <returns>Created subscription</returns>
        [HttpPost("subscriptions")]
        public async Task<IActionResult> CreateSubscription([FromBody] EventSubscription subscription)
        {
            _logger.LogInformation("Creating subscription: {Name}", subscription.Name);

            try
            {
                // Set account ID from authenticated user
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                subscription.AccountId = accountId;

                // Create subscription
                var createdSubscription = await _eventMonitoringService.CreateSubscriptionAsync(subscription);
                return CreatedAtAction(nameof(GetSubscription), new { id = createdSubscription.Id }, createdSubscription);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid subscription data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription: {Name}", subscription.Name);
                return StatusCode(500, new { Message = "An error occurred while creating the subscription" });
            }
        }

        /// <summary>
        /// Gets a subscription by ID
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <returns>Subscription</returns>
        [HttpGet("subscriptions/{id}")]
        public async Task<IActionResult> GetSubscription(Guid id)
        {
            _logger.LogInformation("Getting subscription: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                var subscription = await _eventMonitoringService.GetSubscriptionAsync(id);
                if (subscription == null)
                {
                    return NotFound(new { Message = "Subscription not found" });
                }

                // Check if the subscription belongs to the authenticated user
                if (subscription.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while getting the subscription" });
            }
        }

        /// <summary>
        /// Gets subscriptions for the authenticated user
        /// </summary>
        /// <returns>List of subscriptions</returns>
        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetSubscriptions()
        {
            _logger.LogInformation("Getting subscriptions for authenticated user");

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                var subscriptions = await _eventMonitoringService.GetSubscriptionsByAccountAsync(accountId);
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions for authenticated user");
                return StatusCode(500, new { Message = "An error occurred while getting subscriptions" });
            }
        }

        /// <summary>
        /// Updates a subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <param name="subscription">Updated subscription</param>
        /// <returns>Updated subscription</returns>
        [HttpPut("subscriptions/{id}")]
        public async Task<IActionResult> UpdateSubscription(Guid id, [FromBody] EventSubscription subscription)
        {
            _logger.LogInformation("Updating subscription: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check if the subscription exists
                var existingSubscription = await _eventMonitoringService.GetSubscriptionAsync(id);
                if (existingSubscription == null)
                {
                    return NotFound(new { Message = "Subscription not found" });
                }

                // Check if the subscription belongs to the authenticated user
                if (existingSubscription.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Update subscription
                subscription.Id = id;
                subscription.AccountId = existingSubscription.AccountId;
                var updatedSubscription = await _eventMonitoringService.UpdateSubscriptionAsync(subscription);
                return Ok(updatedSubscription);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid subscription data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while updating the subscription" });
            }
        }

        /// <summary>
        /// Deletes a subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("subscriptions/{id}")]
        public async Task<IActionResult> DeleteSubscription(Guid id)
        {
            _logger.LogInformation("Deleting subscription: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check if the subscription exists
                var existingSubscription = await _eventMonitoringService.GetSubscriptionAsync(id);
                if (existingSubscription == null)
                {
                    return NotFound(new { Message = "Subscription not found" });
                }

                // Check if the subscription belongs to the authenticated user
                if (existingSubscription.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Delete subscription
                var success = await _eventMonitoringService.DeleteSubscriptionAsync(id);
                if (success)
                {
                    return Ok(new { Message = "Subscription deleted successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to delete subscription" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subscription: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while deleting the subscription" });
            }
        }

        /// <summary>
        /// Activates a subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <returns>Activated subscription</returns>
        [HttpPost("subscriptions/{id}/activate")]
        public async Task<IActionResult> ActivateSubscription(Guid id)
        {
            _logger.LogInformation("Activating subscription: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check if the subscription exists
                var existingSubscription = await _eventMonitoringService.GetSubscriptionAsync(id);
                if (existingSubscription == null)
                {
                    return NotFound(new { Message = "Subscription not found" });
                }

                // Check if the subscription belongs to the authenticated user
                if (existingSubscription.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Activate subscription
                var activatedSubscription = await _eventMonitoringService.ActivateSubscriptionAsync(id);
                return Ok(activatedSubscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating subscription: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while activating the subscription" });
            }
        }

        /// <summary>
        /// Pauses a subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <returns>Paused subscription</returns>
        [HttpPost("subscriptions/{id}/pause")]
        public async Task<IActionResult> PauseSubscription(Guid id)
        {
            _logger.LogInformation("Pausing subscription: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check if the subscription exists
                var existingSubscription = await _eventMonitoringService.GetSubscriptionAsync(id);
                if (existingSubscription == null)
                {
                    return NotFound(new { Message = "Subscription not found" });
                }

                // Check if the subscription belongs to the authenticated user
                if (existingSubscription.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Pause subscription
                var pausedSubscription = await _eventMonitoringService.PauseSubscriptionAsync(id);
                return Ok(pausedSubscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing subscription: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while pausing the subscription" });
            }
        }

        /// <summary>
        /// Gets event logs for a subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs</returns>
        [HttpGet("subscriptions/{id}/logs")]
        public async Task<IActionResult> GetEventLogs(Guid id, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            _logger.LogInformation("Getting event logs for subscription: {Id}, limit: {Limit}, offset: {Offset}", id, limit, offset);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check if the subscription exists
                var existingSubscription = await _eventMonitoringService.GetSubscriptionAsync(id);
                if (existingSubscription == null)
                {
                    return NotFound(new { Message = "Subscription not found" });
                }

                // Check if the subscription belongs to the authenticated user
                if (existingSubscription.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Get event logs
                var eventLogs = await _eventMonitoringService.GetEventLogsBySubscriptionAsync(id, limit, offset);
                return Ok(eventLogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for subscription: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while getting event logs" });
            }
        }

        /// <summary>
        /// Gets event logs for the authenticated user
        /// </summary>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs</returns>
        [HttpGet("logs")]
        public async Task<IActionResult> GetEventLogs([FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            _logger.LogInformation("Getting event logs for authenticated user, limit: {Limit}, offset: {Offset}", limit, offset);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Get event logs
                var eventLogs = await _eventMonitoringService.GetEventLogsByAccountAsync(accountId, limit, offset);
                return Ok(eventLogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for authenticated user");
                return StatusCode(500, new { Message = "An error occurred while getting event logs" });
            }
        }

        /// <summary>
        /// Retries a notification for an event log
        /// </summary>
        /// <param name="id">Event log ID</param>
        /// <returns>Success status</returns>
        [HttpPost("logs/{id}/retry")]
        public async Task<IActionResult> RetryNotification(Guid id)
        {
            _logger.LogInformation("Retrying notification for event log: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check if the event log exists
                var eventLogs = await _eventMonitoringService.GetEventLogsByAccountAsync(accountId, 1, 0);
                var eventLog = eventLogs.FirstOrDefault(l => l.Id == id);
                if (eventLog == null && !User.IsInRole("Admin"))
                {
                    // If not found in user's logs, check if admin
                    return NotFound(new { Message = "Event log not found" });
                }

                // Retry notification
                var success = await _eventMonitoringService.RetryNotificationAsync(id);
                if (success)
                {
                    return Ok(new { Message = "Notification retried successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to retry notification" });
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying notification for event log: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while retrying the notification" });
            }
        }

        /// <summary>
        /// Cancels a notification for an event log
        /// </summary>
        /// <param name="id">Event log ID</param>
        /// <returns>Success status</returns>
        [HttpPost("logs/{id}/cancel")]
        public async Task<IActionResult> CancelNotification(Guid id)
        {
            _logger.LogInformation("Cancelling notification for event log: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check if the event log exists
                var eventLogs = await _eventMonitoringService.GetEventLogsByAccountAsync(accountId, 1, 0);
                var eventLog = eventLogs.FirstOrDefault(l => l.Id == id);
                if (eventLog == null && !User.IsInRole("Admin"))
                {
                    // If not found in user's logs, check if admin
                    return NotFound(new { Message = "Event log not found" });
                }

                // Cancel notification
                var success = await _eventMonitoringService.CancelNotificationAsync(id);
                if (success)
                {
                    return Ok(new { Message = "Notification cancelled successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to cancel notification" });
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling notification for event log: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while cancelling the notification" });
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
    }
}
