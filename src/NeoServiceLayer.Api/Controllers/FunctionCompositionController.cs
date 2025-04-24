using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.API.Controllers
{
    /// <summary>
    /// Controller for function compositions
    /// </summary>
    [ApiController]
    [Route("api/functions/compositions")]
    [Authorize]
    public class FunctionCompositionController : ControllerBase
    {
        private readonly ILogger<FunctionCompositionController> _logger;
        private readonly IFunctionCompositionService _compositionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionCompositionController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="compositionService">Function composition service</param>
        public FunctionCompositionController(ILogger<FunctionCompositionController> logger, IFunctionCompositionService compositionService)
        {
            _logger = logger;
            _compositionService = compositionService;
        }

        /// <summary>
        /// Gets a function composition by ID
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>The function composition</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionComposition>> GetByIdAsync(Guid id)
        {
            var composition = await _compositionService.GetByIdAsync(id);
            if (composition == null)
            {
                return NotFound();
            }

            return Ok(composition);
        }

        /// <summary>
        /// Gets function compositions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of function compositions</returns>
        [HttpGet("account/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionComposition>>> GetByAccountIdAsync(Guid accountId)
        {
            var compositions = await _compositionService.GetByAccountIdAsync(accountId);
            return Ok(compositions);
        }

        /// <summary>
        /// Gets function compositions by tags
        /// </summary>
        /// <param name="tags">Tags</param>
        /// <returns>List of function compositions</returns>
        [HttpGet("tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionComposition>>> GetByTagsAsync([FromQuery] List<string> tags)
        {
            var compositions = await _compositionService.GetByTagsAsync(tags);
            return Ok(compositions);
        }

        /// <summary>
        /// Creates a new function composition
        /// </summary>
        /// <param name="composition">Function composition to create</param>
        /// <returns>The created function composition</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FunctionComposition>> CreateAsync([FromBody] FunctionComposition composition)
        {
            // Set the account ID from the claims
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                composition.AccountId = userGuid;
                composition.CreatedBy = userGuid;
                composition.UpdatedBy = userGuid;
            }

            try
            {
                var createdComposition = await _compositionService.CreateAsync(composition);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = createdComposition.Id }, createdComposition);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <param name="composition">Function composition to update</param>
        /// <returns>The updated function composition</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionComposition>> UpdateAsync(Guid id, [FromBody] FunctionComposition composition)
        {
            // Check if the composition exists
            var existingComposition = await _compositionService.GetByIdAsync(id);
            if (existingComposition == null)
            {
                return NotFound();
            }

            // Ensure the ID in the path matches the ID in the body
            if (id != composition.Id)
            {
                return BadRequest("ID in the path does not match ID in the body");
            }

            // Check if the user is the owner
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (existingComposition.AccountId != userGuid)
                {
                    return Forbid("You are not the owner of this composition");
                }

                composition.UpdatedBy = userGuid;
            }

            try
            {
                var updatedComposition = await _compositionService.UpdateAsync(composition);
                return Ok(updatedComposition);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAsync(Guid id)
        {
            // Check if the composition exists
            var existingComposition = await _compositionService.GetByIdAsync(id);
            if (existingComposition == null)
            {
                return NotFound();
            }

            // Check if the user is the owner
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (existingComposition.AccountId != userGuid)
                {
                    return Forbid("You are not the owner of this composition");
                }
            }

            var result = await _compositionService.DeleteAsync(id);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete the composition");
            }

            return NoContent();
        }

        /// <summary>
        /// Executes a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <param name="inputParameters">Input parameters</param>
        /// <returns>The execution result</returns>
        [HttpPost("{id}/execute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FunctionCompositionExecution>> ExecuteAsync(Guid id, [FromBody] Dictionary<string, object> inputParameters)
        {
            // Check if the composition exists
            var existingComposition = await _compositionService.GetByIdAsync(id);
            if (existingComposition == null)
            {
                return NotFound();
            }

            try
            {
                var execution = await _compositionService.ExecuteAsync(id, inputParameters);
                return Ok(execution);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets an execution by ID
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>The execution</returns>
        [HttpGet("executions/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionCompositionExecution>> GetExecutionByIdAsync(Guid id)
        {
            var execution = await _compositionService.GetExecutionByIdAsync(id);
            if (execution == null)
            {
                return NotFound();
            }

            return Ok(execution);
        }

        /// <summary>
        /// Gets executions by composition ID
        /// </summary>
        /// <param name="compositionId">Composition ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of executions</returns>
        [HttpGet("{compositionId}/executions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<FunctionCompositionExecution>>> GetExecutionsByCompositionIdAsync(Guid compositionId, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            // Check if the composition exists
            var existingComposition = await _compositionService.GetByIdAsync(compositionId);
            if (existingComposition == null)
            {
                return NotFound();
            }

            var executions = await _compositionService.GetExecutionsByCompositionIdAsync(compositionId, limit, offset);
            return Ok(executions);
        }

        /// <summary>
        /// Gets executions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of executions</returns>
        [HttpGet("executions/account/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionCompositionExecution>>> GetExecutionsByAccountIdAsync(Guid accountId, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            var executions = await _compositionService.GetExecutionsByAccountIdAsync(accountId, limit, offset);
            return Ok(executions);
        }

        /// <summary>
        /// Gets executions by status
        /// </summary>
        /// <param name="status">Status</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of executions</returns>
        [HttpGet("executions/status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionCompositionExecution>>> GetExecutionsByStatusAsync(string status, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            var executions = await _compositionService.GetExecutionsByStatusAsync(status, limit, offset);
            return Ok(executions);
        }

        /// <summary>
        /// Cancels an execution
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>The cancelled execution</returns>
        [HttpPost("executions/{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FunctionCompositionExecution>> CancelExecutionAsync(Guid id)
        {
            try
            {
                var execution = await _compositionService.CancelExecutionAsync(id);
                return Ok(execution);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets logs for an execution
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>List of logs</returns>
        [HttpGet("executions/{id}/logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<string>>> GetExecutionLogsAsync(Guid id)
        {
            try
            {
                var logs = await _compositionService.GetExecutionLogsAsync(id);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Gets logs for a step execution
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <param name="stepId">Step ID</param>
        /// <returns>List of logs</returns>
        [HttpGet("executions/{executionId}/steps/{stepId}/logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<string>>> GetStepExecutionLogsAsync(Guid executionId, Guid stepId)
        {
            try
            {
                var logs = await _compositionService.GetStepExecutionLogsAsync(executionId, stepId);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Gets the input schema for a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>The input schema</returns>
        [HttpGet("{id}/schema/input")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> GetInputSchemaAsync(Guid id)
        {
            try
            {
                var schema = await _compositionService.GetInputSchemaAsync(id);
                return Ok(schema);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Gets the output schema for a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>The output schema</returns>
        [HttpGet("{id}/schema/output")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> GetOutputSchemaAsync(Guid id)
        {
            try
            {
                var schema = await _compositionService.GetOutputSchemaAsync(id);
                return Ok(schema);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Generates the input schema for a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>The generated input schema</returns>
        [HttpPost("{id}/schema/input/generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> GenerateInputSchemaAsync(Guid id)
        {
            try
            {
                var schema = await _compositionService.GenerateInputSchemaAsync(id);
                return Ok(schema);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Generates the output schema for a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>The generated output schema</returns>
        [HttpPost("{id}/schema/output/generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> GenerateOutputSchemaAsync(Guid id)
        {
            try
            {
                var schema = await _compositionService.GenerateOutputSchemaAsync(id);
                return Ok(schema);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Adds a step to a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <param name="step">Step to add</param>
        /// <returns>The updated function composition</returns>
        [HttpPost("{id}/steps")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FunctionComposition>> AddStepAsync(Guid id, [FromBody] FunctionCompositionStep step)
        {
            // Check if the composition exists
            var existingComposition = await _compositionService.GetByIdAsync(id);
            if (existingComposition == null)
            {
                return NotFound();
            }

            // Check if the user is the owner
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (existingComposition.AccountId != userGuid)
                {
                    return Forbid("You are not the owner of this composition");
                }
            }

            try
            {
                var updatedComposition = await _compositionService.AddStepAsync(id, step);
                return Ok(updatedComposition);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates a step in a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <param name="stepId">Step ID</param>
        /// <param name="step">Step to update</param>
        /// <returns>The updated function composition</returns>
        [HttpPut("{id}/steps/{stepId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FunctionComposition>> UpdateStepAsync(Guid id, Guid stepId, [FromBody] FunctionCompositionStep step)
        {
            // Check if the composition exists
            var existingComposition = await _compositionService.GetByIdAsync(id);
            if (existingComposition == null)
            {
                return NotFound("Composition not found");
            }

            // Ensure the ID in the path matches the ID in the body
            if (stepId != step.Id)
            {
                return BadRequest("Step ID in the path does not match ID in the body");
            }

            // Check if the user is the owner
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (existingComposition.AccountId != userGuid)
                {
                    return Forbid("You are not the owner of this composition");
                }
            }

            try
            {
                var updatedComposition = await _compositionService.UpdateStepAsync(id, step);
                return Ok(updatedComposition);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Removes a step from a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <param name="stepId">Step ID</param>
        /// <returns>The updated function composition</returns>
        [HttpDelete("{id}/steps/{stepId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionComposition>> RemoveStepAsync(Guid id, Guid stepId)
        {
            // Check if the composition exists
            var existingComposition = await _compositionService.GetByIdAsync(id);
            if (existingComposition == null)
            {
                return NotFound("Composition not found");
            }

            // Check if the user is the owner
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (existingComposition.AccountId != userGuid)
                {
                    return Forbid("You are not the owner of this composition");
                }
            }

            try
            {
                var updatedComposition = await _compositionService.RemoveStepAsync(id, stepId);
                return Ok(updatedComposition);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Reorders steps in a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <param name="stepIds">Ordered list of step IDs</param>
        /// <returns>The updated function composition</returns>
        [HttpPost("{id}/steps/reorder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FunctionComposition>> ReorderStepsAsync(Guid id, [FromBody] List<Guid> stepIds)
        {
            // Check if the composition exists
            var existingComposition = await _compositionService.GetByIdAsync(id);
            if (existingComposition == null)
            {
                return NotFound("Composition not found");
            }

            // Check if the user is the owner
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (existingComposition.AccountId != userGuid)
                {
                    return Forbid("You are not the owner of this composition");
                }
            }

            try
            {
                var updatedComposition = await _compositionService.ReorderStepsAsync(id, stepIds);
                return Ok(updatedComposition);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
