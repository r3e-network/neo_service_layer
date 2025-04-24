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
    /// Controller for notification template operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationTemplateController : ControllerBase
    {
        private readonly ILogger<NotificationTemplateController> _logger;
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTemplateController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="notificationService">Notification service</param>
        public NotificationTemplateController(ILogger<NotificationTemplateController> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gets templates for the authenticated user
        /// </summary>
        /// <returns>List of templates</returns>
        [HttpGet]
        public async Task<IActionResult> GetTemplates()
        {
            _logger.LogInformation("Getting templates for authenticated user");

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                var templates = await _notificationService.GetTemplatesByAccountAsync(accountId);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting templates for authenticated user");
                return StatusCode(500, new { Message = "An error occurred while getting templates" });
            }
        }

        /// <summary>
        /// Gets a template by ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>Template</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTemplate(Guid id)
        {
            _logger.LogInformation("Getting template: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                var template = await _notificationService.GetTemplateAsync(id);
                if (template == null)
                {
                    return NotFound(new { Message = "Template not found" });
                }

                // Check if the template belongs to the authenticated user
                if (template.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while getting the template" });
            }
        }

        /// <summary>
        /// Creates a template
        /// </summary>
        /// <param name="template">Template to create</param>
        /// <returns>Created template</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTemplate([FromBody] NotificationTemplate template)
        {
            _logger.LogInformation("Creating template: {Name}", template.Name);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Set account ID from authenticated user
                template.AccountId = accountId;

                // Create template
                var createdTemplate = await _notificationService.CreateTemplateAsync(template);
                return CreatedAtAction(nameof(GetTemplate), new { id = createdTemplate.Id }, createdTemplate);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid template data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template: {Name}", template.Name);
                return StatusCode(500, new { Message = "An error occurred while creating the template" });
            }
        }

        /// <summary>
        /// Updates a template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="template">Updated template</param>
        /// <returns>Updated template</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] NotificationTemplate template)
        {
            _logger.LogInformation("Updating template: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check if the template exists
                var existingTemplate = await _notificationService.GetTemplateAsync(id);
                if (existingTemplate == null)
                {
                    return NotFound(new { Message = "Template not found" });
                }

                // Check if the template belongs to the authenticated user
                if (existingTemplate.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Update template
                template.Id = id;
                template.AccountId = existingTemplate.AccountId;
                var updatedTemplate = await _notificationService.UpdateTemplateAsync(template);
                return Ok(updatedTemplate);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid template data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while updating the template" });
            }
        }

        /// <summary>
        /// Deletes a template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            _logger.LogInformation("Deleting template: {Id}", id);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check if the template exists
                var template = await _notificationService.GetTemplateAsync(id);
                if (template == null)
                {
                    return NotFound(new { Message = "Template not found" });
                }

                // Check if the template belongs to the authenticated user
                if (template.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Delete template
                var success = await _notificationService.DeleteTemplateAsync(id);
                if (success)
                {
                    return Ok(new { Message = "Template deleted successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to delete template" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while deleting the template" });
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
