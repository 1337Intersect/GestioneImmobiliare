using System;
using System.Globalization;
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
}