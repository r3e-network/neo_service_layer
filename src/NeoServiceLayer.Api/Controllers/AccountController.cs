using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Api.Models;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Api.Controllers
{
    /// <summary>
    /// Controller for account management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountService _accountService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="accountService">Account service</param>
        public AccountController(ILogger<AccountController> logger, IAccountService accountService)
        {
            _logger = logger;
            _accountService = accountService;
        }

        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="request">Registration request</param>
        /// <returns>The created account</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("Registering new account: {Username}, {Email}", request.Username, request.Email);

            try
            {
                var account = await _accountService.RegisterAsync(request.Username, request.Email, request.Password, request.NeoAddress);
                return Ok(new
                {
                    Id = account.Id,
                    Username = account.Username,
                    Email = account.Email,
                    NeoAddress = account.NeoAddress,
                    IsVerified = account.IsVerified,
                    CreatedAt = account.CreatedAt
                });
            }
            catch (AccountException ex)
            {
                _logger.LogError(ex, "Error registering account: {Username}, {Email}", request.Username, request.Email);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error registering account: {Username}, {Email}", request.Username, request.Email);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="request">Authentication request</param>
        /// <returns>JWT token</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Authenticating user: {UsernameOrEmail}", request.UsernameOrEmail);

            try
            {
                var token = await _accountService.AuthenticateAsync(request.UsernameOrEmail, request.Password);
                return Ok(new { Token = token });
            }
            catch (AccountException ex)
            {
                _logger.LogError(ex, "Error authenticating user: {UsernameOrEmail}", request.UsernameOrEmail);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error authenticating user: {UsernameOrEmail}", request.UsernameOrEmail);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets the current user's account
        /// </summary>
        /// <returns>The current user's account</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Getting current user: {UserId}", userId);

            try
            {
                var account = await _accountService.GetByIdAsync(id);
                if (account == null)
                {
                    return NotFound(new { Message = "Account not found" });
                }

                return Ok(new
                {
                    Id = account.Id,
                    Username = account.Username,
                    Email = account.Email,
                    NeoAddress = account.NeoAddress,
                    IsVerified = account.IsVerified,
                    Credits = account.Credits,
                    CreatedAt = account.CreatedAt,
                    UpdatedAt = account.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user: {UserId}", userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Changes the password for the current user
        /// </summary>
        /// <param name="request">Password change request</param>
        /// <returns>Success status</returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Changing password for user: {UserId}", userId);

            try
            {
                var success = await _accountService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
                if (!success)
                {
                    return BadRequest(new { Message = "Failed to change password" });
                }

                return Ok(new { Message = "Password changed successfully" });
            }
            catch (AccountException ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error changing password for user: {UserId}", userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Verifies a user account
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VerifyAccount(Guid id)
        {
            _logger.LogInformation("Verifying account: {AccountId}", id);

            try
            {
                var success = await _accountService.VerifyAccountAsync(id);
                if (!success)
                {
                    return BadRequest(new { Message = "Failed to verify account" });
                }

                return Ok(new { Message = "Account verified successfully" });
            }
            catch (AccountException ex)
            {
                _logger.LogError(ex, "Error verifying account: {AccountId}", id);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error verifying account: {AccountId}", id);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Adds credits to a user account
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <param name="request">Add credits request</param>
        /// <returns>Updated account</returns>
        [HttpPost("{id}/credits/add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddCredits(Guid id, [FromBody] AddCreditsRequest request)
        {
            _logger.LogInformation("Adding credits to account: {AccountId}, Amount: {Amount}", id, request.Amount);

            try
            {
                var account = await _accountService.AddCreditsAsync(id, request.Amount);
                return Ok(new
                {
                    Id = account.Id,
                    Username = account.Username,
                    Credits = account.Credits,
                    UpdatedAt = account.UpdatedAt
                });
            }
            catch (AccountException ex)
            {
                _logger.LogError(ex, "Error adding credits to account: {AccountId}", id);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding credits to account: {AccountId}", id);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets all user accounts
        /// </summary>
        /// <returns>List of all accounts</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAccounts()
        {
            _logger.LogInformation("Getting all accounts");

            try
            {
                var accounts = await _accountService.GetAllAsync();
                var result = new List<object>();

                foreach (var account in accounts)
                {
                    result.Add(new
                    {
                        Id = account.Id,
                        Username = account.Username,
                        Email = account.Email,
                        NeoAddress = account.NeoAddress,
                        IsVerified = account.IsVerified,
                        Credits = account.Credits,
                        CreatedAt = account.CreatedAt,
                        UpdatedAt = account.UpdatedAt
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all accounts");
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets a user account by ID
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <returns>The account</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAccountById(Guid id)
        {
            _logger.LogInformation("Getting account by ID: {AccountId}", id);

            try
            {
                var account = await _accountService.GetByIdAsync(id);
                if (account == null)
                {
                    return NotFound(new { Message = "Account not found" });
                }

                return Ok(new
                {
                    Id = account.Id,
                    Username = account.Username,
                    Email = account.Email,
                    NeoAddress = account.NeoAddress,
                    IsVerified = account.IsVerified,
                    Credits = account.Credits,
                    CreatedAt = account.CreatedAt,
                    UpdatedAt = account.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account by ID: {AccountId}", id);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Deletes a user account
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            _logger.LogInformation("Deleting account: {AccountId}", id);

            try
            {
                var success = await _accountService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound(new { Message = "Account not found" });
                }

                return Ok(new { Message = "Account deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account: {AccountId}", id);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }
    }
}
