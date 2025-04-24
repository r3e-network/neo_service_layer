using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Api.Controllers
{
    /// <summary>
    /// Controller for notification preferences operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationPreferencesController : ControllerBase
    {
        private readonly ILogger<NotificationPreferencesController> _logger;
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationPreferencesController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="notificationService">Notification service</param>
        public NotificationPreferencesController(ILogger<NotificationPreferencesController> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gets notification preferences for the authenticated user
        /// </summary>
        /// <returns>Notification preferences</returns>
        [HttpGet]
        public async Task<IActionResult> GetPreferences()
        {
            _logger.LogInformation("Getting notification preferences for authenticated user");

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                var preferences = await _notificationService.GetUserPreferencesAsync(accountId);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification preferences for authenticated user");
                return StatusCode(500, new { Message = "An error occurred while getting notification preferences" });
            }
        }

        /// <summary>
        /// Updates notification preferences for the authenticated user
        /// </summary>
        /// <param name="preferences">Updated preferences</param>
        /// <returns>Updated preferences</returns>
        [HttpPut]
        public async Task<IActionResult> UpdatePreferences([FromBody] UserNotificationPreferences preferences)
        {
            _logger.LogInformation("Updating notification preferences for authenticated user");

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Set account ID from authenticated user
                preferences.AccountId = accountId;

                // Update preferences
                var updatedPreferences = await _notificationService.UpdateUserPreferencesAsync(preferences);
                return Ok(updatedPreferences);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid preferences data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences for authenticated user");
                return StatusCode(500, new { Message = "An error occurred while updating notification preferences" });
            }
        }

        /// <summary>
        /// Registers a device token for push notifications
        /// </summary>
        /// <param name="deviceToken">Device token</param>
        /// <param name="platform">Device platform</param>
        /// <returns>Success status</returns>
        [HttpPost("device-tokens")]
        public async Task<IActionResult> RegisterDeviceToken([FromBody] string deviceToken, [FromQuery] string platform = "unknown")
        {
            _logger.LogInformation("Registering device token for authenticated user");

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Get preferences
                var preferences = await _notificationService.GetUserPreferencesAsync(accountId);

                // Add device token
                if (!preferences.DeviceTokens.Contains(deviceToken))
                {
                    preferences.DeviceTokens.Add(deviceToken);
                    
                    // Enable push channel if not already enabled
                    if (!preferences.EnabledChannels.Contains(NotificationChannel.Push))
                    {
                        preferences.EnabledChannels.Add(NotificationChannel.Push);
                    }

                    // Update preferences
                    await _notificationService.UpdateUserPreferencesAsync(preferences);
                }

                return Ok(new { Message = "Device token registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device token for authenticated user");
                return StatusCode(500, new { Message = "An error occurred while registering device token" });
            }
        }

        /// <summary>
        /// Unregisters a device token for push notifications
        /// </summary>
        /// <param name="deviceToken">Device token</param>
        /// <returns>Success status</returns>
        [HttpDelete("device-tokens/{deviceToken}")]
        public async Task<IActionResult> UnregisterDeviceToken(string deviceToken)
        {
            _logger.LogInformation("Unregistering device token for authenticated user");

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Get preferences
                var preferences = await _notificationService.GetUserPreferencesAsync(accountId);

                // Remove device token
                if (preferences.DeviceTokens.Contains(deviceToken))
                {
                    preferences.DeviceTokens.Remove(deviceToken);
                    
                    // Update preferences
                    await _notificationService.UpdateUserPreferencesAsync(preferences);
                }

                return Ok(new { Message = "Device token unregistered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering device token for authenticated user");
                return StatusCode(500, new { Message = "An error occurred while unregistering device token" });
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
