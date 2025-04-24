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
    /// Controller for wallet management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly ILogger<WalletController> _logger;
        private readonly IWalletService _walletService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="walletService">Wallet service</param>
        public WalletController(ILogger<WalletController> logger, IWalletService walletService)
        {
            _logger = logger;
            _walletService = walletService;
        }

        /// <summary>
        /// Creates a new wallet
        /// </summary>
        /// <param name="request">Wallet creation request</param>
        /// <returns>The created wallet</returns>
        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Creating wallet for user: {UserId}, Name: {Name}", userId, request.Name);

            try
            {
                var wallet = await _walletService.CreateWalletAsync(request.Name, request.Password, accountId);
                return Ok(new
                {
                    Id = wallet.Id,
                    Name = wallet.Name,
                    Address = wallet.Address,
                    ScriptHash = wallet.ScriptHash,
                    PublicKey = wallet.PublicKey,
                    CreatedAt = wallet.CreatedAt
                });
            }
            catch (WalletException ex)
            {
                _logger.LogError(ex, "Error creating wallet for user: {UserId}, Name: {Name}", userId, request.Name);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating wallet for user: {UserId}, Name: {Name}", userId, request.Name);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets all wallets for the current user
        /// </summary>
        /// <returns>List of wallets</returns>
        [HttpGet]
        public async Task<IActionResult> GetWallets()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Getting wallets for user: {UserId}", userId);

            try
            {
                var wallets = await _walletService.GetByAccountIdAsync(accountId);
                var result = new List<object>();

                foreach (var wallet in wallets)
                {
                    result.Add(new
                    {
                        Id = wallet.Id,
                        Name = wallet.Name,
                        Address = wallet.Address,
                        ScriptHash = wallet.ScriptHash,
                        PublicKey = wallet.PublicKey,
                        CreatedAt = wallet.CreatedAt,
                        UpdatedAt = wallet.UpdatedAt
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallets for user: {UserId}", userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets a wallet by ID
        /// </summary>
        /// <param name="id">Wallet ID</param>
        /// <returns>The wallet</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWalletById(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Getting wallet by ID: {WalletId} for user: {UserId}", id, userId);

            try
            {
                var wallet = await _walletService.GetByIdAsync(id);
                if (wallet == null)
                {
                    return NotFound(new { Message = "Wallet not found" });
                }

                // Check if the wallet belongs to the current user or is a service wallet
                if (wallet.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(new
                {
                    Id = wallet.Id,
                    Name = wallet.Name,
                    Address = wallet.Address,
                    ScriptHash = wallet.ScriptHash,
                    PublicKey = wallet.PublicKey,
                    CreatedAt = wallet.CreatedAt,
                    UpdatedAt = wallet.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet by ID: {WalletId} for user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets the NEO balance for a wallet
        /// </summary>
        /// <param name="id">Wallet ID</param>
        /// <returns>NEO balance</returns>
        [HttpGet("{id}/balance/neo")]
        public async Task<IActionResult> GetNeoBalance(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Getting NEO balance for wallet: {WalletId}, user: {UserId}", id, userId);

            try
            {
                var wallet = await _walletService.GetByIdAsync(id);
                if (wallet == null)
                {
                    return NotFound(new { Message = "Wallet not found" });
                }

                // Check if the wallet belongs to the current user or is a service wallet
                if (wallet.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var balance = await _walletService.GetNeoBalanceAsync(wallet.Address);
                return Ok(new { Balance = balance });
            }
            catch (WalletException ex)
            {
                _logger.LogError(ex, "Error getting NEO balance for wallet: {WalletId}, user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting NEO balance for wallet: {WalletId}, user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets the GAS balance for a wallet
        /// </summary>
        /// <param name="id">Wallet ID</param>
        /// <returns>GAS balance</returns>
        [HttpGet("{id}/balance/gas")]
        public async Task<IActionResult> GetGasBalance(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Getting GAS balance for wallet: {WalletId}, user: {UserId}", id, userId);

            try
            {
                var wallet = await _walletService.GetByIdAsync(id);
                if (wallet == null)
                {
                    return NotFound(new { Message = "Wallet not found" });
                }

                // Check if the wallet belongs to the current user or is a service wallet
                if (wallet.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var balance = await _walletService.GetGasBalanceAsync(wallet.Address);
                return Ok(new { Balance = balance });
            }
            catch (WalletException ex)
            {
                _logger.LogError(ex, "Error getting GAS balance for wallet: {WalletId}, user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting GAS balance for wallet: {WalletId}, user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets the token balance for a wallet
        /// </summary>
        /// <param name="id">Wallet ID</param>
        /// <param name="tokenHash">Token script hash</param>
        /// <returns>Token balance</returns>
        [HttpGet("{id}/balance/token/{tokenHash}")]
        public async Task<IActionResult> GetTokenBalance(Guid id, string tokenHash)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Getting token balance for wallet: {WalletId}, token: {TokenHash}, user: {UserId}", id, tokenHash, userId);

            try
            {
                var wallet = await _walletService.GetByIdAsync(id);
                if (wallet == null)
                {
                    return NotFound(new { Message = "Wallet not found" });
                }

                // Check if the wallet belongs to the current user or is a service wallet
                if (wallet.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var balance = await _walletService.GetTokenBalanceAsync(wallet.Address, tokenHash);
                return Ok(new { Balance = balance });
            }
            catch (WalletException ex)
            {
                _logger.LogError(ex, "Error getting token balance for wallet: {WalletId}, token: {TokenHash}, user: {UserId}", id, tokenHash, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting token balance for wallet: {WalletId}, token: {TokenHash}, user: {UserId}", id, tokenHash, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Transfers NEO tokens
        /// </summary>
        /// <param name="id">Wallet ID</param>
        /// <param name="request">Transfer request</param>
        /// <returns>Transaction hash</returns>
        [HttpPost("{id}/transfer/neo")]
        public async Task<IActionResult> TransferNeo(Guid id, [FromBody] TransferRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Transferring NEO from wallet: {WalletId}, to: {ToAddress}, amount: {Amount}, user: {UserId}", id, request.ToAddress, request.Amount, userId);

            try
            {
                var wallet = await _walletService.GetByIdAsync(id);
                if (wallet == null)
                {
                    return NotFound(new { Message = "Wallet not found" });
                }

                // Check if the wallet belongs to the current user or is a service wallet
                if (wallet.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var transactionHash = await _walletService.TransferNeoAsync(id, request.Password, request.ToAddress, request.Amount);
                return Ok(new { TransactionHash = transactionHash });
            }
            catch (WalletException ex)
            {
                _logger.LogError(ex, "Error transferring NEO from wallet: {WalletId}, user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error transferring NEO from wallet: {WalletId}, user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Transfers GAS tokens
        /// </summary>
        /// <param name="id">Wallet ID</param>
        /// <param name="request">Transfer request</param>
        /// <returns>Transaction hash</returns>
        [HttpPost("{id}/transfer/gas")]
        public async Task<IActionResult> TransferGas(Guid id, [FromBody] TransferRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Transferring GAS from wallet: {WalletId}, to: {ToAddress}, amount: {Amount}, user: {UserId}", id, request.ToAddress, request.Amount, userId);

            try
            {
                var wallet = await _walletService.GetByIdAsync(id);
                if (wallet == null)
                {
                    return NotFound(new { Message = "Wallet not found" });
                }

                // Check if the wallet belongs to the current user or is a service wallet
                if (wallet.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var transactionHash = await _walletService.TransferGasAsync(id, request.Password, request.ToAddress, request.Amount);
                return Ok(new { TransactionHash = transactionHash });
            }
            catch (WalletException ex)
            {
                _logger.LogError(ex, "Error transferring GAS from wallet: {WalletId}, user: {UserId}", id, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error transferring GAS from wallet: {WalletId}, user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Transfers NEP-17 tokens
        /// </summary>
        /// <param name="id">Wallet ID</param>
        /// <param name="tokenHash">Token script hash</param>
        /// <param name="request">Transfer request</param>
        /// <returns>Transaction hash</returns>
        [HttpPost("{id}/transfer/token/{tokenHash}")]
        public async Task<IActionResult> TransferToken(Guid id, string tokenHash, [FromBody] TransferRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Transferring token from wallet: {WalletId}, token: {TokenHash}, to: {ToAddress}, amount: {Amount}, user: {UserId}", id, tokenHash, request.ToAddress, request.Amount, userId);

            try
            {
                var wallet = await _walletService.GetByIdAsync(id);
                if (wallet == null)
                {
                    return NotFound(new { Message = "Wallet not found" });
                }

                // Check if the wallet belongs to the current user or is a service wallet
                if (wallet.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var transactionHash = await _walletService.TransferTokenAsync(id, request.Password, request.ToAddress, tokenHash, request.Amount);
                return Ok(new { TransactionHash = transactionHash });
            }
            catch (WalletException ex)
            {
                _logger.LogError(ex, "Error transferring token from wallet: {WalletId}, token: {TokenHash}, user: {UserId}", id, tokenHash, userId);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error transferring token from wallet: {WalletId}, token: {TokenHash}, user: {UserId}", id, tokenHash, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Imports a wallet from WIF
        /// </summary>
        /// <param name="request">Import request</param>
        /// <returns>The imported wallet</returns>
        [HttpPost("import")]
        public async Task<IActionResult> ImportWallet([FromBody] ImportWalletRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Importing wallet for user: {UserId}, Name: {Name}", userId, request.Name);

            try
            {
                var wallet = await _walletService.ImportFromWIFAsync(request.WIF, request.Password, request.Name, accountId);
                return Ok(new
                {
                    Id = wallet.Id,
                    Name = wallet.Name,
                    Address = wallet.Address,
                    ScriptHash = wallet.ScriptHash,
                    PublicKey = wallet.PublicKey,
                    CreatedAt = wallet.CreatedAt
                });
            }
            catch (WalletException ex)
            {
                _logger.LogError(ex, "Error importing wallet for user: {UserId}, Name: {Name}", userId, request.Name);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error importing wallet for user: {UserId}, Name: {Name}", userId, request.Name);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Deletes a wallet
        /// </summary>
        /// <param name="id">Wallet ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWallet(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var accountId))
            {
                return Unauthorized(new { Message = "Invalid user ID" });
            }

            _logger.LogInformation("Deleting wallet: {WalletId}, user: {UserId}", id, userId);

            try
            {
                var wallet = await _walletService.GetByIdAsync(id);
                if (wallet == null)
                {
                    return NotFound(new { Message = "Wallet not found" });
                }

                // Check if the wallet belongs to the current user or is a service wallet
                if (wallet.AccountId != accountId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var success = await _walletService.DeleteAsync(id);
                if (!success)
                {
                    return BadRequest(new { Message = "Failed to delete wallet" });
                }

                return Ok(new { Message = "Wallet deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting wallet: {WalletId}, user: {UserId}", id, userId);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Creates a service wallet (Admin only)
        /// </summary>
        /// <param name="request">Service wallet creation request</param>
        /// <returns>The created service wallet</returns>
        [HttpPost("service")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateServiceWallet([FromBody] CreateServiceWalletRequest request)
        {
            _logger.LogInformation("Creating service wallet: {Name}", request.Name);

            try
            {
                var wallet = await _walletService.CreateWalletAsync(request.Name, request.Password, null, true);
                return Ok(new
                {
                    Id = wallet.Id,
                    Name = wallet.Name,
                    Address = wallet.Address,
                    ScriptHash = wallet.ScriptHash,
                    PublicKey = wallet.PublicKey,
                    IsServiceWallet = wallet.IsServiceWallet,
                    CreatedAt = wallet.CreatedAt
                });
            }
            catch (WalletException ex)
            {
                _logger.LogError(ex, "Error creating service wallet: {Name}", request.Name);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating service wallet: {Name}", request.Name);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets all service wallets (Admin only)
        /// </summary>
        /// <returns>List of service wallets</returns>
        [HttpGet("service")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetServiceWallets()
        {
            _logger.LogInformation("Getting service wallets");

            try
            {
                var wallets = await _walletService.GetServiceWalletsAsync();
                var result = new List<object>();

                foreach (var wallet in wallets)
                {
                    result.Add(new
                    {
                        Id = wallet.Id,
                        Name = wallet.Name,
                        Address = wallet.Address,
                        ScriptHash = wallet.ScriptHash,
                        PublicKey = wallet.PublicKey,
                        IsServiceWallet = wallet.IsServiceWallet,
                        CreatedAt = wallet.CreatedAt,
                        UpdatedAt = wallet.UpdatedAt
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service wallets");
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }
    }
}
