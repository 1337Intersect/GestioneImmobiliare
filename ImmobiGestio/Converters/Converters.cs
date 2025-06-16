using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImmobiGestio.Converters
{
    // Converter per decimal (Prezzo)
    public class DecimalToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
                return decimalValue == 0 ? string.Empty : decimalValue.ToString("F2", culture);
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                if (string.IsNullOrWhiteSpace(stringValue))
                    return 0m;
                if (decimal.TryParse(stringValue, NumberStyles.Number, culture, out decimal result))
                    return result;
            }
            return 0m;
        }
    }

    // Converter per int (Superficie, NumeroLocali, etc.)
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return intValue == 0 ? string.Empty : intValue.ToString();
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                if (string.IsNullOrWhiteSpace(stringValue))
                    return 0;
                if (int.TryParse(stringValue, out int result))
                    return result;
            }
            return 0;
        }
    }

    // Converter per int nullable
    public class NullableIntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return intValue.ToString();
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                if (int.TryParse(stringValue, out int result))
                    return result;
            }
            return null;
        }
    }

    // Converter per decimal nullable
    public class NullableDecimalToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
                return decimalValue.ToString("F2", culture);
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                if (decimal.TryParse(stringValue, NumberStyles.Number, culture, out decimal result))
                    return result;
            }
            return null;
        }
    }

    // Converter per string numerici
    public class NumericStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }
    }
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isToday && isToday)
                return FontWeights.Bold;
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    // ✅ CONVERTER OTTIMIZZATO PER COMPARAZIONE NUMERICA
    public class GreaterThanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count && parameter is string threshold)
            {
                if (int.TryParse(threshold, out int thresholdValue))
                {
                    return count > thresholdValue ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    // ✅ CONVERTER OTTIMIZZATO PER COLORI
    public class OptimizedHexToColorConverter : IValueConverter
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, object> _cache
            = new System.Collections.Concurrent.ConcurrentDictionary<string, object>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
            {
                // ✅ CACHE per performance
                return _cache.GetOrAdd(hexColor, hex =>
                {
                    try
                    {
                        hex = hex.TrimStart('#');
                        if (hex.Length == 6)
                        {
                            var r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                            var g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                            var b = System.Convert.ToByte(hex.Substring(4, 2), 16);
                            return System.Windows.Media.Color.FromRgb(r, g, b);
                        }
                    }
                    catch
                    {
                        // Fallback silenzioso
                    }
                    return System.Windows.Media.Color.FromRgb(33, 150, 243); // Default blue
                });
            }
            return System.Windows.Media.Color.FromRgb(33, 150, 243);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    // ✅ CONVERTER OTTIMIZZATO PER BACKGROUND COLOR
    public class EventBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stato)
            {
                return stato switch
                {
                    "Programmato" => "#2196F3",
                    "Confermato" => "#4CAF50",
                    "Completato" => "#9E9E9E",
                    "Annullato" => "#F44336",
                    _ => "#2196F3"
                };
            }
            return "#2196F3";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    // ✅ CONVERTER SEMPLIFICATO PER VISIBILITÀ
    public class SimpleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Visible : Visibility.Collapsed;

            if (value is int intValue)
                return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;

            if (value is string stringValue)
                return !string.IsNullOrEmpty(stringValue) ? Visibility.Visible : Visibility.Collapsed;

            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}