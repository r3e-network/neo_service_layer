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
    /// Controller for function test suites
    /// </summary>
    [ApiController]
    [Route("api/functions/test-suites")]
    [Authorize]
    public class FunctionTestSuiteController : ControllerBase
    {
        private readonly ILogger<FunctionTestSuiteController> _logger;
        private readonly IFunctionTestService _testService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionTestSuiteController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="testService">Function test service</param>
        public FunctionTestSuiteController(ILogger<FunctionTestSuiteController> logger, IFunctionTestService testService)
        {
            _logger = logger;
            _testService = testService;
        }

        /// <summary>
        /// Gets a function test suite by ID
        /// </summary>
        /// <param name="id">Suite ID</param>
        /// <returns>The function test suite</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionTestSuite>> GetByIdAsync(Guid id)
        {
            var suite = await _testService.GetTestSuiteByIdAsync(id);
            if (suite == null)
            {
                return NotFound();
            }

            return Ok(suite);
        }

        /// <summary>
        /// Gets function test suites by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of function test suites</returns>
        [HttpGet("function/{functionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionTestSuite>>> GetByFunctionIdAsync(Guid functionId)
        {
            var suites = await _testService.GetTestSuitesByFunctionIdAsync(functionId);
            return Ok(suites);
        }

        /// <summary>
        /// Creates a new function test suite
        /// </summary>
        /// <param name="suite">Function test suite to create</param>
        /// <returns>The created function test suite</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FunctionTestSuite>> CreateAsync([FromBody] FunctionTestSuite suite)
        {
            // Set the user ID from the claims
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                suite.CreatedBy = userGuid;
                suite.UpdatedBy = userGuid;
            }

            var createdSuite = await _testService.CreateTestSuiteAsync(suite);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = createdSuite.Id }, createdSuite);
        }

        /// <summary>
        /// Updates a function test suite
        /// </summary>
        /// <param name="id">Suite ID</param>
        /// <param name="suite">Function test suite to update</param>
        /// <returns>The updated function test suite</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionTestSuite>> UpdateAsync(Guid id, [FromBody] FunctionTestSuite suite)
        {
            // Check if the suite exists
            var existingSuite = await _testService.GetTestSuiteByIdAsync(id);
            if (existingSuite == null)
            {
                return NotFound();
            }

            // Ensure the ID in the path matches the ID in the body
            if (id != suite.Id)
            {
                return BadRequest("ID in the path does not match ID in the body");
            }

            // Set the user ID from the claims
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                suite.UpdatedBy = userGuid;
            }

            var updatedSuite = await _testService.UpdateTestSuiteAsync(suite);
            return Ok(updatedSuite);
        }

        /// <summary>
        /// Deletes a function test suite
        /// </summary>
        /// <param name="id">Suite ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAsync(Guid id)
        {
            // Check if the suite exists
            var existingSuite = await _testService.GetTestSuiteByIdAsync(id);
            if (existingSuite == null)
            {
                return NotFound();
            }

            var result = await _testService.DeleteTestSuiteAsync(id);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete the test suite");
            }

            return NoContent();
        }

        /// <summary>
        /// Adds a test to a function test suite
        /// </summary>
        /// <param name="id">Suite ID</param>
        /// <param name="testId">Test ID</param>
        /// <returns>The updated function test suite</returns>
        [HttpPost("{id}/tests/{testId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionTestSuite>> AddTestAsync(Guid id, Guid testId)
        {
            // Check if the suite exists
            var existingSuite = await _testService.GetTestSuiteByIdAsync(id);
            if (existingSuite == null)
            {
                return NotFound("Test suite not found");
            }

            // Check if the test exists
            var existingTest = await _testService.GetByIdAsync(testId);
            if (existingTest == null)
            {
                return NotFound("Test not found");
            }

            var updatedSuite = await _testService.AddTestToSuiteAsync(id, testId);
            return Ok(updatedSuite);
        }

        /// <summary>
        /// Removes a test from a function test suite
        /// </summary>
        /// <param name="id">Suite ID</param>
        /// <param name="testId">Test ID</param>
        /// <returns>The updated function test suite</returns>
        [HttpDelete("{id}/tests/{testId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionTestSuite>> RemoveTestAsync(Guid id, Guid testId)
        {
            // Check if the suite exists
            var existingSuite = await _testService.GetTestSuiteByIdAsync(id);
            if (existingSuite == null)
            {
                return NotFound("Test suite not found");
            }

            var updatedSuite = await _testService.RemoveTestFromSuiteAsync(id, testId);
            return Ok(updatedSuite);
        }

        /// <summary>
        /// Runs all tests in a function test suite
        /// </summary>
        /// <param name="id">Suite ID</param>
        /// <param name="functionVersion">Function version</param>
        /// <returns>List of test results</returns>
        [HttpPost("{id}/run")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<FunctionTestResult>>> RunSuiteAsync(Guid id, [FromQuery] string functionVersion = null)
        {
            // Check if the suite exists
            var existingSuite = await _testService.GetTestSuiteByIdAsync(id);
            if (existingSuite == null)
            {
                return NotFound();
            }

            var results = await _testService.RunTestSuiteAsync(id, functionVersion);
            return Ok(results);
        }
    }
}
