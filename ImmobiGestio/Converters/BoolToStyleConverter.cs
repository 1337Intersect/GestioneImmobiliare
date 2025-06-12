using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImmobiGestio.Converters
{
    public class BoolToStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue && isTrue && parameter is Style trueStyle)
            {
                return trueStyle;
            }

            // Restituisce il FallbackValue se disponibile
            if (parameter is FrameworkElement element && element.TryFindResource("SidebarButtonStyle") is Style fallbackStyle)
            {
                return fallbackStyle;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}