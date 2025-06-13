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
                // Database SQLite nella cartella del progetto
                var connectionString = "Data Source=immobili.db";
                optionsBuilder.UseSqlite(connectionString);

                // Abilita logging per debug
#if DEBUG
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
#endif
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurazione Immobile
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

            // Configurazione Cliente
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

                // Indici (non unici per permettere valori vuoti/duplicati)
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

            // Configurazione Appuntamento - CRITICA!
            modelBuilder.Entity<Appuntamento>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Campi obbligatori con valori di default
                entity.Property(e => e.Titolo).IsRequired().HasMaxLength(200).HasDefaultValue("Nuovo Appuntamento");
                entity.Property(e => e.DataInizio).IsRequired();
                entity.Property(e => e.DataFine).IsRequired();
                entity.Property(e => e.DataCreazione).IsRequired();
                entity.Property(e => e.CreatoDa).IsRequired().HasMaxLength(100).HasDefaultValue("Sistema");
                entity.Property(e => e.Luogo).IsRequired().HasMaxLength(500).HasDefaultValue("Ufficio");

                // Campi con valori di default espliciti - IMPORTANTE!
                entity.Property(e => e.TipoAppuntamento).IsRequired().HasMaxLength(50).HasDefaultValue("Visita");
                entity.Property(e => e.StatoAppuntamento).IsRequired().HasMaxLength(50).HasDefaultValue("Programmato");
                entity.Property(e => e.Priorita).IsRequired().HasMaxLength(20).HasDefaultValue("Media");

                // Campi booleani con default
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

                // Relazioni
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
            });

            // Configurazione DocumentoImmobile
            modelBuilder.Entity<DocumentoImmobile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TipoDocumento).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NomeFile).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PercorsoFile).IsRequired().HasMaxLength(500);
                entity.Property(e => e.DataCaricamento).IsRequired();
                entity.Property(e => e.Descrizione).HasMaxLength(255).IsRequired(false);
            });

            // Configurazione FotoImmobile
            modelBuilder.Entity<FotoImmobile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NomeFile).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PercorsoFile).IsRequired().HasMaxLength(500);
                entity.Property(e => e.DataCaricamento).IsRequired();
                entity.Property(e => e.IsPrincipale).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.Descrizione).HasMaxLength(255).IsRequired(false);
            });

            // Configurazione ClienteImmobile
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
            });
        }

        public override int SaveChanges()
        {
            try
            {
                // Log delle entità che stanno per essere salvate
                var addedEntities = ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added)
                    .ToList();

                foreach (var entry in addedEntities)
                {
                    System.Diagnostics.Debug.WriteLine($"Salvando entità: {entry.Entity.GetType().Name}");

                    // Per gli Appuntamenti, verifica i campi obbligatori
                    if (entry.Entity is Appuntamento app)
                    {
                        System.Diagnostics.Debug.WriteLine($"  TipoAppuntamento: '{app.TipoAppuntamento}'");
                        System.Diagnostics.Debug.WriteLine($"  StatoAppuntamento: '{app.StatoAppuntamento}'");
                        System.Diagnostics.Debug.WriteLine($"  Priorita: '{app.Priorita}'");
                        System.Diagnostics.Debug.WriteLine($"  Titolo: '{app.Titolo}'");
                        System.Diagnostics.Debug.WriteLine($"  Luogo: '{app.Luogo}'");
                    }
                }

                // Aggiorna automaticamente DataUltimaModifica per le entità modificate
                var modifiedEntries = ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Modified);

                foreach (var entry in modifiedEntries)
                {
                    if (entry.Entity is Cliente cliente)
                    {
                        cliente.DataUltimaModifica = DateTime.Now;
                    }
                    else if (entry.Entity is Immobile immobile)
                    {
                        immobile.DataUltimaModifica = DateTime.Now;
                    }
                    else if (entry.Entity is Appuntamento appuntamento)
                    {
                        appuntamento.DataUltimaModifica = DateTime.Now;
                    }
                }

                var result = base.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"SaveChanges completato: {result} record interessati");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in SaveChanges: {ex}");
                throw;
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Stessa logica per l'async
                var entries = ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Modified);

                foreach (var entry in entries)
                {
                    if (entry.Entity is Cliente cliente)
                    {
                        cliente.DataUltimaModifica = DateTime.Now;
                    }
                    else if (entry.Entity is Immobile immobile)
                    {
                        immobile.DataUltimaModifica = DateTime.Now;
                    }
                    else if (entry.Entity is Appuntamento appuntamento)
                    {
                        appuntamento.DataUltimaModifica = DateTime.Now;
                    }
                }

                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore in SaveChangesAsync: {ex}");
                throw;
            }
        }
    }
}