using System.Collections.ObjectModel;
using System.Windows.Input;
using ImmobiGestio.Commands;
using ImmobiGestio.Data;
using ImmobiGestio.Models;
using ImmobiGestio.Helpers;
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
        public ICommand? RefreshCommand { get; set; }
        public ICommand? ExitApplicationCommand { get; set; }
        public ICommand? AboutCommand { get; set; }
        public ICommand? BackupDatabaseCommand { get; set; }
        public ICommand? RestoreDatabaseCommand { get; set; }
        public ICommand? SettingsCommand { get; set; }
        public ICommand? OpenSettingsTabCommand { get; set; }

        // Proprietà per la UI
        public bool IsDashboardActive => CurrentView == "Dashboard";
        public bool IsImmobiliActive => CurrentView == "Immobili";
        public bool IsClientiActive => CurrentView == "Clienti";
        public bool IsAppuntamentiActive => CurrentView == "Appuntamenti";

        public MainViewModel()
        {
            try
            {
                _context = new ImmobiliContext();
                _context.Database.EnsureCreated();

                InitializeViewModels();
                InitializeCommands();
                SetupNavigationEvents();

                // Imposta la vista iniziale
                NavigateToDashboard();

                System.Diagnostics.Debug.WriteLine("MainViewModel inizializzato correttamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore inizializzazione MainViewModel: {ex}");
                MessageBox.Show($"Errore nell'inizializzazione dell'applicazione: {ex.Message}",
                    "Errore Critico", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void InitializeViewModels()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INIZIALIZZAZIONE VIEWMODELS ===");

                DashboardViewModel = new DashboardViewModel(_context);
                System.Diagnostics.Debug.WriteLine("DashboardViewModel creato");

                ImmobiliViewModel = new ImmobiliViewModel(_context);
                System.Diagnostics.Debug.WriteLine("ImmobiliViewModel creato");

                ClientiViewModel = new ClientiViewModel(_context);
                System.Diagnostics.Debug.WriteLine("ClientiViewModel creato");

                AppuntamentiViewModel = new AppuntamentiViewModel(_context);
                System.Diagnostics.Debug.WriteLine("AppuntamentiViewModel creato");

                System.Diagnostics.Debug.WriteLine("Tutti i ViewModels creati con successo");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore InitializeViewModels: {ex}");
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
            RefreshCommand = new RelayCommand(_ => RefreshAllData());
            ExitApplicationCommand = new RelayCommand(_ => ExitApplication());
            AboutCommand = new RelayCommand(_ => ShowAbout());
            BackupDatabaseCommand = new RelayCommand(_ => BackupDatabase());
            RestoreDatabaseCommand = new RelayCommand(_ => RestoreDatabase());
            SettingsCommand = new RelayCommand(_ => ShowSettings());

            System.Diagnostics.Debug.WriteLine("Comandi inizializzati");

            OpenSettingsTabCommand = new RelayCommand(param => {
                if (param is string tabName)
                {
                    ShowSettingsTab(tabName);
                }
                else
                {
                    ShowSettings();
                }
            });
        }

        private void SetupNavigationEvents()
        {
            try
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
                    ImmobiliViewModel.AppuntamentoCreated += () => OnDataChanged("appuntamento");
                }

                // Eventi per sincronizzare i ViewModels quando si creano nuovi oggetti
                if (ClientiViewModel != null)
                {
                    ClientiViewModel.AppuntamentoCreated += () => OnDataChanged("appuntamento");
                }

                System.Diagnostics.Debug.WriteLine("Eventi di navigazione configurati");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore SetupNavigationEvents: {ex.Message}");
            }
        }

        #region Navigation Methods

        public void NavigateToDashboard()
        {
            try
            {
                CurrentView = "Dashboard";
                CurrentViewModel = DashboardViewModel;
                DashboardViewModel?.LoadDashboardData();
                RefreshActiveFlags();
                System.Diagnostics.Debug.WriteLine("Navigazione a Dashboard completata");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore NavigateToDashboard: {ex.Message}");
            }
        }

        public void NavigateToClienti()
        {
            try
            {
                CurrentView = "Clienti";
                CurrentViewModel = ClientiViewModel;
                ClientiViewModel?.LoadClienti();
                RefreshActiveFlags();
                System.Diagnostics.Debug.WriteLine("Navigazione a Clienti completata");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore NavigateToClienti: {ex.Message}");
            }
        }

        public void NavigateToImmobili()
        {
            try
            {
                CurrentView = "Immobili";
                CurrentViewModel = ImmobiliViewModel;
                ImmobiliViewModel?.LoadImmobili();
                RefreshActiveFlags();
                System.Diagnostics.Debug.WriteLine("Navigazione a Immobili completata");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore NavigateToImmobili: {ex.Message}");
            }
        }

        public void NavigateToAppuntamenti()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== NAVIGAZIONE A APPUNTAMENTI ===");

                CurrentView = "Appuntamenti";
                CurrentViewModel = AppuntamentiViewModel;

                // REFRESH AUTOMATICO completo quando si naviga
                AppuntamentiViewModel?.LoadAllData();

                RefreshActiveFlags();

                System.Diagnostics.Debug.WriteLine("Navigazione a Appuntamenti completata con refresh");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore NavigateToAppuntamenti: {ex.Message}");
                MessageBox.Show($"Errore nella navigazione agli appuntamenti: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NavigateToCliente(int clienteId)
        {
            try
            {
                NavigateToClienti();

                var cliente = ClientiViewModel?.Clienti.FirstOrDefault(c => c.Id == clienteId);
                if (cliente != null && ClientiViewModel != null)
                {
                    ClientiViewModel.SelectedCliente = cliente;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore NavigateToCliente: {ex.Message}");
            }
        }

        public void NavigateToImmobile(int immobileId)
        {
            try
            {
                NavigateToImmobili();

                var immobile = ImmobiliViewModel?.Immobili.FirstOrDefault(i => i.Id == immobileId);
                if (immobile != null && ImmobiliViewModel != null)
                {
                    ImmobiliViewModel.SelectedImmobile = immobile;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore NavigateToImmobile: {ex.Message}");
            }
        }

        public void NavigateToAppuntamento(int appuntamentoId)
        {
            try
            {
                NavigateToAppuntamenti();

                var appuntamento = AppuntamentiViewModel?.Appuntamenti.FirstOrDefault(a => a.Id == appuntamentoId);
                if (appuntamento != null && AppuntamentiViewModel != null)
                {
                    AppuntamentiViewModel.SelectedAppuntamento = appuntamento;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore NavigateToAppuntamento: {ex.Message}");
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
            try
            {
                NavigateToImmobili();
                ImmobiliViewModel?.AddImmobileCommand?.Execute(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore QuickAddImmobile: {ex.Message}");
            }
        }

        private void QuickAddCliente()
        {
            try
            {
                NavigateToClienti();
                ClientiViewModel?.AddClienteCommand?.Execute(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore QuickAddCliente: {ex.Message}");
            }
        }

        private void QuickAddAppuntamento()
        {
            try
            {
                NavigateToAppuntamenti();
                AppuntamentiViewModel?.AddAppuntamentoCommand?.Execute(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore QuickAddAppuntamento: {ex.Message}");
            }
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
            try
            {
                System.Diagnostics.Debug.WriteLine("=== APERTURA FINESTRA IMPOSTAZIONI ===");

                // Trova la finestra principale
                var mainWindow = Application.Current.MainWindow;

                // Apri la finestra delle impostazioni
                var result = ImmobiGestio.Views.SettingsWindow.ShowSettingsDialog(mainWindow);

                if (result == true)
                {
                    System.Diagnostics.Debug.WriteLine("Impostazioni salvate - aggiornamento applicazione");

                    // Ricarica le impostazioni nell'applicazione
                    RefreshApplicationSettings();

                    // Mostra messaggio di successo
                    MessageBox.Show("Impostazioni aggiornate con successo!\n\n" +
                                   "Alcune modifiche potrebbero richiedere il riavvio dell'applicazione.",
                        "Impostazioni Salvate", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Finestra impostazioni chiusa senza salvare");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore ShowSettings: {ex.Message}");
                MessageBox.Show($"Errore nell'apertura delle impostazioni: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshApplicationSettings()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== AGGIORNAMENTO IMPOSTAZIONI APPLICAZIONE ===");

                // Ricarica le impostazioni dal servizio
                var settingsService = ImmobiGestio.Services.SettingsService.Instance;
                var newSettings = settingsService.LoadSettings();

                // Aggiorna i timer se necessario
                UpdateTimersFromSettings(newSettings);

                // Notifica ai ViewModels dei cambiamenti
                NotifyViewModelsOfSettingsChange(newSettings);

                // Aggiorna la status bar
                RefreshStatusMessage();

                System.Diagnostics.Debug.WriteLine("Aggiornamento impostazioni completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore RefreshApplicationSettings: {ex.Message}");
            }
        }

        private void UpdateTimersFromSettings(ImmobiGestio.Models.SettingsModel settings)
        {
            try
            {
                // Questo metodo può essere utilizzato per aggiornare timer nell'applicazione
                // basandosi sulle nuove impostazioni

                System.Diagnostics.Debug.WriteLine($"Timer aggiornati:");
                System.Diagnostics.Debug.WriteLine($"- AutoSave: {settings.AutoSaveIntervalFormatted}");
                System.Diagnostics.Debug.WriteLine($"- StatusRefresh: {settings.StatusRefreshIntervalFormatted}");

                // Se hai timer nell'applicazione principale, aggiornali qui
                // Esempio:
                // if (_autoSaveTimer != null)
                // {
                //     _autoSaveTimer.Interval = TimeSpan.FromSeconds(settings.AutoSaveInterval);
                // }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore UpdateTimersFromSettings: {ex.Message}");
            }
        }

        private void NotifyViewModelsOfSettingsChange(ImmobiGestio.Models.SettingsModel settings)
        {
            try
            {
                // Notifica ai ViewModels che le impostazioni sono cambiate

                // Esempio per ImmobiliViewModel - aggiorna i limiti file
                if (ImmobiliViewModel != null)
                {
                    System.Diagnostics.Debug.WriteLine("Notifica impostazioni a ImmobiliViewModel");
                    // ImmobiliViewModel.UpdateFileSettings(settings);
                }

                // Esempio per AppuntamentiViewModel - aggiorna integrazione Outlook
                if (AppuntamentiViewModel != null)
                {
                    System.Diagnostics.Debug.WriteLine("Notifica impostazioni a AppuntamentiViewModel");
                    // AppuntamentiViewModel.UpdateOutlookSettings(settings);
                }

                // Altri ViewModels...

                System.Diagnostics.Debug.WriteLine("Notifiche impostazioni inviate a tutti i ViewModels");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore NotifyViewModelsOfSettingsChange: {ex.Message}");
            }
        }

        public void ShowSettingsTab(string tabName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== APERTURA IMPOSTAZIONI TAB: {tabName} ===");

                var mainWindow = Application.Current.MainWindow;
                var settingsWindow = new ImmobiGestio.Views.SettingsWindow();

                if (mainWindow != null)
                {
                    settingsWindow.Owner = mainWindow;
                }

                // Focalizza sul tab richiesto
                settingsWindow.FocusOnTab(tabName);

                var result = settingsWindow.ShowDialog();

                if (result == true)
                {
                    RefreshApplicationSettings();
                    MessageBox.Show("Impostazioni aggiornate con successo!",
                        "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore ShowSettingsTab: {ex.Message}");
                MessageBox.Show($"Errore nell'apertura delle impostazioni: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore StatusMessage: {ex.Message}");
                    return "Pronto";
                }
            }
        }

        public void RefreshStatusMessage()
        {
            try
            {
                OnPropertyChanged(nameof(StatusMessage));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore RefreshStatusMessage: {ex.Message}");
            }
        }

        public void RefreshAllData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== REFRESH GLOBALE DATI ===");

                // Refresh sempre la dashboard
                DashboardViewModel?.LoadDashboardData();

                // Refresh la vista corrente
                switch (CurrentView)
                {
                    case "Immobili":
                        ImmobiliViewModel?.LoadImmobili();
                        break;
                    case "Clienti":
                        ClientiViewModel?.LoadClienti();
                        ClientiViewModel?.RefreshCurrentCollections();
                        break;
                    case "Appuntamenti":
                        AppuntamentiViewModel?.LoadAllData();
                        break;
                }

                RefreshStatusMessage();

                MessageBox.Show("Dati aggiornati con successo!", "Aggiornamento",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                System.Diagnostics.Debug.WriteLine("Refresh globale completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore RefreshAllData: {ex.Message}");
                MessageBox.Show($"Errore nell'aggiornamento dei dati: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OnApplicationClosing()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CHIUSURA APPLICAZIONE ===");

                ImmobiliViewModel?.OnApplicationClosing();
                ClientiViewModel?.OnApplicationClosing();
                AppuntamentiViewModel?.OnApplicationClosing();

                System.Diagnostics.Debug.WriteLine("Cleanup ViewModels completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante la chiusura: {ex.Message}");
            }
            finally
            {
                try
                {
                    _context?.Dispose();
                    System.Diagnostics.Debug.WriteLine("Context disposed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore dispose context: {ex.Message}");
                }
            }
        }

        private void OnDataChanged(string dataType, int? id = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== NOTIFICA CAMBIO DATI: {dataType} ===");

                switch (dataType.ToLower())
                {
                    case "appuntamento":
                        // Ricarica appuntamenti se è la vista corrente
                        if (CurrentView == "Appuntamenti")
                        {
                            AppuntamentiViewModel?.LoadAllData();
                        }
                        else
                        {
                            // Ricarica solo i dati necessari per gli altri ViewModels
                            AppuntamentiViewModel?.LoadAppuntamenti();
                        }
                        // Aggiorna sempre la dashboard
                        DashboardViewModel?.LoadDashboardData();
                        break;

                    case "cliente":
                        ClientiViewModel?.LoadClienti();
                        AppuntamentiViewModel?.LoadClientiEImmobiliDisponibili();
                        if (CurrentView == "Dashboard")
                            DashboardViewModel?.LoadDashboardData();
                        break;

                    case "immobile":
                        ImmobiliViewModel?.LoadImmobili();
                        AppuntamentiViewModel?.LoadClientiEImmobiliDisponibili();
                        if (CurrentView == "Dashboard")
                            DashboardViewModel?.LoadDashboardData();
                        break;
                }

                RefreshStatusMessage();
                System.Diagnostics.Debug.WriteLine($"Sincronizzazione {dataType} completata");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore sincronizzazione {dataType}: {ex.Message}");
            }
        }

        public void TestSync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== TEST SINCRONIZZAZIONE ===");

                // Forza il refresh di tutti i ViewModels
                DashboardViewModel?.LoadDashboardData();
                ImmobiliViewModel?.LoadImmobili();
                ClientiViewModel?.LoadClienti();
                AppuntamentiViewModel?.LoadAllData();

                RefreshStatusMessage();

                System.Diagnostics.Debug.WriteLine("Test sincronizzazione completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore TestSync: {ex.Message}");
            }
        }
    }
}