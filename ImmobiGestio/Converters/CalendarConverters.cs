using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ImmobiGestio.Models;

namespace ImmobiGestio.Converters
{
    /// <summary>
    /// Converte hex color string a Color
    /// </summary>
    public class HexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
            {
                try
                {
                    hexColor = hexColor.TrimStart('#');
                    if (hexColor.Length == 6)
                    {
                        var r = System.Convert.ToByte(hexColor.Substring(0, 2), 16);
                        var g = System.Convert.ToByte(hexColor.Substring(2, 2), 16);
                        var b = System.Convert.ToByte(hexColor.Substring(4, 2), 16);
                        return Color.FromRgb(r, g, b);
                    }
                    else if (hexColor.Length == 8)
                    {
                        var a = System.Convert.ToByte(hexColor.Substring(0, 2), 16);
                        var r = System.Convert.ToByte(hexColor.Substring(2, 2), 16);
                        var g = System.Convert.ToByte(hexColor.Substring(4, 2), 16);
                        var b = System.Convert.ToByte(hexColor.Substring(6, 2), 16);
                        return Color.FromArgb(a, r, g, b);
                    }
                }
                catch
                {
                    // Fallback
                }
            }
            return Color.FromRgb(33, 150, 243); // Default blue
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            return "#2196F3";
        }
    }

    /// <summary>
    /// Converte appuntamento a spessore bordo
    /// </summary>
    public class AppuntamentoBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte appuntamento a colore bordo
    /// </summary>
    public class AppuntamentoBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "Transparent";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Compara due valori per greater than
    /// </summary>
    public class GreaterThanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is int val1 && values[1] is int val2)
                return val1 > val2;
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte il count degli eventi in testo per overflow
    /// </summary>
    public class OverflowCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count && count > 3)
            {
                var overflow = count - 3;
                return $"+{overflow} altri";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte lo stato dell'appuntamento in colore di sfondo evidenziato
    /// </summary>
    public class AppuntamentoStateToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 1 && values[0] is Appuntamento app)
                return app.StatoColore;
            return "#2196F3"; // Default
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte appuntamento in visibilità del bordo di selezione
    /// </summary>
    public class AppuntamentoSelectionVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 &&
                values[0] is Appuntamento appuntamento &&
                values[1] is Appuntamento selectedAppuntamento)
            {
                return appuntamento == selectedAppuntamento ?
                    System.Windows.Visibility.Visible :
                    System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}