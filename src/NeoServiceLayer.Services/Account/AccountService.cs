using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Extensions;
using NeoServiceLayer.Services.Account.Repositories;
using NeoServiceLayer.Services.Common.Utilities;
using NeoServiceLayer.Services.Common.Extensions;

namespace NeoServiceLayer.Services.Account
{
    /// <summary>
    /// Implementation of the account service
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAccountRepository _accountRepository;
        private readonly IEnclaveService _enclaveService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="accountRepository">Account repository</param>
        /// <param name="enclaveService">Enclave service</param>
        public AccountService(ILogger<AccountService> logger, IConfiguration configuration,
            IAccountRepository accountRepository, IEnclaveService enclaveService)
        {
            _logger = logger;
            _configuration = configuration;
            _accountRepository = accountRepository;
            _enclaveService = enclaveService;
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Account> RegisterAsync(string username, string email, string password, string neoAddress = null)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Username"] = username,
                ["Email"] = email,
                ["HasNeoAddress"] = !string.IsNullOrEmpty(neoAddress)
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "RegisterAccount", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(username, "Username");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(email, "Email");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(password, "Password");

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidEmail(email))
                {
                    throw new AccountException("Invalid email format");
                }

                if (password.Length < 8)
                {
                    throw new AccountException("Password must be at least 8 characters long");
                }

                // Validate Neo address if provided
                if (!string.IsNullOrEmpty(neoAddress) && !NeoServiceLayer.Core.Extensions.StringExtensions.IsValidNeoAddress(neoAddress))
                {
                    throw new AccountException("Invalid Neo address format");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, Core.Models.Account>(
                    _logger,
                    async () =>
                    {
                        // Check if username or email already exists
                        if (await _accountRepository.GetByUsernameAsync(username) != null)
                        {
                            throw new AccountException("Username already exists");
                        }

                        if (await _accountRepository.GetByEmailAsync(email) != null)
                        {
                            throw new AccountException("Email already exists");
                        }

                        // Send registration request to enclave
                        var registrationRequest = new
                        {
                            Username = username,
                            Email = email,
                            Password = password,
                            NeoAddress = neoAddress
                        };

                        var account = await _enclaveService.SendRequestAsync<object, Core.Models.Account>(
                            Constants.EnclaveServiceTypes.Account,
                            Constants.AccountOperations.Register,
                            registrationRequest);

                        // Save account to repository
                        await _accountRepository.AddAsync(account);

                        additionalData["AccountId"] = account.Id;

                        return account;
                    },
                    "RegisterAccount",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new AccountException("Failed to register account");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "RegisterAccount", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "RegisterAccount", requestId, ex, 0, additionalData);
                throw new AccountException("Error registering account", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> AuthenticateAsync(string usernameOrEmail, string password)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["UsernameOrEmail"] = usernameOrEmail
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "AuthenticateUser", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(usernameOrEmail, "Username or email");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(password, "Password");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, string>(
                    _logger,
                    async () =>
                    {
                        // Send authentication request to enclave
                        var authRequest = new
                        {
                            UsernameOrEmail = usernameOrEmail,
                            Password = password
                        };

                        var authResult = await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Account,
                            Constants.AccountOperations.Authenticate,
                            authRequest);

                        // Get account from repository
                        Core.Models.Account account;
                        if (usernameOrEmail.Contains("@"))
                        {
                            account = await _accountRepository.GetByEmailAsync(usernameOrEmail);
                        }
                        else
                        {
                            account = await _accountRepository.GetByUsernameAsync(usernameOrEmail);
                        }

                        if (account == null)
                        {
                            throw new AccountException("Account not found");
                        }

                        additionalData["AccountId"] = account.Id;
                        additionalData["Username"] = account.Username;
                        additionalData["IsVerified"] = account.IsVerified;

                        // Generate JWT token
                        var token = GenerateJwtToken(account);

                        return token;
                    },
                    "AuthenticateUser",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new AccountException("Failed to authenticate user");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "AuthenticateUser", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "AuthenticateUser", requestId, ex, 0, additionalData);
                throw new AccountException("Error authenticating user", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Account> GetByIdAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetAccountById", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Account ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, Core.Models.Account>(
                    _logger,
                    async () => await _accountRepository.GetByIdAsync(id),
                    "GetAccountById",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["Username"] = result.result.Username;
                    additionalData["Email"] = result.result.Email;
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetAccountById", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetAccountById", requestId, ex, 0, additionalData);
                throw new AccountException($"Error getting account by ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Account> GetByUsernameAsync(string username)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Username"] = username
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetAccountByUsername", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(username, "Username");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, Core.Models.Account>(
                    _logger,
                    async () => await _accountRepository.GetByUsernameAsync(username),
                    "GetAccountByUsername",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["AccountId"] = result.result.Id;
                    additionalData["Email"] = result.result.Email;
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetAccountByUsername", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetAccountByUsername", requestId, ex, 0, additionalData);
                throw new AccountException($"Error getting account by username {username}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Account> GetByEmailAsync(string email)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Email"] = email
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetAccountByEmail", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(email, "Email");

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidEmail(email))
                {
                    throw new AccountException("Invalid email format");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, Core.Models.Account>(
                    _logger,
                    async () => await _accountRepository.GetByEmailAsync(email),
                    "GetAccountByEmail",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["AccountId"] = result.result.Id;
                    additionalData["Username"] = result.result.Username;
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetAccountByEmail", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetAccountByEmail", requestId, ex, 0, additionalData);
                throw new AccountException($"Error getting account by email {email}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Account> GetByNeoAddressAsync(string neoAddress)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["NeoAddress"] = neoAddress
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetAccountByNeoAddress", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(neoAddress, "Neo address");

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidNeoAddress(neoAddress))
                {
                    throw new AccountException("Invalid Neo address format");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, Core.Models.Account>(
                    _logger,
                    async () => await _accountRepository.GetByNeoAddressAsync(neoAddress),
                    "GetAccountByNeoAddress",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["AccountId"] = result.result.Id;
                    additionalData["Username"] = result.result.Username;
                    additionalData["Email"] = result.result.Email;
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetAccountByNeoAddress", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetAccountByNeoAddress", requestId, ex, 0, additionalData);
                throw new AccountException($"Error getting account by Neo address {neoAddress}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Account> UpdateAsync(Core.Models.Account account)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["AccountId"] = account?.Id
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "UpdateAccount", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNull(account, nameof(account));
                Common.Utilities.ValidationUtility.ValidateGuid(account.Id, "Account ID");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(account.Username, "Username");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(account.Email, "Email");

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidEmail(account.Email))
                {
                    throw new AccountException("Invalid email format");
                }

                if (!string.IsNullOrEmpty(account.NeoAddress) && !NeoServiceLayer.Core.Extensions.StringExtensions.IsValidNeoAddress(account.NeoAddress))
                {
                    throw new AccountException("Invalid Neo address format");
                }

                additionalData["Username"] = account.Username;
                additionalData["Email"] = account.Email;

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, Core.Models.Account>(
                    _logger,
                    async () => await _accountRepository.UpdateAsync(account),
                    "UpdateAccount",
                    requestId,
                    additionalData);

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "UpdateAccount", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "UpdateAccount", requestId, ex, 0, additionalData);
                throw new AccountException($"Error updating account {account?.Id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["AccountId"] = accountId
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "ChangePassword", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(accountId, "Account ID");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(currentPassword, "Current password");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(newPassword, "New password");

                if (newPassword.Length < 8)
                {
                    throw new AccountException("New password must be at least 8 characters long");
                }

                if (currentPassword == newPassword)
                {
                    throw new AccountException("New password must be different from current password");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, bool>(
                    _logger,
                    async () =>
                    {
                        // Verify account exists
                        var account = await _accountRepository.GetByIdAsync(accountId);
                        if (account == null)
                        {
                            throw new AccountException("Account not found");
                        }

                        additionalData["Username"] = account.Username;

                        // Send password change request to enclave
                        var passwordChangeRequest = new
                        {
                            AccountId = accountId,
                            CurrentPassword = currentPassword,
                            NewPassword = newPassword
                        };

                        await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Account,
                            Constants.AccountOperations.ChangePassword,
                            passwordChangeRequest);

                        return true;
                    },
                    "ChangePassword",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new AccountException("Failed to change password");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "ChangePassword", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "ChangePassword", requestId, ex, 0, additionalData);
                throw new AccountException("Error changing password", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> VerifyAccountAsync(Guid accountId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["AccountId"] = accountId
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "VerifyAccount", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(accountId, "Account ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, bool>(
                    _logger,
                    async () =>
                    {
                        // Send account verification request to enclave
                        var verificationRequest = new
                        {
                            AccountId = accountId
                        };

                        await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Account,
                            Constants.AccountOperations.VerifyAccount,
                            verificationRequest);

                        // Update account in repository
                        var account = await _accountRepository.GetByIdAsync(accountId);
                        if (account == null)
                        {
                            throw new AccountException("Account not found");
                        }

                        additionalData["Username"] = account.Username;
                        additionalData["Email"] = account.Email;

                        if (account.IsVerified)
                        {
                            Common.Utilities.LoggingUtility.LogWarning(_logger, "Account is already verified", requestId, additionalData);
                            return true;
                        }

                        account.IsVerified = true;
                        account.UpdatedAt = DateTime.UtcNow;
                        await _accountRepository.UpdateAsync(account);

                        return true;
                    },
                    "VerifyAccount",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new AccountException("Failed to verify account");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "VerifyAccount", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "VerifyAccount", requestId, ex, 0, additionalData);
                throw new AccountException("Error verifying account", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Account> AddCreditsAsync(Guid accountId, decimal amount)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["AccountId"] = accountId,
                ["Amount"] = amount
            };

            LoggingUtility.LogOperationStart(_logger, "AddCredits", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(accountId, "Account ID");
                ValidationUtility.ValidateGreaterThanZero(amount, "Amount");

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, Core.Models.Account>(
                    _logger,
                    async () =>
                    {
                        var account = await _accountRepository.GetByIdAsync(accountId);
                        if (account == null)
                        {
                            throw new AccountException("Account not found");
                        }

                        additionalData["Username"] = account.Username;
                        additionalData["PreviousBalance"] = account.Credits;

                        account.Credits += amount;
                        account.UpdatedAt = DateTime.UtcNow;

                        var updatedAccount = await _accountRepository.UpdateAsync(account);

                        additionalData["NewBalance"] = updatedAccount.Credits;

                        return updatedAccount;
                    },
                    "AddCredits",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new AccountException("Failed to add credits");
                }

                LoggingUtility.LogOperationSuccess(_logger, "AddCredits", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "AddCredits", requestId, ex, 0, additionalData);
                throw new AccountException("Error adding credits", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Account> DeductCreditsAsync(Guid accountId, decimal amount)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["AccountId"] = accountId,
                ["Amount"] = amount
            };

            LoggingUtility.LogOperationStart(_logger, "DeductCredits", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(accountId, "Account ID");
                ValidationUtility.ValidateGreaterThanZero(amount, "Amount");

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, Core.Models.Account>(
                    _logger,
                    async () =>
                    {
                        var account = await _accountRepository.GetByIdAsync(accountId);
                        if (account == null)
                        {
                            throw new AccountException("Account not found");
                        }

                        additionalData["Username"] = account.Username;
                        additionalData["PreviousBalance"] = account.Credits;

                        if (account.Credits < amount)
                        {
                            throw new AccountException("Insufficient credits");
                        }

                        account.Credits -= amount;
                        account.UpdatedAt = DateTime.UtcNow;

                        var updatedAccount = await _accountRepository.UpdateAsync(account);

                        additionalData["NewBalance"] = updatedAccount.Credits;

                        return updatedAccount;
                    },
                    "DeductCredits",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new AccountException("Failed to deduct credits");
                }

                LoggingUtility.LogOperationSuccess(_logger, "DeductCredits", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "DeductCredits", requestId, ex, 0, additionalData);
                throw new AccountException("Error deducting credits", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Account> GetUserAsync(Guid id)
        {
            // This is just an alias for GetByIdAsync for compatibility with the interface
            return await GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["UserId"] = userId
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetUserRoles", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(userId, "User ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, IEnumerable<string>>(
                    _logger,
                    async () =>
                    {
                        // Get account from repository
                        var account = await _accountRepository.GetByIdAsync(userId);
                        if (account == null)
                        {
                            throw new AccountException("Account not found");
                        }

                        additionalData["Username"] = account.Username;

                        // For now, return a simple role based on account type
                        // In a real implementation, this would fetch roles from a database
                        var roles = new List<string>();

                        if (account.IsAdmin)
                        {
                            roles.Add("admin");
                        }
                        else
                        {
                            roles.Add("user");
                        }

                        return roles;
                    },
                    "GetUserRoles",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new AccountException("Failed to get user roles");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetUserRoles", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetUserRoles", requestId, ex, 0, additionalData);
                throw new AccountException($"Error getting roles for user {userId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["UserId"] = userId
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetUserPermissions", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(userId, "User ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, IEnumerable<string>>(
                    _logger,
                    async () =>
                    {
                        // Get account from repository
                        var account = await _accountRepository.GetByIdAsync(userId);
                        if (account == null)
                        {
                            throw new AccountException("Account not found");
                        }

                        additionalData["Username"] = account.Username;

                        // For now, return simple permissions based on account type
                        // In a real implementation, this would fetch permissions from a database
                        var permissions = new List<string>();

                        if (account.IsAdmin)
                        {
                            permissions.Add("function:create");
                            permissions.Add("function:update");
                            permissions.Add("function:delete");
                            permissions.Add("function:execute");
                            permissions.Add("account:manage");
                        }
                        else
                        {
                            permissions.Add("function:execute");
                            permissions.Add("function:create");
                            permissions.Add("function:update:own");
                            permissions.Add("function:delete:own");
                        }

                        return permissions;
                    },
                    "GetUserPermissions",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new AccountException("Failed to get user permissions");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetUserPermissions", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetUserPermissions", requestId, ex, 0, additionalData);
                throw new AccountException($"Error getting permissions for user {userId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Account>> GetAllAsync()
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>();

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetAllAccounts", requestId, additionalData);

            try
            {
                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, IEnumerable<Core.Models.Account>>(
                    _logger,
                    async () => await _accountRepository.GetAllAsync(),
                    "GetAllAccounts",
                    requestId,
                    additionalData);

                if (result.success)
                {
                    if (result.result != null)
                    {
                        additionalData["AccountCount"] = result.result.Count();
                    }

                    Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetAllAccounts", requestId, 0, additionalData);

                    return result.result;
                }
                else
                {
                    throw new AccountException("Failed to get all accounts");
                }
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetAllAccounts", requestId, ex, 0, additionalData);
                throw new AccountException("Error getting all accounts", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid accountId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["AccountId"] = accountId
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "DeleteAccount", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(accountId, "Account ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<AccountService, bool>(
                    _logger,
                    async () =>
                    {
                        // Verify account exists
                        var account = await _accountRepository.GetByIdAsync(accountId);
                        if (account == null)
                        {
                            throw new AccountException("Account not found");
                        }

                        additionalData["Username"] = account.Username;
                        additionalData["Email"] = account.Email;

                        return await _accountRepository.DeleteAsync(accountId);
                    },
                    "DeleteAccount",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new AccountException("Failed to delete account");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "DeleteAccount", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "DeleteAccount", requestId, ex, 0, additionalData);
                throw new AccountException($"Error deleting account {accountId}", ex);
            }
        }

        private string GenerateJwtToken(Core.Models.Account account)
        {
            try
            {
                Common.Utilities.ValidationUtility.ValidateNotNull(account, nameof(account));
                Common.Utilities.ValidationUtility.ValidateGuid(account.Id, "Account ID");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(account.Username, "Username");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(account.Email, "Email");

                var jwtSecret = _configuration["Jwt:Secret"];
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(jwtSecret, "JWT Secret");

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(jwtSecret);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                    new Claim(ClaimTypes.Name, account.Username),
                    new Claim(ClaimTypes.Email, account.Email)
                };

                if (account.IsVerified)
                {
                    claims.Add(new Claim("verified", "true"));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(Constants.JwtConfig.TokenExpirationMinutes),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = Constants.JwtConfig.Issuer,
                    Audience = Constants.JwtConfig.Audience
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for account: {Id}", account?.Id);
                throw new AccountException("Error generating JWT token", ex);
            }
        }
    }
}
