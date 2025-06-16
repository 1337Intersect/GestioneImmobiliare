using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace ImmobiGestio.Services
{
    public class FileManagerService
    {
        private static string? _baseDirectory;

        public static string BaseDirectory
        {
            get
            {
                if (_baseDirectory == null)
                {
                    // Prova prima AppData
                    try
                    {
                        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        var appFolder = Path.Combine(appDataPath, "ImmobiGestio");
                        Directory.CreateDirectory(appFolder);
                        _baseDirectory = appFolder;
                    }
                    catch
                    {
                        // Fallback alla directory corrente
                        _baseDirectory = Environment.CurrentDirectory;
                    }
                }
                return _baseDirectory;
            }
        }

        public static string GetDocumentsPath(int immobileId)
        {
            var path = Path.Combine(BaseDirectory, "Documenti", immobileId.ToString());
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetPhotosPath(int immobileId)
        {
            var path = Path.Combine(BaseDirectory, "Foto", immobileId.ToString());
            Directory.CreateDirectory(path);
            return path;
        }

        public static string GetUniqueFileName(string directory, string fileName)
        {
            var fullPath = Path.Combine(directory, fileName);

            if (!File.Exists(fullPath))
                return fileName;

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var counter = 1;

            do
            {
                fileName = $"{fileNameWithoutExt}_{counter}{extension}";
                fullPath = Path.Combine(directory, fileName);
                counter++;
            }
            while (File.Exists(fullPath));

            return fileName;
        }

        public static bool CopyFileToDestination(string sourceFile, string destinationDirectory, out string newFileName, out string errorMessage)
        {
            newFileName = string.Empty;
            errorMessage = string.Empty;

            try
            {
                if (!File.Exists(sourceFile))
                {
                    errorMessage = "File sorgente non trovato";
                    return false;
                }

                var originalFileName = Path.GetFileName(sourceFile);
                newFileName = GetUniqueFileName(destinationDirectory, originalFileName);
                var destinationFile = Path.Combine(destinationDirectory, newFileName);

                File.Copy(sourceFile, destinationFile, false);

                if (!File.Exists(destinationFile))
                {
                    errorMessage = "Errore nella copia del file";
                    return false;
                }

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = "Accesso negato. Verifica i permessi della cartella.";
                return false;
            }
            catch (IOException ex)
            {
                errorMessage = $"Errore I/O: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Errore imprevisto: {ex.Message}";
                return false;
            }
        }

        public static string? FindFile(string fileName, int immobileId, string fileType = "Documenti")
        {
            var searchPaths = new List<string>
            {
                // Path principale
                Path.Combine(BaseDirectory, fileType, immobileId.ToString(), fileName),
                
                // Path alternativi
                Path.Combine(Environment.CurrentDirectory, fileType, immobileId.ToString(), fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileType, immobileId.ToString(), fileName)
            };

            // Aggiunge anche la directory dell'assembly
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(assemblyLocation))
            {
                var assemblyDir = Path.GetDirectoryName(assemblyLocation);
                if (!string.IsNullOrEmpty(assemblyDir))
                {
                    searchPaths.Add(Path.Combine(assemblyDir, fileType, immobileId.ToString(), fileName));
                }
            }

            return searchPaths.FirstOrDefault(File.Exists);
        }

        public static bool DeleteDirectory(int immobileId, string fileType = "Documenti")
        {
            try
            {
                var pathsToTry = new List<string>
                {
                    Path.Combine(BaseDirectory, fileType, immobileId.ToString()),
                    Path.Combine(Environment.CurrentDirectory, fileType, immobileId.ToString()),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileType, immobileId.ToString())
                };

                var deleted = false;
                foreach (var path in pathsToTry)
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                        deleted = true;
                    }
                }

                return deleted;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidImageFile(string filePath)
        {
            try
            {
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".gif", ".webp" };
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                return validExtensions.Contains(extension);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidDocumentFile(string filePath)
        {
            try
            {
                var validExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".rtf", ".jpg", ".jpeg", ".png", ".bmp", ".tiff" };
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                return validExtensions.Contains(extension);
            }
            catch
            {
                return false;
            }
        }

        public static long GetFileSize(string filePath)
        {
            try
            {
                return new FileInfo(filePath).Length;
            }
            catch
            {
                return 0;
            }
        }

        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}