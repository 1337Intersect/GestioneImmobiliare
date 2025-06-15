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

        [Required]
        public DateTime DataInizio { get; set; }

        [Required]
        public DateTime DataFine { get; set; }

        [Required]
        [StringLength(500)]
        public string Luogo { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TipoAppuntamento { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string StatoAppuntamento { get; set; } = string.Empty;

        public int? ClienteId { get; set; }
        public int? ImmobileId { get; set; }

        [Required]
        [StringLength(20)]
        public string Priorita { get; set; } = string.Empty;

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

        [Required]
        public DateTime DataCreazione { get; set; }

        public DateTime? DataUltimaModifica { get; set; }

        [Required]
        [StringLength(100)]
        public string CreatoDa { get; set; } = string.Empty;

        // Proprietà calcolate
        [NotMapped]
        public TimeSpan Durata => DataFine - DataInizio;

        [NotMapped]
        public bool IsOggi => DataInizio.Date == DateTime.Today;

        [NotMapped]
        public bool IsProssimo => DataInizio.Date > DateTime.Today && DataInizio.Date <= DateTime.Today.AddDays(7);

        [NotMapped]
        public bool IsScaduto => DataInizio < DateTime.Now && StatoAppuntamento == StatiAppuntamento.Programmato;

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

        // COSTRUTTORE CORRETTO CON COSTANTI
        public Appuntamento()
        {
            // Date
            DataCreazione = DateTime.Now;
            DataInizio = DateTime.Now.AddHours(1);
            DataFine = DateTime.Now.AddHours(2);

            // USA LE COSTANTI - CRITICO!
            StatoAppuntamento = StatiAppuntamento.Programmato;
            TipoAppuntamento = TipiAppuntamento.Visita;
            Priorita = PrioritaAppuntamento.Media;

            // Valori di default
            Titolo = "Nuovo Appuntamento";
            Luogo = "Ufficio";
            CreatoDa = "Sistema";

            // Booleani
            RichiedeConferma = true;
            NotificaInviata = false;
            SincronizzatoOutlook = false;

            // Inizializza tutte le stringhe
            Descrizione = string.Empty;
            NotePrivate = string.Empty;
            EsitoIncontro = string.Empty;
            OutlookEventId = string.Empty;
        }

        // FACTORY METHOD per creare appuntamenti corretti
        public static Appuntamento CreaPerCliente(int clienteId, string nomeCliente)
        {
            return new Appuntamento
            {
                ClienteId = clienteId,
                Titolo = $"Appuntamento con {nomeCliente}",
                Descrizione = $"Incontro con il cliente {nomeCliente}",
                DataInizio = DateTime.Now.AddDays(1),
                DataFine = DateTime.Now.AddDays(1).AddHours(1),
                TipoAppuntamento = TipiAppuntamento.Incontro,
                StatoAppuntamento = StatiAppuntamento.Programmato,
                Priorita = PrioritaAppuntamento.Media,
                Luogo = "Ufficio",
                CreatoDa = "Sistema",
                DataCreazione = DateTime.Now
            };
        }

        public static Appuntamento CreaPerImmobile(int immobileId, string titoloImmobile, string indirizzo)
        {
            return new Appuntamento
            {
                ImmobileId = immobileId,
                Titolo = $"Visita - {titoloImmobile}",
                Descrizione = $"Visita dell'immobile: {titoloImmobile}",
                DataInizio = DateTime.Now.AddDays(1),
                DataFine = DateTime.Now.AddDays(1).AddHours(1),
                TipoAppuntamento = TipiAppuntamento.Visita,
                StatoAppuntamento = StatiAppuntamento.Programmato,
                Priorita = PrioritaAppuntamento.Media,
                Luogo = indirizzo,
                CreatoDa = "Sistema",
                DataCreazione = DateTime.Now
            };
        }

        // Metodi di utilità
        public bool PuoEssereModificato()
        {
            return StatoAppuntamento == StatiAppuntamento.Programmato ||
                   StatoAppuntamento == StatiAppuntamento.Confermato;
        }

        public bool PuoEssereCancellato()
        {
            return StatoAppuntamento != StatiAppuntamento.Completato;
        }

        public void Conferma()
        {
            if (StatoAppuntamento == StatiAppuntamento.Programmato)
            {
                StatoAppuntamento = StatiAppuntamento.Confermato;
                DataConferma = DateTime.Now;
                DataUltimaModifica = DateTime.Now;
            }
        }

        public void Completa(string? esito = null)
        {
            StatoAppuntamento = StatiAppuntamento.Completato;
            if (!string.IsNullOrEmpty(esito))
                EsitoIncontro = esito;
            DataUltimaModifica = DateTime.Now;
        }

        public void Annulla()
        {
            if (PuoEssereCancellato())
            {
                StatoAppuntamento = StatiAppuntamento.Annullato;
                DataUltimaModifica = DateTime.Now;
            }
        }

        // Metodo per validare che tutti i campi obbligatori siano impostati
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Titolo) &&
                   !string.IsNullOrEmpty(TipoAppuntamento) &&
                   !string.IsNullOrEmpty(StatoAppuntamento) &&
                   !string.IsNullOrEmpty(Priorita) &&
                   !string.IsNullOrEmpty(Luogo) &&
                   !string.IsNullOrEmpty(CreatoDa) &&
                   DataInizio != default &&
                   DataFine != default &&
                   DataCreazione != default &&
                   DataFine > DataInizio;
        }
    }

    // Enum helper per migliorare l'usabilità - CORRETTI E FUNZIONANTI
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