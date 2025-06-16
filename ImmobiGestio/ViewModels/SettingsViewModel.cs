using System;
using System.Windows;
using System.Windows.Input;
using ImmobiGestio.Commands;
using ImmobiGestio.Services;

namespace ImmobiGestio.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ThemeService _themeService;
        private AppTheme _currentTheme;
        private AppTheme _originalTheme;

        public SettingsViewModel()
        {
            _themeService = ThemeService.Instance;
            _currentTheme = _themeService.CurrentTheme;
            _originalTheme = _currentTheme; // Salva il tema originale per il reset

            InitializeCommands();

            // Sottoscrivi ai cambiamenti del tema di sistema
            _themeService.ThemeChanged += OnThemeServiceChanged;
        }

        public AppTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (SetProperty(ref _currentTheme, value))
                {
                    // Applica immediatamente il tema
                    _themeService.CurrentTheme = value;

                    // Notifica il cambio delle proprietà display
                    OnPropertyChanged(nameof(CurrentThemeDisplay));
                    OnPropertyChanged(nameof(ActiveThemeDisplay));
                }
            }
        }

        public string CurrentThemeDisplay
        {
            get
            {
                return CurrentTheme switch
                {
                    AppTheme.System => "Segui sistema",
                    AppTheme.Light => "Chiaro",
                    AppTheme.Dark => "Scuro",
                    _ => "Sconosciuto"
                };
            }
        }

        public string SystemThemeDisplay
        {
            get
            {
                return _themeService.IsSystemDarkMode ? "🌙 Scuro" : "☀️ Chiaro";
            }
        }

        public string ActiveThemeDisplay
        {
            get
            {
                return _themeService.IsDarkMode ? "🌙 Modalità Scura" : "☀️ Modalità Chiara";
            }
        }

        // Commands
        public ICommand? TestThemeCommand { get; private set; }
        public ICommand? ResetThemeCommand { get; private set; }
        public ICommand? SaveAndCloseCommand { get; private set; }

        private void InitializeCommands()
        {
            TestThemeCommand = new RelayCommand(TestTheme);
            ResetThemeCommand = new RelayCommand(ResetTheme);
            SaveAndCloseCommand = new RelayCommand(SaveAndClose);
        }

        private void TestTheme(object? parameter)
        {
            try
            {
                // Cicla tra i temi per test
                var nextTheme = CurrentTheme switch
                {
                    AppTheme.System => AppTheme.Light,
                    AppTheme.Light => AppTheme.Dark,
                    AppTheme.Dark => AppTheme.System,
                    _ => AppTheme.System
                };

                CurrentTheme = nextTheme;

                var message = $"Tema di test applicato: {CurrentThemeDisplay}\n\n" +
                             $"Sistema: {SystemThemeDisplay}\n" +
                             $"Modalità attiva: {ActiveThemeDisplay}";

                MessageBox.Show(message, "Test Tema", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel test del tema: {ex.Message}",
                               "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetTheme(object? parameter)
        {
            try
            {
                var result = MessageBox.Show(
                    "Vuoi ripristinare il tema originale?",
                    "Ripristina Tema",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    CurrentTheme = _originalTheme;

                    MessageBox.Show($"Tema ripristinato: {CurrentThemeDisplay}",
                                   "Tema Ripristinato", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel ripristino del tema: {ex.Message}",
                               "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAndClose(object? parameter)
        {
            try
            {
                // Salva le impostazioni
                _themeService.SaveThemePreference();

                // Aggiorna il tema originale per le prossime volte
                _originalTheme = CurrentTheme;

                // Chiudi la finestra
                CloseWindow();

                var message = $"Impostazioni salvate!\n\n" +
                             $"Tema: {CurrentThemeDisplay}\n" +
                             $"Modalità attiva: {ActiveThemeDisplay}";

                MessageBox.Show(message, "Impostazioni Salvate",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel salvataggio delle impostazioni: {ex.Message}",
                               "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnThemeServiceChanged(object? sender, ThemeChangedEventArgs e)
        {
            // Aggiorna le proprietà display quando cambia il tema di sistema
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(SystemThemeDisplay));
                OnPropertyChanged(nameof(ActiveThemeDisplay));
            });
        }

        public void CloseWindow()
        {
            // Trova e chiudi la finestra delle impostazioni
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        // Cleanup quando il ViewModel viene distrutto
        ~SettingsViewModel()
        {
            _themeService.ThemeChanged -= OnThemeServiceChanged;
        }
    }
}