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
            // Database SQLite nella cartella del progetto
            optionsBuilder.UseSqlite("Data Source=immobili.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurazione Immobile
            modelBuilder.Entity<Immobile>()
                .HasMany(i => i.Documenti)
                .WithOne(d => d.Immobile)
                .HasForeignKey(d => d.ImmobileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Immobile>()
                .HasMany(i => i.Foto)
                .WithOne(f => f.Immobile)
                .HasForeignKey(f => f.ImmobileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurazione Cliente
            modelBuilder.Entity<Cliente>()
                .HasMany(c => c.Appuntamenti)
                .WithOne(a => a.Cliente)
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cliente>()
                .HasMany(c => c.ImmobiliDiInteresse)
                .WithOne(ci => ci.Cliente)
                .HasForeignKey(ci => ci.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurazione Appuntamento
            modelBuilder.Entity<Appuntamento>()
                .HasOne(a => a.Cliente)
                .WithMany(c => c.Appuntamenti)
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Appuntamento>()
                .HasOne(a => a.Immobile)
                .WithMany()
                .HasForeignKey(a => a.ImmobileId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configurazione ClienteImmobile (tabella di collegamento)
            modelBuilder.Entity<ClienteImmobile>()
                .HasOne(ci => ci.Cliente)
                .WithMany(c => c.ImmobiliDiInteresse)
                .HasForeignKey(ci => ci.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClienteImmobile>()
                .HasOne(ci => ci.Immobile)
                .WithMany()
                .HasForeignKey(ci => ci.ImmobileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indici per performance
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.CodiceFiscale)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => new { c.Nome, c.Cognome });

            modelBuilder.Entity<Appuntamento>()
                .HasIndex(a => a.DataInizio);

            modelBuilder.Entity<Appuntamento>()
                .HasIndex(a => a.StatoAppuntamento);

            modelBuilder.Entity<Immobile>()
                .HasIndex(i => i.StatoVendita);

            modelBuilder.Entity<Immobile>()
                .HasIndex(i => i.TipoImmobile);

            modelBuilder.Entity<Immobile>()
                .HasIndex(i => i.Citta);

            // Configurazioni di precisione per decimali
            modelBuilder.Entity<Cliente>()
                .Property(c => c.BudgetMin)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Cliente>()
                .Property(c => c.BudgetMax)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Immobile>()
                .Property(i => i.Prezzo)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ClienteImmobile>()
                .Property(ci => ci.OffertaProposta)
                .HasPrecision(18, 2);

            // Seed data (dati di esempio)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed per tipologie immobili (se necessario per lookup tables future)

            // Esempio di cliente di test (opzionale)
            /*
            modelBuilder.Entity<Cliente>().HasData(
                new Cliente
                {
                    Id = 1,
                    Nome = "Mario",
                    Cognome = "Rossi",
                    Email = "mario.rossi@email.com",
                    Telefono = "0123456789",
                    TipoCliente = "Acquirente",
                    BudgetMin = 150000,
                    BudgetMax = 250000,
                    StatoCliente = "Attivo",
                    DataInserimento = DateTime.Now.AddDays(-30)
                }
            );
            */
        }

        public override int SaveChanges()
        {
            // Aggiorna automaticamente DataUltimaModifica
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

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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
    }
}