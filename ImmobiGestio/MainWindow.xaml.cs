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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'inizializzazione dell'applicazione: {ex.Message}",
                    "Errore Critico", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void InitializeTimer()
        {
            // Timer per aggiornare la status bar
            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _statusTimer.Tick += (s, e) => _viewModel?.RefreshStatusMessage();
            _statusTimer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _statusTimer?.Stop();
                _viewModel?.OnApplicationClosing();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante la chiusura: {ex.Message}");
            }
        }

        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel?.RefreshStatusMessage();
                MessageBox.Show("Dati aggiornati con successo!", "Aggiornamento",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'aggiornamento dei dati: {ex.Message}", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Imposta il focus iniziale
            Focus();
        }
    }
}