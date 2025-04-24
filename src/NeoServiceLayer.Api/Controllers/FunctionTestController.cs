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
    /// Controller for function tests
    /// </summary>
    [ApiController]
    [Route("api/functions/tests")]
    [Authorize]
    public class FunctionTestController : ControllerBase
    {
        private readonly ILogger<FunctionTestController> _logger;
        private readonly IFunctionTestService _testService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionTestController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="testService">Function test service</param>
        public FunctionTestController(ILogger<FunctionTestController> logger, IFunctionTestService testService)
        {
            _logger = logger;
            _testService = testService;
        }

        /// <summary>
        /// Gets a function test by ID
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <returns>The function test</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionTest>> GetByIdAsync(Guid id)
        {
            var test = await _testService.GetByIdAsync(id);
            if (test == null)
            {
                return NotFound();
            }

            return Ok(test);
        }

        /// <summary>
        /// Gets function tests by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of function tests</returns>
        [HttpGet("function/{functionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionTest>>> GetByFunctionIdAsync(Guid functionId)
        {
            var tests = await _testService.GetByFunctionIdAsync(functionId);
            return Ok(tests);
        }

        /// <summary>
        /// Creates a new function test
        /// </summary>
        /// <param name="test">Function test to create</param>
        /// <returns>The created function test</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FunctionTest>> CreateAsync([FromBody] FunctionTest test)
        {
            // Validate the test
            var validationErrors = await _testService.ValidateTestAsync(test);
            if (validationErrors != null && validationErrors.Any())
            {
                return BadRequest(new { Errors = validationErrors });
            }

            // Set the user ID from the claims
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                test.CreatedBy = userGuid;
                test.UpdatedBy = userGuid;
            }

            var createdTest = await _testService.CreateAsync(test);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = createdTest.Id }, createdTest);
        }

        /// <summary>
        /// Updates a function test
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <param name="test">Function test to update</param>
        /// <returns>The updated function test</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionTest>> UpdateAsync(Guid id, [FromBody] FunctionTest test)
        {
            // Check if the test exists
            var existingTest = await _testService.GetByIdAsync(id);
            if (existingTest == null)
            {
                return NotFound();
            }

            // Ensure the ID in the path matches the ID in the body
            if (id != test.Id)
            {
                return BadRequest("ID in the path does not match ID in the body");
            }

            // Validate the test
            var validationErrors = await _testService.ValidateTestAsync(test);
            if (validationErrors != null && validationErrors.Any())
            {
                return BadRequest(new { Errors = validationErrors });
            }

            // Set the user ID from the claims
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                test.UpdatedBy = userGuid;
            }

            var updatedTest = await _testService.UpdateAsync(test);
            return Ok(updatedTest);
        }

        /// <summary>
        /// Deletes a function test
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAsync(Guid id)
        {
            // Check if the test exists
            var existingTest = await _testService.GetByIdAsync(id);
            if (existingTest == null)
            {
                return NotFound();
            }

            var result = await _testService.DeleteAsync(id);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete the test");
            }

            return NoContent();
        }

        /// <summary>
        /// Runs a function test
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <param name="functionVersion">Function version</param>
        /// <returns>The test result</returns>
        [HttpPost("{id}/run")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionTestResult>> RunTestAsync(Guid id, [FromQuery] string functionVersion = null)
        {
            // Check if the test exists
            var existingTest = await _testService.GetByIdAsync(id);
            if (existingTest == null)
            {
                return NotFound();
            }

            var result = await _testService.RunTestAsync(id, functionVersion);
            return Ok(result);
        }

        /// <summary>
        /// Gets test results for a function test
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of test results</returns>
        [HttpGet("{id}/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<FunctionTestResult>>> GetResultsAsync(Guid id, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            // Check if the test exists
            var existingTest = await _testService.GetByIdAsync(id);
            if (existingTest == null)
            {
                return NotFound();
            }

            var results = await _testService.GetTestResultsAsync(id, limit, offset);
            return Ok(results);
        }

        /// <summary>
        /// Gets the latest test result for a function test
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <returns>The latest test result</returns>
        [HttpGet("{id}/results/latest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionTestResult>> GetLatestResultAsync(Guid id)
        {
            // Check if the test exists
            var existingTest = await _testService.GetByIdAsync(id);
            if (existingTest == null)
            {
                return NotFound();
            }

            var result = await _testService.GetLatestTestResultAsync(id);
            if (result == null)
            {
                return NotFound("No test results found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Generates tests for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of generated tests</returns>
        [HttpPost("generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<FunctionTest>>> GenerateTestsAsync([FromQuery] Guid functionId)
        {
            var tests = await _testService.GenerateTestsAsync(functionId);
            return Ok(tests);
        }

        /// <summary>
        /// Gets test coverage for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>Test coverage information</returns>
        [HttpGet("coverage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetCoverageAsync([FromQuery] Guid functionId)
        {
            var coverage = await _testService.GetTestCoverageAsync(functionId);
            return Ok(coverage);
        }
    }
}
