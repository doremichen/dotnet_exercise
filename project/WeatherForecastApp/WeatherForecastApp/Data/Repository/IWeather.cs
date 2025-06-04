using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecastApp.Data.Repository
{
    public interface IWeather<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task AddAsync(T weather);
        Task DeleteAsync(T weather);
        Task UpdateAsync(T weather);
        Task ClearAllAsync();
    }
}
