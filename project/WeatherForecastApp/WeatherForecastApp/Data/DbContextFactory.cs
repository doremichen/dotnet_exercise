using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecastApp.Data
{
    public class DbContextFactory
    {
        // lazy new AppDbContext() instance
        private static readonly Lazy<AppDbContext> _instance = new(() =>
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=weatherBroadcast.db")
                .UseLazyLoadingProxies()
                .Options;

            return new AppDbContext(options);
        });

        public static AppDbContext Instance => _instance.Value;
    }
}
