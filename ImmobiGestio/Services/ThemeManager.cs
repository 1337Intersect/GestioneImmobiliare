using System;
using System.Windows;
using Microsoft.Win32;
using System.IO;

namespace ImmobiGestio.Services
{
    public enum Theme
    {
        Light,
        Dark,
        Auto
    }

    public class ThemeManager
    {
        private static ThemeManager? _instance;
        private Theme _currentTheme = Theme.Auto;
        private bool _systemUsesLightTheme = true;

        public static ThemeManager Instance => _instance ??= new ThemeManager();

        public event Action<Theme>? ThemeChanged;

        private ThemeManager()
        {
            // Rileva il tema del sistema all'avvio
            DetectSystemTheme();

            // Monitora i cambiamenti del tema di sistema
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

        public void SetTheme(Theme theme)
        {
            try
            {
                CurrentTheme = theme;
                ApplyTheme(GetEffectiveTheme());

                // Salva la preferenza
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

                // Rimuovi i dizionari di tema esistenti
                RemoveThemeDictionaries();

                // Aggiungi il nuovo dizionario tema
                var themeUri = effectiveTheme == Theme.Dark
                    ? new Uri("pack://application:,,,/Themes/DarkTheme.xaml")
                    : new Uri("pack://application:,,,/Themes/LightTheme.xaml");

                try
                {
                    var themeDict = new ResourceDictionary { Source = themeUri };
                    application.Resources.MergedDictionaries.Add(themeDict);

                    System.Diagnostics.Debug.WriteLine($"Tema {effectiveTheme} applicato con successo");
                }
                catch (IOException)
                {
                    // File tema non trovato, usa i colori di fallback
                    System.Diagnostics.Debug.WriteLine($"File tema {themeUri} non trovato, uso colori di fallback");
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

                var fallbackDict = new ResourceDictionary();

                if (theme == Theme.Dark)
                {
                    // Colori Dark Mode di fallback
                    fallbackDict["BackgroundBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 32, 32));
                    fallbackDict["SurfaceBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 45));
                    fallbackDict["PrimaryBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                    fallbackDict["AccentBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 102, 204));
                    fallbackDict["TextPrimary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 240, 240));
                    fallbackDict["TextSecondary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 180, 180));
                    fallbackDict["TextMuted"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(120, 120, 120));
                    fallbackDict["BorderBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 60, 60));
                    fallbackDict["HoverBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(55, 55, 55));
                }
                else
                {
                    // Colori Light Mode di fallback (quelli già esistenti in App.xaml)
                    fallbackDict["BackgroundBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(250, 250, 250));
                    fallbackDict["SurfaceBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                    fallbackDict["PrimaryBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 51, 51));
                    fallbackDict["AccentBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 102, 204));
                    fallbackDict["TextPrimary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 26, 26));
                    fallbackDict["TextSecondary"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(102, 102, 102));
                    fallbackDict["TextMuted"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 153, 153));
                    fallbackDict["BorderBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 229, 229));
                    fallbackDict["HoverBrush"] = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245));
                }

                application.Resources.MergedDictionaries.Add(fallbackDict);
                System.Diagnostics.Debug.WriteLine("Tema di fallback applicato");
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

                // Rimuovi i dizionari di tema esistenti
                for (int i = application.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
                {
                    var dict = application.Resources.MergedDictionaries[i];
                    if (dict.Source?.ToString().Contains("Theme.xaml") == true)
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
                // Legge la chiave di registro per determinare se Windows usa il tema scuro
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");

                _systemUsesLightTheme = value == null || (int)value == 1;

                System.Diagnostics.Debug.WriteLine($"Tema di sistema rilevato: {(_systemUsesLightTheme ? "Light" : "Dark")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel rilevamento del tema di sistema: {ex.Message}");
                _systemUsesLightTheme = true; // Default a light theme
            }
        }

        private void OnSystemPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            try
            {
                if (e.Category == UserPreferenceCategory.General)
                {
                    var oldTheme = _systemUsesLightTheme;
                    DetectSystemTheme();

                    // Se il tema di sistema è cambiato e stiamo usando Auto, aggiorna
                    if (oldTheme != _systemUsesLightTheme && CurrentTheme == Theme.Auto)
                    {
                        ApplyTheme(GetEffectiveTheme());
                        System.Diagnostics.Debug.WriteLine("Tema aggiornato automaticamente in base al sistema");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel cambio automatico del tema: {ex.Message}");
            }
        }

        public void LoadSavedTheme()
        {
            try
            {
                // Carica la preferenza salvata (in futuro useremo Settings.Default)
                var savedTheme = LoadThemePreference();
                SetTheme(savedTheme);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel caricamento del tema salvato: {ex.Message}");
                SetTheme(Theme.Auto); // Default
            }
        }

        private void SaveThemePreference(Theme theme)
        {
            try
            {
                // Usa le Settings.settings esistenti
                Properties.Settings.Default.AppTheme = theme.ToString();
                Properties.Settings.Default.Save();
                System.Diagnostics.Debug.WriteLine($"Tema salvato nelle Settings: {theme}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel salvataggio della preferenza tema: {ex.Message}");

                // Fallback al registro se le Settings non funzionano
                try
                {
                    using var key = Registry.CurrentUser.CreateSubKey(@"Software\ImmobiGestio");
                    key.SetValue("Theme", theme.ToString());
                }
                catch (Exception regEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore anche nel fallback registro: {regEx.Message}");
                }
            }
        }

        private Theme LoadThemePreference()
        {
            try
            {
                // Prova a caricare dalle Settings.settings
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

            // Fallback al registro
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

        // Cleanup
        public void Dispose()
        {
            SystemEvents.UserPreferenceChanged -= OnSystemPreferenceChanged;
        }
    }
}