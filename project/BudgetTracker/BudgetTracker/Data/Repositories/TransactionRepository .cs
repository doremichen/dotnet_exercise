using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BudgetTracker.Data.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _dbContext; // Assuming you have a DbContext for database operations    

        // constructor
        public TransactionRepository()
        {
            _dbContext = DbContextFactory.Instance; // Using the singleton instance of AppDbContext
        }


        public async Task AddAsync(Transaction transaction)
        {
            await _dbContext.Transactions.AddAsync(transaction); // Assuming Transactions is a DbSet<Transaction> in your AppDbContext
            await _dbContext.SaveChangesAsync(); // Save changes to the database
        }

        public async Task DeleteAsync(Transaction transaction)
        {
            _dbContext.Transactions.Remove(transaction); // Assuming Transactions is a DbSet<Transaction> in your AppDbContext
            await _dbContext.SaveChangesAsync(); // Save changes to the database
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _dbContext.Transactions.ToListAsync(); // Assuming Transactions is a DbSet<Transaction> in your AppDbContext
        }

        public async Task<Transaction?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Transactions.FindAsync(id); // Assuming Transactions is a DbSet<Transaction> in your AppDbContext
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            var existing = await _dbContext.Transactions.FindAsync(transaction.Id);
            if (existing != null)
            {
                _dbContext.Entry(existing).CurrentValues.SetValues(transaction);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ClearAllAsync()
        {
            _dbContext.Transactions.RemoveRange(_dbContext.Transactions); // Assuming Transactions is a DbSet<Transaction> in your AppDbContext
            await _dbContext.SaveChangesAsync(); // Save changes to the database
        }
    }
}
