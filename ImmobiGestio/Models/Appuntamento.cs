using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImmobiGestio.Models
{
    public class Appuntamento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Titolo { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Descrizione { get; set; } = string.Empty;

        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }

        [StringLength(500)]
        public string Luogo { get; set; } = string.Empty;

        [StringLength(50)]
        public string TipoAppuntamento { get; set; } = "Visita"; // Visita, Incontro, Chiamata, Firma

        [StringLength(50)]
        public string StatoAppuntamento { get; set; } = "Programmato"; // Programmato, Confermato, Completato, Annullato

        public int? ClienteId { get; set; }
        public int? ImmobileId { get; set; }

        [StringLength(20)]
        public string Priorita { get; set; } = "Media"; // Bassa, Media, Alta

        public bool RichiedeConferma { get; set; } = true;
        public DateTime? DataConferma { get; set; }

        public bool NotificaInviata { get; set; } = false;
        public DateTime? DataNotifica { get; set; }

        [StringLength(1000)]
        public string NotePrivate { get; set; } = string.Empty;

        [StringLength(1000)]
        public string EsitoIncontro { get; set; } = string.Empty;

        public bool SincronizzatoOutlook { get; set; } = false;

        [StringLength(255)]
        public string OutlookEventId { get; set; } = string.Empty;

        public DateTime DataCreazione { get; set; }
        public DateTime? DataUltimaModifica { get; set; }

        [StringLength(100)]
        public string CreatoDa { get; set; } = "Sistema";

        // Proprietà calcolate
        [NotMapped]
        public TimeSpan Durata => DataFine - DataInizio;

        [NotMapped]
        public bool IsOggi => DataInizio.Date == DateTime.Today;

        [NotMapped]
        public bool IsProssimo => DataInizio.Date > DateTime.Today && DataInizio.Date <= DateTime.Today.AddDays(7);

        [NotMapped]
        public bool IsScaduto => DataInizio < DateTime.Now && StatoAppuntamento == "Programmato";

        [NotMapped]
        public string DurataFormattata
        {
            get
            {
                var durata = Durata;
                if (durata.TotalDays >= 1)
                    return $"{durata.Days}g {durata.Hours}h {durata.Minutes}m";
                else if (durata.TotalHours >= 1)
                    return $"{durata.Hours}h {durata.Minutes}m";
                else
                    return $"{durata.Minutes}m";
            }
        }

        [NotMapped]
        public string StatoColore
        {
            get
            {
                return StatoAppuntamento switch
                {
                    "Programmato" => "#FF2196F3", // Blu
                    "Confermato" => "#FF4CAF50",  // Verde
                    "Completato" => "#FF9E9E9E",  // Grigio
                    "Annullato" => "#FFF44336",   // Rosso
                    _ => "#FF2196F3"
                };
            }
        }

        // Relazioni
        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey("ImmobileId")]
        public virtual Immobile? Immobile { get; set; }

        public Appuntamento()
        {
            DataCreazione = DateTime.Now;
            StatoAppuntamento = "Programmato";
            TipoAppuntamento = "Visita";
            Priorita = "Media";
            RichiedeConferma = true;
            CreatoDa = "Sistema";
        }

        // Metodi di utilità
        public bool PuoEssereModificato()
        {
            return StatoAppuntamento == "Programmato" || StatoAppuntamento == "Confermato";
        }

        public bool PuoEssereCancellato()
        {
            return StatoAppuntamento != "Completato";
        }

        public void Conferma()
        {
            if (StatoAppuntamento == "Programmato")
            {
                StatoAppuntamento = "Confermato";
                DataConferma = DateTime.Now;
                DataUltimaModifica = DateTime.Now;
            }
        }

        public void Completa(string? esito = null)
        {
            StatoAppuntamento = "Completato";
            if (!string.IsNullOrEmpty(esito))
                EsitoIncontro = esito;
            DataUltimaModifica = DateTime.Now;
        }

        public void Annulla()
        {
            if (PuoEssereCancellato())
            {
                StatoAppuntamento = "Annullato";
                DataUltimaModifica = DateTime.Now;
            }
        }
    }

    // Enum helper per migliorare l'usabilità
    public static class TipiAppuntamento
    {
        public const string Visita = "Visita";
        public const string Incontro = "Incontro";
        public const string Chiamata = "Chiamata";
        public const string Firma = "Firma";
        public const string Valutazione = "Valutazione";
        public const string Sopralluogo = "Sopralluogo";

        public static string[] GetAll() => new[]
        {
            Visita, Incontro, Chiamata, Firma, Valutazione, Sopralluogo
        };
    }

    public static class StatiAppuntamento
    {
        public const string Programmato = "Programmato";
        public const string Confermato = "Confermato";
        public const string Completato = "Completato";
        public const string Annullato = "Annullato";

        public static string[] GetAll() => new[]
        {
            Programmato, Confermato, Completato, Annullato
        };
    }

    public static class PrioritaAppuntamento
    {
        public const string Bassa = "Bassa";
        public const string Media = "Media";
        public const string Alta = "Alta";

        public static string[] GetAll() => new[]
        {
            Bassa, Media, Alta
        };
    }
}