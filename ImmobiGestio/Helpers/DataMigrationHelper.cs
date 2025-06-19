using ImmobiGestio.Data;
using ImmobiGestio.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImmobiGestio.Helpers
{
    /// <summary>
    /// Helper per migrazioni del database e backup automatici
    /// </summary>
    public static class DatabaseMigrationHelper
    {
        private const string BACKUP_FOLDER = "Backup";
        private const string DATABASE_FILE = "immobili.db";

        /// <summary>
        /// Esegue un backup del database corrente
        /// </summary>
        public static bool CreateBackup(string? customName = null)
        {
            try
            {
                // Crea cartella backup se non esiste
                if (!Directory.Exists(BACKUP_FOLDER))
                {
                    Directory.CreateDirectory(BACKUP_FOLDER);
                }

                // Nome file backup
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupName = customName ?? $"immobili_backup_{timestamp}.db";
                var backupPath = Path.Combine(BACKUP_FOLDER, backupName);

                // Verifica che il database esista
                if (!File.Exists(DATABASE_FILE))
                {
                    System.Diagnostics.Debug.WriteLine("Database non trovato per il backup");
                    return false;
                }

                // Copia il file database
                File.Copy(DATABASE_FILE, backupPath, true);

                System.Diagnostics.Debug.WriteLine($"Backup creato: {backupPath}");

                // Pulisci backup vecchi (mantieni solo gli ultimi 10)
                CleanOldBackups();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante il backup: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ripristina un backup del database
        /// </summary>
        public static bool RestoreBackup(string backupFileName)
        {
            try
            {
                var backupPath = Path.Combine(BACKUP_FOLDER, backupFileName);

                if (!File.Exists(backupPath))
                {
                    System.Diagnostics.Debug.WriteLine($"File backup non trovato: {backupPath}");
                    return false;
                }

                // Crea backup del database corrente prima del ripristino
                CreateBackup($"pre_restore_{DateTime.Now:yyyyMMdd_HHmmss}.db");

                // Ripristina il backup
                File.Copy(backupPath, DATABASE_FILE, true);

                System.Diagnostics.Debug.WriteLine($"Database ripristinato da: {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante il ripristino: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Esegue la migrazione e pulizia dei dati esistenti
        /// </summary>
        public static async Task<bool> MigrateAndCleanData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INIZIO MIGRAZIONE DATI ===");

                // Crea backup prima della migrazione
                CreateBackup($"pre_migration_{DateTime.Now:yyyyMMdd_HHmmss}.db");

                using var context = new ImmobiliContext();

                // Assicurati che il database sia creato
                await context.Database.EnsureCreatedAsync();

                // Migra i dati
                await MigrateAppuntamenti(context);
                await MigrateClienti(context);
                await MigrateImmobili(context);

                System.Diagnostics.Debug.WriteLine("=== MIGRAZIONE COMPLETATA ===");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante la migrazione: {ex.Message}");
                return false;
            }
        }

        private static async Task MigrateAppuntamenti(ImmobiliContext context)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== MIGRAZIONE APPUNTAMENTI ===");

                var appuntamenti = await context.Appuntamenti.ToListAsync();
                var modificati = 0;

                foreach (var appuntamento in appuntamenti)
                {
                    var modificato = false;

                    // Fix campi obbligatori mancanti
                    if (string.IsNullOrEmpty(appuntamento.TipoAppuntamento))
                    {
                        appuntamento.TipoAppuntamento = TipiAppuntamento.Visita;
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(appuntamento.StatoAppuntamento))
                    {
                        appuntamento.StatoAppuntamento = StatiAppuntamento.Programmato;
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(appuntamento.Priorita))
                    {
                        appuntamento.Priorita = PrioritaAppuntamento.Media;
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(appuntamento.Titolo))
                    {
                        appuntamento.Titolo = "Appuntamento";
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(appuntamento.Luogo))
                    {
                        appuntamento.Luogo = "Ufficio";
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(appuntamento.CreatoDa))
                    {
                        appuntamento.CreatoDa = "Sistema";
                        modificato = true;
                    }

                    // Valida e correggi valori non validi
                    if (!Array.Exists(TipiAppuntamento.All, t => t == appuntamento.TipoAppuntamento))
                    {
                        appuntamento.TipoAppuntamento = TipiAppuntamento.Visita;
                        modificato = true;
                    }

                    if (!Array.Exists(StatiAppuntamento.All, s => s == appuntamento.StatoAppuntamento))
                    {
                        appuntamento.StatoAppuntamento = StatiAppuntamento.Programmato;
                        modificato = true;
                    }

                    if (!Array.Exists(PrioritaAppuntamento.All, p => p == appuntamento.Priorita))
                    {
                        appuntamento.Priorita = PrioritaAppuntamento.Media;
                        modificato = true;
                    }

                    // Fix date illogiche
                    if (appuntamento.DataInizio >= appuntamento.DataFine)
                    {
                        appuntamento.DataFine = appuntamento.DataInizio.AddHours(1);
                        modificato = true;
                    }

                    if (modificato)
                    {
                        appuntamento.DataUltimaModifica = DateTime.Now;
                        modificati++;
                    }
                }

                if (modificati > 0)
                {
                    await context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Migrati {modificati} appuntamenti");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore migrazione appuntamenti: {ex.Message}");
            }
        }

        private static async Task MigrateClienti(ImmobiliContext context)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== MIGRAZIONE CLIENTI ===");

                var clienti = await context.Clienti.ToListAsync();
                var modificati = 0;

                foreach (var cliente in clienti)
                {
                    var modificato = false;

                    // Fix campi obbligatori mancanti
                    if (string.IsNullOrEmpty(cliente.Nome))
                    {
                        cliente.Nome = "Nome";
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(cliente.Cognome))
                    {
                        cliente.Cognome = "Cognome";
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(cliente.TipoCliente))
                    {
                        cliente.TipoCliente = TipiCliente.Acquirente;
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(cliente.StatoCliente))
                    {
                        cliente.StatoCliente = StatiCliente.Attivo;
                        modificato = true;
                    }

                    // Valida e correggi codice fiscale
                    if (!string.IsNullOrEmpty(cliente.CodiceFiscale) &&
                        !ItalianValidationHelper.IsValidCodiceFiscale(cliente.CodiceFiscale))
                    {
                        System.Diagnostics.Debug.WriteLine($"Cliente {cliente.Id}: Codice fiscale non valido '{cliente.CodiceFiscale}'");
                        // Non rimuovere, ma segnala per controllo manuale
                    }

                    // Valida e correggi CAP
                    if (!string.IsNullOrEmpty(cliente.CAP) &&
                        !ItalianValidationHelper.IsValidCAP(cliente.CAP))
                    {
                        System.Diagnostics.Debug.WriteLine($"Cliente {cliente.Id}: CAP non valido '{cliente.CAP}'");
                        cliente.CAP = ""; // Rimuovi CAP non valido
                        modificato = true;
                    }

                    // Valida e correggi provincia
                    if (!string.IsNullOrEmpty(cliente.Provincia) &&
                        !ItalianValidationHelper.IsValidProvincia(cliente.Provincia))
                    {
                        System.Diagnostics.Debug.WriteLine($"Cliente {cliente.Id}: Provincia non valida '{cliente.Provincia}'");
                        cliente.Provincia = ""; // Rimuovi provincia non valida
                        modificato = true;
                    }

                    // Valida email
                    if (!string.IsNullOrEmpty(cliente.Email) &&
                        !ItalianValidationHelper.IsValidEmail(cliente.Email))
                    {
                        System.Diagnostics.Debug.WriteLine($"Cliente {cliente.Id}: Email non valida '{cliente.Email}'");
                    }

                    // Correggi budget negativi
                    if (cliente.BudgetMin < 0)
                    {
                        cliente.BudgetMin = 0;
                        modificato = true;
                    }

                    if (cliente.BudgetMax < 0)
                    {
                        cliente.BudgetMax = 0;
                        modificato = true;
                    }

                    // Correggi budget logicamente errati
                    if (cliente.BudgetMax > 0 && cliente.BudgetMin > cliente.BudgetMax)
                    {
                        var temp = cliente.BudgetMin;
                        cliente.BudgetMin = cliente.BudgetMax;
                        cliente.BudgetMax = temp;
                        modificato = true;
                    }

                    if (modificato)
                    {
                        cliente.DataUltimaModifica = DateTime.Now;
                        modificati++;
                    }
                }

                if (modificati > 0)
                {
                    await context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Migrati {modificati} clienti");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore migrazione clienti: {ex.Message}");
            }
        }

        private static async Task MigrateImmobili(ImmobiliContext context)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== MIGRAZIONE IMMOBILI ===");

                var immobili = await context.Immobili.ToListAsync();
                var modificati = 0;

                foreach (var immobile in immobili)
                {
                    var modificato = false;

                    // Fix campi obbligatori mancanti
                    if (string.IsNullOrEmpty(immobile.Titolo))
                    {
                        immobile.Titolo = "Immobile";
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(immobile.Indirizzo))
                    {
                        immobile.Indirizzo = "Indirizzo da specificare";
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(immobile.TipoImmobile))
                    {
                        immobile.TipoImmobile = TipiImmobile.Appartamento;
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(immobile.StatoConservazione))
                    {
                        immobile.StatoConservazione = StatiConservazione.Buono;
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(immobile.ClasseEnergetica))
                    {
                        immobile.ClasseEnergetica = ClassiEnergetiche.G;
                        modificato = true;
                    }

                    if (string.IsNullOrEmpty(immobile.StatoVendita))
                    {
                        immobile.StatoVendita = StatiVendita.Disponibile;
                        modificato = true;
                    }

                    // Valida e correggi CAP
                    if (!string.IsNullOrEmpty(immobile.CAP) &&
                        !ItalianValidationHelper.IsValidCAP(immobile.CAP))
                    {
                        System.Diagnostics.Debug.WriteLine($"Immobile {immobile.Id}: CAP non valido '{immobile.CAP}'");
                        immobile.CAP = "";
                        modificato = true;
                    }

                    // Valida e correggi provincia
                    if (!string.IsNullOrEmpty(immobile.Provincia) &&
                        !ItalianValidationHelper.IsValidProvincia(immobile.Provincia))
                    {
                        System.Diagnostics.Debug.WriteLine($"Immobile {immobile.Id}: Provincia non valida '{immobile.Provincia}'");
                        immobile.Provincia = "";
                        modificato = true;
                    }

                    // Correggi prezzo negativo
                    if (immobile.Prezzo < 0)
                    {
                        immobile.Prezzo = 0;
                        modificato = true;
                    }

                    // Correggi valori numerici negativi
                    if (immobile.Superficie.HasValue && immobile.Superficie < 0)
                    {
                        immobile.Superficie = null;
                        modificato = true;
                    }

                    if (immobile.NumeroLocali.HasValue && immobile.NumeroLocali < 0)
                    {
                        immobile.NumeroLocali = null;
                        modificato = true;
                    }

                    if (immobile.NumeroBagni.HasValue && immobile.NumeroBagni < 0)
                    {
                        immobile.NumeroBagni = null;
                        modificato = true;
                    }

                    // Genera codice immobile se mancante
                    if (string.IsNullOrEmpty(immobile.CodiceImmobile))
                    {
                        immobile.GenerateCodiceImmobile();
                        modificato = true;
                    }

                    if (modificato)
                    {
                        immobile.DataUltimaModifica = DateTime.Now;
                        modificati++;
                    }
                }

                if (modificati > 0)
                {
                    await context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Migrati {modificati} immobili");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore migrazione immobili: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica l'integrità del database
        /// </summary>
        public static async Task<(bool IsValid, string[] Issues)> ValidateDatabaseIntegrity()
        {
            var issues = new List<string>();

            try
            {
                using var context = new ImmobiliContext();

                // Verifica appuntamenti
                var appuntamentiProblematici = await context.Appuntamenti
                    .Where(a => string.IsNullOrEmpty(a.TipoAppuntamento) ||
                               string.IsNullOrEmpty(a.StatoAppuntamento) ||
                               string.IsNullOrEmpty(a.Priorita) ||
                               a.DataInizio >= a.DataFine)
                    .CountAsync();

                if (appuntamentiProblematici > 0)
                    issues.Add($"{appuntamentiProblematici} appuntamenti con dati non validi");

                // Verifica clienti
                var clientiProblematici = await context.Clienti
                    .Where(c => string.IsNullOrEmpty(c.Nome) ||
                               string.IsNullOrEmpty(c.Cognome) ||
                               c.BudgetMin < 0 ||
                               c.BudgetMax < 0)
                    .CountAsync();

                if (clientiProblematici > 0)
                    issues.Add($"{clientiProblematici} clienti con dati non validi");

                // Verifica immobili
                var immobiliProblematici = await context.Immobili
                    .Where(i => string.IsNullOrEmpty(i.Titolo) ||
                               string.IsNullOrEmpty(i.Indirizzo) ||
                               i.Prezzo < 0)
                    .CountAsync();

                if (immobiliProblematici > 0)
                    issues.Add($"{immobiliProblematici} immobili con dati non validi");

                // Verifica relazioni orfane
                var appuntamentiSenzaCliente = await context.Appuntamenti
                    .Where(a => a.ClienteId.HasValue && a.Cliente == null)
                    .CountAsync();

                if (appuntamentiSenzaCliente > 0)
                    issues.Add($"{appuntamentiSenzaCliente} appuntamenti con riferimenti cliente non validi");

                var appuntamentiSenzaImmobile = await context.Appuntamenti
                    .Where(a => a.ImmobileId.HasValue && a.Immobile == null)
                    .CountAsync();

                if (appuntamentiSenzaImmobile > 0)
                    issues.Add($"{appuntamentiSenzaImmobile} appuntamenti con riferimenti immobile non validi");

                return (issues.Count == 0, issues.ToArray());
            }
            catch (Exception ex)
            {
                issues.Add($"Errore durante la verifica: {ex.Message}");
                return (false, issues.ToArray());
            }
        }

        /// <summary>
        /// Pulisce i backup vecchi mantenendo solo i più recenti
        /// </summary>
        private static void CleanOldBackups(int maxBackups = 10)
        {
            try
            {
                if (!Directory.Exists(BACKUP_FOLDER))
                    return;

                var backupFiles = Directory.GetFiles(BACKUP_FOLDER, "*.db")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToArray();

                if (backupFiles.Length > maxBackups)
                {
                    var filesToDelete = backupFiles.Skip(maxBackups);
                    foreach (var file in filesToDelete)
                    {
                        try
                        {
                            file.Delete();
                            System.Diagnostics.Debug.WriteLine($"Backup rimosso: {file.Name}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Errore rimozione backup {file.Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore pulizia backup: {ex.Message}");
            }
        }

        /// <summary>
        /// Esegue un backup automatico giornaliero se necessario
        /// </summary>
        public static void PerformAutomaticBackup()
        {
            try
            {
                if (!Directory.Exists(BACKUP_FOLDER))
                    Directory.CreateDirectory(BACKUP_FOLDER);

                // Verifica se esiste già un backup di oggi
                var today = DateTime.Today.ToString("yyyyMMdd");
                var todayBackups = Directory.GetFiles(BACKUP_FOLDER, $"*{today}*.db");

                if (todayBackups.Length == 0)
                {
                    var success = CreateBackup($"auto_daily_{today}.db");
                    if (success)
                    {
                        System.Diagnostics.Debug.WriteLine("Backup automatico giornaliero creato");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore backup automatico: {ex.Message}");
            }
        }

        /// <summary>
        /// Esporta tutti i dati in formato CSV per backup esterno
        /// </summary>
        public static async Task<bool> ExportAllDataToCSV(string exportPath)
        {
            try
            {
                using var context = new ImmobiliContext();

                // Export Immobili
                var immobili = await context.Immobili.ToListAsync();
                var immobiliCsv = "Id,Titolo,Indirizzo,Citta,CAP,Provincia,TipoImmobile,Prezzo,Superficie,NumeroLocali,StatoVendita,ClasseEnergetica,DataInserimento\n";

                foreach (var i in immobili)
                {
                    immobiliCsv += $"{i.Id},\"{i.Titolo}\",\"{i.Indirizzo}\",\"{i.Citta}\",\"{i.CAP}\",\"{i.Provincia}\"," +
                                  $"\"{i.TipoImmobile}\",{i.Prezzo},{i.Superficie},{i.NumeroLocali}," +
                                  $"\"{i.StatoVendita}\",\"{i.ClasseEnergetica}\",{i.DataInserimento:yyyy-MM-dd}\n";
                }

                await File.WriteAllTextAsync(Path.Combine(exportPath, "immobili.csv"), immobiliCsv);

                // Export Clienti
                var clienti = await context.Clienti.ToListAsync();
                var clientiCsv = "Id,Nome,Cognome,Email,Telefono,Cellulare,TipoCliente,StatoCliente,BudgetMin,BudgetMax,DataInserimento\n";

                foreach (var c in clienti)
                {
                    clientiCsv += $"{c.Id},\"{c.Nome}\",\"{c.Cognome}\",\"{c.Email}\",\"{c.Telefono}\",\"{c.Cellulare}\"," +
                                 $"\"{c.TipoCliente}\",\"{c.StatoCliente}\",{c.BudgetMin},{c.BudgetMax},{c.DataInserimento:yyyy-MM-dd}\n";
                }

                await File.WriteAllTextAsync(Path.Combine(exportPath, "clienti.csv"), clientiCsv);

                // Export Appuntamenti
                var appuntamenti = await context.Appuntamenti.Include(a => a.Cliente).Include(a => a.Immobile).ToListAsync();
                var appuntamentiCsv = "Id,Titolo,DataInizio,DataFine,TipoAppuntamento,StatoAppuntamento,Cliente,Immobile,Luogo\n";

                foreach (var a in appuntamenti)
                {
                    appuntamentiCsv += $"{a.Id},\"{a.Titolo}\",{a.DataInizio:yyyy-MM-dd HH:mm},{a.DataFine:yyyy-MM-dd HH:mm}," +
                                      $"\"{a.TipoAppuntamento}\",\"{a.StatoAppuntamento}\",\"{a.Cliente?.NomeCompleto ?? ""}\"," +
                                      $"\"{a.Immobile?.Titolo ?? ""}\",\"{a.Luogo}\"\n";
                }

                await File.WriteAllTextAsync(Path.Combine(exportPath, "appuntamenti.csv"), appuntamentiCsv);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore export CSV: {ex.Message}");
                return false;
            }
        }
    }
}