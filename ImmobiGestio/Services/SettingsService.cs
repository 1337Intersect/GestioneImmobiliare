using ImmobiGestio.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;

namespace ImmobiGestio.Services
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        private static readonly object _lock = new object();

        public static SettingsService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new SettingsService();
                    }
                }
                return _instance;
            }
        }

        private SettingsService() { }

        /// <summary>
        /// Carica le impostazioni dall'app.config
        /// </summary>
        public SettingsModel LoadSettings()
        {
            try
            {
                var settings = new SettingsModel();

                // Carica Generale
                settings.ApplicationName = GetAppSetting("ApplicationName", "ImmobiGestio");
                settings.ApplicationVersion = GetAppSetting("ApplicationVersion", "1.0.0");
                settings.AutoSaveInterval = GetAppSettingInt("AutoSaveInterval", 300);
                settings.StatusRefreshInterval = GetAppSettingInt("StatusRefreshInterval", 60);

                // Carica Percorsi
                settings.DocumentsPath = GetAppSetting("DocumentsPath", "Documenti");
                settings.PhotosPath = GetAppSetting("PhotosPath", "Foto");
                settings.BackupPath = GetAppSetting("BackupPath", "Backup");

                // Carica File
                settings.MaxDocumentSize = GetAppSettingLong("MaxDocumentSize", 52428800);
                settings.MaxPhotoSize = GetAppSettingLong("MaxPhotoSize", 10485760);
                settings.SupportedDocumentFormats = GetAppSetting("SupportedDocumentFormats",
                    ".pdf,.doc,.docx,.txt,.rtf,.jpg,.jpeg,.png,.bmp,.tiff");
                settings.SupportedImageFormats = GetAppSetting("SupportedImageFormats",
                    ".jpg,.jpeg,.png,.bmp,.tiff,.gif,.webp");

                // Carica Backup
                settings.AutoBackupEnabled = GetAppSettingBool("AutoBackupEnabled", true);
                settings.AutoBackupDays = GetAppSettingInt("AutoBackupDays", 7);
                settings.MaxBackupFiles = GetAppSettingInt("MaxBackupFiles", 10);

                // Carica Email
                settings.SMTPServer = GetAppSetting("SMTPServer", "");
                settings.SMTPPort = GetAppSettingInt("SMTPPort", 587);
                settings.SMTPUsername = GetAppSetting("SMTPUsername", "");
                settings.SMTPPassword = GetAppSetting("SMTPPassword", "");
                settings.EmailFrom = GetAppSetting("EmailFrom", "");

                // Carica Outlook
                settings.OutlookIntegrationEnabled = GetAppSettingBool("OutlookIntegrationEnabled", false);
                settings.OutlookSyncInterval = GetAppSettingInt("OutlookSyncInterval", 900);

                // Carica Logging
                settings.LogLevel = GetAppSetting("LogLevel", "Info");
                settings.LogToFile = GetAppSettingBool("LogToFile", true);
                settings.LogFilePath = GetAppSetting("LogFilePath", "logs\\immobigestio.log");
                settings.MaxLogFiles = GetAppSettingInt("MaxLogFiles", 30);

                System.Diagnostics.Debug.WriteLine("Impostazioni caricate dall'app.config");
                return settings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel caricamento impostazioni: {ex.Message}");
                return new SettingsModel(); // Restituisce impostazioni di default
            }
        }

        /// <summary>
        /// Salva le impostazioni nell'app.config
        /// </summary>
        public bool SaveSettings(SettingsModel settings)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // Salva Generale
                SetAppSetting(config, "ApplicationName", settings.ApplicationName);
                SetAppSetting(config, "ApplicationVersion", settings.ApplicationVersion);
                SetAppSetting(config, "AutoSaveInterval", settings.AutoSaveInterval.ToString());
                SetAppSetting(config, "StatusRefreshInterval", settings.StatusRefreshInterval.ToString());

                // Salva Percorsi
                SetAppSetting(config, "DocumentsPath", settings.DocumentsPath);
                SetAppSetting(config, "PhotosPath", settings.PhotosPath);
                SetAppSetting(config, "BackupPath", settings.BackupPath);

                // Salva File
                SetAppSetting(config, "MaxDocumentSize", settings.MaxDocumentSize.ToString());
                SetAppSetting(config, "MaxPhotoSize", settings.MaxPhotoSize.ToString());
                SetAppSetting(config, "SupportedDocumentFormats", settings.SupportedDocumentFormats);
                SetAppSetting(config, "SupportedImageFormats", settings.SupportedImageFormats);

                // Salva Backup
                SetAppSetting(config, "AutoBackupEnabled", settings.AutoBackupEnabled.ToString().ToLower());
                SetAppSetting(config, "AutoBackupDays", settings.AutoBackupDays.ToString());
                SetAppSetting(config, "MaxBackupFiles", settings.MaxBackupFiles.ToString());

                // Salva Email
                SetAppSetting(config, "SMTPServer", settings.SMTPServer);
                SetAppSetting(config, "SMTPPort", settings.SMTPPort.ToString());
                SetAppSetting(config, "SMTPUsername", settings.SMTPUsername);
                SetAppSetting(config, "SMTPPassword", settings.SMTPPassword);
                SetAppSetting(config, "EmailFrom", settings.EmailFrom);

                // Salva Outlook
                SetAppSetting(config, "OutlookIntegrationEnabled", settings.OutlookIntegrationEnabled.ToString().ToLower());
                SetAppSetting(config, "OutlookSyncInterval", settings.OutlookSyncInterval.ToString());

                // Salva Logging
                SetAppSetting(config, "LogLevel", settings.LogLevel);
                SetAppSetting(config, "LogToFile", settings.LogToFile.ToString().ToLower());
                SetAppSetting(config, "LogFilePath", settings.LogFilePath);
                SetAppSetting(config, "MaxLogFiles", settings.MaxLogFiles.ToString());

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                System.Diagnostics.Debug.WriteLine("Impostazioni salvate nell'app.config");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel salvataggio impostazioni: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ripristina le impostazioni ai valori di default
        /// </summary>
        public SettingsModel GetDefaultSettings()
        {
            return new SettingsModel();
        }

        /// <summary>
        /// Valida le impostazioni
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateSettings(SettingsModel settings)
        {
            // Validazione Generale
            if (settings.AutoSaveInterval < 30)
                return (false, "L'intervallo di salvataggio automatico deve essere almeno 30 secondi");

            if (settings.StatusRefreshInterval < 10)
                return (false, "L'intervallo di aggiornamento stato deve essere almeno 10 secondi");

            // Validazione File
            if (settings.MaxDocumentSize < 1024 * 1024) // 1MB
                return (false, "La dimensione massima documenti deve essere almeno 1MB");

            if (settings.MaxPhotoSize < 512 * 1024) // 512KB
                return (false, "La dimensione massima foto deve essere almeno 512KB");

            // Validazione Backup
            if (settings.AutoBackupDays < 1)
                return (false, "L'intervallo di backup deve essere almeno 1 giorno");

            if (settings.MaxBackupFiles < 1)
                return (false, "Il numero massimo di backup deve essere almeno 1");

            // Validazione Email
            if (!string.IsNullOrEmpty(settings.SMTPServer))
            {
                if (settings.SMTPPort < 1 || settings.SMTPPort > 65535)
                    return (false, "La porta SMTP deve essere tra 1 e 65535");

                if (string.IsNullOrEmpty(settings.SMTPUsername))
                    return (false, "Username SMTP richiesto quando il server è specificato");
            }

            // Validazione Outlook
            if (settings.OutlookSyncInterval < 60)
                return (false, "L'intervallo di sincronizzazione Outlook deve essere almeno 60 secondi");

            // Validazione Logging
            if (settings.MaxLogFiles < 1)
                return (false, "Il numero massimo di file log deve essere almeno 1");

            return (true, "");
        }

        // Metodi helper privati
        private string GetAppSetting(string key, string defaultValue)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                return string.IsNullOrEmpty(value) ? defaultValue : value;
            }
            catch
            {
                return defaultValue;
            }
        }

        private int GetAppSettingInt(string key, int defaultValue)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                return int.TryParse(value, out int result) ? result : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private long GetAppSettingLong(string key, long defaultValue)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                return long.TryParse(value, out long result) ? result : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private bool GetAppSettingBool(string key, bool defaultValue)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                return bool.TryParse(value, out bool result) ? result : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private void SetAppSetting(Configuration config, string key, string value)
        {
            if (config.AppSettings.Settings[key] != null)
            {
                config.AppSettings.Settings[key].Value = value;
            }
            else
            {
                config.AppSettings.Settings.Add(key, value);
            }
        }

        /// <summary>
        /// Esporta le impostazioni in un file
        /// </summary>
        public bool ExportSettings(SettingsModel settings, string filePath)
        {
            try
            {
                var lines = new List<string>
                {
                    $"# Impostazioni ImmobiGestio - Esportate il {DateTime.Now:dd/MM/yyyy HH:mm}",
                    "",
                    "# Generale",
                    $"ApplicationName={settings.ApplicationName}",
                    $"ApplicationVersion={settings.ApplicationVersion}",
                    $"AutoSaveInterval={settings.AutoSaveInterval}",
                    $"StatusRefreshInterval={settings.StatusRefreshInterval}",
                    "",
                    "# Percorsi",
                    $"DocumentsPath={settings.DocumentsPath}",
                    $"PhotosPath={settings.PhotosPath}",
                    $"BackupPath={settings.BackupPath}",
                    "",
                    "# File",
                    $"MaxDocumentSize={settings.MaxDocumentSize}",
                    $"MaxPhotoSize={settings.MaxPhotoSize}",
                    $"SupportedDocumentFormats={settings.SupportedDocumentFormats}",
                    $"SupportedImageFormats={settings.SupportedImageFormats}",
                    "",
                    "# Backup",
                    $"AutoBackupEnabled={settings.AutoBackupEnabled}",
                    $"AutoBackupDays={settings.AutoBackupDays}",
                    $"MaxBackupFiles={settings.MaxBackupFiles}",
                    "",
                    "# Email",
                    $"SMTPServer={settings.SMTPServer}",
                    $"SMTPPort={settings.SMTPPort}",
                    $"SMTPUsername={settings.SMTPUsername}",
                    $"EmailFrom={settings.EmailFrom}",
                    "",
                    "# Outlook",
                    $"OutlookIntegrationEnabled={settings.OutlookIntegrationEnabled}",
                    $"OutlookSyncInterval={settings.OutlookSyncInterval}",
                    "",
                    "# Logging",
                    $"LogLevel={settings.LogLevel}",
                    $"LogToFile={settings.LogToFile}",
                    $"LogFilePath={settings.LogFilePath}",
                    $"MaxLogFiles={settings.MaxLogFiles}"
                };

                System.IO.File.WriteAllLines(filePath, lines);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore esportazione impostazioni: {ex.Message}");
                return false;
            }
        }
    }
}