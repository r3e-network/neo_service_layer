using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for account management service
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="username">Username for the account</param>
        /// <param name="email">Email address for the account</param>
        /// <param name="password">Password for the account</param>
        /// <param name="neoAddress">Optional Neo N3 address to associate with the account</param>
        /// <returns>The created account</returns>
        Task<Account> RegisterAsync(string username, string email, string password, string neoAddress = null);

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="usernameOrEmail">Username or email address</param>
        /// <param name="password">Password for the account</param>
        /// <returns>JWT token if authentication is successful</returns>
        Task<string> AuthenticateAsync(string usernameOrEmail, string password);

        /// <summary>
        /// Gets an account by ID
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <returns>The account if found, null otherwise</returns>
        Task<Account> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>The user if found, null otherwise</returns>
        Task<Account> GetUserAsync(Guid id);

        /// <summary>
        /// Gets user roles by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user roles</returns>
        Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// Gets user permissions by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user permissions</returns>
        Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);

        /// <summary>
        /// Gets an account by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>The account if found, null otherwise</returns>
        Task<Account> GetByUsernameAsync(string username);

        /// <summary>
        /// Gets an account by email address
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>The account if found, null otherwise</returns>
        Task<Account> GetByEmailAsync(string email);

        /// <summary>
        /// Gets an account by Neo N3 address
        /// </summary>
        /// <param name="neoAddress">Neo N3 address</param>
        /// <returns>The account if found, null otherwise</returns>
        Task<Account> GetByNeoAddressAsync(string neoAddress);

        /// <summary>
        /// Updates an account
        /// </summary>
        /// <param name="account">Account to update</param>
        /// <returns>The updated account</returns>
        Task<Account> UpdateAsync(Account account);

        /// <summary>
        /// Changes the password for an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="currentPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if password was changed successfully, false otherwise</returns>
        Task<bool> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword);

        /// <summary>
        /// Verifies an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>True if account was verified successfully, false otherwise</returns>
        Task<bool> VerifyAccountAsync(Guid accountId);

        /// <summary>
        /// Adds credits to an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="amount">Amount of credits to add</param>
        /// <returns>The updated account</returns>
        Task<Account> AddCreditsAsync(Guid accountId, decimal amount);

        /// <summary>
        /// Deducts credits from an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="amount">Amount of credits to deduct</param>
        /// <returns>The updated account</returns>
        Task<Account> DeductCreditsAsync(Guid accountId, decimal amount);

        /// <summary>
        /// Gets all accounts
        /// </summary>
        /// <returns>List of all accounts</returns>
        Task<IEnumerable<Account>> GetAllAsync();

        /// <summary>
        /// Deletes an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>True if account was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid accountId);
    }
}
