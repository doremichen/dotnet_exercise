using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecastApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Models.WeatherInfo> WeatherInfos { get; set; }

        // constructor
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.EnsureCreated(); // Uncomment this line if you want to create the database on startup
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=weatherBroadcast.db");
                optionsBuilder.UseLazyLoadingProxies(); // Enable lazy loading proxies
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.WeatherInfo>().ToTable("WeatherInfos");
        }
    }
}
