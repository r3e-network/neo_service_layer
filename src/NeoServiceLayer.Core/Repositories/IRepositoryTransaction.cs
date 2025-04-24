using System;
using System.Threading.Tasks;

namespace NeoServiceLayer.Core.Repositories
{
    /// <summary>
    /// Repository transaction interface
    /// </summary>
    public interface IRepositoryTransaction : IDisposable
    {
        /// <summary>
        /// Commits the transaction
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task CommitAsync();

        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task RollbackAsync();
    }
}
