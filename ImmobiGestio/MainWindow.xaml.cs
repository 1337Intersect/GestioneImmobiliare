using System.Windows;
using ImmobiGestio.ViewModels;
using System;
using System.Windows.Threading;

namespace ImmobiGestio
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private DispatcherTimer? _statusTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeViewModel();
            InitializeTimer();
        }

        private void InitializeViewModel()
        {
            try
            {
                _viewModel = new MainViewModel();
                DataContext = _viewModel;

                System.Diagnostics.Debug.WriteLine("MainWindow: ViewModel inizializzato correttamente");
            }
            catch (Exception ex)
            {
                var message = $"Errore nell'inizializzazione dell'applicazione:\n\n{ex.Message}";
                if (ex.InnerException != null)
                {
                    message += $"\n\nDettagli: {ex.InnerException.Message}";
                }

                MessageBox.Show(message, "Errore Critico",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                System.Diagnostics.Debug.WriteLine($"Errore MainWindow Init: {ex}");
                Application.Current.Shutdown();
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
                _statusTimer.Tick += (s, e) => _viewModel?.RefreshStatusMessage();
                _statusTimer.Start();

                System.Diagnostics.Debug.WriteLine("MainWindow: Timer inizializzato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore inizializzazione timer: {ex.Message}");
                // Non blocchiamo l'app per un errore del timer
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainWindow: Inizio chiusura applicazione");

                // Ferma il timer
                if (_statusTimer != null)
                {
                    _statusTimer.Stop();
                    _statusTimer = null;
                    System.Diagnostics.Debug.WriteLine("MainWindow: Timer fermato");
                }

                // Chiama il cleanup del ViewModel
                if (_viewModel != null)
                {
                    _viewModel.OnApplicationClosing();
                    System.Diagnostics.Debug.WriteLine("MainWindow: ViewModel cleanup completato");
                }
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
                System.Diagnostics.Debug.WriteLine("MainWindow: RefreshData_Click chiamato");

                if (_viewModel?.RefreshCommand?.CanExecute(null) == true)
                {
                    _viewModel.RefreshCommand.Execute(null);
                    System.Diagnostics.Debug.WriteLine("MainWindow: RefreshCommand eseguito");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("MainWindow: RefreshCommand non disponibile");
                    MessageBox.Show("Comando di aggiornamento non disponibile.", "Avviso",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                var message = $"Errore nell'aggiornamento dei dati:\n\n{ex.Message}";
                MessageBox.Show(message, "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                System.Diagnostics.Debug.WriteLine($"Errore RefreshData: {ex}");
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            try
            {
                base.OnSourceInitialized(e);

                // Imposta il focus iniziale
                Focus();

                System.Diagnostics.Debug.WriteLine("MainWindow: SourceInitialized completato");
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

                System.Diagnostics.Debug.WriteLine("MainWindow: OnClosed completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnClosed: {ex.Message}");
            }
        }

        // Metodo helper per debug
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainWindow: Window_Loaded - Applicazione pronta");

                // Verifica che tutto sia inizializzato correttamente
                if (_viewModel == null)
                {
                    System.Diagnostics.Debug.WriteLine("AVVISO: ViewModel è null dopo il caricamento!");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"MainWindow: ViewModel tipo: {_viewModel.GetType().Name}");
                    System.Diagnostics.Debug.WriteLine($"MainWindow: Vista corrente: {_viewModel.CurrentView}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore Window_Loaded: {ex.Message}");
            }
        }
    }
}