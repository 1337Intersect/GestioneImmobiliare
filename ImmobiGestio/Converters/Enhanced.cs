using System;
using System.Globalization;
using System.Windows.Data;

namespace ImmobiGestio.Converters
{
    /// <summary>
    /// Converter per convertire byte in stringa leggibile e viceversa
    /// </summary>
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long bytes)
            {
                return FormatFileSize(bytes);
            }
            return "0 B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                return ParseFileSize(stringValue);
            }
            return 0L;
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private long ParseFileSize(string sizeString)
        {
            try
            {
                sizeString = sizeString.Trim().ToUpper();

                double value;
                string unit;

                // Trova la parte numerica e l'unità
                int i = 0;
                while (i < sizeString.Length && (char.IsDigit(sizeString[i]) || sizeString[i] == '.' || sizeString[i] == ','))
                {
                    i++;
                }

                if (i == 0) return 0;

                value = double.Parse(sizeString.Substring(0, i).Replace(',', '.'), CultureInfo.InvariantCulture);
                unit = sizeString.Substring(i).Trim();

                return unit switch
                {
                    "B" or "" => (long)value,
                    "KB" => (long)(value * 1024),
                    "MB" => (long)(value * 1024 * 1024),
                    "GB" => (long)(value * 1024 * 1024 * 1024),
                    _ => (long)value
                };
            }
            catch
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Converter per convertire secondi in stringa tempo leggibile
    /// </summary>
    public class TimeIntervalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int seconds)
            {
                return FormatTimeInterval(seconds);
            }
            return "0 secondi";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                return ParseTimeInterval(stringValue);
            }
            return 0;
        }

        private string FormatTimeInterval(int seconds)
        {
            if (seconds < 60)
                return $"{seconds} secondi";
            else if (seconds < 3600)
                return $"{seconds / 60} minuti";
            else if (seconds < 86400)
                return $"{seconds / 3600} ore";
            else
                return $"{seconds / 86400} giorni";
        }

        private int ParseTimeInterval(string intervalString)
        {
            try
            {
                intervalString = intervalString.Trim().ToLower();

                // Estrai il numero
                int i = 0;
                while (i < intervalString.Length && (char.IsDigit(intervalString[i]) || intervalString[i] == '.'))
                {
                    i++;
                }

                if (i == 0) return 0;

                var value = int.Parse(intervalString.Substring(0, i));
                var unit = intervalString.Substring(i).Trim();

                if (unit.Contains("minuto") || unit.Contains("minuti"))
                    return value * 60;
                else if (unit.Contains("ora") || unit.Contains("ore"))
                    return value * 3600;
                else if (unit.Contains("giorno") || unit.Contains("giorni"))
                    return value * 86400;
                else
                    return value; // assume secondi
            }
            catch
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Converter per abilitare/disabilitare controlli basato su booleano
    /// </summary>
    public class BooleanToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Se parameter è "Invert", inverte il valore
                if (parameter?.ToString() == "Invert")
                    return !boolValue;
                return boolValue;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter?.ToString() == "Invert")
                    return !boolValue;
                return boolValue;
            }
            return false;
        }
    }

    /// <summary>
    /// Converter per mostrare password come asterischi
    /// </summary>
    public class PasswordMaskConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string password && !string.IsNullOrEmpty(password))
            {
                return new string('*', password.Length);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Non implementato per motivi di sicurezza
            throw new NotImplementedException("La conversione inversa non è supportata per le password");
        }
    }

    /// <summary>
    /// Converter per validazione email
    /// </summary>
    public class EmailValidationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string email)
            {
                return IsValidEmail(email) ? "✅" : "❌";
            }
            return "❌";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Converter per evidenziare campi modificati
    /// </summary>
    public class ModifiedFieldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isModified && isModified)
            {
                return "#FFF3E0"; // Sfondo arancione chiaro per campi modificati
            }
            return "White";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Multi-converter per validazione complessa
    /// </summary>
    public class SettingsValidationMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Esempio: valida che AutoSaveInterval sia sempre >= StatusRefreshInterval
            if (values.Length >= 2 &&
                values[0] is int autoSave &&
                values[1] is int statusRefresh)
            {
                if (autoSave < statusRefresh)
                {
                    return "L'intervallo di salvataggio automatico deve essere maggiore o uguale all'intervallo di aggiornamento status";
                }
            }

            return null; // Nessun errore
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}