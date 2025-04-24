using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Api.Models;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Api.Controllers
{
    /// <summary>
    /// Controller for function management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FunctionController : ControllerBase
    {
        private readonly ILogger<FunctionController> _logger;
        private readonly IFunctionService _functionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="functionService">Function service</param>
        public FunctionController(ILogger<FunctionController> logger, IFunctionService functionService)
        {
            _logger = logger;
            _functionService = functionService;
        }

        /// <summary>
        /// Creates a new function
        /// </summary>
        /// <param name="request">Function creation request</param>
        /// <returns>The created function</returns>
        [HttpPost]
        public async Task<IActionResult> CreateFunction([FromBody] CreateFunctionRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Creating function: {Name}, Runtime: {Runtime}, AccountId: {AccountId}", request.Name, request.Runtime, accountId);

            try
            {
                var function = await _functionService.CreateFunctionAsync(
                    request.Name,
                    request.Description,
                    request.Runtime,
                    request.SourceCode,
                    request.EntryPoint,
                    accountId,
                    request.MaxExecutionTime,
                    request.MaxMemory,
                    request.SecretIds,
                    request.EnvironmentVariables);

                return Ok(new
                {
                    Id = function.Id,
                    Name = function.Name,
                    Description = function.Description,
                    Runtime = function.Runtime,
                    EntryPoint = function.EntryPoint,
                    MaxExecutionTime = function.MaxExecutionTime,
                    MaxMemory = function.MaxMemory,
                    Status = function.Status,
                    CreatedAt = function.CreatedAt
                });
            }
            catch (FunctionException ex)
            {
                _logger.LogError(ex, "Error creating function: {Name}, Runtime: {Runtime}, AccountId: {AccountId}", request.Name, request.Runtime, accountId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating function: {Name}, Runtime: {Runtime}, AccountId: {AccountId}", request.Name, request.Runtime, accountId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets all functions for the current user
        /// </summary>
        /// <returns>List of functions</returns>
        [HttpGet]
        public async Task<IActionResult> GetFunctions()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Getting functions for user: {UserId}", userId);

            try
            {
                var functions = await _functionService.GetByAccountIdAsync(accountId);
                var result = new List<object>();

                foreach (var function in functions)
                {
                    result.Add(new
                    {
                        Id = function.Id,
                        Name = function.Name,
                        Description = function.Description,
                        Runtime = function.Runtime,
                        EntryPoint = function.EntryPoint,
                        MaxExecutionTime = function.MaxExecutionTime,
                        MaxMemory = function.MaxMemory,
                        Status = function.Status,
                        CreatedAt = function.CreatedAt,
                        UpdatedAt = function.UpdatedAt,
                        LastExecutedAt = function.LastExecutedAt
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting functions for user: {UserId}", userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets a function by ID
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>The function</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFunctionById(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Getting function by ID: {FunctionId} for user: {UserId}", id, userId);

            try
            {
                var function = await _functionService.GetByIdAsync(id);
                if (function == null)
                {
                    return NotFound(new { Message = "Function not found" });
                }

                // Check if the function belongs to the current user
                if (function.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(new
                {
                    Id = function.Id,
                    Name = function.Name,
                    Description = function.Description,
                    Runtime = function.Runtime,
                    SourceCode = function.SourceCode,
                    EntryPoint = function.EntryPoint,
                    MaxExecutionTime = function.MaxExecutionTime,
                    MaxMemory = function.MaxMemory,
                    SecretIds = function.SecretIds,
                    EnvironmentVariables = function.EnvironmentVariables,
                    Status = function.Status,
                    CreatedAt = function.CreatedAt,
                    UpdatedAt = function.UpdatedAt,
                    LastExecutedAt = function.LastExecutedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function by ID: {FunctionId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Updates a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="request">Update request</param>
        /// <returns>The updated function</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFunction(Guid id, [FromBody] UpdateFunctionRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Updating function: {FunctionId} for user: {UserId}", id, userId);

            try
            {
                var function = await _functionService.GetByIdAsync(id);
                if (function == null)
                {
                    return NotFound(new { Message = "Function not found" });
                }

                // Check if the function belongs to the current user
                if (function.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Update function properties
                function.Name = request.Name;
                function.Description = request.Description;
                function.EntryPoint = request.EntryPoint;
                function.MaxExecutionTime = request.MaxExecutionTime;
                function.MaxMemory = request.MaxMemory;

                var updatedFunction = await _functionService.UpdateAsync(function);
                return Ok(new
                {
                    Id = updatedFunction.Id,
                    Name = updatedFunction.Name,
                    Description = updatedFunction.Description,
                    Runtime = updatedFunction.Runtime,
                    EntryPoint = updatedFunction.EntryPoint,
                    MaxExecutionTime = updatedFunction.MaxExecutionTime,
                    MaxMemory = updatedFunction.MaxMemory,
                    Status = updatedFunction.Status,
                    UpdatedAt = updatedFunction.UpdatedAt
                });
            }
            catch (FunctionException ex)
            {
                _logger.LogError(ex, "Error updating function: {FunctionId} for user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating function: {FunctionId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Updates the source code of a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="request">Update request</param>
        /// <returns>The updated function</returns>
        [HttpPut("{id}/source")]
        public async Task<IActionResult> UpdateSourceCode(Guid id, [FromBody] UpdateSourceCodeRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Updating source code for function: {FunctionId} for user: {UserId}", id, userId);

            try
            {
                var function = await _functionService.GetByIdAsync(id);
                if (function == null)
                {
                    return NotFound(new { Message = "Function not found" });
                }

                // Check if the function belongs to the current user
                if (function.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var updatedFunction = await _functionService.UpdateSourceCodeAsync(id, request.SourceCode);
                return Ok(new
                {
                    Id = updatedFunction.Id,
                    Name = updatedFunction.Name,
                    UpdatedAt = updatedFunction.UpdatedAt
                });
            }
            catch (FunctionException ex)
            {
                _logger.LogError(ex, "Error updating source code for function: {FunctionId} for user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating source code for function: {FunctionId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Updates the environment variables of a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="request">Update request</param>
        /// <returns>The updated function</returns>
        [HttpPut("{id}/environment")]
        public async Task<IActionResult> UpdateEnvironmentVariables(Guid id, [FromBody] UpdateEnvironmentVariablesRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Updating environment variables for function: {FunctionId} for user: {UserId}", id, userId);

            try
            {
                var function = await _functionService.GetByIdAsync(id);
                if (function == null)
                {
                    return NotFound(new { Message = "Function not found" });
                }

                // Check if the function belongs to the current user
                if (function.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var updatedFunction = await _functionService.UpdateEnvironmentVariablesAsync(id, request.EnvironmentVariables);
                return Ok(new
                {
                    Id = updatedFunction.Id,
                    Name = updatedFunction.Name,
                    EnvironmentVariables = updatedFunction.EnvironmentVariables,
                    UpdatedAt = updatedFunction.UpdatedAt
                });
            }
            catch (FunctionException ex)
            {
                _logger.LogError(ex, "Error updating environment variables for function: {FunctionId} for user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating environment variables for function: {FunctionId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Updates the secret access of a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="request">Update request</param>
        /// <returns>The updated function</returns>
        [HttpPut("{id}/secrets")]
        public async Task<IActionResult> UpdateSecretAccess(Guid id, [FromBody] UpdateSecretAccessRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Updating secret access for function: {FunctionId} for user: {UserId}", id, userId);

            try
            {
                var function = await _functionService.GetByIdAsync(id);
                if (function == null)
                {
                    return NotFound(new { Message = "Function not found" });
                }

                // Check if the function belongs to the current user
                if (function.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var updatedFunction = await _functionService.UpdateSecretAccessAsync(id, request.SecretIds);
                return Ok(new
                {
                    Id = updatedFunction.Id,
                    Name = updatedFunction.Name,
                    SecretIds = updatedFunction.SecretIds,
                    UpdatedAt = updatedFunction.UpdatedAt
                });
            }
            catch (FunctionException ex)
            {
                _logger.LogError(ex, "Error updating secret access for function: {FunctionId} for user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating secret access for function: {FunctionId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Executes a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="request">Execution request</param>
        /// <returns>Function execution result</returns>
        [HttpPost("{id}/execute")]
        public async Task<IActionResult> ExecuteFunction(Guid id, [FromBody] ExecuteFunctionRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Executing function: {FunctionId} for user: {UserId}", id, userId);

            try
            {
                var function = await _functionService.GetByIdAsync(id);
                if (function == null)
                {
                    return NotFound(new { Message = "Function not found" });
                }

                // Check if the function belongs to the current user
                if (function.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var result = await _functionService.ExecuteAsync(id, request.Parameters);
                return Ok(new { Result = result });
            }
            catch (FunctionException ex)
            {
                _logger.LogError(ex, "Error executing function: {FunctionId} for user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error executing function: {FunctionId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets the execution history of a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="startTime">Start time for the history</param>
        /// <param name="endTime">End time for the history</param>
        /// <returns>List of execution records</returns>
        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetExecutionHistory(Guid id, [FromQuery] DateTime? startTime = null, [FromQuery] DateTime? endTime = null)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Getting execution history for function: {FunctionId} for user: {UserId}", id, userId);

            try
            {
                var function = await _functionService.GetByIdAsync(id);
                if (function == null)
                {
                    return NotFound(new { Message = "Function not found" });
                }

                // Check if the function belongs to the current user
                if (function.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var start = startTime ?? DateTime.UtcNow.AddDays(-7);
                var end = endTime ?? DateTime.UtcNow;

                var history = await _functionService.GetExecutionHistoryAsync(id, start, end);
                return Ok(history);
            }
            catch (FunctionException ex)
            {
                _logger.LogError(ex, "Error getting execution history for function: {FunctionId} for user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting execution history for function: {FunctionId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Activates a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>The activated function</returns>
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateFunction(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Activating function: {FunctionId} for user: {UserId}", id, userId);

            try
            {
                var function = await _functionService.GetByIdAsync(id);
                if (function == null)
                {
                    return NotFound(new { Message = "Function not found" });
                }

                // Check if the function belongs to the current user
                if (function.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var activatedFunction = await _functionService.ActivateAsync(id);
                return Ok(new
                {
                    Id = activatedFunction.Id,
                    Name = activatedFunction.Name,
                    Status = activatedFunction.Status,
                    UpdatedAt = activatedFunction.UpdatedAt
                });
            }
            catch (FunctionException ex)
            {
                _logger.LogError(ex, "Error activating function: {FunctionId} for user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error activating function: {FunctionId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Deactivates a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>The deactivated function</returns>
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateFunction(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Deactivating function: {FunctionId} for user: {UserId}", id, userId);

            try
            {
                var function = await _functionService.GetByIdAsync(id);
                if (function == null)
                {
                    return NotFound(new { Message = "Function not found" });
                }

                // Check if the function belongs to the current user
                if (function.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var deactivatedFunction = await _functionService.DeactivateAsync(id);
                return Ok(new
                {
                    Id = deactivatedFunction.Id,
                    Name = deactivatedFunction.Name,
                    Status = deactivatedFunction.Status,
                    UpdatedAt = deactivatedFunction.UpdatedAt
                });
            }
            catch (FunctionException ex)
            {
                _logger.LogError(ex, "Error deactivating function: {FunctionId} for user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deactivating function: {FunctionId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Deletes a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFunction(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Deleting function: {FunctionId} for user: {UserId}", id, userId);

            try
            {
                var function = await _functionService.GetByIdAsync(id);
                if (function == null)
                {
                    return NotFound(new { Message = "Function not found" });
                }

                // Check if the function belongs to the current user
                if (function.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var success = await _functionService.DeleteAsync(id);
                if (!success)
                {
                    return BadRequest(new { Message = "Failed to delete function" });
                }

                return Ok(new { Message = "Function deleted successfully" });
            }
            catch (FunctionException ex)
            {
                _logger.LogError(ex, "Error deleting function: {FunctionId} for user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting function: {FunctionId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }
    }
}
