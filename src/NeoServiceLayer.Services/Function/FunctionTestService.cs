using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function
{
    /// <summary>
    /// Service for managing function tests
    /// </summary>
    public class FunctionTestService : IFunctionTestService
    {
        private readonly ILogger<FunctionTestService> _logger;
        private readonly IFunctionTestRepository _testRepository;
        private readonly IFunctionTestResultRepository _testResultRepository;
        private readonly IFunctionTestSuiteRepository _testSuiteRepository;
        private readonly IFunctionService _functionService;
        private readonly IFunctionExecutor _functionExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionTestService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="testRepository">Test repository</param>
        /// <param name="testResultRepository">Test result repository</param>
        /// <param name="testSuiteRepository">Test suite repository</param>
        /// <param name="functionService">Function service</param>
        /// <param name="functionExecutor">Function executor</param>
        public FunctionTestService(
            ILogger<FunctionTestService> logger,
            IFunctionTestRepository testRepository,
            IFunctionTestResultRepository testResultRepository,
            IFunctionTestSuiteRepository testSuiteRepository,
            IFunctionService functionService,
            IFunctionExecutor functionExecutor)
        {
            _logger = logger;
            _testRepository = testRepository;
            _testResultRepository = testResultRepository;
            _testSuiteRepository = testSuiteRepository;
            _functionService = functionService;
            _functionExecutor = functionExecutor;
        }

        /// <inheritdoc/>
        public async Task<FunctionTest> CreateAsync(FunctionTest test)
        {
            return await CreateTestAsync(test);
        }

        /// <inheritdoc/>
        public async Task<FunctionTest> CreateTestAsync(FunctionTest test)
        {
            _logger.LogInformation("Creating function test: {Name} for function {FunctionId}", test.Name, test.FunctionId);

            try
            {
                // Validate the test
                var validationErrors = await ValidateTestAsync(test);
                if (validationErrors.Any())
                {
                    throw new Exception($"Test validation failed: {string.Join(", ", validationErrors)}");
                }

                // Set default values
                test.Id = Guid.NewGuid();
                test.CreatedAt = DateTime.UtcNow;
                test.UpdatedAt = DateTime.UtcNow;

                return await _testRepository.CreateAsync(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating function test: {Name} for function {FunctionId}", test.Name, test.FunctionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionTest> UpdateAsync(FunctionTest test)
        {
            return await UpdateTestAsync(test);
        }

        /// <inheritdoc/>
        public async Task<FunctionTest> UpdateTestAsync(FunctionTest test)
        {
            _logger.LogInformation("Updating function test: {Id}", test.Id);

            try
            {
                // Validate the test
                var validationErrors = await ValidateTestAsync(test);
                if (validationErrors.Any())
                {
                    throw new Exception($"Test validation failed: {string.Join(", ", validationErrors)}");
                }

                // Update timestamp
                test.UpdatedAt = DateTime.UtcNow;

                return await _testRepository.UpdateAsync(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating function test: {Id}", test.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionTest> GetByIdAsync(Guid id)
        {
            return await GetTestByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<FunctionTest> GetTestByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function test by ID: {Id}", id);

            try
            {
                return await _testRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function test by ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTest>> GetByFunctionIdAsync(Guid functionId)
        {
            return await GetTestsByFunctionIdAsync(functionId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTest>> GetTestsByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Getting function tests by function ID: {FunctionId}", functionId);

            try
            {
                return await _testRepository.GetByFunctionIdAsync(functionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function tests by function ID: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            return await DeleteTestAsync(id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteTestAsync(Guid id)
        {
            _logger.LogInformation("Deleting function test: {Id}", id);

            try
            {
                // Delete test results first
                await _testResultRepository.DeleteByTestIdAsync(id);

                // Delete the test
                return await _testRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting function test: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionTestResult> RunTestAsync(Guid testId, string functionVersion = null)
        {
            _logger.LogInformation("Running function test: {TestId}, version: {Version}", testId, functionVersion ?? "latest");

            try
            {
                // Get the test
                var test = await _testRepository.GetByIdAsync(testId);
                if (test == null)
                {
                    throw new Exception($"Test not found: {testId}");
                }

                // Get the function
                var function = await _functionService.GetByIdAsync(test.FunctionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {test.FunctionId}");
                }

                // Create a test result
                var testResult = new FunctionTestResult
                {
                    Id = Guid.NewGuid(),
                    TestId = testId,
                    FunctionId = test.FunctionId,
                    FunctionVersion = functionVersion ?? function.Version,
                    Status = "running",
                    StartTime = DateTime.UtcNow,
                    RunBy = Guid.Empty // TODO: Get from context
                };

                // Save the initial test result
                await _testResultRepository.CreateAsync(testResult);

                try
                {
                    // Execute the function
                    var executionContext = new FunctionExecutionContext
                    {
                        FunctionId = test.FunctionId,
                        AccountId = function.AccountId,
                        MaxExecutionTime = test.TimeoutMs,
                        MaxMemory = function.MaxMemory,
                        EnvironmentVariables = test.EnvironmentVariables
                    };

                    var startTime = DateTime.UtcNow;
                    var result = await _functionExecutor.ExecuteAsync(test.FunctionId, test.InputParameters, executionContext);
                    var endTime = DateTime.UtcNow;

                    // Update the test result
                    testResult.Status = "passed";
                    testResult.ActualOutput = result;
                    testResult.EndTime = endTime;
                    testResult.ExecutionTimeMs = (endTime - startTime).TotalMilliseconds;

                    // Evaluate assertions
                    testResult.AssertionResults = EvaluateAssertions(test.Assertions, result);

                    // Check if any required assertions failed
                    if (testResult.AssertionResults.Any(a => a.IsRequired && !a.Passed))
                    {
                        testResult.Status = "failed";
                        testResult.ErrorMessage = "One or more required assertions failed";
                    }
                }
                catch (Exception ex)
                {
                    // Update the test result with the error
                    testResult.Status = "error";
                    testResult.ErrorMessage = ex.Message;
                    testResult.StackTrace = ex.StackTrace;
                    testResult.EndTime = DateTime.UtcNow;
                }

                // Save the final test result
                return await _testResultRepository.UpdateAsync(testResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running function test: {TestId}", testId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestResult>> RunTestsByFunctionIdAsync(Guid functionId, string functionVersion = null)
        {
            _logger.LogInformation("Running function tests by function ID: {FunctionId}, version: {Version}", functionId, functionVersion ?? "latest");

            try
            {
                // Get the tests
                var tests = await _testRepository.GetByFunctionIdAsync(functionId);
                if (!tests.Any())
                {
                    return new List<FunctionTestResult>();
                }

                // Run each test
                var results = new List<FunctionTestResult>();
                foreach (var test in tests)
                {
                    var result = await RunTestAsync(test.Id, functionVersion);
                    results.Add(result);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running function tests by function ID: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionTestResult> GetLatestTestResultAsync(Guid testId)
        {
            _logger.LogInformation("Getting latest function test result for test: {TestId}", testId);

            try
            {
                var results = await _testResultRepository.GetByTestIdAsync(testId, 1, 0);
                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest function test result for test: {TestId}", testId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionTestResult> GetTestResultByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function test result by ID: {Id}", id);

            try
            {
                return await _testResultRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function test result by ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestResult>> GetTestResultsAsync(Guid testId, int limit = 10, int offset = 0)
        {
            return await GetTestResultsByTestIdAsync(testId, limit, offset);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestResult>> GetTestResultsByTestIdAsync(Guid testId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function test results by test ID: {TestId}, limit: {Limit}, offset: {Offset}", testId, limit, offset);

            try
            {
                return await _testResultRepository.GetByTestIdAsync(testId, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function test results by test ID: {TestId}", testId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestResult>> GetTestResultsByFunctionIdAsync(Guid functionId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function test results by function ID: {FunctionId}, limit: {Limit}, offset: {Offset}", functionId, limit, offset);

            try
            {
                return await _testResultRepository.GetByFunctionIdAsync(functionId, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function test results by function ID: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionTestSuite> CreateTestSuiteAsync(FunctionTestSuite suite)
        {
            _logger.LogInformation("Creating function test suite: {Name} for function {FunctionId}", suite.Name, suite.FunctionId);

            try
            {
                // Set default values
                suite.Id = Guid.NewGuid();
                suite.CreatedAt = DateTime.UtcNow;
                suite.UpdatedAt = DateTime.UtcNow;

                return await _testSuiteRepository.CreateAsync(suite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating function test suite: {Name} for function {FunctionId}", suite.Name, suite.FunctionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionTestSuite> UpdateTestSuiteAsync(FunctionTestSuite suite)
        {
            _logger.LogInformation("Updating function test suite: {Id}", suite.Id);

            try
            {
                // Update timestamp
                suite.UpdatedAt = DateTime.UtcNow;

                return await _testSuiteRepository.UpdateAsync(suite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating function test suite: {Id}", suite.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionTestSuite> GetTestSuiteByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function test suite by ID: {Id}", id);

            try
            {
                return await _testSuiteRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function test suite by ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestSuite>> GetTestSuitesByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Getting function test suites by function ID: {FunctionId}", functionId);

            try
            {
                return await _testSuiteRepository.GetByFunctionIdAsync(functionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function test suites by function ID: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteTestSuiteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function test suite: {Id}", id);

            try
            {
                return await _testSuiteRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting function test suite: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestResult>> RunTestSuiteAsync(Guid suiteId, string functionVersion = null)
        {
            _logger.LogInformation("Running function test suite: {SuiteId}, version: {Version}", suiteId, functionVersion ?? "latest");

            try
            {
                // Get the suite
                var suite = await _testSuiteRepository.GetByIdAsync(suiteId);
                if (suite == null)
                {
                    throw new Exception($"Test suite not found: {suiteId}");
                }

                // Get the tests in the suite
                var testResults = new List<FunctionTestResult>();
                foreach (var testId in suite.TestIds)
                {
                    try
                    {
                        var result = await RunTestAsync(testId, functionVersion);
                        testResults.Add(result);

                        // If stop on first failure is enabled and the test failed, stop running tests
                        if (suite.StopOnFirstFailure && (result.Status == "failed" || result.Status == "error"))
                        {
                            _logger.LogInformation("Stopping test suite execution due to test failure: {TestId}", testId);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error running test: {TestId} in suite: {SuiteId}", testId, suiteId);
                        // Continue with the next test
                    }
                }

                return testResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running function test suite: {SuiteId}", suiteId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionTestSuite> AddTestToSuiteAsync(Guid suiteId, Guid testId)
        {
            _logger.LogInformation("Adding test {TestId} to suite {SuiteId}", testId, suiteId);

            try
            {
                // Get the suite
                var suite = await _testSuiteRepository.GetByIdAsync(suiteId);
                if (suite == null)
                {
                    throw new Exception($"Test suite not found: {suiteId}");
                }

                // Get the test
                var test = await _testRepository.GetByIdAsync(testId);
                if (test == null)
                {
                    throw new Exception($"Test not found: {testId}");
                }

                // Check if the test is already in the suite
                if (suite.TestIds.Contains(testId))
                {
                    _logger.LogInformation("Test {TestId} is already in suite {SuiteId}", testId, suiteId);
                    return suite;
                }

                // Add the test to the suite
                suite.TestIds.Add(testId);
                suite.UpdatedAt = DateTime.UtcNow;

                return await _testSuiteRepository.UpdateAsync(suite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding test {TestId} to suite {SuiteId}", testId, suiteId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionTestSuite> RemoveTestFromSuiteAsync(Guid suiteId, Guid testId)
        {
            _logger.LogInformation("Removing test {TestId} from suite {SuiteId}", testId, suiteId);

            try
            {
                // Get the suite
                var suite = await _testSuiteRepository.GetByIdAsync(suiteId);
                if (suite == null)
                {
                    throw new Exception($"Test suite not found: {suiteId}");
                }

                // Check if the test is in the suite
                if (!suite.TestIds.Contains(testId))
                {
                    _logger.LogInformation("Test {TestId} is not in suite {SuiteId}", testId, suiteId);
                    return suite;
                }

                // Remove the test from the suite
                suite.TestIds.Remove(testId);
                suite.UpdatedAt = DateTime.UtcNow;

                return await _testSuiteRepository.UpdateAsync(suite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing test {TestId} from suite {SuiteId}", testId, suiteId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTest>> GenerateTestsAsync(Guid functionId)
        {
            _logger.LogInformation("Generating tests for function: {FunctionId}", functionId);

            try
            {
                // Get the function
                var function = await _functionService.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Generate tests based on the function's source code and metadata
                var tests = new List<FunctionTest>();

                // Basic test with empty parameters
                tests.Add(new FunctionTest
                {
                    Id = Guid.NewGuid(),
                    FunctionId = functionId,
                    Name = "Basic Test",
                    Description = $"Basic test for function {function.Name}",
                    Type = "unit",
                    InputParameters = new Dictionary<string, object>(),
                    Assertions = new List<FunctionTestAssertion>
                    {
                        new FunctionTestAssertion
                        {
                            Type = "notNull",
                            Path = "result",
                            Message = "Result should not be null"
                        }
                    },
                    TimeoutMs = 5000,
                    IsEnabled = true,
                    IsRequired = true,
                    Tags = new List<string> { "generated", "basic" },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Empty, // TODO: Get from context
                    UpdatedBy = Guid.Empty  // TODO: Get from context
                });

                // TODO: Generate more sophisticated tests based on function analysis

                // Save the generated tests
                foreach (var test in tests)
                {
                    await _testRepository.CreateAsync(test);
                }

                return tests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tests for function: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ValidateTestAsync(FunctionTest test)
        {
            _logger.LogInformation("Validating function test: {Name}", test.Name);

            var errors = new List<string>();

            try
            {
                // Check if the function exists
                var function = await _functionService.GetByIdAsync(test.FunctionId);
                if (function == null)
                {
                    errors.Add($"Function not found: {test.FunctionId}");
                    return errors;
                }

                // Validate name
                if (string.IsNullOrWhiteSpace(test.Name))
                {
                    errors.Add("Test name is required");
                }

                // Validate timeout
                if (test.TimeoutMs <= 0)
                {
                    errors.Add("Timeout must be greater than 0");
                }

                // Validate assertions
                if (test.Assertions == null || !test.Assertions.Any())
                {
                    errors.Add("At least one assertion is required");
                }
                else
                {
                    foreach (var assertion in test.Assertions)
                    {
                        if (string.IsNullOrWhiteSpace(assertion.Type))
                        {
                            errors.Add("Assertion type is required");
                        }

                        if (string.IsNullOrWhiteSpace(assertion.Path))
                        {
                            errors.Add("Assertion path is required");
                        }
                    }
                }

                return errors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating function test: {Name}", test.Name);
                errors.Add($"Validation error: {ex.Message}");
                return errors;
            }
        }

        /// <inheritdoc/>
        public async Task<object> GetTestCoverageAsync(Guid functionId)
        {
            _logger.LogInformation("Getting test coverage for function: {FunctionId}", functionId);

            try
            {
                // Get the function
                var function = await _functionService.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Get the tests
                var tests = await _testRepository.GetByFunctionIdAsync(functionId);

                // Get the latest test results
                var testResults = await _testResultRepository.GetLatestByFunctionIdAsync(functionId);

                // Calculate coverage metrics
                var totalTests = tests.Count();
                var passedTests = testResults.Count(r => r.Status == "passed");
                var failedTests = testResults.Count(r => r.Status == "failed");
                var errorTests = testResults.Count(r => r.Status == "error");
                var skippedTests = totalTests - passedTests - failedTests - errorTests;

                // TODO: Implement more sophisticated coverage analysis

                return new
                {
                    FunctionId = functionId,
                    FunctionName = function.Name,
                    TotalTests = totalTests,
                    PassedTests = passedTests,
                    FailedTests = failedTests,
                    ErrorTests = errorTests,
                    SkippedTests = skippedTests,
                    Coverage = totalTests > 0 ? (double)passedTests / totalTests * 100 : 0,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting test coverage for function: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTest>> ImportTestsAsync(Guid functionId, string fileContent, string fileType)
        {
            _logger.LogInformation("Importing tests for function: {FunctionId}, fileType: {FileType}", functionId, fileType);

            try
            {
                // Get the function
                var function = await _functionService.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Parse the file content based on the file type
                List<FunctionTest> tests = new List<FunctionTest>();

                switch (fileType.ToLower())
                {
                    case "json":
                        tests = JsonSerializer.Deserialize<List<FunctionTest>>(fileContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        break;

                    case "yaml":
                    case "yml":
                        // TODO: Implement YAML parsing
                        throw new NotImplementedException("YAML parsing is not implemented yet");

                    default:
                        throw new Exception($"Unsupported file type: {fileType}");
                }

                // Validate and save the imported tests
                var importedTests = new List<FunctionTest>();
                foreach (var test in tests)
                {
                    // Set the function ID
                    test.FunctionId = functionId;

                    // Validate the test
                    var validationErrors = await ValidateTestAsync(test);
                    if (validationErrors.Any())
                    {
                        _logger.LogWarning("Validation errors for test {Name}: {Errors}", test.Name, string.Join(", ", validationErrors));
                        continue;
                    }

                    // Set default values
                    test.Id = Guid.NewGuid();
                    test.CreatedAt = DateTime.UtcNow;
                    test.UpdatedAt = DateTime.UtcNow;

                    // Save the test
                    await _testRepository.CreateAsync(test);
                    importedTests.Add(test);
                }

                return importedTests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing tests for function: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> ExportTestsAsync(Guid functionId, string fileType)
        {
            _logger.LogInformation("Exporting tests for function: {FunctionId}, fileType: {FileType}", functionId, fileType);

            try
            {
                // Get the function
                var function = await _functionService.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Get the tests
                var tests = await _testRepository.GetByFunctionIdAsync(functionId);

                // Export the tests based on the file type
                switch (fileType.ToLower())
                {
                    case "json":
                        return JsonSerializer.Serialize(tests, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                    case "yaml":
                    case "yml":
                        // TODO: Implement YAML serialization
                        throw new NotImplementedException("YAML serialization is not implemented yet");

                    default:
                        throw new Exception($"Unsupported file type: {fileType}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting tests for function: {FunctionId}", functionId);
                throw;
            }
        }

        /// <summary>
        /// Evaluates assertions against a result
        /// </summary>
        /// <param name="assertions">Assertions to evaluate</param>
        /// <param name="result">Result to evaluate against</param>
        /// <returns>List of assertion results</returns>
        private List<FunctionTestAssertionResult> EvaluateAssertions(List<FunctionTestAssertion> assertions, object result)
        {
            var assertionResults = new List<FunctionTestAssertionResult>();

            foreach (var assertion in assertions)
            {
                var assertionResult = new FunctionTestAssertionResult
                {
                    Type = assertion.Type,
                    Path = assertion.Path,
                    ExpectedValue = assertion.ExpectedValue,
                    IsRequired = assertion.IsRequired
                };

                try
                {
                    // Get the actual value from the result using the path
                    var actualValue = GetValueFromPath(result, assertion.Path);
                    assertionResult.ActualValue = actualValue;

                    // Evaluate the assertion based on its type
                    switch (assertion.Type.ToLower())
                    {
                        case "equals":
                            assertionResult.Passed = AreEqual(actualValue, assertion.ExpectedValue, assertion.CaseSensitive);
                            break;

                        case "notequals":
                            assertionResult.Passed = !AreEqual(actualValue, assertion.ExpectedValue, assertion.CaseSensitive);
                            break;

                        case "contains":
                            assertionResult.Passed = Contains(actualValue, assertion.ExpectedValue, assertion.CaseSensitive);
                            break;

                        case "notcontains":
                            assertionResult.Passed = !Contains(actualValue, assertion.ExpectedValue, assertion.CaseSensitive);
                            break;

                        case "null":
                            assertionResult.Passed = actualValue == null;
                            break;

                        case "notnull":
                            assertionResult.Passed = actualValue != null;
                            break;

                        case "greaterthan":
                            assertionResult.Passed = IsGreaterThan(actualValue, assertion.ExpectedValue);
                            break;

                        case "lessthan":
                            assertionResult.Passed = IsLessThan(actualValue, assertion.ExpectedValue);
                            break;

                        case "regex":
                            assertionResult.Passed = MatchesRegex(actualValue, assertion.ExpectedValue?.ToString(), assertion.CaseSensitive);
                            break;

                        default:
                            assertionResult.Passed = false;
                            assertionResult.Message = $"Unknown assertion type: {assertion.Type}";
                            break;
                    }

                    if (!assertionResult.Passed)
                    {
                        assertionResult.Message = assertion.Message ?? $"Assertion failed: {assertion.Type} {assertion.Path}";
                    }
                }
                catch (Exception ex)
                {
                    assertionResult.Passed = false;
                    assertionResult.Message = $"Error evaluating assertion: {ex.Message}";
                }

                assertionResults.Add(assertionResult);
            }

            return assertionResults;
        }

        /// <summary>
        /// Gets a value from a path in an object
        /// </summary>
        /// <param name="obj">Object to get value from</param>
        /// <param name="path">Path to the value</param>
        /// <returns>The value at the path</returns>
        private object GetValueFromPath(object obj, string path)
        {
            if (obj == null || string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            // Handle the special case of "result" as the root object
            if (path == "result")
            {
                return obj;
            }

            // Split the path into parts
            var parts = path.Split('.');

            // Start with the root object
            var current = obj;

            // Navigate through the path
            foreach (var part in parts)
            {
                if (current == null)
                {
                    return null;
                }

                // Handle dictionary
                if (current is IDictionary<string, object> dict)
                {
                    if (dict.TryGetValue(part, out var value))
                    {
                        current = value;
                    }
                    else
                    {
                        return null;
                    }
                }
                // Handle object properties
                else
                {
                    var property = current.GetType().GetProperty(part);
                    if (property != null)
                    {
                        current = property.GetValue(current);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return current;
        }

        /// <summary>
        /// Checks if two values are equal
        /// </summary>
        /// <param name="actual">Actual value</param>
        /// <param name="expected">Expected value</param>
        /// <param name="caseSensitive">Whether to use case-sensitive comparison for strings</param>
        /// <returns>True if the values are equal, false otherwise</returns>
        private bool AreEqual(object actual, object expected, bool caseSensitive = true)
        {
            if (actual == null && expected == null)
            {
                return true;
            }

            if (actual == null || expected == null)
            {
                return false;
            }

            // Handle string comparison
            if (actual is string actualStr && expected is string expectedStr)
            {
                return caseSensitive
                    ? actualStr == expectedStr
                    : actualStr.Equals(expectedStr, StringComparison.OrdinalIgnoreCase);
            }

            // Handle numeric comparison
            if (IsNumeric(actual) && IsNumeric(expected))
            {
                return Convert.ToDouble(actual) == Convert.ToDouble(expected);
            }

            // Handle boolean comparison
            if (actual is bool actualBool && expected is bool expectedBool)
            {
                return actualBool == expectedBool;
            }

            // Default comparison
            return actual.Equals(expected);
        }

        /// <summary>
        /// Checks if a value contains another value
        /// </summary>
        /// <param name="actual">Actual value</param>
        /// <param name="expected">Expected value</param>
        /// <param name="caseSensitive">Whether to use case-sensitive comparison for strings</param>
        /// <returns>True if the actual value contains the expected value, false otherwise</returns>
        private bool Contains(object actual, object expected, bool caseSensitive = true)
        {
            if (actual == null || expected == null)
            {
                return false;
            }

            // Handle string comparison
            if (actual is string actualStr && expected is string expectedStr)
            {
                return caseSensitive
                    ? actualStr.Contains(expectedStr)
                    : actualStr.Contains(expectedStr, StringComparison.OrdinalIgnoreCase);
            }

            // Handle collection comparison
            if (actual is IEnumerable<object> collection)
            {
                return collection.Contains(expected);
            }

            return false;
        }

        /// <summary>
        /// Checks if a value is greater than another value
        /// </summary>
        /// <param name="actual">Actual value</param>
        /// <param name="expected">Expected value</param>
        /// <returns>True if the actual value is greater than the expected value, false otherwise</returns>
        private bool IsGreaterThan(object actual, object expected)
        {
            if (actual == null || expected == null)
            {
                return false;
            }

            // Handle numeric comparison
            if (IsNumeric(actual) && IsNumeric(expected))
            {
                return Convert.ToDouble(actual) > Convert.ToDouble(expected);
            }

            // Handle string comparison
            if (actual is string actualStr && expected is string expectedStr)
            {
                return string.Compare(actualStr, expectedStr, StringComparison.Ordinal) > 0;
            }

            return false;
        }

        /// <summary>
        /// Checks if a value is less than another value
        /// </summary>
        /// <param name="actual">Actual value</param>
        /// <param name="expected">Expected value</param>
        /// <returns>True if the actual value is less than the expected value, false otherwise</returns>
        private bool IsLessThan(object actual, object expected)
        {
            if (actual == null || expected == null)
            {
                return false;
            }

            // Handle numeric comparison
            if (IsNumeric(actual) && IsNumeric(expected))
            {
                return Convert.ToDouble(actual) < Convert.ToDouble(expected);
            }

            // Handle string comparison
            if (actual is string actualStr && expected is string expectedStr)
            {
                return string.Compare(actualStr, expectedStr, StringComparison.Ordinal) < 0;
            }

            return false;
        }

        /// <summary>
        /// Checks if a value matches a regular expression
        /// </summary>
        /// <param name="actual">Actual value</param>
        /// <param name="pattern">Regular expression pattern</param>
        /// <param name="caseSensitive">Whether to use case-sensitive matching</param>
        /// <returns>True if the actual value matches the pattern, false otherwise</returns>
        private bool MatchesRegex(object actual, string pattern, bool caseSensitive = true)
        {
            if (actual == null || pattern == null)
            {
                return false;
            }

            // Handle string comparison
            if (actual is string actualStr)
            {
                var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                return Regex.IsMatch(actualStr, pattern, options);
            }

            return false;
        }

        /// <summary>
        /// Checks if a value is numeric
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>True if the value is numeric, false otherwise</returns>
        private bool IsNumeric(object value)
        {
            return value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint ||
                   value is long || value is ulong || value is float || value is double || value is decimal;
        }
    }
}
