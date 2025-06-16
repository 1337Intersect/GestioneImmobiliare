// App.xaml.cs aggiornato con integrazione del ThemeService

using System.Windows;
using System;
using ImmobiGestio.Services;

namespace ImmobiGestio
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== AVVIO APPLICAZIONE CON THEME SERVICE ===");

                // Inizializza il servizio temi PRIMA di tutto
                var themeService = ThemeService.Instance;

                // Carica le preferenze del tema salvate
                themeService.LoadThemePreference();

                // Applica il tema iniziale
                themeService.ApplyTheme();

                System.Diagnostics.Debug.WriteLine($"Tema applicato: {themeService.CurrentTheme}");
                System.Diagnostics.Debug.WriteLine($"Sistema usa tema scuro: {themeService.IsSystemDarkMode}");
                System.Diagnostics.Debug.WriteLine($"App in modalità scura: {themeService.IsDarkMode}");

                // Sottoscrivi ai cambiamenti di tema
                themeService.ThemeChanged += OnThemeChanged;

                System.Diagnostics.Debug.WriteLine("Applicazione avviata correttamente con gestione temi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante l'avvio: {ex.Message}");
                MessageBox.Show($"Errore critico durante l'avvio dell'applicazione:\n\n{ex.Message}",
                    "Errore Avvio", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Tema cambiato: {e.NewTheme}");

            // Aggiorna tutte le finestre aperte
            foreach (Window window in Windows)
            {
                try
                {
                    // Forza l'aggiornamento dell'UI
                    window.InvalidateVisual();

                    // Se la finestra ha metodi specifici per il tema, chiamali qui
                    if (window is MainWindow mainWindow)
                    {
                        mainWindow.OnThemeChanged(e.NewTheme);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore aggiornamento tema finestra: {ex.Message}");
                }
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CHIUSURA APPLICAZIONE ===");

                // Salva le preferenze del tema
                var themeService = ThemeService.Instance;
                themeService.ThemeChanged -= OnThemeChanged;
                themeService.SaveThemePreference();

                System.Diagnostics.Debug.WriteLine("Preferenze tema salvate");
                System.Diagnostics.Debug.WriteLine("Applicazione chiusa correttamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante la chiusura: {ex.Message}");
            }
        }

        // Metodo helper per cambiare tema programmaticamente
        public static void ChangeTheme(AppTheme newTheme)
        {
            try
            {
                var themeService = ThemeService.Instance;
                themeService.CurrentTheme = newTheme;

                System.Diagnostics.Debug.WriteLine($"Tema cambiato programmaticamente: {newTheme}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore cambio tema: {ex.Message}");
            }
        }

        // Metodo helper per toggle tra light e dark
        public static void ToggleTheme()
        {
            try
            {
                var themeService = ThemeService.Instance;
                var currentEffective = themeService.IsDarkMode ? AppTheme.Dark : AppTheme.Light;
                var newTheme = currentEffective == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;

                themeService.CurrentTheme = newTheme;

                System.Diagnostics.Debug.WriteLine($"Tema toggled: {currentEffective} -> {newTheme}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore toggle tema: {ex.Message}");
            }
        }

        // Proprietà helper per accesso al tema corrente
        public static bool IsDarkMode => ThemeService.Instance.IsDarkMode;
        public static AppTheme CurrentTheme => ThemeService.Instance.CurrentTheme;
    }
}

// Aggiorna anche il MainWindow.xaml.cs per gestire i cambiamenti di tema

// Aggiungi questo metodo alla classe MainWindow:
/*

*/