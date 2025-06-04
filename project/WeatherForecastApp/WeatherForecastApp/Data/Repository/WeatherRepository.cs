using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherForecastApp.Models;

namespace WeatherForecastApp.Data.Repository
{
    public class WeatherRepository : IWeather<Models.WeatherInfo>
    {
        // AppDbContext _context;
        private readonly AppDbContext _dbContext;

        // Constructor to inject the AppDbContext
        public WeatherRepository()
        {
            this._dbContext = Data.DbContextFactory.Instance;
        }

        public async Task AddAsync(WeatherInfo weather)
        {
            await this._dbContext.WeatherInfos.AddAsync(weather);
            await this._dbContext.SaveChangesAsync();
        }

        public async Task ClearAllAsync()
        {
            _dbContext.WeatherInfos.RemoveRange(_dbContext.WeatherInfos); // Assuming Transactions is a DbSet<Transaction> in your AppDbContext
            await _dbContext.SaveChangesAsync(); // Save changes to the database
        }

        public async Task DeleteAsync(WeatherInfo weather)
        {
            await this._dbContext.WeatherInfos
                .Where(w => w.Id == weather.Id)
                .ExecuteDeleteAsync();
        }

        public async Task<IEnumerable<WeatherInfo>> GetAllAsync()
        {
            return await this._dbContext.WeatherInfos.ToListAsync();
        }

        public async Task<WeatherInfo?> GetByIdAsync(Guid id)
        {
            return await this._dbContext.WeatherInfos.FindAsync(id);
        }

        public async Task UpdateAsync(WeatherInfo weather)
        {
            var existingWeather = await this._dbContext.WeatherInfos.FindAsync(weather.Id);
            if (existingWeather != null)
            {
                existingWeather.Location = weather.Location;
                existingWeather.DateTime = weather.DateTime;
                existingWeather.Temperature = weather.Temperature;
                existingWeather.Humidity = weather.Humidity;
                existingWeather.WindSpeed = weather.WindSpeed;
                existingWeather.WeatherCondition = weather.WeatherCondition;

                await this._dbContext.SaveChangesAsync();
            }
        }
    }
}
