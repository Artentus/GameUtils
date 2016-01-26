using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MagicMesh
{
    sealed class InverseBooleanToVisibilityConverter : IValueConverter
    {
        readonly BooleanToVisibilityConverter converter;

        public InverseBooleanToVisibilityConverter()
        {
            converter = new BooleanToVisibilityConverter();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return converter.Convert(!(bool)value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)converter.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
