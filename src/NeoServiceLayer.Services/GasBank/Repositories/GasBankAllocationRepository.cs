using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Utilities;

namespace NeoServiceLayer.Services.GasBank.Repositories
{
    /// <summary>
    /// Implementation of the GasBank allocation repository
    /// </summary>
    public class GasBankAllocationRepository : IGasBankAllocationRepository
    {
        private readonly ILogger<GasBankAllocationRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private const string CollectionName = "gasbank_allocations";

        /// <summary>
        /// Initializes a new instance of the <see cref="GasBankAllocationRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public GasBankAllocationRepository(ILogger<GasBankAllocationRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<GasBankAllocation> CreateAsync(GasBankAllocation allocation)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = allocation.Id,
                ["GasBankAccountId"] = allocation.GasBankAccountId,
                ["FunctionId"] = allocation.FunctionId,
                ["Amount"] = allocation.Amount
            };

            LoggingUtility.LogOperationStart(_logger, "CreateGasBankAllocation", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateNotNull(allocation, nameof(allocation));
                ValidationUtility.ValidateGuid(allocation.Id, "GasBank allocation ID");
                ValidationUtility.ValidateGuid(allocation.GasBankAccountId, "GasBank account ID");
                ValidationUtility.ValidateGuid(allocation.FunctionId, "Function ID");
                ValidationUtility.ValidateGreaterThanZero(allocation.Amount, "Amount");

                // Set timestamps
                allocation.CreatedAt = DateTime.UtcNow;
                allocation.UpdatedAt = DateTime.UtcNow;

                // Create allocation
                await _storageProvider.CreateAsync(CollectionName, allocation);

                LoggingUtility.LogOperationSuccess(_logger, "CreateGasBankAllocation", requestId, 0, additionalData);

                return allocation;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "CreateGasBankAllocation", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<GasBankAllocation> GetByIdAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            LoggingUtility.LogOperationStart(_logger, "GetGasBankAllocationById", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(id, "GasBank allocation ID");

                // Get allocation
                var allocation = await _storageProvider.GetByIdAsync<GasBankAllocation, Guid>(CollectionName, id);

                if (allocation != null)
                {
                    additionalData["GasBankAccountId"] = allocation.GasBankAccountId;
                    additionalData["FunctionId"] = allocation.FunctionId;
                    additionalData["Amount"] = allocation.Amount;
                }

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankAllocationById", requestId, 0, additionalData);

                return allocation;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankAllocationById", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GasBankAllocation>> GetByGasBankAccountIdAsync(Guid gasBankAccountId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["GasBankAccountId"] = gasBankAccountId
            };

            LoggingUtility.LogOperationStart(_logger, "GetGasBankAllocationsByGasBankAccountId", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(gasBankAccountId, "GasBank account ID");

                // Get allocations
                var allocations = await _storageProvider.GetByFilterAsync<GasBankAllocation>(
                    CollectionName,
                    allocation => allocation.GasBankAccountId == gasBankAccountId);

                additionalData["Count"] = allocations.Count();

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankAllocationsByGasBankAccountId", requestId, 0, additionalData);

                return allocations;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankAllocationsByGasBankAccountId", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GasBankAllocation>> GetByFunctionIdAsync(Guid functionId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["FunctionId"] = functionId
            };

            LoggingUtility.LogOperationStart(_logger, "GetGasBankAllocationsByFunctionId", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(functionId, "Function ID");

                // Get allocations
                var allocations = await _storageProvider.GetByFilterAsync<GasBankAllocation>(
                    CollectionName,
                    allocation => allocation.FunctionId == functionId);

                additionalData["Count"] = allocations.Count();

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankAllocationsByFunctionId", requestId, 0, additionalData);

                return allocations;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankAllocationsByFunctionId", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<GasBankAllocation> UpdateAsync(GasBankAllocation allocation)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = allocation.Id,
                ["GasBankAccountId"] = allocation.GasBankAccountId,
                ["FunctionId"] = allocation.FunctionId,
                ["Amount"] = allocation.Amount
            };

            LoggingUtility.LogOperationStart(_logger, "UpdateGasBankAllocation", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateNotNull(allocation, nameof(allocation));
                ValidationUtility.ValidateGuid(allocation.Id, "GasBank allocation ID");
                ValidationUtility.ValidateGuid(allocation.GasBankAccountId, "GasBank account ID");
                ValidationUtility.ValidateGuid(allocation.FunctionId, "Function ID");
                ValidationUtility.ValidateGreaterThanZero(allocation.Amount, "Amount");

                // Update timestamp
                allocation.UpdatedAt = DateTime.UtcNow;

                // Update allocation
                await _storageProvider.UpdateAsync<GasBankAllocation, Guid>(CollectionName, allocation.Id, allocation);

                LoggingUtility.LogOperationSuccess(_logger, "UpdateGasBankAllocation", requestId, 0, additionalData);

                return allocation;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "UpdateGasBankAllocation", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            LoggingUtility.LogOperationStart(_logger, "DeleteGasBankAllocation", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(id, "GasBank allocation ID");

                // Delete allocation
                var result = await _storageProvider.DeleteAsync<GasBankAllocation, Guid>(CollectionName, id);

                LoggingUtility.LogOperationSuccess(_logger, "DeleteGasBankAllocation", requestId, 0, additionalData);

                return result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "DeleteGasBankAllocation", requestId, ex, 0, additionalData);
                throw;
            }
        }
    }
}
