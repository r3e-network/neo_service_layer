using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for secrets management service
    /// </summary>
    public interface ISecretsService
    {
        /// <summary>
        /// Creates a new secret
        /// </summary>
        /// <param name="name">Name for the secret</param>
        /// <param name="value">Value of the secret</param>
        /// <param name="description">Description of the secret</param>
        /// <param name="accountId">Account ID that owns the secret</param>
        /// <param name="allowedFunctionIds">List of function IDs that have access to the secret</param>
        /// <param name="expiresAt">Optional expiration date for the secret</param>
        /// <returns>The created secret</returns>
        Task<Secret> CreateSecretAsync(string name, string value, string description, Guid accountId, List<Guid> allowedFunctionIds, DateTime? expiresAt = null);

        /// <summary>
        /// Gets a secret by ID
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <returns>The secret if found, null otherwise</returns>
        Task<Secret> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a secret by name and account ID
        /// </summary>
        /// <param name="name">Secret name</param>
        /// <param name="accountId">Account ID</param>
        /// <returns>The secret if found, null otherwise</returns>
        Task<Secret> GetByNameAsync(string name, Guid accountId);

        /// <summary>
        /// Gets secrets by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of secrets owned by the account</returns>
        Task<IEnumerable<Secret>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Gets secrets accessible by a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of secrets accessible by the function</returns>
        Task<IEnumerable<Secret>> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Updates a secret
        /// </summary>
        /// <param name="secret">Secret to update</param>
        /// <returns>The updated secret</returns>
        Task<Secret> UpdateAsync(Secret secret);

        /// <summary>
        /// Updates the value of a secret
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <param name="value">New value for the secret</param>
        /// <returns>The updated secret</returns>
        Task<Secret> UpdateValueAsync(Guid id, string value);

        /// <summary>
        /// Updates the allowed functions for a secret
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <param name="allowedFunctionIds">List of function IDs that have access to the secret</param>
        /// <returns>The updated secret</returns>
        Task<Secret> UpdateAllowedFunctionsAsync(Guid id, List<Guid> allowedFunctionIds);

        /// <summary>
        /// Decrypts and returns the value of a secret
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <param name="functionId">Function ID requesting access</param>
        /// <returns>The decrypted value of the secret</returns>
        Task<string> GetSecretValueAsync(Guid id, Guid functionId);

        /// <summary>
        /// Rotates the value of a secret
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <param name="newValue">New value for the secret</param>
        /// <returns>The updated secret</returns>
        Task<Secret> RotateSecretAsync(Guid id, string newValue);

        /// <summary>
        /// Deletes a secret
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <returns>True if secret was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Checks if a function has access to a secret
        /// </summary>
        /// <param name="secretId">Secret ID</param>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the function has access to the secret, false otherwise</returns>
        Task<bool> HasAccessAsync(Guid secretId, Guid functionId);
    }
}
