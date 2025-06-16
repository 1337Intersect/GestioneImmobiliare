using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImmobiGestio.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Cognome { get; set; } = string.Empty;

        [StringLength(16)]
        public string CodiceFiscale { get; set; } = string.Empty;

        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [StringLength(20)]
        public string Cellulare { get; set; } = string.Empty;

        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(500)]
        public string Indirizzo { get; set; } = string.Empty;

        [StringLength(100)]
        public string Citta { get; set; } = string.Empty;

        [StringLength(10)]
        public string CAP { get; set; } = string.Empty;

        [StringLength(50)]
        public string Provincia { get; set; } = string.Empty;

        public DateTime DataNascita { get; set; } = DateTime.Today.AddYears(-30);

        [StringLength(50)]
        public string TipoCliente { get; set; } = "Acquirente"; // Acquirente, Venditore, Locatario, Locatore

        [Column(TypeName = "decimal(18,2)")]
        public decimal BudgetMin { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BudgetMax { get; set; } = 0;

        [StringLength(500)]
        public string PreferenzeTipologia { get; set; } = string.Empty; // Appartamento, Villa, etc.

        [StringLength(500)]
        public string PreferenzeZone { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Note { get; set; } = string.Empty;

        [StringLength(50)]
        public string StatoCliente { get; set; } = "Attivo"; // Attivo, Inattivo, Prospect, Concluso

        public DateTime DataInserimento { get; set; } = DateTime.Now;
        public DateTime? DataUltimaModifica { get; set; }
        public DateTime? DataUltimoContatto { get; set; }

        [StringLength(100)]
        public string FonteContatto { get; set; } = string.Empty; // Web, Telefono, Referral, etc.

        public int? ImmobileDiInteresseId { get; set; }

        // Proprietà calcolate
        [NotMapped]
        public string NomeCompleto => $"{Nome} {Cognome}".Trim();

        [NotMapped]
        public int Eta => DateTime.Now.Year - DataNascita.Year - (DateTime.Now.DayOfYear < DataNascita.DayOfYear ? 1 : 0);

        [NotMapped]
        public string BudgetRange
        {
            get
            {
                if (BudgetMax > 0)
                    return $"€ {BudgetMin:N0} - € {BudgetMax:N0}";
                else if (BudgetMin > 0)
                    return $"Da € {BudgetMin:N0}";
                else
                    return "Non specificato";
            }
        }

        [NotMapped]
        public string ContattoCompleto
        {
            get
            {
                var contatti = new List<string>();
                if (!string.IsNullOrEmpty(Telefono)) contatti.Add(Telefono);
                if (!string.IsNullOrEmpty(Cellulare)) contatti.Add(Cellulare);
                if (!string.IsNullOrEmpty(Email)) contatti.Add(Email);
                return string.Join(" | ", contatti);
            }
        }

        [NotMapped]
        public string IndirizzoCompleto
        {
            get
            {
                var parti = new List<string>();
                if (!string.IsNullOrEmpty(Indirizzo)) parti.Add(Indirizzo);
                if (!string.IsNullOrEmpty(Citta)) parti.Add(Citta);
                if (!string.IsNullOrEmpty(CAP)) parti.Add(CAP);
                if (!string.IsNullOrEmpty(Provincia)) parti.Add($"({Provincia})");
                return string.Join(", ", parti);
            }
        }

        // Relazioni
        public virtual ICollection<Appuntamento> Appuntamenti { get; set; }
        public virtual ICollection<ClienteImmobile> ImmobiliDiInteresse { get; set; }

        [ForeignKey("ImmobileDiInteresseId")]
        public virtual Immobile? ImmobilePrincipale { get; set; }

        public Cliente()
        {
            // Inizializza le collezioni
            Appuntamenti = new HashSet<Appuntamento>();
            ImmobiliDiInteresse = new HashSet<ClienteImmobile>();

            // Imposta i valori di default
            DataInserimento = DateTime.Now;
            StatoCliente = "Attivo";
            TipoCliente = "Acquirente";
            DataNascita = DateTime.Today.AddYears(-30);

            // Inizializza tutte le stringhe per evitare null
            Nome = string.Empty;
            Cognome = string.Empty;
            CodiceFiscale = string.Empty;
            Telefono = string.Empty;
            Cellulare = string.Empty;
            Email = string.Empty;
            Indirizzo = string.Empty;
            Citta = string.Empty;
            CAP = string.Empty;
            Provincia = string.Empty;
            PreferenzeTipologia = string.Empty;
            PreferenzeZone = string.Empty;
            Note = string.Empty;
            FonteContatto = string.Empty;

            // Valori numerici
            BudgetMin = 0;
            BudgetMax = 0;
        }
    }

    // Tabella di collegamento Cliente-Immobile per gestire gli interessi multipli
    public class ClienteImmobile
    {
        [Key]
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public int ImmobileId { get; set; }

        public DateTime DataInteresse { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string StatoInteresse { get; set; } = "Interessato"; // Interessato, Visionato, Scartato, Offerta

        [StringLength(1000)]
        public string Note { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OffertaProposta { get; set; }

        public DateTime? DataOfferta { get; set; }

        [StringLength(50)]
        public string EsitoOfferta { get; set; } = string.Empty; // Accettata, Rifiutata, In_Trattativa

        // Proprietà calcolate
        [NotMapped]
        public string DescrizioneInteresse
        {
            get
            {
                var desc = StatoInteresse;
                if (OffertaProposta.HasValue && OffertaProposta > 0)
                {
                    desc += $" - Offerta: € {OffertaProposta:N0}";
                }
                return desc;
            }
        }

        // Relazioni
        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey("ImmobileId")]
        public virtual Immobile? Immobile { get; set; }

        public ClienteImmobile()
        {
            DataInteresse = DateTime.Now;
            StatoInteresse = "Interessato";
            Note = string.Empty;
            EsitoOfferta = string.Empty;
        }
    }
}