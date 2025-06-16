using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImmobiGestio.Converters
{
    public class StringToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string fontWeightString)
            {
                return fontWeightString.ToLower() switch
                {
                    "thin" => FontWeights.Thin,
                    "extralight" => FontWeights.ExtraLight,
                    "light" => FontWeights.Light,
                    "normal" => FontWeights.Normal,
                    "medium" => FontWeights.Medium,
                    "semibold" => FontWeights.SemiBold,
                    "bold" => FontWeights.Bold,
                    "extrabold" => FontWeights.ExtraBold,
                    "black" => FontWeights.Black,
                    _ => FontWeights.Normal
                };
            }
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FontWeight fontWeight)
            {
                // Conversione approssimativa da FontWeight a stringa
                if (fontWeight == FontWeights.Bold) return "Bold";
                if (fontWeight == FontWeights.SemiBold) return "SemiBold";
                if (fontWeight == FontWeights.Medium) return "Medium";
                if (fontWeight == FontWeights.Light) return "Light";
                return "Normal";
            }
            return "Normal";
        }
    }
}