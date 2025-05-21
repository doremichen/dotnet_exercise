using System.Globalization;
using System.Windows.Data;

namespace WpfApp_DataBinding
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "True" : "False";
            }

            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return strValue.Equals("True", StringComparison.OrdinalIgnoreCase) ? true : false;
            }

            throw new InvalidOperationException("Invalid conversion back to boolean.");
        }
    }
}
