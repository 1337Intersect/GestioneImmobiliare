// Crea questo file come ThemeService.cs nella cartella Services

using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace ImmobiGestio.Services
{
    public enum AppTheme
    {
        Light,
        Dark,
        System // Segue le impostazioni di sistema
    }

    public class ThemeService
    {
        private static ThemeService? _instance;
        public static ThemeService Instance => _instance ??= new ThemeService();

        private AppTheme _currentTheme = AppTheme.System;
        private bool _systemUsesLightTheme = true;

        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        private ThemeService()
        {
            // Leggi le impostazioni di sistema all'avvio
            ReadSystemTheme();

            // Monitora i cambiamenti delle impostazioni di sistema
            SystemEvents.UserPreferenceChanged += OnSystemPreferenceChanged;
        }

        public AppTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ApplyTheme();
                    ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(GetEffectiveTheme()));
                }
            }
        }

        public bool IsSystemDarkMode => !_systemUsesLightTheme;

        public bool IsDarkMode => GetEffectiveTheme() == AppTheme.Dark;

        private AppTheme GetEffectiveTheme()
        {
            return CurrentTheme switch
            {
                AppTheme.System => _systemUsesLightTheme ? AppTheme.Light : AppTheme.Dark,
                _ => CurrentTheme
            };
        }

        private void ReadSystemTheme()
        {
            try
            {
                // Legge dalla registry di Windows 10/11
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key?.GetValue("AppsUseLightTheme") is int value)
                {
                    _systemUsesLightTheme = value != 0;
                }
                else
                {
                    // Fallback per Windows più vecchi
                    _systemUsesLightTheme = true;
                }
            }
            catch
            {
                // Default a light theme se non riusciamo a leggere
                _systemUsesLightTheme = true;
            }
        }

        private void OnSystemPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                var previousSystemTheme = _systemUsesLightTheme;
                ReadSystemTheme();

                // Solo se il tema di sistema è cambiato e stiamo seguendo il sistema
                if (previousSystemTheme != _systemUsesLightTheme && CurrentTheme == AppTheme.System)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ApplyTheme();
                        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(GetEffectiveTheme()));
                    });
                }
            }
        }

        public void ApplyTheme()
        {
            var app = Application.Current;
            if (app == null) return;

            var effectiveTheme = GetEffectiveTheme();

            // Rimuovi i temi esistenti
            app.Resources.MergedDictionaries.Clear();

            // Carica il tema appropriato
            var themeUri = effectiveTheme == AppTheme.Dark
                ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

            try
            {
                var themeDict = new ResourceDictionary { Source = themeUri };
                app.Resources.MergedDictionaries.Add(themeDict);
            }
            catch
            {
                // Se non trova il file del tema, usa i colori hardcoded
                ApplyHardcodedTheme(effectiveTheme);
            }
        }

        private void ApplyHardcodedTheme(AppTheme theme)
        {
            var app = Application.Current;
            if (app == null) return;

            // Crea un resource dictionary con i colori del tema
            var themeDict = new ResourceDictionary();

            if (theme == AppTheme.Dark)
            {
                // Dark Theme Colors
                themeDict["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(32, 32, 32));
                themeDict["SurfaceBrush"] = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                themeDict["PrimaryBrush"] = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                themeDict["AccentBrush"] = new SolidColorBrush(Color.FromRgb(100, 181, 246));
                themeDict["TextPrimary"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                themeDict["TextSecondary"] = new SolidColorBrush(Color.FromRgb(170, 170, 170));
                themeDict["TextMuted"] = new SolidColorBrush(Color.FromRgb(120, 120, 120));
                themeDict["BorderBrush"] = new SolidColorBrush(Color.FromRgb(70, 70, 70));
                themeDict["HoverBrush"] = new SolidColorBrush(Color.FromRgb(55, 55, 55));
            }
            else
            {
                // Light Theme Colors (default)
                themeDict["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(250, 250, 250));
                themeDict["SurfaceBrush"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                themeDict["PrimaryBrush"] = new SolidColorBrush(Color.FromRgb(51, 51, 51));
                themeDict["AccentBrush"] = new SolidColorBrush(Color.FromRgb(0, 102, 204));
                themeDict["TextPrimary"] = new SolidColorBrush(Color.FromRgb(26, 26, 26));
                themeDict["TextSecondary"] = new SolidColorBrush(Color.FromRgb(102, 102, 102));
                themeDict["TextMuted"] = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                themeDict["BorderBrush"] = new SolidColorBrush(Color.FromRgb(229, 229, 229));
                themeDict["HoverBrush"] = new SolidColorBrush(Color.FromRgb(245, 245, 245));
            }

            app.Resources.MergedDictionaries.Add(themeDict);
        }

        public void SaveThemePreference()
        {
            try
            {
                // Salva nelle impostazioni dell'applicazione
                Properties.Settings.Default.Theme = CurrentTheme.ToString();
                Properties.Settings.Default.Save();
            }
            catch
            {
                // Ignore errors when saving settings
            }
        }

        public void LoadThemePreference()
        {
            try
            {
                var savedTheme = Properties.Settings.Default.Theme;
                if (Enum.TryParse<AppTheme>(savedTheme, out var theme))
                {
                    CurrentTheme = theme;
                }
            }
            catch
            {
                // Default to system theme if can't load
                CurrentTheme = AppTheme.System;
            }
        }

        ~ThemeService()
        {
            SystemEvents.UserPreferenceChanged -= OnSystemPreferenceChanged;
        }
    }

    public class ThemeChangedEventArgs : EventArgs
    {
        public AppTheme NewTheme { get; }

        public ThemeChangedEventArgs(AppTheme newTheme)
        {
            NewTheme = newTheme;
        }
    }
}

// Aggiungi questo nel file Properties/Settings.settings
/*
<?xml version='1.0' encoding='utf-8'?>
<SettingsFile xmlns="http://schemas.microsoft.com/VisualStudio/2004/01/settings" CurrentProfile="(Default)" GeneratedClassNamespace="ImmobiGestio.Properties" GeneratedClassName="Settings">
  <Profiles />
  <Settings>
    <Setting Name="Theme" Type="System.String" Scope="User">
      <Value Profile="(Default)">System</Value>
    </Setting>
  </Settings>
</SettingsFile>
*/