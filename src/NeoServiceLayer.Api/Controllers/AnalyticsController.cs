using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Api.Controllers
{
    /// <summary>
    /// Controller for analytics operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly ILogger<AnalyticsController> _logger;
        private readonly IAnalyticsService _analyticsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="analyticsService">Analytics service</param>
        public AnalyticsController(ILogger<AnalyticsController> logger, IAnalyticsService analyticsService)
        {
            _logger = logger;
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Gets analytics statistics
        /// </summary>
        /// <returns>Analytics statistics</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            _logger.LogInformation("Getting analytics statistics");

            try
            {
                var statistics = await _analyticsService.GetStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics statistics");
                return StatusCode(500, new { Message = "An error occurred while getting analytics statistics" });
            }
        }

        /// <summary>
        /// Tracks an event
        /// </summary>
        /// <param name="event">Event to track</param>
        /// <returns>Tracked event</returns>
        [HttpPost("events")]
        public async Task<IActionResult> TrackEvent([FromBody] AnalyticsEvent @event)
        {
            _logger.LogInformation("Tracking event: {Name}, category: {Category}", @event.Name, @event.Category);

            try
            {
                // Set account ID from authenticated user
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                @event.AccountId = accountId;

                // Set user ID if available
                var userId = GetUserId();
                if (userId != Guid.Empty)
                {
                    @event.UserId = userId;
                }

                // Set IP address and user agent
                @event.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                @event.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                // Track event
                var trackedEvent = await _analyticsService.TrackEventAsync(@event);
                return Ok(trackedEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking event: {Name}, category: {Category}", @event.Name, @event.Category);
                return StatusCode(500, new { Message = "An error occurred while tracking the event" });
            }
        }

        /// <summary>
        /// Gets events
        /// </summary>
        /// <param name="filter">Event filter</param>
        /// <returns>List of events</returns>
        [HttpGet("events")]
        public async Task<IActionResult> GetEvents([FromQuery] EventFilter filter)
        {
            _logger.LogInformation("Getting events");

            try
            {
                // Set account ID from authenticated user
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Add account ID to filter
                if (filter.AccountIds == null)
                {
                    filter.AccountIds = new List<Guid>();
                }
                filter.AccountIds.Add(accountId);

                // Get events
                var events = await _analyticsService.GetEventsAsync(filter);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events");
                return StatusCode(500, new { Message = "An error occurred while getting events" });
            }
        }

        /// <summary>
        /// Gets event count
        /// </summary>
        /// <param name="filter">Event filter</param>
        /// <returns>Event count</returns>
        [HttpGet("events/count")]
        public async Task<IActionResult> GetEventCount([FromQuery] EventFilter filter)
        {
            _logger.LogInformation("Getting event count");

            try
            {
                // Set account ID from authenticated user
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Add account ID to filter
                if (filter.AccountIds == null)
                {
                    filter.AccountIds = new List<Guid>();
                }
                filter.AccountIds.Add(accountId);

                // Get event count
                var count = await _analyticsService.GetEventCountAsync(filter);
                return Ok(new { Count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event count");
                return StatusCode(500, new { Message = "An error occurred while getting event count" });
            }
        }

        /// <summary>
        /// Creates a metric
        /// </summary>
        /// <param name="metric">Metric to create</param>
        /// <returns>Created metric</returns>
        [HttpPost("metrics")]
        public async Task<IActionResult> CreateMetric([FromBody] Metric metric)
        {
            _logger.LogInformation("Creating metric: {Name}", metric.Name);

            try
            {
                var createdMetric = await _analyticsService.CreateMetricAsync(metric);
                return CreatedAtAction(nameof(GetMetric), new { id = createdMetric.Id }, createdMetric);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid metric data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating metric: {Name}", metric.Name);
                return StatusCode(500, new { Message = "An error occurred while creating the metric" });
            }
        }

        /// <summary>
        /// Gets a metric
        /// </summary>
        /// <param name="id">Metric ID</param>
        /// <returns>Metric</returns>
        [HttpGet("metrics/{id}")]
        public async Task<IActionResult> GetMetric(Guid id)
        {
            _logger.LogInformation("Getting metric: {Id}", id);

            try
            {
                var metric = await _analyticsService.GetMetricAsync(id);
                if (metric == null)
                {
                    return NotFound(new { Message = "Metric not found" });
                }

                return Ok(metric);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metric: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while getting the metric" });
            }
        }

        /// <summary>
        /// Gets all metrics
        /// </summary>
        /// <returns>List of metrics</returns>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics([FromQuery] string category = null)
        {
            _logger.LogInformation("Getting metrics, category: {Category}", category);

            try
            {
                IEnumerable<Metric> metrics;
                if (string.IsNullOrEmpty(category))
                {
                    metrics = await _analyticsService.GetAllMetricsAsync();
                }
                else
                {
                    metrics = await _analyticsService.GetMetricsByCategoryAsync(category);
                }

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics");
                return StatusCode(500, new { Message = "An error occurred while getting metrics" });
            }
        }

        /// <summary>
        /// Updates a metric
        /// </summary>
        /// <param name="id">Metric ID</param>
        /// <param name="metric">Updated metric</param>
        /// <returns>Updated metric</returns>
        [HttpPut("metrics/{id}")]
        public async Task<IActionResult> UpdateMetric(Guid id, [FromBody] Metric metric)
        {
            _logger.LogInformation("Updating metric: {Id}", id);

            try
            {
                // Check if metric exists
                var existingMetric = await _analyticsService.GetMetricAsync(id);
                if (existingMetric == null)
                {
                    return NotFound(new { Message = "Metric not found" });
                }

                // Update metric
                metric.Id = id;
                var updatedMetric = await _analyticsService.UpdateMetricAsync(metric);
                return Ok(updatedMetric);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid metric data: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating metric: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while updating the metric" });
            }
        }

        /// <summary>
        /// Deletes a metric
        /// </summary>
        /// <param name="id">Metric ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("metrics/{id}")]
        public async Task<IActionResult> DeleteMetric(Guid id)
        {
            _logger.LogInformation("Deleting metric: {Id}", id);

            try
            {
                // Check if metric exists
                var existingMetric = await _analyticsService.GetMetricAsync(id);
                if (existingMetric == null)
                {
                    return NotFound(new { Message = "Metric not found" });
                }

                // Delete metric
                var success = await _analyticsService.DeleteMetricAsync(id);
                if (success)
                {
                    return Ok(new { Message = "Metric deleted successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to delete metric" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting metric: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while deleting the metric" });
            }
        }

        /// <summary>
        /// Tracks a metric data point
        /// </summary>
        /// <param name="id">Metric ID</param>
        /// <param name="value">Metric value</param>
        /// <param name="dimensions">Metric dimensions</param>
        /// <returns>Tracked data point</returns>
        [HttpPost("metrics/{id}/datapoints")]
        public async Task<IActionResult> TrackMetric(Guid id, [FromQuery] double value, [FromBody] Dictionary<string, string> dimensions = null)
        {
            _logger.LogInformation("Tracking metric: {Id}, value: {Value}", id, value);

            try
            {
                // Check if metric exists
                var existingMetric = await _analyticsService.GetMetricAsync(id);
                if (existingMetric == null)
                {
                    return NotFound(new { Message = "Metric not found" });
                }

                // Track metric
                var dataPoint = await _analyticsService.TrackMetricAsync(id, value, dimensions);
                return Ok(dataPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking metric: {Id}, value: {Value}", id, value);
                return StatusCode(500, new { Message = "An error occurred while tracking the metric" });
            }
        }

        /// <summary>
        /// Gets metric data points
        /// </summary>
        /// <param name="id">Metric ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="dimensions">Dimensions to filter by</param>
        /// <returns>List of data points</returns>
        [HttpGet("metrics/{id}/datapoints")]
        public async Task<IActionResult> GetMetricDataPoints(Guid id, [FromQuery] DateTime startTime, [FromQuery] DateTime endTime, [FromBody] Dictionary<string, string> dimensions = null)
        {
            _logger.LogInformation("Getting data points for metric: {Id}, start: {StartTime}, end: {EndTime}", id, startTime, endTime);

            try
            {
                // Check if metric exists
                var existingMetric = await _analyticsService.GetMetricAsync(id);
                if (existingMetric == null)
                {
                    return NotFound(new { Message = "Metric not found" });
                }

                // Get data points
                var dataPoints = await _analyticsService.GetMetricDataPointsAsync(id, startTime, endTime, dimensions);
                return Ok(dataPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data points for metric: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while getting data points" });
            }
        }

        /// <summary>
        /// Gets metric aggregation
        /// </summary>
        /// <param name="id">Metric ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="period">Aggregation period</param>
        /// <param name="function">Aggregation function</param>
        /// <param name="groupBy">Dimensions to group by</param>
        /// <returns>Metric aggregation</returns>
        [HttpGet("metrics/{id}/aggregation")]
        public async Task<IActionResult> GetMetricAggregation(
            Guid id,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] AggregationPeriod period = AggregationPeriod.Hour,
            [FromQuery] AggregationFunction function = AggregationFunction.Average,
            [FromQuery] List<string> groupBy = null)
        {
            _logger.LogInformation("Getting aggregation for metric: {Id}, start: {StartTime}, end: {EndTime}, period: {Period}, function: {Function}",
                id, startTime, endTime, period, function);

            try
            {
                // Check if metric exists
                var existingMetric = await _analyticsService.GetMetricAsync(id);
                if (existingMetric == null)
                {
                    return NotFound(new { Message = "Metric not found" });
                }

                // Get aggregation
                var aggregation = await _analyticsService.GetMetricAggregationAsync(id, startTime, endTime, period, function, groupBy);
                return Ok(aggregation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting aggregation for metric: {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while getting aggregation" });
            }
        }

        /// <summary>
        /// Executes a query
        /// </summary>
        /// <param name="query">Query to execute</param>
        /// <param name="parameters">Query parameters</param>
        /// <returns>Query result</returns>
        [HttpPost("query")]
        public async Task<IActionResult> ExecuteQuery([FromQuery] string query, [FromBody] Dictionary<string, object> parameters = null)
        {
            _logger.LogInformation("Executing query: {Query}", query);

            try
            {
                var result = await _analyticsService.ExecuteQueryAsync(query, parameters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query: {Query}", query);
                return StatusCode(500, new { Message = "An error occurred while executing the query" });
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
