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
            // Questo converter ora è usato meno nel design minimal
            // perché stiamo usando DataTrigger direttamente nello XAML

            if (value is bool boolValue && boolValue)
            {
                if (parameter is Style style)
                    return style;

                // Cerca lo stile attivo nelle risorse
                if (Application.Current.TryFindResource("SidebarButtonActiveStyle") is Style activeStyle)
                    return activeStyle;
            }

            // Fallback allo stile normale
            if (Application.Current.TryFindResource("SidebarButtonStyle") is Style normalStyle)
                return normalStyle;

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter semplificato per visibilità
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Visible : Visibility.Collapsed;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility == Visibility.Visible;

            return false;
        }
    }
}