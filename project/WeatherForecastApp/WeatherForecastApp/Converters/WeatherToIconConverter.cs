using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WeatherForecastApp.Domain.Weather;
using WeatherForecastApp.Utils;

namespace WeatherForecastApp.Converters
{
    public class WeatherToIconConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? condition = value as string;

            if (string.IsNullOrWhiteSpace(condition))
                return null;
            Util.Log($"Converting weather condition '{condition}' to icon.");

            var weatherType = WeatherConditionType.GetConditionType(condition);

            string imageName = weatherType?.IconFileName ?? "unknown.png";

            return new BitmapImage(new Uri($"pack://application:,,,/Images/{imageName}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
