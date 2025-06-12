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

        public DateTime DataNascita { get; set; }

        [StringLength(50)]
        public string TipoCliente { get; set; } = "Acquirente"; // Acquirente, Venditore, Locatario, Locatore

        [Column(TypeName = "decimal(18,2)")]
        public decimal BudgetMin { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BudgetMax { get; set; }

        [StringLength(500)]
        public string PreferenzeTipologia { get; set; } = string.Empty; // Appartamento, Villa, etc.

        [StringLength(500)]
        public string PreferenzeZone { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Note { get; set; } = string.Empty;

        [StringLength(50)]
        public string StatoCliente { get; set; } = "Attivo"; // Attivo, Inattivo, Prospect, Concluso

        public DateTime DataInserimento { get; set; }
        public DateTime? DataUltimaModifica { get; set; }
        public DateTime? DataUltimoContatto { get; set; }

        [StringLength(100)]
        public string FonteContatto { get; set; } = string.Empty; // Web, Telefono, Referral, etc.

        public int? ImmobileDiInteresseId { get; set; }

        // Proprietà calcolate
        [NotMapped]
        public string NomeCompleto => $"{Nome} {Cognome}";

        [NotMapped]
        public int Eta => DateTime.Now.Year - DataNascita.Year - (DateTime.Now.DayOfYear < DataNascita.DayOfYear ? 1 : 0);

        [NotMapped]
        public string BudgetRange => BudgetMax > 0 ? $"€ {BudgetMin:N0} - € {BudgetMax:N0}" : "Non specificato";

        // Relazioni
        public virtual ICollection<Appuntamento> Appuntamenti { get; set; }
        public virtual ICollection<ClienteImmobile> ImmobiliDiInteresse { get; set; }

        [ForeignKey("ImmobileDiInteresseId")]
        public virtual Immobile? ImmobilePrincipale { get; set; }

        public Cliente()
        {
            Appuntamenti = new HashSet<Appuntamento>();
            ImmobiliDiInteresse = new HashSet<ClienteImmobile>();
            DataInserimento = DateTime.Now;
            StatoCliente = "Attivo";
            TipoCliente = "Acquirente";
        }
    }

    // Tabella di collegamento Cliente-Immobile per gestire gli interessi multipli
    public class ClienteImmobile
    {
        [Key]
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public int ImmobileId { get; set; }

        public DateTime DataInteresse { get; set; }

        [StringLength(50)]
        public string StatoInteresse { get; set; } = "Interessato"; // Interessato, Visionato, Scartato, Offerta

        [StringLength(1000)]
        public string Note { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OffertaProposta { get; set; }

        public DateTime? DataOfferta { get; set; }

        [StringLength(50)]
        public string EsitoOfferta { get; set; } = string.Empty; // Accettata, Rifiutata, In_Trattativa

        // Relazioni
        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey("ImmobileId")]
        public virtual Immobile? Immobile { get; set; }

        public ClienteImmobile()
        {
            DataInteresse = DateTime.Now;
            StatoInteresse = "Interessato";
        }
    }
}