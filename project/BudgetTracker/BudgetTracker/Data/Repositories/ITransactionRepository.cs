using BudgetTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTracker.Data.Repositories
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task<Transaction?> GetByIdAsync(Guid id);
        Task AddAsync(Transaction transaction);
        Task DeleteAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task ClearAllAsync();
    }
}
