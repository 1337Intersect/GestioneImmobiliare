using ImmobiGestio.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImmobiGestio.Data
{
    public class ImmobiliContext : DbContext
    {
        public DbSet<Immobile> Immobili { get; set; }
        public DbSet<DocumentoImmobile> Documenti { get; set; }
        public DbSet<FotoImmobile> Foto { get; set; }
        public DbSet<Cliente> Clienti { get; set; }
        public DbSet<Appuntamento> Appuntamenti { get; set; }
        public DbSet<ClienteImmobile> ClientiImmobili { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                try
                {
                    // Database SQLite nella cartella del progetto
                    var connectionString = "Data Source=immobili.db";
                    optionsBuilder.UseSqlite(connectionString);

                    // Abilita logging per debug solo in modalità DEBUG
#if DEBUG
                    optionsBuilder.EnableSensitiveDataLogging();
                    optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine($"EF: {message}"));
#endif

                    System.Diagnostics.Debug.WriteLine($"Database configurato: {connectionString}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore configurazione database: {ex.Message}");
                    throw;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Indici per performance query calendario
            modelBuilder.Entity<Appuntamento>()
                .HasIndex(a => new { a.DataInizio, a.StatoAppuntamento })
                .HasDatabaseName("IX_Appuntamenti_DataInizio_Stato");

            modelBuilder.Entity<Appuntamento>()
                .HasIndex(a => a.ClienteId)
                .HasDatabaseName("IX_Appuntamenti_ClienteId");
        }

        private void ConfigureImmobile(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Immobile>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Campi obbligatori
                entity.Property(e => e.Titolo).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Indirizzo).IsRequired().HasMaxLength(500);
                entity.Property(e => e.TipoImmobile).IsRequired().HasMaxLength(50).HasDefaultValue("Appartamento");
                entity.Property(e => e.StatoConservazione).IsRequired().HasMaxLength(50).HasDefaultValue("Buono");
                entity.Property(e => e.ClasseEnergetica).IsRequired().HasMaxLength(50).HasDefaultValue("G");
                entity.Property(e => e.StatoVendita).IsRequired().HasMaxLength(50).HasDefaultValue("Disponibile");
                entity.Property(e => e.DataInserimento).IsRequired();

                // Campi opzionali
                entity.Property(e => e.Citta).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.CAP).HasMaxLength(10).IsRequired(false);
                entity.Property(e => e.Provincia).HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.Descrizione).HasMaxLength(2000).IsRequired(false);

                // Campi numerici
                entity.Property(e => e.Prezzo).HasPrecision(18, 2).IsRequired().HasDefaultValue(0);
                entity.Property(e => e.Superficie).IsRequired(false);
                entity.Property(e => e.NumeroLocali).IsRequired(false);
                entity.Property(e => e.NumeroBagni).IsRequired(false);
                entity.Property(e => e.Piano).IsRequired(false);
                entity.Property(e => e.DataUltimaModifica).IsRequired(false);

                // Relazioni
                entity.HasMany(i => i.Documenti)
                      .WithOne(d => d.Immobile)
                      .HasForeignKey(d => d.ImmobileId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(i => i.Foto)
                      .WithOne(f => f.Immobile)
                      .HasForeignKey(f => f.ImmobileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureCliente(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Campi obbligatori
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Cognome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TipoCliente).IsRequired().HasMaxLength(50).HasDefaultValue("Acquirente");
                entity.Property(e => e.StatoCliente).IsRequired().HasMaxLength(50).HasDefaultValue("Attivo");
                entity.Property(e => e.DataInserimento).IsRequired();

                // Campi opzionali
                entity.Property(e => e.CodiceFiscale).HasMaxLength(16).IsRequired(false);
                entity.Property(e => e.Email).HasMaxLength(200).IsRequired(false);
                entity.Property(e => e.Telefono).HasMaxLength(20).IsRequired(false);
                entity.Property(e => e.Cellulare).HasMaxLength(20).IsRequired(false);
                entity.Property(e => e.Indirizzo).HasMaxLength(500).IsRequired(false);
                entity.Property(e => e.Citta).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.CAP).HasMaxLength(10).IsRequired(false);
                entity.Property(e => e.Provincia).HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.Note).HasMaxLength(2000).IsRequired(false);
                entity.Property(e => e.FonteContatto).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.PreferenzeTipologia).HasMaxLength(500).IsRequired(false);
                entity.Property(e => e.PreferenzeZone).HasMaxLength(500).IsRequired(false);

                // Campi numerici e date
                entity.Property(e => e.BudgetMin).HasPrecision(18, 2).HasDefaultValue(0);
                entity.Property(e => e.BudgetMax).HasPrecision(18, 2).HasDefaultValue(0);
                entity.Property(e => e.DataNascita).IsRequired();
                entity.Property(e => e.DataUltimaModifica).IsRequired(false);
                entity.Property(e => e.DataUltimoContatto).IsRequired(false);
                entity.Property(e => e.ImmobileDiInteresseId).IsRequired(false);

                // Indici non unici per permettere valori vuoti/duplicati
                entity.HasIndex(e => e.Email).IsUnique(false);
                entity.HasIndex(e => e.CodiceFiscale).IsUnique(false);
                entity.HasIndex(e => new { e.Nome, e.Cognome });

                // Relazioni
                entity.HasMany(c => c.Appuntamenti)
                      .WithOne(a => a.Cliente)
                      .HasForeignKey(a => a.ClienteId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(c => c.ImmobiliDiInteresse)
                      .WithOne(ci => ci.Cliente)
                      .HasForeignKey(ci => ci.ClienteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureAppuntamento(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appuntamento>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Campi obbligatori con valori di default SICURI
                entity.Property(e => e.Titolo).IsRequired().HasMaxLength(200).HasDefaultValue("Nuovo Appuntamento");
                entity.Property(e => e.DataInizio).IsRequired();
                entity.Property(e => e.DataFine).IsRequired();
                entity.Property(e => e.DataCreazione).IsRequired();
                entity.Property(e => e.CreatoDa).IsRequired().HasMaxLength(100).HasDefaultValue("Sistema");
                entity.Property(e => e.Luogo).IsRequired().HasMaxLength(500).HasDefaultValue("Ufficio");

                // CAMPI CRITICI con valori di default ESPLICITI
                entity.Property(e => e.TipoAppuntamento).IsRequired().HasMaxLength(50).HasDefaultValue("Visita");
                entity.Property(e => e.StatoAppuntamento).IsRequired().HasMaxLength(50).HasDefaultValue("Programmato");
                entity.Property(e => e.Priorita).IsRequired().HasMaxLength(20).HasDefaultValue("Media");

                // Campi booleani con default chiari
                entity.Property(e => e.RichiedeConferma).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.NotificaInviata).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.SincronizzatoOutlook).IsRequired().HasDefaultValue(false);

                // Campi opzionali
                entity.Property(e => e.Descrizione).HasMaxLength(1000).IsRequired(false);
                entity.Property(e => e.NotePrivate).HasMaxLength(1000).IsRequired(false);
                entity.Property(e => e.EsitoIncontro).HasMaxLength(1000).IsRequired(false);
                entity.Property(e => e.OutlookEventId).HasMaxLength(255).IsRequired(false);
                entity.Property(e => e.DataUltimaModifica).IsRequired(false);
                entity.Property(e => e.DataConferma).IsRequired(false);
                entity.Property(e => e.DataNotifica).IsRequired(false);

                // Foreign Keys nullable
                entity.Property(e => e.ClienteId).IsRequired(false);
                entity.Property(e => e.ImmobileId).IsRequired(false);

                // Relazioni SICURE
                entity.HasOne(a => a.Cliente)
                      .WithMany(c => c.Appuntamenti)
                      .HasForeignKey(a => a.ClienteId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.Immobile)
                      .WithMany()
                      .HasForeignKey(a => a.ImmobileId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Indici per performance
                entity.HasIndex(a => a.DataInizio);
                entity.HasIndex(a => a.StatoAppuntamento);
                entity.HasIndex(a => a.TipoAppuntamento);
                entity.HasIndex(a => a.ClienteId);
                entity.HasIndex(a => a.ImmobileId);
            });
        }

        private void ConfigureDocumentoImmobile(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentoImmobile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TipoDocumento).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NomeFile).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PercorsoFile).IsRequired().HasMaxLength(500);
                entity.Property(e => e.DataCaricamento).IsRequired();
                entity.Property(e => e.Descrizione).HasMaxLength(255).IsRequired(false);

                // Indice per performance
                entity.HasIndex(e => e.ImmobileId);
            });
        }

        private void ConfigureFotoImmobile(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FotoImmobile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NomeFile).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PercorsoFile).IsRequired().HasMaxLength(500);
                entity.Property(e => e.DataCaricamento).IsRequired();
                entity.Property(e => e.IsPrincipale).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.Descrizione).HasMaxLength(255).IsRequired(false);

                // Indice per performance
                entity.HasIndex(e => e.ImmobileId);
            });
        }

        private void ConfigureClienteImmobile(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClienteImmobile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataInteresse).IsRequired();
                entity.Property(e => e.StatoInteresse).IsRequired().HasMaxLength(50).HasDefaultValue("Interessato");
                entity.Property(e => e.Note).HasMaxLength(1000).IsRequired(false);
                entity.Property(e => e.OffertaProposta).HasPrecision(18, 2).IsRequired(false);
                entity.Property(e => e.DataOfferta).IsRequired(false);
                entity.Property(e => e.EsitoOfferta).HasMaxLength(50).IsRequired(false);

                entity.HasOne(ci => ci.Cliente)
                      .WithMany(c => c.ImmobiliDiInteresse)
                      .HasForeignKey(ci => ci.ClienteId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Immobile)
                      .WithMany()
                      .HasForeignKey(ci => ci.ImmobileId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indici per performance
                entity.HasIndex(e => e.ClienteId);
                entity.HasIndex(e => e.ImmobileId);
                entity.HasIndex(e => new { e.ClienteId, e.ImmobileId }).IsUnique();
            });
        }

        public override int SaveChanges()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INIZIO SAVECHANGES ===");

                // Preprocessing delle entità
                PreprocessEntities();

                // Log delle entità che stanno per essere salvate
                LogEntityChanges();

                var result = base.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"SaveChanges completato: {result} record interessati");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERRORE in SaveChanges: {ex}");

                // Log dettagliato per debug
                LogErrorDetails(ex);

                throw;
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INIZIO SAVECHANGES ASYNC ===");

                // Preprocessing delle entità
                PreprocessEntities();

                var result = await base.SaveChangesAsync(cancellationToken);

                System.Diagnostics.Debug.WriteLine($"SaveChangesAsync completato: {result} record interessati");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERRORE in SaveChangesAsync: {ex}");
                LogErrorDetails(ex);
                throw;
            }
        }

        private void PreprocessEntities()
        {
            var now = DateTime.Now;

            // Preprocessing per entità aggiunte
            var addedEntities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added)
                .ToList();

            foreach (var entry in addedEntities)
            {
                // Imposta DataInserimento per nuove entità
                if (entry.Entity is Cliente cliente && cliente.DataInserimento == default)
                {
                    cliente.DataInserimento = now;
                }
                else if (entry.Entity is Immobile immobile && immobile.DataInserimento == default)
                {
                    immobile.DataInserimento = now;
                }
                else if (entry.Entity is Appuntamento appuntamento)
                {
                    if (appuntamento.DataCreazione == default)
                        appuntamento.DataCreazione = now;

                    // VALIDAZIONE CRITICA dei campi obbligatori
                    ValidateAppuntamento(appuntamento);
                }
            }

            // Preprocessing per entità modificate
            var modifiedEntries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified)
                .ToList();

            foreach (var entry in modifiedEntries)
            {
                // Aggiorna DataUltimaModifica
                if (entry.Entity is Cliente cliente)
                {
                    cliente.DataUltimaModifica = now;
                }
                else if (entry.Entity is Immobile immobile)
                {
                    immobile.DataUltimaModifica = now;
                }
                else if (entry.Entity is Appuntamento appuntamento)
                {
                    appuntamento.DataUltimaModifica = now;
                    ValidateAppuntamento(appuntamento);
                }
            }
        }

        private void ValidateAppuntamento(Appuntamento appuntamento)
        {
            // Fix automatico dei valori mancanti
            if (string.IsNullOrEmpty(appuntamento.TipoAppuntamento))
            {
                appuntamento.TipoAppuntamento = "Visita";
                System.Diagnostics.Debug.WriteLine($"Fix TipoAppuntamento per ID {appuntamento.Id}");
            }

            if (string.IsNullOrEmpty(appuntamento.StatoAppuntamento))
            {
                appuntamento.StatoAppuntamento = "Programmato";
                System.Diagnostics.Debug.WriteLine($"Fix StatoAppuntamento per ID {appuntamento.Id}");
            }

            if (string.IsNullOrEmpty(appuntamento.Priorita))
            {
                appuntamento.Priorita = "Media";
                System.Diagnostics.Debug.WriteLine($"Fix Priorita per ID {appuntamento.Id}");
            }

            if (string.IsNullOrEmpty(appuntamento.Titolo))
            {
                appuntamento.Titolo = "Nuovo Appuntamento";
                System.Diagnostics.Debug.WriteLine($"Fix Titolo per ID {appuntamento.Id}");
            }

            if (string.IsNullOrEmpty(appuntamento.Luogo))
            {
                appuntamento.Luogo = "Ufficio";
                System.Diagnostics.Debug.WriteLine($"Fix Luogo per ID {appuntamento.Id}");
            }

            if (string.IsNullOrEmpty(appuntamento.CreatoDa))
            {
                appuntamento.CreatoDa = "Sistema";
                System.Diagnostics.Debug.WriteLine($"Fix CreatoDa per ID {appuntamento.Id}");
            }
        }

        private void LogEntityChanges()
        {
            var changedEntities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in changedEntities)
            {
                var entityName = entry.Entity.GetType().Name;
                System.Diagnostics.Debug.WriteLine($"  {entry.State}: {entityName}");

                if (entry.Entity is Appuntamento app)
                {
                    System.Diagnostics.Debug.WriteLine($"    ID: {app.Id}");
                    System.Diagnostics.Debug.WriteLine($"    Titolo: '{app.Titolo}'");
                    System.Diagnostics.Debug.WriteLine($"    TipoAppuntamento: '{app.TipoAppuntamento}'");
                    System.Diagnostics.Debug.WriteLine($"    StatoAppuntamento: '{app.StatoAppuntamento}'");
                    System.Diagnostics.Debug.WriteLine($"    Priorita: '{app.Priorita}'");
                    System.Diagnostics.Debug.WriteLine($"    ClienteId: {app.ClienteId}");
                    System.Diagnostics.Debug.WriteLine($"    ImmobileId: {app.ImmobileId}");
                }
            }
        }

        private void LogErrorDetails(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== DETTAGLI ERRORE SAVECHANGES ===");
            System.Diagnostics.Debug.WriteLine($"Tipo eccezione: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Messaggio: {ex.Message}");

            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Eccezione interna: {ex.InnerException.Message}");
            }

            // Log delle entità con problemi
            var problemEntities = ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Entità in elaborazione: {problemEntities.Count}");
            foreach (var entry in problemEntities)
            {
                System.Diagnostics.Debug.WriteLine($"  - {entry.Entity.GetType().Name}: {entry.State}");
            }
        }

        // RIMOSSO: override Dispose(bool) che causava errore
        // Il DbContext gestisce già correttamente il dispose
    }
}