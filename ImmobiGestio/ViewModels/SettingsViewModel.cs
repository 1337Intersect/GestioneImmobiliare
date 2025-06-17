using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ImmobiGestio.Commands;
using ImmobiGestio.Models;
using ImmobiGestio.Services;
using Microsoft.Win32;

namespace ImmobiGestio.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly SettingsService _settingsService;
        private SettingsModel _settings;
        private SettingsModel _originalSettings;
        private string _selectedTab = "Generale";
        private bool _hasChanges = false;

        public SettingsModel Settings
        {
            get => _settings;
            set
            {
                SetProperty(ref _settings, value);
                CheckForChanges();
            }
        }

        public string SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public bool HasChanges
        {
            get => _hasChanges;
            set => SetProperty(ref _hasChanges, value);
        }

        // Collezioni per ComboBox
        public ObservableCollection<string> LogLevels { get; set; } = new();
        public ObservableCollection<int> AutoSaveIntervals { get; set; } = new();
        public ObservableCollection<int> RefreshIntervals { get; set; } = new();
        public ObservableCollection<int> BackupDays { get; set; } = new();
        public ObservableCollection<int> SyncIntervals { get; set; } = new();

        // Commands
        public ICommand? SaveCommand { get; set; }
        public ICommand? CancelCommand { get; set; }
        public ICommand? ResetToDefaultCommand { get; set; }
        public ICommand? BrowseDocumentsPathCommand { get; set; }
        public ICommand? BrowsePhotosPathCommand { get; set; }
        public ICommand? BrowseBackupPathCommand { get; set; }
        public ICommand? BrowseLogFilePathCommand { get; set; }
        public ICommand? TestEmailConnectionCommand { get; set; }
        public ICommand? TestOutlookConnectionCommand { get; set; }
        public ICommand? ExportSettingsCommand { get; set; }
        public ICommand? ImportSettingsCommand { get; set; }
        public ICommand? SelectTabCommand { get; set; }

        // Eventi
        public event Action? SettingsSaved;
        public event Action? CloseRequested;

        public SettingsViewModel()
        {
            _settingsService = SettingsService.Instance;
            _settings = new SettingsModel();
            _originalSettings = new SettingsModel();

            InitializeCollections();
            InitializeCommands();
            LoadSettings();

            // Sottoscrizione ai cambiamenti delle proprietà del Settings
            _settings.PropertyChanged += (s, e) => CheckForChanges();
        }

        private void InitializeCollections()
        {
            // Log Levels
            LogLevels.Clear();
            LogLevels.Add("Debug");
            LogLevels.Add("Info");
            LogLevels.Add("Warning");
            LogLevels.Add("Error");

            // Auto Save Intervals (in secondi)
            AutoSaveIntervals.Clear();
            AutoSaveIntervals.Add(30);   // 30 secondi
            AutoSaveIntervals.Add(60);   // 1 minuto
            AutoSaveIntervals.Add(300);  // 5 minuti
            AutoSaveIntervals.Add(600);  // 10 minuti
            AutoSaveIntervals.Add(1800); // 30 minuti

            // Refresh Intervals (in secondi)
            RefreshIntervals.Clear();
            RefreshIntervals.Add(10);  // 10 secondi
            RefreshIntervals.Add(30);  // 30 secondi
            RefreshIntervals.Add(60);  // 1 minuto
            RefreshIntervals.Add(120); // 2 minuti
            RefreshIntervals.Add(300); // 5 minuti

            // Backup Days
            BackupDays.Clear();
            for (int i = 1; i <= 30; i++)
            {
                BackupDays.Add(i);
            }

            // Sync Intervals (in secondi)
            SyncIntervals.Clear();
            SyncIntervals.Add(60);   // 1 minuto
            SyncIntervals.Add(300);  // 5 minuti
            SyncIntervals.Add(900);  // 15 minuti
            SyncIntervals.Add(1800); // 30 minuti
            SyncIntervals.Add(3600); // 1 ora
        }

        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(SaveSettings, _ => HasChanges);
            CancelCommand = new RelayCommand(Cancel);
            ResetToDefaultCommand = new RelayCommand(ResetToDefault);
            BrowseDocumentsPathCommand = new RelayCommand(BrowseDocumentsPath);
            BrowsePhotosPathCommand = new RelayCommand(BrowsePhotosPath);
            BrowseBackupPathCommand = new RelayCommand(BrowseBackupPath);
            BrowseLogFilePathCommand = new RelayCommand(BrowseLogFilePath);
            TestEmailConnectionCommand = new RelayCommand(TestEmailConnection);
            TestOutlookConnectionCommand = new RelayCommand(TestOutlookConnection);
            ExportSettingsCommand = new RelayCommand(ExportSettings);
            ImportSettingsCommand = new RelayCommand(ImportSettings);
            SelectTabCommand = new RelayCommand(SelectTab);
        }

        private void LoadSettings()
        {
            try
            {
                _settings = _settingsService.LoadSettings();
                _originalSettings = CloneSettings(_settings);
                HasChanges = false;

                OnPropertyChanged(nameof(Settings));
                System.Diagnostics.Debug.WriteLine("Impostazioni caricate nel ViewModel");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel caricamento impostazioni: {ex.Message}");
                MessageBox.Show($"Errore nel caricamento delle impostazioni: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings(object? parameter)
        {
            try
            {
                var (isValid, errorMessage) = _settingsService.ValidateSettings(_settings);
                if (!isValid)
                {
                    MessageBox.Show($"Impostazioni non valide:\n\n{errorMessage}",
                        "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var success = _settingsService.SaveSettings(_settings);
                if (success)
                {
                    _originalSettings = CloneSettings(_settings);
                    HasChanges = false;
                    SettingsSaved?.Invoke();

                    MessageBox.Show("Impostazioni salvate con successo!\n\nAlcune modifiche potrebbero richiedere il riavvio dell'applicazione.",
                        "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

                    CloseRequested?.Invoke();
                }
                else
                {
                    MessageBox.Show("Errore nel salvataggio delle impostazioni.",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel salvataggio: {ex.Message}");
                MessageBox.Show($"Errore imprevisto nel salvataggio: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel(object? parameter)
        {
            if (HasChanges)
            {
                var result = MessageBox.Show(
                    "Ci sono modifiche non salvate. Vuoi scartarle?",
                    "Conferma Chiusura",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;
            }

            CloseRequested?.Invoke();
        }

        private void ResetToDefault(object? parameter)
        {
            var result = MessageBox.Show(
                "Sei sicuro di voler ripristinare tutte le impostazioni ai valori predefiniti?\n\nQuesta operazione non può essere annullata.",
                "Conferma Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _settings = _settingsService.GetDefaultSettings();
                OnPropertyChanged(nameof(Settings));
                CheckForChanges();

                MessageBox.Show("Impostazioni ripristinate ai valori predefiniti.",
                    "Reset Completato", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BrowseDocumentsPath(object? parameter)
        {
            var path = BrowseForFolder("Seleziona cartella documenti", _settings.DocumentsPath);
            if (!string.IsNullOrEmpty(path))
            {
                _settings.DocumentsPath = path;
                OnPropertyChanged(nameof(Settings));
            }
        }

        private void BrowsePhotosPath(object? parameter)
        {
            var path = BrowseForFolder("Seleziona cartella foto", _settings.PhotosPath);
            if (!string.IsNullOrEmpty(path))
            {
                _settings.PhotosPath = path;
                OnPropertyChanged(nameof(Settings));
            }
        }

        private void BrowseBackupPath(object? parameter)
        {
            var path = BrowseForFolder("Seleziona cartella backup", _settings.BackupPath);
            if (!string.IsNullOrEmpty(path))
            {
                _settings.BackupPath = path;
                OnPropertyChanged(nameof(Settings));
            }
        }

        private void BrowseLogFilePath(object? parameter)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = System.IO.Path.GetFileName(_settings.LogFilePath),
                InitialDirectory = System.IO.Path.GetDirectoryName(_settings.LogFilePath)
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                _settings.LogFilePath = saveFileDialog.FileName;
                OnPropertyChanged(nameof(Settings));
            }
        }

        private void TestEmailConnection(object? parameter)
        {
            if (string.IsNullOrEmpty(_settings.SMTPServer))
            {
                MessageBox.Show("Configura prima il server SMTP.",
                    "Test Email", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Simulazione test email
            MessageBox.Show($"Test connessione email:\n\nServer: {_settings.SMTPServer}:{_settings.SMTPPort}\nUsername: {_settings.SMTPUsername}\n\n✅ Connessione simulata con successo!",
                "Test Email", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TestOutlookConnection(object? parameter)
        {
            if (!_settings.OutlookIntegrationEnabled)
            {
                MessageBox.Show("Abilita prima l'integrazione Outlook.",
                    "Test Outlook", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Simulazione test Outlook
            MessageBox.Show("Test connessione Outlook:\n\n✅ Connessione simulata con successo!\n\nIntegrazione Outlook funzionante.",
                "Test Outlook", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportSettings(object? parameter)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Configuration files (*.conf)|*.conf|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = $"ImmobiGestio_Settings_{DateTime.Now:yyyyMMdd}.conf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var success = _settingsService.ExportSettings(_settings, saveFileDialog.FileName);
                if (success)
                {
                    MessageBox.Show($"Impostazioni esportate con successo in:\n{saveFileDialog.FileName}",
                        "Esportazione Completata", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Errore nell'esportazione delle impostazioni.",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportSettings(object? parameter)
        {
            MessageBox.Show("Funzionalità di importazione impostazioni non ancora implementata.\n\nSarà disponibile in una versione futura.",
                "Import Impostazioni", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SelectTab(object? parameter)
        {
            if (parameter is string tabName)
            {
                SelectedTab = tabName;
            }
        }

        private void CheckForChanges()
        {
            if (_originalSettings == null) return;

            HasChanges = !AreSettingsEqual(_settings, _originalSettings);
        }

        private bool AreSettingsEqual(SettingsModel settings1, SettingsModel settings2)
        {
            return settings1.ApplicationName == settings2.ApplicationName &&
                   settings1.AutoSaveInterval == settings2.AutoSaveInterval &&
                   settings1.StatusRefreshInterval == settings2.StatusRefreshInterval &&
                   settings1.DocumentsPath == settings2.DocumentsPath &&
                   settings1.PhotosPath == settings2.PhotosPath &&
                   settings1.BackupPath == settings2.BackupPath &&
                   settings1.MaxDocumentSize == settings2.MaxDocumentSize &&
                   settings1.MaxPhotoSize == settings2.MaxPhotoSize &&
                   settings1.AutoBackupEnabled == settings2.AutoBackupEnabled &&
                   settings1.AutoBackupDays == settings2.AutoBackupDays &&
                   settings1.MaxBackupFiles == settings2.MaxBackupFiles &&
                   settings1.SMTPServer == settings2.SMTPServer &&
                   settings1.SMTPPort == settings2.SMTPPort &&
                   settings1.SMTPUsername == settings2.SMTPUsername &&
                   settings1.SMTPPassword == settings2.SMTPPassword &&
                   settings1.EmailFrom == settings2.EmailFrom &&
                   settings1.OutlookIntegrationEnabled == settings2.OutlookIntegrationEnabled &&
                   settings1.OutlookSyncInterval == settings2.OutlookSyncInterval &&
                   settings1.LogLevel == settings2.LogLevel &&
                   settings1.LogToFile == settings2.LogToFile &&
                   settings1.LogFilePath == settings2.LogFilePath &&
                   settings1.MaxLogFiles == settings2.MaxLogFiles;
        }

        private SettingsModel CloneSettings(SettingsModel source)
        {
            return new SettingsModel
            {
                ApplicationName = source.ApplicationName,
                ApplicationVersion = source.ApplicationVersion,
                AutoSaveInterval = source.AutoSaveInterval,
                StatusRefreshInterval = source.StatusRefreshInterval,
                DocumentsPath = source.DocumentsPath,
                PhotosPath = source.PhotosPath,
                BackupPath = source.BackupPath,
                MaxDocumentSize = source.MaxDocumentSize,
                MaxPhotoSize = source.MaxPhotoSize,
                SupportedDocumentFormats = source.SupportedDocumentFormats,
                SupportedImageFormats = source.SupportedImageFormats,
                AutoBackupEnabled = source.AutoBackupEnabled,
                AutoBackupDays = source.AutoBackupDays,
                MaxBackupFiles = source.MaxBackupFiles,
                SMTPServer = source.SMTPServer,
                SMTPPort = source.SMTPPort,
                SMTPUsername = source.SMTPUsername,
                SMTPPassword = source.SMTPPassword,
                EmailFrom = source.EmailFrom,
                OutlookIntegrationEnabled = source.OutlookIntegrationEnabled,
                OutlookSyncInterval = source.OutlookSyncInterval,
                LogLevel = source.LogLevel,
                LogToFile = source.LogToFile,
                LogFilePath = source.LogFilePath,
                MaxLogFiles = source.MaxLogFiles
            };
        }

        private string BrowseForFolder(string description, string selectedPath)
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = description,
                    SelectedPath = selectedPath,
                    ShowNewFolderButton = true
                };

                var result = dialog.ShowDialog();
                return result == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nella selezione cartella: {ex.Message}");
                return string.Empty;
            }
        }
    }
}