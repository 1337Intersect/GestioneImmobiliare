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
            {
                // Se è 0, mostra stringa vuota per un'esperienza utente migliore
                return decimalValue == 0 ? string.Empty : decimalValue.ToString("F2", culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                // Se la stringa è vuota o null, restituisce 0
                if (string.IsNullOrWhiteSpace(stringValue))
                    return 0m;

                // Prova a convertire
                if (decimal.TryParse(stringValue, NumberStyles.Number, culture, out decimal result))
                    return result;
            }

            // Se la conversione fallisce, restituisce 0
            return 0m;
        }
    }

    // Converter per int (Superficie, NumeroLocali, etc.)
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                // Se è 0, mostra stringa vuota per un'esperienza utente migliore
                return intValue == 0 ? string.Empty : intValue.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                // Se la stringa è vuota o null, restituisce 0
                if (string.IsNullOrWhiteSpace(stringValue))
                    return 0;

                // Prova a convertire
                if (int.TryParse(stringValue, out int result))
                    return result;
            }

            // Se la conversione fallisce, restituisce 0
            return 0;
        }
    }

    // Converter per int nullable (per proprietà che possono essere null)
    public class NullableIntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                // Se la stringa è vuota o null, restituisce null
                if (string.IsNullOrWhiteSpace(stringValue))
                    return null;

                // Prova a convertire
                if (int.TryParse(stringValue, out int result))
                    return result;
            }

            // Se la conversione fallisce, restituisce null
            return null;
        }
    }

    // Converter per decimal nullable
    public class NullableDecimalToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return decimalValue.ToString("F2", culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                // Se la stringa è vuota o null, restituisce null
                if (string.IsNullOrWhiteSpace(stringValue))
                    return null;

                // Prova a convertire
                if (decimal.TryParse(stringValue, NumberStyles.Number, culture, out decimal result))
                    return result;
            }

            // Se la conversione fallisce, restituisce null
            return null;
        }
    }

    // Converter combinato che gestisce sia int che decimal
    public class NumericStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            if (value is decimal decimalValue)
            {
                return decimalValue == 0 ? string.Empty : decimalValue.ToString("F2", culture);
            }

            if (value is int intValue)
            {
                return intValue == 0 ? string.Empty : intValue.ToString();
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    // Restituisce il valore di default per il tipo
                    if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                        return targetType == typeof(decimal?) ? (decimal?)null : 0m;

                    if (targetType == typeof(int) || targetType == typeof(int?))
                        return targetType == typeof(int?) ? (int?)null : 0;
                }

                // Prova a convertire a decimal
                if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                {
                    if (decimal.TryParse(stringValue, NumberStyles.Number, culture, out decimal decResult))
                        return decResult;
                    return targetType == typeof(decimal?) ? (decimal?)null : 0m;
                }

                // Prova a convertire a int
                if (targetType == typeof(int) || targetType == typeof(int?))
                {
                    if (int.TryParse(stringValue, out int intResult))
                        return intResult;
                    return targetType == typeof(int?) ? (int?)null : 0;
                }
            }

            return value;
        }
    }
}