using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace ImmobiGestio.Services
{
    public enum Theme
    {
        Light,
        Dark,
        Auto
    }

    public class ThemeManager : IDisposable
    {
        private static ThemeManager? _instance;
        private Theme _currentTheme = Theme.Auto;
        private bool _systemUsesLightTheme = true;
        private bool _disposed = false;

        public static ThemeManager Instance => _instance ??= new ThemeManager();

        public event Action<Theme>? ThemeChanged;

        private ThemeManager()
        {
            DetectSystemTheme();
            SystemEvents.UserPreferenceChanged += OnSystemPreferenceChanged;
        }

        public Theme CurrentTheme
        {
            get => _currentTheme;
            private set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ThemeChanged?.Invoke(value);
                }
            }
        }

        public bool IsSystemDarkMode => !_systemUsesLightTheme;

        public void LoadSavedTheme()
        {
            try
            {
                var savedTheme = LoadSavedThemeInternal();
                SetTheme(savedTheme);
                System.Diagnostics.Debug.WriteLine($"Tema salvato caricato: {savedTheme}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel caricamento del tema salvato: {ex.Message}");
                SetTheme(Theme.Auto); // Fallback
            }
        }

        public void SetTheme(Theme theme)
        {
            try
            {
                CurrentTheme = theme;
                ApplyTheme(GetEffectiveTheme());
                SaveThemePreference(theme);

                System.Diagnostics.Debug.WriteLine($"Tema cambiato a: {theme}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel cambio tema: {ex.Message}");
            }
        }

        public Theme GetEffectiveTheme()
        {
            return CurrentTheme switch
            {
                Theme.Light => Theme.Light,
                Theme.Dark => Theme.Dark,
                Theme.Auto => _systemUsesLightTheme ? Theme.Light : Theme.Dark,
                _ => Theme.Light
            };
        }

        private void ApplyTheme(Theme effectiveTheme)
        {
            try
            {
                var application = Application.Current;
                if (application == null) return;

                RemoveThemeDictionaries();

                // Try multiple possible paths for theme files
                var themeUris = effectiveTheme == Theme.Dark
                    ? new[]
                    {
                        new Uri("pack://application:,,,/Styles/DarkTheme.xaml"),
                        new Uri("pack://application:,,,/Themes/DarkTheme.xaml"),
                        new Uri("/Styles/DarkTheme.xaml", UriKind.Relative),
                        new Uri("/Themes/DarkTheme.xaml", UriKind.Relative)
                    }
                    : new[]
                    {
                        new Uri("pack://application:,,,/Styles/LightTheme.xaml"),
                        new Uri("pack://application:,,,/Themes/LightTheme.xaml"),
                        new Uri("/Styles/LightTheme.xaml", UriKind.Relative),
                        new Uri("/Themes/LightTheme.xaml", UriKind.Relative)
                    };

                ResourceDictionary? themeDict = null;
                foreach (var uri in themeUris)
                {
                    try
                    {
                        themeDict = new ResourceDictionary { Source = uri };
                        System.Diagnostics.Debug.WriteLine($"Tema caricato da: {uri}");
                        break;
                    }
                    catch (IOException)
                    {
                        System.Diagnostics.Debug.WriteLine($"File tema non trovato: {uri}");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Errore caricamento tema {uri}: {ex.Message}");
                        continue;
                    }
                }

                if (themeDict != null)
                {
                    application.Resources.MergedDictionaries.Add(themeDict);
                    System.Diagnostics.Debug.WriteLine($"Tema {effectiveTheme} applicato con successo");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Nessun file tema trovato, uso fallback");
                    ApplyFallbackTheme(effectiveTheme);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nell'applicazione del tema: {ex.Message}");
                ApplyFallbackTheme(effectiveTheme);
            }
        }

        private void ApplyFallbackTheme(Theme theme)
        {
            try
            {
                var application = Application.Current;
                if (application == null) return;

                // Create basic fallback theme resources
                var fallbackDict = new ResourceDictionary();

                if (theme == Theme.Dark)
                {
                    fallbackDict["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(26, 26, 26));
                    fallbackDict["SurfaceBrush"] = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                    fallbackDict["TextPrimary"] = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                    fallbackDict["TextSecondary"] = new SolidColorBrush(Color.FromRgb(180, 180, 180));
                    fallbackDict["BorderBrush"] = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                    fallbackDict["AccentBrush"] = new SolidColorBrush(Color.FromRgb(0, 120, 212));
                    fallbackDict["HoverBrush"] = new SolidColorBrush(Color.FromRgb(55, 55, 55));
                }
                else
                {
                    fallbackDict["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    fallbackDict["SurfaceBrush"] = new SolidColorBrush(Color.FromRgb(250, 250, 250));
                    fallbackDict["TextPrimary"] = new SolidColorBrush(Color.FromRgb(50, 49, 48));
                    fallbackDict["TextSecondary"] = new SolidColorBrush(Color.FromRgb(96, 94, 92));
                    fallbackDict["BorderBrush"] = new SolidColorBrush(Color.FromRgb(225, 223, 221));
                    fallbackDict["AccentBrush"] = new SolidColorBrush(Color.FromRgb(0, 120, 212));
                    fallbackDict["HoverBrush"] = new SolidColorBrush(Color.FromRgb(248, 248, 248));
                }

                application.Resources.MergedDictionaries.Add(fallbackDict);
                System.Diagnostics.Debug.WriteLine($"Tema fallback {theme} applicato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nell'applicazione del tema di fallback: {ex.Message}");
            }
        }

        private void RemoveThemeDictionaries()
        {
            try
            {
                var application = Application.Current;
                if (application == null) return;

                for (int i = application.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
                {
                    var dict = application.Resources.MergedDictionaries[i];
                    var source = dict.Source?.ToString();
                    if (source != null && (source.Contains("Theme.xaml") || source.Contains("DarkTheme") || source.Contains("LightTheme")))
                    {
                        application.Resources.MergedDictionaries.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nella rimozione dei dizionari tema: {ex.Message}");
            }
        }

        private void DetectSystemTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");

                _systemUsesLightTheme = value == null || (int)value == 1;

                System.Diagnostics.Debug.WriteLine($"Tema di sistema rilevato: {(_systemUsesLightTheme ? "Light" : "Dark")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel rilevamento tema sistema: {ex.Message}");
                _systemUsesLightTheme = true;
            }
        }

        private void OnSystemPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                DetectSystemTheme();
                if (CurrentTheme == Theme.Auto)
                {
                    ApplyTheme(GetEffectiveTheme());
                }
            }
        }

        private Theme LoadSavedThemeInternal()
        {
            // FIXED: Uncommented Settings integration
            try
            {
                var settingsValue = Properties.Settings.Default.AppTheme;
                if (!string.IsNullOrEmpty(settingsValue) && Enum.TryParse<Theme>(settingsValue, out var theme))
                {
                    System.Diagnostics.Debug.WriteLine($"Tema caricato dalle Settings: {theme}");
                    return theme;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel caricamento dalle Settings: {ex.Message}");
            }

            // Fallback to registry
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\ImmobiGestio");
                var value = key?.GetValue("Theme")?.ToString();

                if (Enum.TryParse<Theme>(value, out var theme))
                {
                    System.Diagnostics.Debug.WriteLine($"Tema caricato dal registro: {theme}");
                    return theme;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel caricamento dal registro: {ex.Message}");
            }

            return Theme.Auto; // Default
        }

        // FIXED: Also uncomment Settings saving in SaveThemePreference()
        private void SaveThemePreference(Theme theme)
        {
            try
            {
                // FIXED: Save to Settings first (primary method)
                Properties.Settings.Default.AppTheme = theme.ToString();
                Properties.Settings.Default.Save();

                // Also save to registry as backup
                using var key = Registry.CurrentUser.CreateSubKey(@"Software\ImmobiGestio");
                key?.SetValue("Theme", theme.ToString());

                System.Diagnostics.Debug.WriteLine($"Salvata preferenza tema: {theme}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel salvataggio preferenza tema: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                SystemEvents.UserPreferenceChanged -= OnSystemPreferenceChanged;
                _disposed = true;
            }
        }
    }
}