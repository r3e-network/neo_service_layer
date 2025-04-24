using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Implementation of the function repository
    /// </summary>
    public class FunctionRepository : IFunctionRepository
    {
        private readonly ILogger<FunctionRepository> _logger;
        private readonly Dictionary<Guid, Core.Models.Function> _functions;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public FunctionRepository(ILogger<FunctionRepository> logger)
        {
            _logger = logger;
            _functions = new Dictionary<Guid, Core.Models.Function>();
        }

        /// <inheritdoc/>
        public Task<Core.Models.Function> CreateAsync(Core.Models.Function function)
        {
            _logger.LogInformation("Creating function: {Id}, Name: {Name}", function.Id, function.Name);

            if (function.Id == Guid.Empty)
            {
                function.Id = Guid.NewGuid();
            }

            function.CreatedAt = DateTime.UtcNow;
            function.UpdatedAt = DateTime.UtcNow;
            function.Status = "Active";

            _functions[function.Id] = function;

            return Task.FromResult(function);
        }

        /// <inheritdoc/>
        public Task<Core.Models.Function?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function by ID: {Id}", id);

            _functions.TryGetValue(id, out var function);
            return Task.FromResult(function as Core.Models.Function);
        }

        /// <summary>
        /// This method is kept for backward compatibility
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="accountId">Account ID</param>
        /// <returns>The function if found, null otherwise</returns>
        public Task<Core.Models.Function> GetByNameAsync(string name, Guid accountId)
        {
            _logger.LogInformation("Getting function by name: {Name}, AccountId: {AccountId}", name, accountId);

            var function = _functions.Values.FirstOrDefault(f =>
                f.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                f.AccountId == accountId);

            return Task.FromResult(function);
        }

        /// <summary>
        /// This method is kept for backward compatibility
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of functions for the account</returns>
        public Task<IEnumerable<Core.Models.Function>> GetByAccountIdAsync(Guid accountId)
        {
            _logger.LogInformation("Getting functions by account ID: {AccountId}", accountId);

            var functions = _functions.Values.Where(f => f.AccountId == accountId).ToList();
            return Task.FromResult<IEnumerable<Core.Models.Function>>(functions);
        }

        /// <summary>
        /// This method is kept for backward compatibility
        /// </summary>
        /// <param name="function">Function to update</param>
        /// <returns>The updated function</returns>
        public Task<Core.Models.Function> UpdateAsync(Core.Models.Function function)
        {
            _logger.LogInformation("Updating function: {Id}", function.Id);

            if (!_functions.ContainsKey(function.Id))
            {
                return Task.FromResult<Core.Models.Function>(null);
            }

            function.UpdatedAt = DateTime.UtcNow;
            _functions[function.Id] = function;

            return Task.FromResult(function);
        }

        /// <summary>
        /// This method is kept for backward compatibility
        /// </summary>
        /// <returns>List of all functions</returns>
        public Task<IEnumerable<Core.Models.Function>> GetAllAsync()
        {
            _logger.LogInformation("Getting all functions");

            return Task.FromResult<IEnumerable<Core.Models.Function>>(_functions.Values.ToList());
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Core.Models.Function>> GetByNameAsync(string name, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting functions by name: {Name}, limit: {Limit}, offset: {Offset}", name, limit, offset);

            var functions = _functions.Values
                .Where(f => f.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Skip(offset)
                .Take(limit)
                .ToList();

            return Task.FromResult<IEnumerable<Core.Models.Function>>(functions);
        }

        /// <summary>
        /// Gets a function by name and account ID
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="accountId">Account ID</param>
        /// <returns>The function if found, null otherwise</returns>
        public Task<Core.Models.Function> GetByNameAndAccountIdAsync(string name, Guid accountId)
        {
            _logger.LogInformation("Getting function by name: {Name} and account ID: {AccountId}", name, accountId);

            var function = _functions.Values
                .FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && f.AccountId == accountId);

            return Task.FromResult(function);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Core.Models.Function>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting functions by account ID: {AccountId}, limit: {Limit}, offset: {Offset}", accountId, limit, offset);

            var functions = _functions.Values
                .Where(f => f.AccountId == accountId)
                .Skip(offset)
                .Take(limit)
                .ToList();
            return Task.FromResult<IEnumerable<Core.Models.Function>>(functions);
        }

        /// <inheritdoc/>
        public Task<Core.Models.Function> UpdateAsync(Guid id, Core.Models.Function function)
        {
            _logger.LogInformation("Updating function: {Id}", id);

            if (!_functions.ContainsKey(id))
            {
                return Task.FromResult<Core.Models.Function>(null);
            }

            function.Id = id;
            function.UpdatedAt = DateTime.UtcNow;
            _functions[id] = function;

            return Task.FromResult(function);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function: {Id}", id);

            return Task.FromResult(_functions.Remove(id));
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Core.Models.Function>> GetAllAsync(int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting all functions, limit: {Limit}, offset: {Offset}", limit, offset);

            return Task.FromResult<IEnumerable<Core.Models.Function>>(
                _functions.Values
                    .Skip(offset)
                    .Take(limit)
                    .ToList());
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Core.Models.Function>> GetByRuntimeAsync(string runtime, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting functions by runtime: {Runtime}, limit: {Limit}, offset: {Offset}", runtime, limit, offset);

            var functions = _functions.Values
                .Where(f => f.Runtime.Equals(runtime, StringComparison.OrdinalIgnoreCase))
                .Skip(offset)
                .Take(limit)
                .ToList();

            return Task.FromResult<IEnumerable<Core.Models.Function>>(functions);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Core.Models.Function>> GetByTagsAsync(List<string> tags, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting functions by tags: {Tags}, limit: {Limit}, offset: {Offset}", string.Join(", ", tags), limit, offset);

            var functions = _functions.Values
                .Where(f => f.Tags != null && tags.All(tag => f.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                .Skip(offset)
                .Take(limit)
                .ToList();

            return Task.FromResult<IEnumerable<Core.Models.Function>>(functions);
        }

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(Guid id)
        {
            _logger.LogInformation("Checking if function exists: {Id}", id);

            return Task.FromResult(_functions.ContainsKey(id));
        }

        /// <inheritdoc/>
        public Task<int> CountAsync()
        {
            _logger.LogInformation("Counting all functions");

            return Task.FromResult(_functions.Count);
        }

        /// <inheritdoc/>
        public Task<int> CountByAccountIdAsync(Guid accountId)
        {
            _logger.LogInformation("Counting functions by account ID: {AccountId}", accountId);

            return Task.FromResult(_functions.Values.Count(f => f.AccountId == accountId));
        }

        /// <inheritdoc/>
        public Task<int> CountByRuntimeAsync(string runtime)
        {
            _logger.LogInformation("Counting functions by runtime: {Runtime}", runtime);

            return Task.FromResult(_functions.Values.Count(f => f.Runtime.Equals(runtime, StringComparison.OrdinalIgnoreCase)));
        }

        /// <inheritdoc/>
        public Task<int> CountByTagsAsync(List<string> tags)
        {
            _logger.LogInformation("Counting functions by tags: {Tags}", string.Join(", ", tags));

            return Task.FromResult(_functions.Values.Count(f => f.Tags != null && tags.All(tag => f.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))));
        }
    }
}
