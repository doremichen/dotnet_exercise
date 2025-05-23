using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTracker.Data
{
    /**
     * Singleton class to create and manage the DbContext for the application. 
     */
    public class DbContextFactory
    {
        // lazy new AppDbContext() instance
        private static readonly Lazy<AppDbContext> _instance = new(() =>
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=AcountBook.db")
                .UseLazyLoadingProxies()
                .Options;

            return new AppDbContext(options);
        });

        public static AppDbContext Instance => _instance.Value;
    }
}
