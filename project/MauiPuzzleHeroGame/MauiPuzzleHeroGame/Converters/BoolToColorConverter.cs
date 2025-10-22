/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This Class is a converter that converts a boolean value to a color.
 * 
 * Author: Adam Chen
 * Date: 2025/10/22
 * 
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPuzzleHeroGame.Converters
{
    /// <summary>
    /// Convert a bool to a Brush (selected => colored brush, unselected => transparent)
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public Color SelectedBrushColor { get; set; } = Colors.LightSkyBlue;
        public Color UnselectedBrushColor { get; set; } = Colors.Transparent;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isSelected = false;
            if (value is bool boolValue)
            {
                isSelected = boolValue;
            }

            var color = isSelected ? SelectedBrushColor : UnselectedBrushColor;
            return new SolidColorBrush(color);

        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
