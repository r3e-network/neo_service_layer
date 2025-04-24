using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Repositories;
using NeoServiceLayer.Core.Utilities;

namespace NeoServiceLayer.Services.Secrets.Repositories
{
    /// <summary>
    /// Implementation of the secrets repository
    /// </summary>
    public class SecretsRepository : ISecretsRepository
    {
        private readonly ILogger<SecretsRepository> _logger;
        private readonly IGenericRepository<Secret, Guid> _repository;
        private readonly IStorageProvider _storageProvider;
        private const string CollectionName = "secrets";

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretsRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public SecretsRepository(
            ILogger<SecretsRepository> logger,
            IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
            _repository = new GenericRepository<Secret, Guid>(logger, storageProvider, CollectionName);
        }

        /// <inheritdoc/>
        public async Task<Secret> CreateAsync(Secret secret)
        {
            _logger.LogInformation("Creating secret: {Id}, Name: {Name}", secret.Id, secret.Name);

            if (secret.Id == Guid.Empty)
            {
                secret.Id = Guid.NewGuid();
            }

            secret.CreatedAt = DateTime.UtcNow;
            secret.UpdatedAt = DateTime.UtcNow;
            secret.Version = 1;

            return await _repository.CreateAsync(secret);
        }

        /// <inheritdoc/>
        public async Task<Secret> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting secret by ID: {Id}", id);

            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting secret by ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Secret> GetByNameAsync(string name, Guid accountId)
        {
            _logger.LogInformation("Getting secret by name: {Name}, AccountId: {AccountId}", name, accountId);

            try
            {
                var secrets = await _repository.FindAsync(s =>
                    s.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                    s.AccountId == accountId);
                return secrets.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting secret by name: {Name}, AccountId: {AccountId}", name, accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Secret>> GetByAccountIdAsync(Guid accountId)
        {
            _logger.LogInformation("Getting secrets by account ID: {AccountId}", accountId);

            try
            {
                return await _repository.FindAsync(s => s.AccountId == accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting secrets by account ID: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Secret>> GetByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Getting secrets by function ID: {FunctionId}", functionId);

            try
            {
                return await _repository.FindAsync(s => s.AllowedFunctionIds.Contains(functionId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting secrets by function ID: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Secret> UpdateAsync(Secret secret)
        {
            _logger.LogInformation("Updating secret: {Id}", secret.Id);

            try
            {
                // Check if secret exists
                var exists = await _repository.ExistsAsync(secret.Id);
                if (!exists)
                {
                    return null;
                }

                secret.UpdatedAt = DateTime.UtcNow;
                return await _repository.UpdateAsync(secret.Id, secret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating secret: {Id}", secret.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting secret: {Id}", id);

            try
            {
                return await _repository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting secret: {Id}", id);
                throw;
            }
        }
    }
}
