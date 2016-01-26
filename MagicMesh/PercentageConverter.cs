using System;
using System.Globalization;
using System.Windows.Data;

namespace MagicMesh
{
    class PercentageConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (int)Math.Round((float)value * 100) + "%";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value.ToString().TrimEnd('%');
            if (!string.IsNullOrEmpty(stringValue))
            {
                float percent;
                float.TryParse(stringValue, out percent);
                return percent / 100;
            }
            return 0;
        }
    }
}
