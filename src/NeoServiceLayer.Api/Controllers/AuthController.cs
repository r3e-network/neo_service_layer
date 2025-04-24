using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.API.Controllers
{
    /// <summary>
    /// Controller for authentication
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenService _tokenService;
        private readonly IAccountService _accountService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="tokenService">Token service</param>
        /// <param name="accountService">Account service</param>
        public AuthController(
            ILogger<AuthController> logger,
            ITokenService tokenService,
            IAccountService accountService)
        {
            _logger = logger;
            _tokenService = tokenService;
            _accountService = accountService;
        }

        /// <summary>
        /// Logs in a user
        /// </summary>
        /// <param name="request">Login request</param>
        /// <returns>Token response</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for user: {Username}", request.Username);

                // Authenticate user
                var token = await _accountService.AuthenticateAsync(request.Username, request.Password);
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Authentication failed for user: {Username}", request.Username);
                    return Unauthorized(new ErrorResponse { Message = "Invalid username or password" });
                }

                // Get user by username
                var user = await _accountService.GetByUsernameAsync(request.Username);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {Username}", request.Username);
                    return Unauthorized(new ErrorResponse { Message = "User not found" });
                }

                // Get user roles and permissions
                var roles = await _accountService.GetUserRolesAsync(user.Id);
                var permissions = await _accountService.GetUserPermissionsAsync(user.Id);

                // Create User object from Account
                var userObj = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                // Generate token
                var tokenResponse = await _tokenService.GenerateTokenAsync(userObj, roles, permissions);

                _logger.LogInformation("Login successful for user: {Username}", request.Username);
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login: {Message}", ex.Message);
                return BadRequest(new ErrorResponse { Message = "Login failed" });
            }
        }

        /// <summary>
        /// Refreshes a token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>Token response</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Token refresh attempt");

                // Refresh token
                var tokenResponse = await _tokenService.RefreshTokenAsync(request.RefreshToken);

                _logger.LogInformation("Token refresh successful");
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh: {Message}", ex.Message);
                return Unauthorized(new ErrorResponse { Message = "Invalid refresh token" });
            }
        }

        /// <summary>
        /// Logs out a user
        /// </summary>
        /// <param name="request">Logout request</param>
        /// <returns>No content</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                _logger.LogInformation("Logout attempt");

                // Revoke token
                await _tokenService.RevokeTokenAsync(request.RefreshToken);

                _logger.LogInformation("Logout successful");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout: {Message}", ex.Message);
                return BadRequest(new ErrorResponse { Message = "Logout failed" });
            }
        }

        /// <summary>
        /// Gets the current user
        /// </summary>
        /// <returns>User</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                _logger.LogInformation("Getting current user");

                // Get user ID from claims
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return Unauthorized(new ErrorResponse { Message = "User not authenticated" });
                }

                // Get user
                var user = await _accountService.GetUserAsync(userGuid);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return Unauthorized(new ErrorResponse { Message = "User not found" });
                }

                // Get user roles and permissions
                var roles = await _accountService.GetUserRolesAsync(user.Id);
                var permissions = await _accountService.GetUserPermissionsAsync(user.Id);

                // Create User object from Account
                var userObj = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                // Create response
                var response = new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Roles = roles,
                    Permissions = permissions
                };

                _logger.LogInformation("Current user retrieved: {UserId}", userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user: {Message}", ex.Message);
                return Unauthorized(new ErrorResponse { Message = "User not authenticated" });
            }
        }
    }

    /// <summary>
    /// Request for login
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        [Required]
        public string Password { get; set; }
    }

    /// <summary>
    /// Request for refresh token
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        [Required]
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// Request for logout
    /// </summary>
    public class LogoutRequest
    {
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        [Required]
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// Response for user
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the roles
        /// </summary>
        public IEnumerable<string> Roles { get; set; }

        /// <summary>
        /// Gets or sets the permissions
        /// </summary>
        public IEnumerable<string> Permissions { get; set; }
    }

    /// <summary>
    /// Response for error
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public string Message { get; set; }
    }
}
