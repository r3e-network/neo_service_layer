using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Api.Models;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Api.Controllers
{
    /// <summary>
    /// Controller for secrets management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SecretsController : ControllerBase
    {
        private readonly ILogger<SecretsController> _logger;
        private readonly ISecretsService _secretsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretsController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="secretsService">Secrets service</param>
        public SecretsController(ILogger<SecretsController> logger, ISecretsService secretsService)
        {
            _logger = logger;
            _secretsService = secretsService;
        }

        /// <summary>
        /// Creates a new secret
        /// </summary>
        /// <param name="request">Secret creation request</param>
        /// <returns>The created secret</returns>
        [HttpPost]
        public async Task<IActionResult> CreateSecret([FromBody] CreateSecretRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Creating secret: {Name} for user: {UserId}", request.Name, userId);

            try
            {
                var secret = await _secretsService.CreateSecretAsync(
                    request.Name,
                    request.Value,
                    request.Description,
                    accountId,
                    request.AllowedFunctionIds,
                    request.ExpiresAt);

                return Ok(new
                {
                    Id = secret.Id,
                    Name = secret.Name,
                    Description = secret.Description,
                    Version = secret.Version,
                    AllowedFunctionIds = secret.AllowedFunctionIds,
                    ExpiresAt = secret.ExpiresAt,
                    CreatedAt = secret.CreatedAt
                });
            }
            catch (SecretsException ex)
            {
                _logger.LogError(ex, "Error creating secret: {Name} for user: {UserId}", request.Name, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating secret: {Name} for user: {UserId}", request.Name, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets all secrets for the current user
        /// </summary>
        /// <returns>List of secrets</returns>
        [HttpGet]
        public async Task<IActionResult> GetSecrets()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Getting secrets for user: {UserId}", userId);

            try
            {
                var secrets = await _secretsService.GetByAccountIdAsync(accountId);
                var result = new List<object>();

                foreach (var secret in secrets)
                {
                    result.Add(new
                    {
                        Id = secret.Id,
                        Name = secret.Name,
                        Description = secret.Description,
                        Version = secret.Version,
                        AllowedFunctionIds = secret.AllowedFunctionIds,
                        ExpiresAt = secret.ExpiresAt,
                        CreatedAt = secret.CreatedAt,
                        UpdatedAt = secret.UpdatedAt
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting secrets for user: {UserId}", userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets a secret by ID
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <returns>The secret</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSecretById(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Getting secret by ID: {SecretId} for user: {UserId}", id, userId);

            try
            {
                var secret = await _secretsService.GetByIdAsync(id);
                if (secret == null)
                {
                    return NotFound(new { Message = "Secret not found" });
                }

                // Check if the secret belongs to the current user
                if (secret.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(new
                {
                    Id = secret.Id,
                    Name = secret.Name,
                    Description = secret.Description,
                    Version = secret.Version,
                    AllowedFunctionIds = secret.AllowedFunctionIds,
                    ExpiresAt = secret.ExpiresAt,
                    CreatedAt = secret.CreatedAt,
                    UpdatedAt = secret.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting secret by ID: {SecretId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Updates a secret's value
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <param name="request">Update request</param>
        /// <returns>The updated secret</returns>
        [HttpPut("{id}/value")]
        public async Task<IActionResult> UpdateSecretValue(Guid id, [FromBody] UpdateSecretValueRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Updating value for secret: {SecretId} for user: {UserId}", id, userId);

            try
            {
                var secret = await _secretsService.GetByIdAsync(id);
                if (secret == null)
                {
                    return NotFound(new { Message = "Secret not found" });
                }

                // Check if the secret belongs to the current user
                if (secret.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var updatedSecret = await _secretsService.UpdateValueAsync(id, request.Value);
                return Ok(new
                {
                    Id = updatedSecret.Id,
                    Name = updatedSecret.Name,
                    Description = updatedSecret.Description,
                    Version = updatedSecret.Version,
                    AllowedFunctionIds = updatedSecret.AllowedFunctionIds,
                    ExpiresAt = updatedSecret.ExpiresAt,
                    UpdatedAt = updatedSecret.UpdatedAt
                });
            }
            catch (SecretsException ex)
            {
                _logger.LogError(ex, "Error updating value for secret: {SecretId} for user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating value for secret: {SecretId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Updates a secret's allowed functions
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <param name="request">Update request</param>
        /// <returns>The updated secret</returns>
        [HttpPut("{id}/functions")]
        public async Task<IActionResult> UpdateAllowedFunctions(Guid id, [FromBody] UpdateAllowedFunctionsRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Updating allowed functions for secret: {SecretId} for user: {UserId}", id, userId);

            try
            {
                var secret = await _secretsService.GetByIdAsync(id);
                if (secret == null)
                {
                    return NotFound(new { Message = "Secret not found" });
                }

                // Check if the secret belongs to the current user
                if (secret.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var updatedSecret = await _secretsService.UpdateAllowedFunctionsAsync(id, request.AllowedFunctionIds);
                return Ok(new
                {
                    Id = updatedSecret.Id,
                    Name = updatedSecret.Name,
                    Description = updatedSecret.Description,
                    Version = updatedSecret.Version,
                    AllowedFunctionIds = updatedSecret.AllowedFunctionIds,
                    ExpiresAt = updatedSecret.ExpiresAt,
                    UpdatedAt = updatedSecret.UpdatedAt
                });
            }
            catch (SecretsException ex)
            {
                _logger.LogError(ex, "Error updating allowed functions for secret: {SecretId} for user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating allowed functions for secret: {SecretId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Rotates a secret's value
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <param name="request">Rotation request</param>
        /// <returns>The rotated secret</returns>
        [HttpPost("{id}/rotate")]
        public async Task<IActionResult> RotateSecret(Guid id, [FromBody] RotateSecretRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Rotating secret: {SecretId} for user: {UserId}", id, userId);

            try
            {
                var secret = await _secretsService.GetByIdAsync(id);
                if (secret == null)
                {
                    return NotFound(new { Message = "Secret not found" });
                }

                // Check if the secret belongs to the current user
                if (secret.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var rotatedSecret = await _secretsService.RotateSecretAsync(id, request.NewValue);
                return Ok(new
                {
                    Id = rotatedSecret.Id,
                    Name = rotatedSecret.Name,
                    Description = rotatedSecret.Description,
                    Version = rotatedSecret.Version,
                    AllowedFunctionIds = rotatedSecret.AllowedFunctionIds,
                    ExpiresAt = rotatedSecret.ExpiresAt,
                    UpdatedAt = rotatedSecret.UpdatedAt
                });
            }
            catch (SecretsException ex)
            {
                _logger.LogError(ex, "Error rotating secret: {SecretId} for user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error rotating secret: {SecretId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Deletes a secret
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSecret(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Deleting secret: {SecretId} for user: {UserId}", id, userId);

            try
            {
                var secret = await _secretsService.GetByIdAsync(id);
                if (secret == null)
                {
                    return NotFound(new { Message = "Secret not found" });
                }

                // Check if the secret belongs to the current user
                if (secret.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var success = await _secretsService.DeleteAsync(id);
                if (!success)
                {
                    return BadRequest(new { Message = "Failed to delete secret" });
                }

                return Ok(new { Message = "Secret deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting secret: {SecretId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets secrets accessible by a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of secrets</returns>
        [HttpGet("function/{functionId}")]
        public async Task<IActionResult> GetSecretsByFunction(Guid functionId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Getting secrets for function: {FunctionId}, user: {UserId}", functionId, userId);

            try
            {
                var secrets = await _secretsService.GetByFunctionIdAsync(functionId);
                var result = new List<object>();

                foreach (var secret in secrets)
                {
                    // Only include secrets owned by the current user
                    if (secret.AccountId == accountId || User.IsInRole("Admin"))
                    {
                        result.Add(new
                        {
                            Id = secret.Id,
                            Name = secret.Name,
                            Description = secret.Description,
                            Version = secret.Version,
                            AllowedFunctionIds = secret.AllowedFunctionIds,
                            ExpiresAt = secret.ExpiresAt,
                            CreatedAt = secret.CreatedAt,
                            UpdatedAt = secret.UpdatedAt
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting secrets for function: {FunctionId}, user: {UserId}", functionId, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }
    }
}
