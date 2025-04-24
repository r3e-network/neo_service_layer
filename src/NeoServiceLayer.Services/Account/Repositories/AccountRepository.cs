using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Common.Repositories;

namespace NeoServiceLayer.Services.Account.Repositories
{
    /// <summary>
    /// Implementation of the account repository
    /// </summary>
    public class AccountRepository : GenericRepository<Core.Models.Account, Guid>, IAccountRepository
    {
        private readonly ILogger<AccountRepository> _logger;
        private readonly IDatabaseService _databaseService;
        private readonly string _collectionName = "accounts";

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public AccountRepository(ILogger<AccountRepository> logger, IDatabaseService databaseService)
            : base(logger, databaseService, "accounts")
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <inheritdoc/>
        public new async Task<Core.Models.Account> AddAsync(Core.Models.Account account)
        {
            _logger.LogInformation("Creating account: {Id}", account.Id);

            if (account.Id == Guid.Empty)
            {
                account.Id = Guid.NewGuid();
            }

            account.CreatedAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;

            return await base.AddAsync(account);
        }

        /// <inheritdoc/>
        public new async Task<Core.Models.Account> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting account by ID: {Id}", id);

            return await base.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Account> GetByUsernameAsync(string username)
        {
            _logger.LogInformation("Getting account by username: {Username}", username);

            // Get all accounts and filter in memory
            var accounts = await GetAllAsync();
            return accounts.FirstOrDefault(a => a.Username != null && a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Account> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Getting account by email: {Email}", email);

            // Get all accounts and filter in memory
            var accounts = await GetAllAsync();
            return accounts.FirstOrDefault(a => a.Email != null && a.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Account> GetByNeoAddressAsync(string neoAddress)
        {
            _logger.LogInformation("Getting account by Neo address: {NeoAddress}", neoAddress);

            // Get all accounts and filter in memory
            var accounts = await GetAllAsync();
            return accounts.FirstOrDefault(a => a.NeoAddress != null && a.NeoAddress.Equals(neoAddress, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public new async Task<Core.Models.Account> UpdateAsync(Core.Models.Account account)
        {
            _logger.LogInformation("Updating account: {Id}", account.Id);

            account.UpdatedAt = DateTime.UtcNow;
            return await base.UpdateAsync(account);
        }

        /// <inheritdoc/>
        public new async Task<IEnumerable<Core.Models.Account>> GetAllAsync()
        {
            _logger.LogInformation("Getting all accounts");

            return await base.GetAllAsync();
        }

        /// <inheritdoc/>
        public new async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting account: {Id}", id);

            return await base.DeleteAsync(id);
        }
    }
}
