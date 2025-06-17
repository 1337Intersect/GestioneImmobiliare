using System;
using System.Collections.Generic;
using System.Linq;
using ImmobiGestio.Models;
using ImmobiGestio.Services;

namespace ImmobiGestio.Helpers
{
    /// <summary>
    /// Helper per integrare le impostazioni nei ViewModels esistenti
    /// </summary>
    public static class SettingsIntegrationHelper
    {
        private static readonly List<Action<SettingsModel>> _settingsChangedCallbacks = new();
        private static SettingsModel? _currentSettings;

        /// <summary>
        /// Inizializza l'helper e carica le impostazioni
        /// </summary>
        public static void Initialize()
        {
            try
            {
                _currentSettings = SettingsService.Instance.LoadSettings();
                System.Diagnostics.Debug.WriteLine("SettingsIntegrationHelper inizializzato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore inizializzazione SettingsIntegrationHelper: {ex.Message}");
                _currentSettings = new SettingsModel(); // Fallback a default
            }
        }

        /// <summary>
        /// Ottiene le impostazioni correnti
        /// </summary>
        public static SettingsModel CurrentSettings
        {
            get
            {
                if (_currentSettings == null)
                {
                    Initialize();
                }
                return _currentSettings!;
            }
        }

        /// <summary>
        /// Notifica che le impostazioni sono cambiate
        /// </summary>
        public static void NotifySettingsChanged(SettingsModel newSettings)
        {
            try
            {
                _currentSettings = newSettings;

                // Notifica tutti i callback registrati
                foreach (var callback in _settingsChangedCallbacks.ToList())
                {
                    try
                    {
                        callback.Invoke(newSettings);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Errore nel callback settings: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Notifica settings inviata a {_settingsChangedCallbacks.Count} listener");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore NotifySettingsChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra un callback per quando le impostazioni cambiano
        /// </summary>
        public static void RegisterSettingsChangedCallback(Action<SettingsModel> callback)
        {
            if (callback != null && !_settingsChangedCallbacks.Contains(callback))
            {
                _settingsChangedCallbacks.Add(callback);
            }
        }

        /// <summary>
        /// Rimuove un callback
        /// </summary>
        public static void UnregisterSettingsChangedCallback(Action<SettingsModel> callback)
        {
            _settingsChangedCallbacks.Remove(callback);
        }

        /// <summary>
        /// Ottiene la dimensione massima file per documenti in bytes
        /// </summary>
        public static long GetMaxDocumentSize()
        {
            return CurrentSettings.MaxDocumentSize;
        }

        /// <summary>
        /// Ottiene la dimensione massima file per foto in bytes
        /// </summary>
        public static long GetMaxPhotoSize()
        {
            return CurrentSettings.MaxPhotoSize;
        }

        /// <summary>
        /// Verifica se un file è supportato come documento
        /// </summary>
        public static bool IsDocumentFormatSupported(string fileExtension)
        {
            var supportedFormats = CurrentSettings.SupportedDocumentFormats
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim().ToLowerInvariant())
                .ToArray();

            return supportedFormats.Contains(fileExtension.ToLowerInvariant());
        }

        /// <summary>
        /// Verifica se un file è supportato come immagine
        /// </summary>
        public static bool IsImageFormatSupported(string fileExtension)
        {
            var supportedFormats = CurrentSettings.SupportedImageFormats
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim().ToLowerInvariant())
                .ToArray();

            return supportedFormats.Contains(fileExtension.ToLowerInvariant());
        }

        /// <summary>
        /// Ottiene l'intervallo di salvataggio automatico in millisecondi
        /// </summary>
        public static int GetAutoSaveIntervalMs()
        {
            return CurrentSettings.AutoSaveInterval * 1000;
        }

        /// <summary>
        /// Ottiene l'intervallo di aggiornamento status in millisecondi
        /// </summary>
        public static int GetStatusRefreshIntervalMs()
        {
            return CurrentSettings.StatusRefreshInterval * 1000;
        }

        /// <summary>
        /// Verifica se il backup automatico è abilitato
        /// </summary>
        public static bool IsAutoBackupEnabled()
        {
            return CurrentSettings.AutoBackupEnabled;
        }

        /// <summary>
        /// Ottiene la configurazione email completa
        /// </summary>
        public static EmailConfiguration GetEmailConfiguration()
        {
            return new EmailConfiguration
            {
                SMTPServer = CurrentSettings.SMTPServer,
                SMTPPort = CurrentSettings.SMTPPort,
                Username = CurrentSettings.SMTPUsername,
                Password = CurrentSettings.SMTPPassword,
                FromAddress = CurrentSettings.EmailFrom,
                IsConfigured = !string.IsNullOrEmpty(CurrentSettings.SMTPServer) &&
                              !string.IsNullOrEmpty(CurrentSettings.SMTPUsername)
            };
        }

        /// <summary>
        /// Verifica se l'integrazione Outlook è abilitata
        /// </summary>
        public static bool IsOutlookIntegrationEnabled()
        {
            return CurrentSettings.OutlookIntegrationEnabled;
        }

        /// <summary>
        /// Ottiene l'intervallo di sincronizzazione Outlook in millisecondi
        /// </summary>
        public static int GetOutlookSyncIntervalMs()
        {
            return CurrentSettings.OutlookSyncInterval * 1000;
        }

        /// <summary>
        /// Ottiene la configurazione di logging
        /// </summary>
        public static LoggingConfiguration GetLoggingConfiguration()
        {
            return new LoggingConfiguration
            {
                LogLevel = CurrentSettings.LogLevel,
                LogToFile = CurrentSettings.LogToFile,
                LogFilePath = CurrentSettings.LogFilePath,
                MaxLogFiles = CurrentSettings.MaxLogFiles
            };
        }

        /// <summary>
        /// Applica le impostazioni ai servizi esistenti
        /// </summary>
        public static void ApplySettingsToServices()
        {
            try
            {
                var settings = CurrentSettings;

                // Aggiorna FileManagerService se necessario
                UpdateFileManagerService(settings);

                // Aggiorna OutlookService se necessario
                UpdateOutlookService(settings);

                // Altri servizi...

                System.Diagnostics.Debug.WriteLine("Impostazioni applicate ai servizi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore ApplySettingsToServices: {ex.Message}");
            }
        }

        private static void UpdateFileManagerService(SettingsModel settings)
        {
            // Esempio: aggiorna i limiti di FileManagerService
            // Questo dovrebbe essere implementato nel FileManagerService stesso
            System.Diagnostics.Debug.WriteLine($"FileManager aggiornato: Doc={settings.MaxDocumentSizeFormatted}, Photo={settings.MaxPhotoSizeFormatted}");
        }

        private static void UpdateOutlookService(SettingsModel settings)
        {
            // Esempio: aggiorna la configurazione Outlook
            System.Diagnostics.Debug.WriteLine($"Outlook aggiornato: Enabled={settings.OutlookIntegrationEnabled}, Sync={settings.OutlookSyncIntervalFormatted}");
        }

        /// <summary>
        /// Valida che le impostazioni siano consistenti
        /// </summary>
        public static (bool IsValid, List<string> Warnings) ValidateSettingsConsistency()
        {
            var warnings = new List<string>();
            var settings = CurrentSettings;

            // Validazioni logiche
            if (settings.AutoSaveInterval < settings.StatusRefreshInterval)
            {
                warnings.Add("L'intervallo di salvataggio automatico è minore dell'intervallo di aggiornamento status. Questo potrebbe causare problemi di performance.");
            }

            if (settings.MaxDocumentSize < 1024 * 1024) // 1MB
            {
                warnings.Add("La dimensione massima documenti è molto bassa. Considera di aumentarla.");
            }

            if (settings.MaxPhotoSize < 512 * 1024) // 512KB
            {
                warnings.Add("La dimensione massima foto è molto bassa. Considera di aumentarla.");
            }

            if (!string.IsNullOrEmpty(settings.SMTPServer) && string.IsNullOrEmpty(settings.SMTPUsername))
            {
                warnings.Add("È configurato un server SMTP ma manca l'username. L'invio email potrebbe non funzionare.");
            }

            if (settings.OutlookIntegrationEnabled && settings.OutlookSyncInterval < 60)
            {
                warnings.Add("L'intervallo di sincronizzazione Outlook è molto frequente. Questo potrebbe impattare le performance.");
            }

            return (warnings.Count == 0, warnings);
        }

        /// <summary>
        /// Crea un backup delle impostazioni correnti
        /// </summary>
        public static bool BackupCurrentSettings(string backupPath)
        {
            try
            {
                var settings = CurrentSettings;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"Settings_Backup_{timestamp}.conf";
                var fullPath = System.IO.Path.Combine(backupPath, fileName);

                return SettingsService.Instance.ExportSettings(settings, fullPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore BackupCurrentSettings: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Configurazione email
    /// </summary>
    public class EmailConfiguration
    {
        public string SMTPServer { get; set; } = "";
        public int SMTPPort { get; set; } = 587;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromAddress { get; set; } = "";
        public bool IsConfigured { get; set; } = false;
    }

    /// <summary>
    /// Configurazione logging
    /// </summary>
    public class LoggingConfiguration
    {
        public string LogLevel { get; set; } = "Info";
        public bool LogToFile { get; set; } = true;
        public string LogFilePath { get; set; } = "";
        public int MaxLogFiles { get; set; } = 30;
    }
}