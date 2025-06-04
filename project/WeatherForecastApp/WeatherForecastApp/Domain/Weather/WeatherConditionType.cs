using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecastApp.Domain.Weather
{
    public abstract class WeatherConditionType
    {
        // abstract IconFileName
        public abstract string IconFileName { get; }


        // instances of WeatherConditionType
        public static readonly WeatherConditionType Sunny = new SunnyType();
        public static readonly WeatherConditionType Cloudy = new CloudyType();
        public static readonly WeatherConditionType Rain = new RainType();
        public static readonly WeatherConditionType Thunder = new ThunderType();
        public static readonly WeatherConditionType Snow = new SnowType();
        public static readonly WeatherConditionType Fog = new FogType();
        public static readonly WeatherConditionType Drizzle = new DrizzleType();
        public static readonly WeatherConditionType Unknown = new UnknownType();

        private class SunnyType : WeatherConditionType
        {
            public override string IconFileName => "sun.png";
        }

        private class CloudyType : WeatherConditionType
        {
            public override string IconFileName => "cloud.png";
        }

        private class RainType : WeatherConditionType
        {
            public override string IconFileName => "rain.png";
        }

        private class ThunderType : WeatherConditionType
        {
            public override string IconFileName => "thunder.png";
        }

        private class SnowType : WeatherConditionType
        {
            public override string IconFileName => "snow.png";
        }

        private class FogType : WeatherConditionType
        {
            public override string IconFileName => "fog.png";
        }

        private class DrizzleType : WeatherConditionType
        {
            public override string IconFileName => "drizzle.png";
        }

        private class UnknownType : WeatherConditionType
        {
            public override string IconFileName => "unknown.png";
        }

        // build weather condition map
        private static Dictionary<string, WeatherConditionType> _conditionMap = new Dictionary<string, WeatherConditionType>(StringComparer.OrdinalIgnoreCase)
        {
            { "Sunny", Sunny },
            { "Clear Sky", Sunny },
            { "Scattered Clouds", Cloudy },
            { "Overcast Clouds", Cloudy },
            { "Broken Clouds", Cloudy },
            { "Light Rain", Rain },
            { "Heavy Intensity Rain", Rain },
            { "Moderate Rain", Thunder },
            { "Snow", Snow },
            { "Drizzle", Drizzle },
            { "Mist", Fog }
        };

        public static WeatherConditionType GetConditionType(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return Unknown;

            return _conditionMap.TryGetValue(condition.Trim(), out var type)
                ? type
                : Unknown;
        }


    }

}
