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
    /// Controller for notification operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="notificationService">Notification service</param>
        public NotificationController(ILogger<NotificationController> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gets notifications for the authenticated user
        /// </summary>
        /// <param name="limit">Maximum number of notifications to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of notifications</returns>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            _logger.LogInformation("Getting notifications for authenticated user, limit: {Limit}, offset: {Offset}", limit, offset);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                var notifications = await _notificationService.GetNotificationsByAccountAsync(accountId, limit, offset);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for authenticated user");
                return StatusCode(500, new { Message = "An error occurred while getting notifications" });
            }
        }

        /// <summary>
        /// Gets a notification by ID
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Notification</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification(Guid id)
        {
            _logger.LogInformation("Getting notification: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                var notification = await _notificationService.GetNotificationAsync(id);
                if (notification == null)
                {
                    return NotFound(new { Message = "Notification not found" });
                }

                // Check if the notification belongs to the authenticated user
                if (notification.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while getting the notification" });
            }
        }

        /// <summary>
        /// Sends a notification
        /// </summary>
        /// <param name="notification">Notification to send</param>
        /// <returns>Sent notification</returns>
        [HttpPost]
        public async Task<IActionResult> SendNotification([FromBody] Notification notification)
        {
            _logger.LogInformation("Sending notification: {Type}", notification.Type);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Set account ID from authenticated user
                notification.AccountId = accountId;

                // Send notification
                var sentNotification = await _notificationService.SendNotificationAsync(notification);
                return CreatedAtAction(nameof(GetNotification), new { id = sentNotification.Id }, sentNotification);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid notification data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification: {Type}", notification.Type);
                return StatusCode(500, new { Message = "An error occurred while sending the notification" });
            }
        }

        /// <summary>
        /// Sends a notification using a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="templateData">Template data</param>
        /// <returns>Sent notification</returns>
        [HttpPost("templates/{templateId}")]
        public async Task<IActionResult> SendTemplateNotification(Guid templateId, [FromBody] Dictionary<string, object> templateData)
        {
            _logger.LogInformation("Sending template notification: {TemplateId}", templateId);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Send notification
                var notification = await _notificationService.SendTemplateNotificationAsync(accountId, templateId, templateData);
                return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid template data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending template notification: {TemplateId}", templateId);
                return StatusCode(500, new { Message = "An error occurred while sending the template notification" });
            }
        }

        /// <summary>
        /// Schedules a notification
        /// </summary>
        /// <param name="notification">Notification to schedule</param>
        /// <param name="scheduledTime">Scheduled time</param>
        /// <returns>Scheduled notification</returns>
        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleNotification([FromBody] Notification notification, [FromQuery] DateTime scheduledTime)
        {
            _logger.LogInformation("Scheduling notification: {Type}, scheduled time: {ScheduledTime}", notification.Type, scheduledTime);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Set account ID from authenticated user
                notification.AccountId = accountId;

                // Schedule notification
                var scheduledNotification = await _notificationService.ScheduleNotificationAsync(notification, scheduledTime);
                return CreatedAtAction(nameof(GetNotification), new { id = scheduledNotification.Id }, scheduledNotification);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid notification data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling notification: {Type}", notification.Type);
                return StatusCode(500, new { Message = "An error occurred while scheduling the notification" });
            }
        }

        /// <summary>
        /// Cancels a notification
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelNotification(Guid id)
        {
            _logger.LogInformation("Cancelling notification: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check if the notification exists
                var notification = await _notificationService.GetNotificationAsync(id);
                if (notification == null)
                {
                    return NotFound(new { Message = "Notification not found" });
                }

                // Check if the notification belongs to the authenticated user
                if (notification.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Cancel notification
                var success = await _notificationService.CancelNotificationAsync(id);
                if (success)
                {
                    return Ok(new { Message = "Notification cancelled successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to cancel notification" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling notification: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while cancelling the notification" });
            }
        }

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            _logger.LogInformation("Marking notification as read: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check if the notification exists
                var notification = await _notificationService.GetNotificationAsync(id);
                if (notification == null)
                {
                    return NotFound(new { Message = "Notification not found" });
                }

                // Check if the notification belongs to the authenticated user
                if (notification.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Mark as read
                var success = await _notificationService.MarkNotificationAsReadAsync(id);
                if (success)
                {
                    return Ok(new { Message = "Notification marked as read successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to mark notification as read" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while marking the notification as read" });
            }
        }

        /// <summary>
        /// Marks all notifications as read
        /// </summary>
        /// <returns>Number of notifications marked as read</returns>
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            _logger.LogInformation("Marking all notifications as read");

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Mark all as read
                var count = await _notificationService.MarkAllNotificationsAsReadAsync(accountId);
                return Ok(new { Count = count, Message = $"{count} notifications marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, new { Message = "An error occurred while marking all notifications as read" });
            }
        }

        /// <summary>
        /// Deletes a notification
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            _logger.LogInformation("Deleting notification: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check if the notification exists
                var notification = await _notificationService.GetNotificationAsync(id);
                if (notification == null)
                {
                    return NotFound(new { Message = "Notification not found" });
                }

                // Check if the notification belongs to the authenticated user
                if (notification.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Delete notification
                var success = await _notificationService.DeleteNotificationAsync(id);
                if (success)
                {
                    return Ok(new { Message = "Notification deleted successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to delete notification" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while deleting the notification" });
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
