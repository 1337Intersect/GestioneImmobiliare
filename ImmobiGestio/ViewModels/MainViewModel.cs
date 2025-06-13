using System.Collections.ObjectModel;
using System.Windows.Input;
using ImmobiGestio.Commands;
using ImmobiGestio.Data;
using ImmobiGestio.Models;
using System;
using System.Windows;
using System.Linq;

namespace ImmobiGestio.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ImmobiliContext _context;
        private BaseViewModel? _currentViewModel;
        private string _currentView = "Dashboard";
        private string _windowTitle = "ImmobiGestio - Dashboard";

        // ViewModels per le diverse sezioni
        public DashboardViewModel? DashboardViewModel { get; set; }
        public ImmobiliViewModel? ImmobiliViewModel { get; set; }
        public ClientiViewModel? ClientiViewModel { get; set; }
        public AppuntamentiViewModel? AppuntamentiViewModel { get; set; }

        public BaseViewModel? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                SetProperty(ref _currentViewModel, value);
                UpdateWindowTitle();
            }
        }

        public string CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

        // Navigation Commands
        public ICommand? NavigateToDashboardCommand { get; set; }
        public ICommand? NavigateToImmobiliCommand { get; set; }
        public ICommand? NavigateToClientiCommand { get; set; }
        public ICommand? NavigateToAppuntamentiCommand { get; set; }

        // Quick Action Commands
        public ICommand? QuickAddImmobileCommand { get; set; }
        public ICommand? QuickAddClienteCommand { get; set; }
        public ICommand? QuickAddAppuntamentoCommand { get; set; }

        // Menu Commands
        public ICommand? RefreshCommand { get; set; } // <- QUESTO ERA MANCANTE!
        public ICommand? ExitApplicationCommand { get; set; }
        public ICommand? AboutCommand { get; set; }
        public ICommand? BackupDatabaseCommand { get; set; }
        public ICommand? RestoreDatabaseCommand { get; set; }
        public ICommand? SettingsCommand { get; set; }

        // Proprietà per la UI
        public bool IsDashboardActive => CurrentView == "Dashboard";
        public bool IsImmobiliActive => CurrentView == "Immobili";
        public bool IsClientiActive => CurrentView == "Clienti";
        public bool IsAppuntamentiActive => CurrentView == "Appuntamenti";

        public MainViewModel()
        {
            _context = new ImmobiliContext();
            _context.Database.EnsureCreated();

            InitializeViewModels();
            InitializeCommands();
            SetupNavigationEvents();

            // Imposta la vista iniziale
            NavigateToDashboard();
        }

        private void InitializeViewModels()
        {
            try
            {
                DashboardViewModel = new DashboardViewModel(_context);
                ImmobiliViewModel = new ImmobiliViewModel(_context);
                ClientiViewModel = new ClientiViewModel(_context);
                AppuntamentiViewModel = new AppuntamentiViewModel(_context);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'inizializzazione: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void InitializeCommands()
        {
            // Navigation Commands
            NavigateToDashboardCommand = new RelayCommand(_ => NavigateToDashboard());
            NavigateToImmobiliCommand = new RelayCommand(_ => NavigateToImmobili());
            NavigateToClientiCommand = new RelayCommand(_ => NavigateToClienti());
            NavigateToAppuntamentiCommand = new RelayCommand(_ => NavigateToAppuntamenti());

            // Quick Actions
            QuickAddImmobileCommand = new RelayCommand(_ => QuickAddImmobile());
            QuickAddClienteCommand = new RelayCommand(_ => QuickAddCliente());
            QuickAddAppuntamentoCommand = new RelayCommand(_ => QuickAddAppuntamento());

            // Menu Commands
            RefreshCommand = new RelayCommand(_ => RefreshAllData()); // <- AGGIUNTO!
            ExitApplicationCommand = new RelayCommand(_ => ExitApplication());
            AboutCommand = new RelayCommand(_ => ShowAbout());
            BackupDatabaseCommand = new RelayCommand(_ => BackupDatabase());
            RestoreDatabaseCommand = new RelayCommand(_ => RestoreDatabase());
            SettingsCommand = new RelayCommand(_ => ShowSettings());
        }

        private void SetupNavigationEvents()
        {
            // Dashboard navigation events
            if (DashboardViewModel != null)
            {
                DashboardViewModel.NavigateToClienti += () => NavigateToClienti();
                DashboardViewModel.NavigateToImmobili += () => NavigateToImmobili();
                DashboardViewModel.NavigateToAppuntamenti += () => NavigateToAppuntamenti();
                DashboardViewModel.NavigateToCliente += (id) => NavigateToCliente(id);
                DashboardViewModel.NavigateToImmobile += (id) => NavigateToImmobile(id);
                DashboardViewModel.NavigateToAppuntamento += (id) => NavigateToAppuntamento(id);
            }

            // Immobili navigation events
            if (ImmobiliViewModel != null)
            {
                ImmobiliViewModel.NavigateToCliente += (id) => NavigateToCliente(id);
                ImmobiliViewModel.NavigateToAppuntamento += (id) => NavigateToAppuntamento(id);
            }

            // Eventi per sincronizzare i ViewModels quando si creano nuovi oggetti
            if (ClientiViewModel != null)
            {
                ClientiViewModel.AppuntamentoCreated += OnAppuntamentoCreated;
            }

            if (ImmobiliViewModel != null)
            {
                ImmobiliViewModel.AppuntamentoCreated += OnAppuntamentoCreated;
            }
        }

        // Evento per sincronizzare quando viene creato un appuntamento
        private void OnAppuntamentoCreated()
        {
            try
            {
                // Ricarica gli appuntamenti in AppuntamentiViewModel
                AppuntamentiViewModel?.LoadAppuntamenti();

                // Ricarica anche la dashboard se necessario
                if (CurrentView == "Dashboard")
                {
                    DashboardViewModel?.LoadDashboardData();
                }

                // Aggiorna lo status
                RefreshStatusMessage();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nell'aggiornamento dopo creazione appuntamento: {ex.Message}");
            }
        }

        #region Navigation Methods

        public void NavigateToDashboard()
        {
            CurrentView = "Dashboard";
            CurrentViewModel = DashboardViewModel;
            DashboardViewModel?.LoadDashboardData();
            RefreshActiveFlags();
        }

        public void NavigateToImmobili()
        {
            CurrentView = "Immobili";
            CurrentViewModel = ImmobiliViewModel;
            RefreshActiveFlags();
        }

        public void NavigateToClienti()
        {
            CurrentView = "Clienti";
            CurrentViewModel = ClientiViewModel;
            RefreshActiveFlags();
        }

        public void NavigateToAppuntamenti()
        {
            CurrentView = "Appuntamenti";
            CurrentViewModel = AppuntamentiViewModel;
            RefreshActiveFlags();
        }

        public void NavigateToCliente(int clienteId)
        {
            NavigateToClienti();

            var cliente = ClientiViewModel?.Clienti.FirstOrDefault(c => c.Id == clienteId);
            if (cliente != null && ClientiViewModel != null)
            {
                ClientiViewModel.SelectedCliente = cliente;
            }
        }

        public void NavigateToImmobile(int immobileId)
        {
            NavigateToImmobili();

            var immobile = ImmobiliViewModel?.Immobili.FirstOrDefault(i => i.Id == immobileId);
            if (immobile != null && ImmobiliViewModel != null)
            {
                ImmobiliViewModel.SelectedImmobile = immobile;
            }
        }

        public void NavigateToAppuntamento(int appuntamentoId)
        {
            NavigateToAppuntamenti();

            var appuntamento = AppuntamentiViewModel?.Appuntamenti.FirstOrDefault(a => a.Id == appuntamentoId);
            if (appuntamento != null && AppuntamentiViewModel != null)
            {
                AppuntamentiViewModel.SelectedAppuntamento = appuntamento;
            }
        }

        private void RefreshActiveFlags()
        {
            OnPropertyChanged(nameof(IsDashboardActive));
            OnPropertyChanged(nameof(IsImmobiliActive));
            OnPropertyChanged(nameof(IsClientiActive));
            OnPropertyChanged(nameof(IsAppuntamentiActive));
        }

        private void UpdateWindowTitle()
        {
            WindowTitle = CurrentView switch
            {
                "Dashboard" => "ImmobiGestio - Dashboard",
                "Immobili" => "ImmobiGestio - Gestione Immobili",
                "Clienti" => "ImmobiGestio - Gestione Clienti",
                "Appuntamenti" => "ImmobiGestio - Calendario e Appuntamenti",
                _ => "ImmobiGestio"
            };
        }

        #endregion

        #region Quick Actions

        private void QuickAddImmobile()
        {
            NavigateToImmobili();
            ImmobiliViewModel?.AddImmobileCommand?.Execute(null);
        }

        private void QuickAddCliente()
        {
            NavigateToClienti();
            ClientiViewModel?.AddClienteCommand?.Execute(null);
        }

        private void QuickAddAppuntamento()
        {
            NavigateToAppuntamenti();
            AppuntamentiViewModel?.AddAppuntamentoCommand?.Execute(null);
        }

        #endregion

        #region Menu Actions

        private void ExitApplication()
        {
            var result = MessageBox.Show(
                "Sei sicuro di voler chiudere l'applicazione?",
                "Chiudi ImmobiGestio",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void ShowAbout()
        {
            var aboutMessage = "ImmobiGestio v2.0\n\n" +
                              "Sistema di gestione immobiliare completo\n\n" +
                              "Funzionalità:\n" +
                              "• Dashboard con statistiche\n" +
                              "• Gestione immobili completa\n" +
                              "• Anagrafica clienti\n" +
                              "• Calendario appuntamenti\n" +
                              "• Report e analisi\n\n" +
                              "Sviluppato con WPF e Entity Framework Core\n" +
                              "Database SQLite";

            MessageBox.Show(aboutMessage, "Informazioni su ImmobiGestio",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BackupDatabase()
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Database files (*.db)|*.db|All files (*.*)|*.*",
                    FileName = $"immobili_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                    Title = "Salva backup database"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var sourceFile = "immobili.db";
                    if (System.IO.File.Exists(sourceFile))
                    {
                        System.IO.File.Copy(sourceFile, saveFileDialog.FileName, true);
                        MessageBox.Show($"Backup salvato con successo in:\n{saveFileDialog.FileName}",
                            "Backup Completato", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("File database non trovato!", "Errore",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il backup: {ex.Message}", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreDatabase()
        {
            MessageBox.Show("Funzionalità Restore Database - In sviluppo", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowSettings()
        {
            MessageBox.Show("Funzionalità Impostazioni - In sviluppo", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        // Proprietà per la status bar
        public string StatusMessage
        {
            get
            {
                try
                {
                    var totaleImmobili = _context.Immobili.Count();
                    var totaleClienti = _context.Clienti.Count();
                    var appuntamentiOggi = _context.Appuntamenti
                        .Count(a => a.DataInizio.Date == DateTime.Today);

                    return $"Immobili: {totaleImmobili} • Clienti: {totaleClienti} • Appuntamenti oggi: {appuntamentiOggi}";
                }
                catch
                {
                    return "Pronto";
                }
            }
        }

        public void RefreshStatusMessage()
        {
            OnPropertyChanged(nameof(StatusMessage));
        }

        public void RefreshAllData()
        {
            try
            {
                DashboardViewModel?.LoadDashboardData();

                if (CurrentView == "Immobili")
                {
                    ImmobiliViewModel?.LoadImmobili();
                }
                else if (CurrentView == "Clienti")
                {
                    ClientiViewModel?.LoadClienti();
                    ClientiViewModel?.RefreshCurrentCollections();
                }
                else if (CurrentView == "Appuntamenti")
                {
                    AppuntamentiViewModel?.LoadAppuntamenti();
                }

                RefreshStatusMessage();

                MessageBox.Show("Dati aggiornati con successo!", "Aggiornamento",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'aggiornamento dei dati: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OnApplicationClosing()
        {
            try
            {
                ImmobiliViewModel?.OnApplicationClosing();
                ClientiViewModel?.OnApplicationClosing();
                AppuntamentiViewModel?.OnApplicationClosing();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante la chiusura: {ex.Message}");
            }
            finally
            {
                _context?.Dispose();
            }
        }
    }
}