using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImmobiGestio.Models
{
    public class Immobile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Titolo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Indirizzo { get; set; } = string.Empty;

        [StringLength(100)]
        public string Citta { get; set; } = string.Empty;

        [StringLength(10)]
        public string CAP { get; set; } = string.Empty;

        [StringLength(50)]
        public string Provincia { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Prezzo { get; set; } = 0;

        [StringLength(2000)]
        public string Descrizione { get; set; } = string.Empty;

        // Rende nullable le proprietà che possono essere vuote
        public int? Superficie { get; set; }
        public int? NumeroLocali { get; set; }
        public int? NumeroBagni { get; set; }
        public int? Piano { get; set; }

        [StringLength(50)]
        public string TipoImmobile { get; set; } = "Appartamento";

        [StringLength(50)]
        public string StatoConservazione { get; set; } = "Buono";

        [StringLength(50)]
        public string ClasseEnergetica { get; set; } = "G";

        public DateTime DataInserimento { get; set; }
        public DateTime? DataUltimaModifica { get; set; }

        [StringLength(50)]
        public string StatoVendita { get; set; } = "Disponibile";

        // Proprietà calcolate per display
        [NotMapped]
        public string SuperficieDisplay => Superficie?.ToString() ?? "N/D";

        [NotMapped]
        public string NumeroLocaliDisplay => NumeroLocali?.ToString() ?? "N/D";

        [NotMapped]
        public string NumeroBagniDisplay => NumeroBagni?.ToString() ?? "N/D";

        [NotMapped]
        public string PianoDisplay => Piano?.ToString() ?? "N/D";

        [NotMapped]
        public string PrezzoFormattato => Prezzo > 0 ? $"€ {Prezzo:N0}" : "Prezzo da definire";

        // Documenti e file
        public virtual ICollection<DocumentoImmobile> Documenti { get; set; }
        public virtual ICollection<FotoImmobile> Foto { get; set; }

        public Immobile()
        {
            Documenti = new HashSet<DocumentoImmobile>();
            Foto = new HashSet<FotoImmobile>();
            DataInserimento = DateTime.Now;
            StatoVendita = "Disponibile";
            TipoImmobile = "Appartamento";
            StatoConservazione = "Buono";
            ClasseEnergetica = "G";

            // Inizializza le stringhe per evitare null
            Titolo = string.Empty;
            Indirizzo = string.Empty;
            Citta = string.Empty;
            CAP = string.Empty;
            Provincia = string.Empty;
            Descrizione = string.Empty;
        }
    }

    public class DocumentoImmobile
    {
        [Key]
        public int Id { get; set; }

        public int ImmobileId { get; set; }

        [Required]
        [StringLength(100)]
        public string TipoDocumento { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string NomeFile { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string PercorsoFile { get; set; } = string.Empty;

        public DateTime DataCaricamento { get; set; }

        [StringLength(255)]
        public string Descrizione { get; set; } = string.Empty;

        [ForeignKey("ImmobileId")]
        public virtual Immobile? Immobile { get; set; }

        public DocumentoImmobile()
        {
            DataCaricamento = DateTime.Now;
            TipoDocumento = string.Empty;
            NomeFile = string.Empty;
            PercorsoFile = string.Empty;
            Descrizione = string.Empty;
        }
    }

    public class FotoImmobile
    {
        [Key]
        public int Id { get; set; }

        public int ImmobileId { get; set; }

        [Required]
        [StringLength(255)]
        public string NomeFile { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string PercorsoFile { get; set; } = string.Empty;

        public bool IsPrincipale { get; set; }

        [StringLength(255)]
        public string Descrizione { get; set; } = string.Empty;

        public DateTime DataCaricamento { get; set; }

        [ForeignKey("ImmobileId")]
        public virtual Immobile? Immobile { get; set; }

        public FotoImmobile()
        {
            DataCaricamento = DateTime.Now;
            IsPrincipale = false;
            NomeFile = string.Empty;
            PercorsoFile = string.Empty;
            Descrizione = string.Empty;
        }
    }
}