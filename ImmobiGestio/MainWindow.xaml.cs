using ImmobiGestio.Services;
using ImmobiGestio.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace ImmobiGestio
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private DispatcherTimer? _statusTimer;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                InitializeViewModel();
                InitializeTimer();

                System.Diagnostics.Debug.WriteLine("MainWindow inizializzato correttamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore inizializzazione MainWindow: {ex}");
                HandleCriticalError(ex);
            }

        }

        private void InitializeViewModel()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INIZIALIZZAZIONE VIEWMODEL MAINWINDOW ===");

                _viewModel = new MainViewModel();
                DataContext = _viewModel;

                System.Diagnostics.Debug.WriteLine("MainWindow: ViewModel inizializzato e DataContext impostato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore InitializeViewModel: {ex}");
                HandleCriticalError(ex, "Errore nell'inizializzazione dell'applicazione");
            }
        }

        private void InitializeTimer()
        {
            try
            {
                // Timer per aggiornare la status bar ogni minuto
                _statusTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMinutes(1)
                };
                _statusTimer.Tick += StatusTimer_Tick;
                _statusTimer.Start();

                System.Diagnostics.Debug.WriteLine("MainWindow: Timer status inizializzato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore inizializzazione timer: {ex.Message}");
                // Non blocchiamo l'app per un errore del timer
            }
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Cast esplicito per evitare problemi di compilazione
                if (_viewModel is ViewModels.MainViewModel mainViewModel)
                {
                    mainViewModel.RefreshStatusMessage();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore StatusTimer_Tick: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INIZIO CHIUSURA APPLICAZIONE ===");

                // Ferma il timer
                if (_statusTimer != null)
                {
                    _statusTimer.Stop();
                    _statusTimer.Tick -= StatusTimer_Tick;
                    _statusTimer = null;
                    System.Diagnostics.Debug.WriteLine("Timer fermato");
                }

                // Chiama il cleanup del ViewModel
                if (_viewModel != null)
                {
                    _viewModel.OnApplicationClosing();
                    System.Diagnostics.Debug.WriteLine("ViewModel cleanup completato");
                }

                System.Diagnostics.Debug.WriteLine("Chiusura applicazione completata");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante la chiusura: {ex.Message}");
                // Non blocchiamo la chiusura per errori di cleanup
            }
        }

        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("RefreshData_Click chiamato");

                if (_viewModel?.RefreshCommand?.CanExecute(null) == true)
                {
                    _viewModel.RefreshCommand.Execute(null);
                    System.Diagnostics.Debug.WriteLine("RefreshCommand eseguito con successo");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("RefreshCommand non disponibile");
                    ShowMessage("Comando di aggiornamento non disponibile.", "Avviso", MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore RefreshData: {ex}");
                ShowMessage($"Errore nell'aggiornamento dei dati:\n\n{ex.Message}", "Errore", MessageBoxImage.Error);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            try
            {
                base.OnSourceInitialized(e);

                // Imposta il focus iniziale
                Focus();

                System.Diagnostics.Debug.WriteLine("OnSourceInitialized completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnSourceInitialized: {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                base.OnClosed(e);

                // Cleanup finale
                _viewModel = null;
                _statusTimer = null;
                DataContext = null;

                System.Diagnostics.Debug.WriteLine("OnClosed completato - cleanup finale eseguito");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnClosed: {ex.Message}");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== WINDOW LOADED ===");

                // Verifica che tutto sia inizializzato correttamente
                if (_viewModel == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERRORE CRITICO: ViewModel è null dopo il caricamento!");
                    HandleCriticalError(new InvalidOperationException("ViewModel non inizializzato"),
                        "Errore critico: applicazione non correttamente inizializzata");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"ViewModel tipo: {_viewModel.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Vista corrente: {_viewModel.CurrentView}");
                System.Diagnostics.Debug.WriteLine($"Window Title: {_viewModel.WindowTitle}");

                // Test iniziale di sincronizzazione
                _viewModel.TestSync();

                System.Diagnostics.Debug.WriteLine("Applicazione pronta e funzionante");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore Window_Loaded: {ex}");
                HandleCriticalError(ex, "Errore nel caricamento della finestra principale");
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            try
            {
                // Gestisce il cambio di stato della finestra (minimizzata/massimizzata/normale)
                if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized)
                {
                    // Refresh dello status quando la finestra viene ripristinata
                    _viewModel?.RefreshStatusMessage();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore Window_StateChanged: {ex.Message}");
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            try
            {
                // Refresh dello status quando la finestra viene attivata
                _viewModel?.RefreshStatusMessage();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore Window_Activated: {ex.Message}");
            }
        }

        // Helper method per gestire errori critici
        private void HandleCriticalError(Exception ex, string? customMessage = null)
        {
            var message = customMessage ?? "Errore critico nell'applicazione";
            var fullMessage = $"{message}:\n\n{ex.Message}";

            if (ex.InnerException != null)
            {
                fullMessage += $"\n\nDettagli: {ex.InnerException.Message}";
            }

            System.Diagnostics.Debug.WriteLine($"ERRORE CRITICO: {ex}");

            try
            {
                MessageBox.Show(fullMessage, "Errore Critico",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                // Se anche MessageBox fallisce, non possiamo fare altro
                System.Diagnostics.Debug.WriteLine("Impossibile mostrare MessageBox per errore critico");
            }

            try
            {
                Application.Current.Shutdown();
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Impossibile fare shutdown dell'applicazione");
                Environment.Exit(1);
            }
        }

        // Helper method per messaggi sicuri
        private void ShowMessage(string message, string title, MessageBoxImage icon = MessageBoxImage.Information)
        {
            try
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, icon);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore ShowMessage: {ex.Message}");
            }
        }

        // Metodo per testing da codice esterno
        public void TestApplication()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== TEST APPLICAZIONE ===");

                if (_viewModel == null)
                {
                    System.Diagnostics.Debug.WriteLine("TEST FALLITO: ViewModel è null");
                    return;
                }

                // Test navigazione
                _viewModel.NavigateToDashboard();
                System.Diagnostics.Debug.WriteLine("Test navigazione Dashboard: OK");

                _viewModel.NavigateToAppuntamenti();
                System.Diagnostics.Debug.WriteLine("Test navigazione Appuntamenti: OK");

                _viewModel.NavigateToClienti();
                System.Diagnostics.Debug.WriteLine("Test navigazione Clienti: OK");

                _viewModel.NavigateToImmobili();
                System.Diagnostics.Debug.WriteLine("Test navigazione Immobili: OK");

                // Test refresh
                _viewModel.RefreshAllData();
                System.Diagnostics.Debug.WriteLine("Test refresh dati: OK");

                System.Diagnostics.Debug.WriteLine("=== TUTTI I TEST COMPLETATI CON SUCCESSO ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERRORE NEI TEST: {ex}");
            }
        }

        // Proprietà per accesso al ViewModel da esterno
        public MainViewModel? ViewModel => _viewModel;

        // Metodo per forzare un refresh completo
        public void ForceRefresh()
        {
            try
            {
                _viewModel?.RefreshAllData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore ForceRefresh: {ex.Message}");
            }
        }
        public void OnThemeChanged(AppTheme newTheme)
        {
            try
            {
                // Notifica ai ViewModel del cambio tema se necessario
                System.Diagnostics.Debug.WriteLine($"MainWindow: Tema cambiato a {newTheme}");

                // Qui puoi aggiungere logica specifica per il cambio tema se necessario
                // Ad esempio, refresh di colori personalizzati o icone

                _viewModel?.RefreshStatusMessage();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnThemeChanged MainWindow: {ex.Message}");
            }
        }
    }

}