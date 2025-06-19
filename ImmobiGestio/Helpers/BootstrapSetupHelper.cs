using ImmobiGestio.Data;
using ImmobiGestio.Services;
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
    /// Helper per l'inizializzazione e bootstrap dell'applicazione
    /// </summary>
    public static class BootstrapSetupHelper
    {
        private const string APP_DATA_FOLDER = "ImmobiGestio";
        private const string DATABASE_FILE = "immobili.db";
        private const string DOCUMENTS_FOLDER = "Documenti";
        private const string PHOTOS_FOLDER = "Foto";
        private const string BACKUP_FOLDER = "Backup";
        private const string TEMPLATES_FOLDER = "Templates";

        /// <summary>
        /// Esegue l'inizializzazione completa dell'applicazione
        /// </summary>
        public static async Task<bool> InitializeApplication()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== BOOTSTRAP APPLICAZIONE IMMOBIGESTIO ===");

                // 1. Crea struttura cartelle
                CreateFolderStructure();

                // 2. Inizializza database
                await InitializeDatabase();

                // 3. Esegue migrazione se necessaria
                await DatabaseMigrationHelper.MigrateAndCleanData();

                // 4. Verifica integrità
                var (isValid, issues) = await DatabaseMigrationHelper.ValidateDatabaseIntegrity();
                if (!isValid)
                {
                    System.Diagnostics.Debug.WriteLine($"Problemi di integrità rilevati: {string.Join(", ", issues)}");
                }

                // 5. Backup automatico
                DatabaseMigrationHelper.PerformAutomaticBackup();

                // 6. Inizializza servizi
                InitializeServices();

                // 7. Crea file template se non esistono
                CreateTemplateFiles();

                System.Diagnostics.Debug.WriteLine("=== BOOTSTRAP COMPLETATO ===");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante il bootstrap: {ex.Message}");
                MessageBox.Show($"Errore durante l'inizializzazione dell'applicazione:\n\n{ex.Message}\n\nL'applicazione potrebbe non funzionare correttamente.",
                    "Errore Inizializzazione", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Crea la struttura di cartelle necessaria all'applicazione
        /// </summary>
        private static void CreateFolderStructure()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Creazione struttura cartelle...");

                // Cartella principale nell'AppData
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_DATA_FOLDER);

                // Cartelle nel directory corrente (per portabilità)
                var folders = new[]
                {
                    DOCUMENTS_FOLDER,
                    PHOTOS_FOLDER,
                    BACKUP_FOLDER,
                    TEMPLATES_FOLDER,
                    Path.Combine(DOCUMENTS_FOLDER, "APE"),
                    Path.Combine(DOCUMENTS_FOLDER, "Planimetrie"),
                    Path.Combine(DOCUMENTS_FOLDER, "Contratti"),
                    Path.Combine(DOCUMENTS_FOLDER, "Visure"),
                    Path.Combine(PHOTOS_FOLDER, "Principale"),
                    Path.Combine(PHOTOS_FOLDER, "Dettaglio"),
                    Path.Combine(BACKUP_FOLDER, "Automatici"),
                    Path.Combine(BACKUP_FOLDER, "Manuali")
                };

                foreach (var folder in folders)
                {
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                        System.Diagnostics.Debug.WriteLine($"Cartella creata: {folder}");
                    }
                }

                // Crea anche cartella AppData per configurazioni
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                    System.Diagnostics.Debug.WriteLine($"Cartella AppData creata: {appDataPath}");
                }

                System.Diagnostics.Debug.WriteLine("Struttura cartelle completata");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore creazione cartelle: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Inizializza il database
        /// </summary>
        private static async Task InitializeDatabase()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Inizializzazione database...");

                using var context = new ImmobiliContext();

                // Crea il database se non esiste
                var created = await context.Database.EnsureCreatedAsync();

                if (created)
                {
                    System.Diagnostics.Debug.WriteLine("Database creato");
                    await SeedInitialData(context);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Database esistente trovato");

                    // Verifica se ha dati o è vuoto
                    var hasData = await context.Immobili.AnyAsync() ||
                                 await context.Clienti.AnyAsync() ||
                                 await context.Appuntamenti.AnyAsync();

                    if (!hasData)
                    {
                        System.Diagnostics.Debug.WriteLine("Database vuoto, popolamento con dati iniziali");
                        await SeedInitialData(context);
                    }
                }

                System.Diagnostics.Debug.WriteLine("Database inizializzato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore inizializzazione database: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Popola il database con dati iniziali di esempio
        /// </summary>
        private static async Task SeedInitialData(ImmobiliContext context)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Popolamento dati iniziali...");

                // Cliente di esempio
                var clienteEsempio = new Models.Cliente
                {
                    Nome = "Mario",
                    Cognome = "Rossi",
                    Email = "mario.rossi@email.it",
                    Telefono = "02 1234567",
                    Cellulare = "339 1234567",
                    TipoCliente = Models.TipiCliente.Acquirente,
                    StatoCliente = Models.StatiCliente.Attivo,
                    BudgetMin = 150000,
                    BudgetMax = 250000,
                    Citta = "Milano",
                    Provincia = "MI",
                    CAP = "20100",
                    FonteContatto = Models.FontiContatto.Web,
                    PreferenzeTipologia = "Appartamento, Trilocale",
                    PreferenzeZone = "Centro, Navigli",
                    Note = "Cliente di esempio creato durante l'inizializzazione",
                    DataInserimento = DateTime.Now,
                    DataNascita = new DateTime(1985, 5, 15)
                };

                context.Clienti.Add(clienteEsempio);

                // Immobile di esempio
                var immobileEsempio = new Models.Immobile
                {
                    Titolo = "Trilocale Milano Centro",
                    Indirizzo = "Via Roma, 123",
                    Citta = "Milano",
                    Provincia = "MI",
                    CAP = "20100",
                    TipoImmobile = Models.TipiImmobile.Trilocale,
                    Prezzo = 180000,
                    Superficie = 85,
                    NumeroLocali = 3,
                    NumeroBagni = 1,
                    Piano = 2,
                    Descrizione = "Bellissimo trilocale in zona centrale, completamente ristrutturato. " +
                                 "Composto da ingresso, soggiorno con angolo cottura, due camere da letto e bagno. " +
                                 "Immobile di esempio creato durante l'inizializzazione.",
                    StatoConservazione = Models.StatiConservazione.Ottimo,
                    ClasseEnergetica = Models.ClassiEnergetiche.C,
                    StatoVendita = Models.StatiVendita.Disponibile,
                    HasTerrazzo = true,
                    HasAscensore = true,
                    HasRiscaldamentoAutonomo = true,
                    TipoRiscaldamento = "Autonomo a metano",
                    Orientamento = "Sud-Est",
                    AnnoCostruzione = 1980,
                    DataInserimento = DateTime.Now
                };

                immobileEsempio.GenerateCodiceImmobile();
                context.Immobili.Add(immobileEsempio);

                // Salva per ottenere gli ID
                await context.SaveChangesAsync();

                // Appuntamento di esempio
                var appuntamentoEsempio = Models.Appuntamento.CreaPerClienteEImmobile(
                    clienteEsempio.Id,
                    clienteEsempio.NomeCompleto,
                    immobileEsempio.Id,
                    immobileEsempio.Titolo,
                    immobileEsempio.IndirizzoCompleto
                );

                appuntamentoEsempio.DataInizio = DateTime.Now.AddDays(1).Date.AddHours(14); // Domani alle 14:00
                appuntamentoEsempio.DataFine = appuntamentoEsempio.DataInizio.AddHours(1);
                appuntamentoEsempio.Descrizione = "Appuntamento di esempio creato durante l'inizializzazione";

                context.Appuntamenti.Add(appuntamentoEsempio);

                // Interesse cliente-immobile
                var interesse = new Models.ClienteImmobile
                {
                    ClienteId = clienteEsempio.Id,
                    ImmobileId = immobileEsempio.Id,
                    DataInteresse = DateTime.Now,
                    StatoInteresse = "Interessato",
                    Note = "Primo contatto tramite sito web"
                };

                context.ClientiImmobili.Add(interesse);

                await context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine("Dati iniziali popolati");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore popolamento dati iniziali: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Inizializza i servizi dell'applicazione
        /// </summary>
        private static void InitializeServices()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Inizializzazione servizi...");

                // Inizializza SettingsIntegrationHelper
                SettingsIntegrationHelper.Initialize();

                // Inizializza ThemeManager (se disponibile)
                try
                {
                    var themeManager = ThemeManager.Instance;
                    themeManager?.LoadSavedTheme();
                    System.Diagnostics.Debug.WriteLine("ThemeManager inizializzato");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore inizializzazione ThemeManager: {ex.Message}");
                }

                System.Diagnostics.Debug.WriteLine("Servizi inizializzati");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore inizializzazione servizi: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Crea file template utili
        /// </summary>
        private static void CreateTemplateFiles()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Creazione file template...");

                var templatesPath = TEMPLATES_FOLDER;

                // Template contratto di vendita
                var contrattoTemplate = @"CONTRATTO DI COMPRAVENDITA IMMOBILIARE

Venditore: _______________________
Acquirente: ______________________
Immobile: _______________________
Prezzo: € _______________________
Data: ___________________________

[Template di esempio - personalizzare secondo necessità]

Questo template è stato creato automaticamente da ImmobiGestio.
";

                var contrattoPath = Path.Combine(templatesPath, "template_contratto_vendita.txt");
                if (!File.Exists(contrattoPath))
                {
                    File.WriteAllText(contrattoPath, contrattoTemplate);
                    System.Diagnostics.Debug.WriteLine("Template contratto creato");
                }

                // Template email cliente
                var emailTemplate = @"Gentile Cliente,

La ringraziamo per l'interesse mostrato nei confronti dell'immobile:
{TITOLO_IMMOBILE}
Situato in: {INDIRIZZO_IMMOBILE}
Prezzo richiesto: {PREZZO_IMMOBILE}

Restiamo a disposizione per ulteriori informazioni e per organizzare una visita.

Cordiali saluti,
Il Team di ImmobiGestio

---
Questo messaggio è stato generato automaticamente da ImmobiGestio
";

                var emailPath = Path.Combine(templatesPath, "template_email_cliente.txt");
                if (!File.Exists(emailPath))
                {
                    File.WriteAllText(emailPath, emailTemplate);
                    System.Diagnostics.Debug.WriteLine("Template email creato");
                }

                // README per l'utente
                var readmeContent = @"IMMOBIGESTIO - GUIDA RAPIDA

1. PRIMO AVVIO
   - L'applicazione ha creato automaticamente le cartelle necessarie
   - È stato popolato il database con dati di esempio
   - È possibile eliminare i dati di esempio dal menu

2. STRUTTURA CARTELLE
   - Documenti/: File PDF, contratti, certificati
   - Foto/: Immagini degli immobili
   - Backup/: Backup automatici del database
   - Templates/: Modelli per contratti ed email

3. FUNZIONALITÀ PRINCIPALI
   - Gestione Immobili: Inserimento, modifica, ricerca
   - Gestione Clienti: Anagrafica completa con validazione italiana
   - Gestione Appuntamenti: Calendario integrato, sync Outlook
   - Dashboard: Statistiche e panoramica generale

4. VALIDAZIONI ITALIANE
   - Codice Fiscale: Validazione algoritmo ufficiale
   - CAP: Controllo 5 cifre
   - Province: Sigle ufficiali italiane
   - Telefoni: Formati italiani fissi e mobili

5. BACKUP E SICUREZZA
   - Backup automatico giornaliero
   - Possibilità di backup manuali
   - Ripristino da backup precedenti

6. SUPPORTO
   - Per assistenza, consultare la documentazione online
   - Report bug: utilizzare il sistema di feedback integrato

Versione: 1.0 - Market: ITALIA
Creato il: " + DateTime.Now.ToString("dd/MM/yyyy") + @"
";

                var readmePath = "README_ImmobiGestio.txt";
                if (!File.Exists(readmePath))
                {
                    File.WriteAllText(readmePath, readmeContent);
                    System.Diagnostics.Debug.WriteLine("README creato");
                }

                System.Diagnostics.Debug.WriteLine("File template creati");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore creazione template: {ex.Message}");
                // Non è critico, continua
            }
        }

        /// <summary>
        /// Verifica i prerequisiti di sistema
        /// </summary>
        public static (bool IsValid, string[] Issues) CheckSystemRequirements()
        {
            var issues = new List<string>();

            try
            {
                // Verifica .NET Framework/Core
                var netVersion = Environment.Version;
                System.Diagnostics.Debug.WriteLine($".NET Version: {netVersion}");

                // Verifica spazio su disco (almeno 100MB)
                var currentDrive = new DriveInfo(Environment.CurrentDirectory);
                var freeSpaceGB = currentDrive.AvailableFreeSpace / (1024 * 1024 * 1024);

                if (freeSpaceGB < 0.1) // Meno di 100MB
                {
                    issues.Add($"Spazio insufficiente su disco: {freeSpaceGB:F1}GB disponibili");
                }

                // Verifica permessi di scrittura
                try
                {
                    var testFile = Path.Combine(Environment.CurrentDirectory, "test_write.tmp");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                }
                catch
                {
                    issues.Add("Permessi di scrittura insufficienti nella directory corrente");
                }

                // Verifica che SQLite sia disponibile
                try
                {
                    using var context = new ImmobiliContext();
                    context.Database.CanConnect();
                }
                catch (Exception ex)
                {
                    issues.Add($"Problema con il database SQLite: {ex.Message}");
                }

                return (issues.Count == 0, issues.ToArray());
            }
            catch (Exception ex)
            {
                issues.Add($"Errore durante la verifica dei prerequisiti: {ex.Message}");
                return (false, issues.ToArray());
            }
        }

        /// <summary>
        /// Esegue un controllo di integrità completo dell'installazione
        /// </summary>
        public static async Task<(bool IsHealthy, string[] Issues)> PerformHealthCheck()
        {
            var issues = new List<string>();

            try
            {
                System.Diagnostics.Debug.WriteLine("=== CONTROLLO INTEGRITÀ APPLICAZIONE ===");

                // 1. Verifica prerequisiti di sistema
                var (sysValid, sysIssues) = CheckSystemRequirements();
                if (!sysValid)
                {
                    issues.AddRange(sysIssues);
                }

                // 2. Verifica struttura cartelle
                var requiredFolders = new[] { DOCUMENTS_FOLDER, PHOTOS_FOLDER, BACKUP_FOLDER };
                foreach (var folder in requiredFolders)
                {
                    if (!Directory.Exists(folder))
                    {
                        issues.Add($"Cartella mancante: {folder}");
                    }
                }

                // 3. Verifica database
                var (dbValid, dbIssues) = await DatabaseMigrationHelper.ValidateDatabaseIntegrity();
                if (!dbValid)
                {
                    issues.AddRange(dbIssues);
                }

                // 4. Verifica file di configurazione
                try
                {
                    var settings = SettingsIntegrationHelper.CurrentSettings;
                    if (settings == null)
                    {
                        issues.Add("Impossibile caricare le impostazioni");
                    }
                }
                catch (Exception ex)
                {
                    issues.Add($"Errore caricamento impostazioni: {ex.Message}");
                }

                // 5. Verifica spazio utilizzato
                try
                {
                    var dbSize = new FileInfo(DATABASE_FILE).Length;
                    var documentsSize = GetDirectorySize(DOCUMENTS_FOLDER);
                    var photosSize = GetDirectorySize(PHOTOS_FOLDER);
                    var backupSize = GetDirectorySize(BACKUP_FOLDER);

                    var totalSizeMB = (dbSize + documentsSize + photosSize + backupSize) / (1024 * 1024);

                    System.Diagnostics.Debug.WriteLine($"Spazio utilizzato: {totalSizeMB:F1}MB");

                    if (totalSizeMB > 1000) // Più di 1GB
                    {
                        issues.Add($"Utilizzo spazio elevato: {totalSizeMB:F1}MB - considerare pulizia backup");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore calcolo spazio: {ex.Message}");
                }

                var isHealthy = issues.Count == 0;
                System.Diagnostics.Debug.WriteLine($"Controllo integrità completato: {(isHealthy ? "OK" : $"{issues.Count} problemi")}");

                return (isHealthy, issues.ToArray());
            }
            catch (Exception ex)
            {
                issues.Add($"Errore durante il controllo di integrità: {ex.Message}");
                return (false, issues.ToArray());
            }
        }

        /// <summary>
        /// Calcola la dimensione totale di una directory
        /// </summary>
        private static long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path))
                return 0;

            try
            {
                return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                    .Sum(file => new FileInfo(file).Length);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Pulisce i file temporanei e ottimizza l'applicazione
        /// </summary>
        public static void PerformMaintenance()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== MANUTENZIONE APPLICAZIONE ===");

                // Pulisce backup vecchi
                DatabaseMigrationHelper.PerformAutomaticBackup();

                // Pulisce file temporanei
                var tempFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.tmp", SearchOption.AllDirectories);
                foreach (var file in tempFiles)
                {
                    try
                    {
                        File.Delete(file);
                        System.Diagnostics.Debug.WriteLine($"File temporaneo rimosso: {file}");
                    }
                    catch
                    {
                        // Ignora errori sui file temporanei
                    }
                }

                // Compatta database se supportato
                try
                {
                    using var context = new ImmobiliContext();
                    context.Database.ExecuteSqlRaw("VACUUM;");
                    System.Diagnostics.Debug.WriteLine("Database compattato");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore compattazione database: {ex.Message}");
                }

                System.Diagnostics.Debug.WriteLine("Manutenzione completata");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante la manutenzione: {ex.Message}");
            }
        }
    }
}