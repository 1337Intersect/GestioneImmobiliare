// ===== APP CODE-BEHIND AGGIORNATO - App.xaml.cs =====
using System.Windows;
using System;
using ImmobiGestio.Services;
using Microsoft.Win32;

namespace ImmobiGestio
{
    public partial class App : Application
    {
        private ThemeManager? _themeManager;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== AVVIO APPLICAZIONE ===");

                // Inizializza il sistema di temi PRIMA di tutto
                InitializeThemeSystem();

                // Inizializzazione globale se necessaria
                // Ad esempio, configurazione cultura, logging, etc.

                System.Diagnostics.Debug.WriteLine("Applicazione avviata correttamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante l'avvio: {ex.Message}");
                MessageBox.Show($"Errore critico durante l'avvio dell'applicazione:\n\n{ex.Message}",
                    "Errore Avvio", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void InitializeThemeSystem()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INIZIALIZZAZIONE SISTEMA TEMI ===");

                // Inizializza il ThemeManager
                _themeManager = ThemeManager.Instance;

                // Carica il tema salvato dall'utente (o usa Auto come default)
                _themeManager.LoadSavedTheme();

                // Opzionale: Log del tema corrente
                var currentTheme = _themeManager.CurrentTheme;
                var effectiveTheme = _themeManager.GetEffectiveTheme();
                var isSystemDark = _themeManager.IsSystemDarkMode;

                System.Diagnostics.Debug.WriteLine($"Tema utente: {currentTheme}");
                System.Diagnostics.Debug.WriteLine($"Tema effettivo: {effectiveTheme}");
                System.Diagnostics.Debug.WriteLine($"Sistema in dark mode: {isSystemDark}");

                // Registra l'evento per cambiamenti di tema
                _themeManager.ThemeChanged += OnThemeChanged;

                System.Diagnostics.Debug.WriteLine("Sistema temi inizializzato con successo");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nell'inizializzazione del sistema temi: {ex.Message}");

                // In caso di errore, prova ad applicare un tema di fallback
                try
                {
                    ApplyFallbackTheme();
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore anche nel tema di fallback: {fallbackEx.Message}");
                }
            }
        }

        private void OnThemeChanged(Theme newTheme)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== CAMBIO TEMA: {newTheme} ===");

                // Qui potresti aggiungere logica aggiuntiva quando il tema cambia
                // Ad esempio, aggiornare icone, notificare altri servizi, etc.

                // Forza un refresh delle finestre se necessario
                foreach (Window window in Windows)
                {
                    try
                    {
                        window.InvalidateVisual();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Errore nel refresh della finestra: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nella gestione del cambio tema: {ex.Message}");
            }
        }

        private void ApplyFallbackTheme()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Applicazione tema di fallback...");

                // Crea un dizionario di risorse di base per garantire che l'app funzioni
                var fallbackDict = new ResourceDictionary();

                // Colori di base Light Theme
                fallbackDict["BackgroundBrush"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(250, 250, 250));
                fallbackDict["SurfaceBrush"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(255, 255, 255));
                fallbackDict["TextPrimary"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(26, 26, 26));
                fallbackDict["AccentBrush"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0, 102, 204));
                fallbackDict["BorderBrush"] = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(229, 229, 229));

                Resources.MergedDictionaries.Add(fallbackDict);

                System.Diagnostics.Debug.WriteLine("Tema di fallback applicato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nell'applicazione del tema di fallback: {ex.Message}");
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CHIUSURA APPLICAZIONE ===");

                // Cleanup del ThemeManager
                if (_themeManager != null)
                {
                    _themeManager.ThemeChanged -= OnThemeChanged;
                    _themeManager.Dispose();
                    _themeManager = null;
                    System.Diagnostics.Debug.WriteLine("ThemeManager disposed");
                }

                // Cleanup globale se necessario
                // Ad esempio, salvataggio configurazioni, chiusura risorse, etc.

                System.Diagnostics.Debug.WriteLine("Applicazione chiusa correttamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante la chiusura: {ex.Message}");
                // Non mostrare messaggi durante la chiusura per evitare di bloccare
            }
        }

        // Metodo helper per cambiare tema da codice (per future implementazioni nelle Settings)
        public void ChangeTheme(Theme theme)
        {
            try
            {
                _themeManager?.SetTheme(theme);
                System.Diagnostics.Debug.WriteLine($"Tema cambiato programmaticamente a: {theme}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel cambio programmatico del tema: {ex.Message}");
            }
        }

        // Proprietà per accedere al ThemeManager da altre parti dell'app
        public static ThemeManager? ThemeManager => (Current as App)?._themeManager;

        // Metodo per testare il sistema di temi
        public void TestThemeSystem()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== TEST SISTEMA TEMI ===");

                if (_themeManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("ThemeManager non inizializzato!");
                    return;
                }

                // Testa tutti i temi
                System.Diagnostics.Debug.WriteLine("Test tema Light...");
                _themeManager.SetTheme(Theme.Light);
                System.Threading.Thread.Sleep(1000);

                System.Diagnostics.Debug.WriteLine("Test tema Dark...");
                _themeManager.SetTheme(Theme.Dark);
                System.Threading.Thread.Sleep(1000);

                System.Diagnostics.Debug.WriteLine("Ripristino tema Auto...");
                _themeManager.SetTheme(Theme.Auto);

                System.Diagnostics.Debug.WriteLine("Test sistema temi completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel test del sistema temi: {ex.Message}");
            }
        }

        // Override per gestire eccezioni non gestite
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Registra il gestore per eccezioni non gestite
                DispatcherUnhandledException += App_DispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in OnStartup: {ex.Message}");
                throw;
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== ECCEZIONE NON GESTITA UI ===");
                System.Diagnostics.Debug.WriteLine($"Eccezione: {e.Exception}");

                MessageBox.Show($"Si è verificato un errore imprevisto:\n\n{e.Exception.Message}\n\nL'applicazione continuerà a funzionare.",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);

                e.Handled = true; // Previeni il crash dell'app
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nella gestione dell'eccezione UI: {ex.Message}");
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== ECCEZIONE NON GESTITA DOMINIO ===");
                System.Diagnostics.Debug.WriteLine($"Eccezione: {e.ExceptionObject}");
                System.Diagnostics.Debug.WriteLine($"È terminale: {e.IsTerminating}");

                if (e.ExceptionObject is Exception ex)
                {
                    MessageBox.Show($"Errore critico:\n\n{ex.Message}\n\nL'applicazione verrà chiusa.",
                        "Errore Critico", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nella gestione dell'eccezione del dominio: {ex.Message}");
            }
        }
    }
}