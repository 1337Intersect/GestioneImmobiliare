using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImmobiGestio.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        // Sezione Generale
        private string _applicationName = "ImmobiGestio";
        private string _applicationVersion = "0.1.15";
        private int _autoSaveInterval = 300; // secondi
        private int _statusRefreshInterval = 60; // secondi

        // Sezione Percorsi
        private string _documentsPath = "Documenti";
        private string _photosPath = "Foto";
        private string _backupPath = "Backup";

        // Sezione File
        private long _maxDocumentSize = 52428800; // 50MB
        private long _maxPhotoSize = 10485760; // 10MB
        private string _supportedDocumentFormats = ".pdf,.doc,.docx,.txt,.rtf,.jpg,.jpeg,.png,.bmp,.tiff";
        private string _supportedImageFormats = ".jpg,.jpeg,.png,.bmp,.tiff,.gif,.webp";

        // Sezione Backup
        private bool _autoBackupEnabled = true;
        private int _autoBackupDays = 7;
        private int _maxBackupFiles = 10;

        // Sezione Email
        private string _smtpServer = "";
        private int _smtpPort = 587;
        private string _smtpUsername = "";
        private string _smtpPassword = "";
        private string _emailFrom = "";

        // Sezione Outlook
        private bool _outlookIntegrationEnabled = false;
        private int _outlookSyncInterval = 900; // secondi

        // Sezione Logging
        private string _logLevel = "Info";
        private bool _logToFile = true;
        private string _logFilePath = "logs\\immobigestio.log";
        private int _maxLogFiles = 30;

        //Sezione Theme
        private string _appTheme = "Auto";

        // Proprietà Generale
        public string ApplicationName
        {
            get => _applicationName;
            set => SetProperty(ref _applicationName, value);
        }

        public string ApplicationVersion
        {
            get => _applicationVersion;
            set => SetProperty(ref _applicationVersion, value);
        }

        public int AutoSaveInterval
        {
            get => _autoSaveInterval;
            set => SetProperty(ref _autoSaveInterval, value);
        }

        public int StatusRefreshInterval
        {
            get => _statusRefreshInterval;
            set => SetProperty(ref _statusRefreshInterval, value);
        }

        // Proprietà Percorsi
        public string DocumentsPath
        {
            get => _documentsPath;
            set => SetProperty(ref _documentsPath, value);
        }

        public string PhotosPath
        {
            get => _photosPath;
            set => SetProperty(ref _photosPath, value);
        }

        public string BackupPath
        {
            get => _backupPath;
            set => SetProperty(ref _backupPath, value);
        }

        // Proprietà File
        public long MaxDocumentSize
        {
            get => _maxDocumentSize;
            set => SetProperty(ref _maxDocumentSize, value);
        }

        public long MaxPhotoSize
        {
            get => _maxPhotoSize;
            set => SetProperty(ref _maxPhotoSize, value);
        }

        public string SupportedDocumentFormats
        {
            get => _supportedDocumentFormats;
            set => SetProperty(ref _supportedDocumentFormats, value);
        }

        public string SupportedImageFormats
        {
            get => _supportedImageFormats;
            set => SetProperty(ref _supportedImageFormats, value);
        }

        // Proprietà Backup
        public bool AutoBackupEnabled
        {
            get => _autoBackupEnabled;
            set => SetProperty(ref _autoBackupEnabled, value);
        }

        public int AutoBackupDays
        {
            get => _autoBackupDays;
            set => SetProperty(ref _autoBackupDays, value);
        }

        public int MaxBackupFiles
        {
            get => _maxBackupFiles;
            set => SetProperty(ref _maxBackupFiles, value);
        }

        // Proprietà Email
        public string SMTPServer
        {
            get => _smtpServer;
            set => SetProperty(ref _smtpServer, value);
        }

        public int SMTPPort
        {
            get => _smtpPort;
            set => SetProperty(ref _smtpPort, value);
        }

        public string SMTPUsername
        {
            get => _smtpUsername;
            set => SetProperty(ref _smtpUsername, value);
        }

        public string SMTPPassword
        {
            get => _smtpPassword;
            set => SetProperty(ref _smtpPassword, value);
        }

        public string EmailFrom
        {
            get => _emailFrom;
            set => SetProperty(ref _emailFrom, value);
        }

        // Proprietà Outlook
        public bool OutlookIntegrationEnabled
        {
            get => _outlookIntegrationEnabled;
            set => SetProperty(ref _outlookIntegrationEnabled, value);
        }

        public int OutlookSyncInterval
        {
            get => _outlookSyncInterval;
            set => SetProperty(ref _outlookSyncInterval, value);
        }

        // Proprietà Logging
        public string LogLevel
        {
            get => _logLevel;
            set => SetProperty(ref _logLevel, value);
        }

        public bool LogToFile
        {
            get => _logToFile;
            set => SetProperty(ref _logToFile, value);
        }

        public string LogFilePath
        {
            get => _logFilePath;
            set => SetProperty(ref _logFilePath, value);
        }

        public int MaxLogFiles
        {
            get => _maxLogFiles;
            set => SetProperty(ref _maxLogFiles, value);
        }

        //Proprietà Theme
        public string AppTheme
        {
            get => _appTheme;
            set => SetProperty(ref _appTheme, value);
        }

        // Proprietà calcolate per visualizzazione
        public string MaxDocumentSizeFormatted => FormatFileSize(MaxDocumentSize);
        public string MaxPhotoSizeFormatted => FormatFileSize(MaxPhotoSize);
        public string AutoSaveIntervalFormatted => FormatTimeInterval(AutoSaveInterval);
        public string StatusRefreshIntervalFormatted => FormatTimeInterval(StatusRefreshInterval);
        public string OutlookSyncIntervalFormatted => FormatTimeInterval(OutlookSyncInterval);

        // Metodi helper
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string FormatTimeInterval(int seconds)
        {
            if (seconds < 60)
                return $"{seconds} secondi";
            else if (seconds < 3600)
                return $"{seconds / 60} minuti";
            else
                return $"{seconds / 3600} ore";
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);

            // Notifica anche le proprietà formattate quando necessario
            switch (propertyName)
            {
                case nameof(MaxDocumentSize):
                    OnPropertyChanged(nameof(MaxDocumentSizeFormatted));
                    break;
                case nameof(MaxPhotoSize):
                    OnPropertyChanged(nameof(MaxPhotoSizeFormatted));
                    break;
                case nameof(AutoSaveInterval):
                    OnPropertyChanged(nameof(AutoSaveIntervalFormatted));
                    break;
                case nameof(StatusRefreshInterval):
                    OnPropertyChanged(nameof(StatusRefreshIntervalFormatted));
                    break;
                case nameof(OutlookSyncInterval):
                    OnPropertyChanged(nameof(OutlookSyncIntervalFormatted));
                    break;
            }

            return true;
        }
    }
}