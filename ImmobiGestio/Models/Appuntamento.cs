using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImmobiGestio.Models
{
    // COSTANTI STATICHE per garantire coerenza
    public static class TipiAppuntamento
    {
        public const string Visita = "Visita";
        public const string Incontro = "Incontro";
        public const string Chiamata = "Chiamata";
        public const string Valutazione = "Valutazione";
        public const string Firma = "Firma";
        public const string Sopralluogo = "Sopralluogo";
        public const string Altro = "Altro";

        public static readonly string[] All = { Visita, Incontro, Chiamata, Valutazione, Firma, Sopralluogo, Altro };
    }

    public static class StatiAppuntamento
    {
        public const string Programmato = "Programmato";
        public const string Confermato = "Confermato";
        public const string InCorso = "In Corso";
        public const string Completato = "Completato";
        public const string Cancellato = "Cancellato";
        public const string Rimandato = "Rimandato";
        public const string NonPresentato = "Non Presentato";

        public static readonly string[] All = { Programmato, Confermato, InCorso, Completato, Cancellato, Rimandato, NonPresentato };
    }

    public static class PrioritaAppuntamento
    {
        public const string Bassa = "Bassa";
        public const string Media = "Media";
        public const string Alta = "Alta";
        public const string Urgente = "Urgente";

        public static readonly string[] All = { Bassa, Media, Alta, Urgente };
    }

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
        public bool InPassato => DateTime.Now > DataFine;

        [NotMapped]
        public bool OggiODomani => DataInizio.Date == DateTime.Today || DataInizio.Date == DateTime.Today.AddDays(1);

        [NotMapped]
        public string DataOraFormattata => $"{DataInizio:dd/MM/yyyy HH:mm} - {DataFine:HH:mm}";

        [NotMapped]
        public string DurataFormattata
        {
            get
            {
                var durata = Durata;
                if (durata.TotalHours >= 1)
                    return $"{durata.Hours}h {durata.Minutes}m";
                else
                    return $"{durata.Minutes}m";
            }
        }

        [NotMapped]
        public string DescrizioneCompleta
        {
            get
            {
                var desc = Titolo;
                if (Cliente != null)
                    desc += $" - {Cliente.NomeCompleto}";
                if (Immobile != null)
                    desc += $" - {Immobile.Titolo}";
                return desc;
            }
        }

        [NotMapped]
        public string StatoColore
        {
            get
            {
                return StatoAppuntamento switch
                {
                    StatiAppuntamento.Programmato => "#2196F3",
                    StatiAppuntamento.Confermato => "#4CAF50",
                    StatiAppuntamento.InCorso => "#FF9800",
                    StatiAppuntamento.Completato => "#8BC34A",
                    StatiAppuntamento.Cancellato => "#F44336",
                    StatiAppuntamento.Rimandato => "#9C27B0",
                    StatiAppuntamento.NonPresentato => "#607D8B",
                    _ => "#2196F3"
                };
            }
        }

        [NotMapped]
        public string PrioritaColore
        {
            get
            {
                return Priorita switch
                {
                    PrioritaAppuntamento.Bassa => "#4CAF50",
                    PrioritaAppuntamento.Media => "#FF9800",
                    PrioritaAppuntamento.Alta => "#F44336",
                    PrioritaAppuntamento.Urgente => "#9C27B0",
                    _ => "#FF9800"
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

        // METODI DI VALIDAZIONE
        public bool IsValid()
        {
            // Controlla campi obbligatori
            if (string.IsNullOrWhiteSpace(Titolo)) return false;
            if (string.IsNullOrWhiteSpace(TipoAppuntamento)) return false;
            if (string.IsNullOrWhiteSpace(StatoAppuntamento)) return false;
            if (string.IsNullOrWhiteSpace(Priorita)) return false;
            if (string.IsNullOrWhiteSpace(Luogo)) return false;
            if (string.IsNullOrWhiteSpace(CreatoDa)) return false;

            // Controlla date logiche
            if (DataInizio >= DataFine) return false;
            if (DataCreazione > DateTime.Now.AddMinutes(5)) return false; // Permette piccolo scarto per timezone

            // Controlla valori validi
            if (!Array.Exists(TipiAppuntamento.All, t => t == TipoAppuntamento)) return false;
            if (!Array.Exists(StatiAppuntamento.All, s => s == StatoAppuntamento)) return false;
            if (!Array.Exists(PrioritaAppuntamento.All, p => p == Priorita)) return false;

            return true;
        }

        public string GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Titolo))
                errors.Add("Il titolo è obbligatorio");

            if (string.IsNullOrWhiteSpace(TipoAppuntamento))
                errors.Add("Il tipo di appuntamento è obbligatorio");
            else if (!Array.Exists(TipiAppuntamento.All, t => t == TipoAppuntamento))
                errors.Add($"Tipo appuntamento '{TipoAppuntamento}' non valido");

            if (string.IsNullOrWhiteSpace(StatoAppuntamento))
                errors.Add("Lo stato dell'appuntamento è obbligatorio");
            else if (!Array.Exists(StatiAppuntamento.All, s => s == StatoAppuntamento))
                errors.Add($"Stato appuntamento '{StatoAppuntamento}' non valido");

            if (string.IsNullOrWhiteSpace(Priorita))
                errors.Add("La priorità è obbligatoria");
            else if (!Array.Exists(PrioritaAppuntamento.All, p => p == Priorita))
                errors.Add($"Priorità '{Priorita}' non valida");

            if (string.IsNullOrWhiteSpace(Luogo))
                errors.Add("Il luogo è obbligatorio");

            if (DataInizio >= DataFine)
                errors.Add("La data di inizio deve essere precedente alla data di fine");

            if (DataCreazione > DateTime.Now.AddMinutes(5))
                errors.Add("La data di creazione non può essere nel futuro");

            return string.Join("; ", errors);
        }

        // FACTORY METHODS per creare appuntamenti corretti
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

        public static Appuntamento CreaPerClienteEImmobile(int clienteId, string nomeCliente, int immobileId, string titoloImmobile, string indirizzo)
        {
            return new Appuntamento
            {
                ClienteId = clienteId,
                ImmobileId = immobileId,
                Titolo = $"Visita con {nomeCliente} - {titoloImmobile}",
                Descrizione = $"Visita dell'immobile {titoloImmobile} con il cliente {nomeCliente}",
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

        // METODI DI UTILITÀ
        public void MarkAsCompleted(string esito = "")
        {
            StatoAppuntamento = StatiAppuntamento.Completato;
            EsitoIncontro = esito;
            DataUltimaModifica = DateTime.Now;
        }

        public void MarkAsConfirmed()
        {
            StatoAppuntamento = StatiAppuntamento.Confermato;
            DataConferma = DateTime.Now;
            DataUltimaModifica = DateTime.Now;
        }

        public void Cancel(string motivo = "")
        {
            StatoAppuntamento = StatiAppuntamento.Cancellato;
            NotePrivate = string.IsNullOrEmpty(NotePrivate) ? motivo : $"{NotePrivate}; {motivo}";
            DataUltimaModifica = DateTime.Now;
        }

        public void Reschedule(DateTime nuovaDataInizio, DateTime nuovaDataFine)
        {
            DataInizio = nuovaDataInizio;
            DataFine = nuovaDataFine;
            StatoAppuntamento = StatiAppuntamento.Rimandato;
            DataUltimaModifica = DateTime.Now;
        }

        // OVERRIDE PER DEBUG
        public override string ToString()
        {
            return $"Appuntamento {Id}: {Titolo} - {DataInizio:dd/MM/yyyy HH:mm} ({StatoAppuntamento})";
        }
    }
}