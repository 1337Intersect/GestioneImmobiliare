using System;
using System.Collections.Generic;

namespace ImmobiGestio.Models
{
    public class StatisticheDashboard
    {
        public int TotaleImmobili { get; set; }
        public int ImmobiliDisponibili { get; set; }
        public int ImmobiliVenduti { get; set; }
        public int TotaleClienti { get; set; }
        public int ClientiAttivi { get; set; }
        public int AppuntamentiOggi { get; set; }
        public int AppuntamentiProssimaSettimana { get; set; }
        public decimal ValoreTotalePortafoglio { get; set; }
        public decimal ValoreVenditeUltimoMese { get; set; }
        public decimal TicketMedio { get; set; }

        // Grafici
        public List<VenditeMensili> VenditeMensili { get; set; } = new();
        public List<TipologiaImmobili> DistribuzioneTipologie { get; set; } = new();
        public List<AttivitaRecente> AttivitaRecenti { get; set; } = new();
        public List<ClientiPerTipologia> ClientiPerTipologie { get; set; } = new();

        // Performance
        public double PercentualeVendite => TotaleImmobili > 0 ? (double)ImmobiliVenduti / TotaleImmobili * 100 : 0;
        public double PercentualeClientiAttivi => TotaleClienti > 0 ? (double)ClientiAttivi / TotaleClienti * 100 : 0;
        public int MediaAppuntamentiGiornalieri => (int)Math.Round(AppuntamentiProssimaSettimana / 7.0);
    }

    public class VenditeMensili
    {
        public string Mese { get; set; } = string.Empty;
        public int Anno { get; set; }
        public decimal Valore { get; set; }
        public int NumeroVendite { get; set; }
        public DateTime DataMese { get; set; }
    }

    public class TipologiaImmobili
    {
        public string Tipologia { get; set; } = string.Empty;
        public int Quantita { get; set; }
        public decimal ValoreTotale { get; set; }
        public double Percentuale { get; set; }
    }

    public class AttivitaRecente
    {
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Nuovo Cliente", "Nuovo Immobile", "Appuntamento", "Vendita"
        public string Descrizione { get; set; } = string.Empty;
        public string Icona { get; set; } = string.Empty;
        public string Colore { get; set; } = string.Empty;
        public int? RelatedId { get; set; }
    }

    public class ClientiPerTipologia
    {
        public string Tipologia { get; set; } = string.Empty;
        public int Quantita { get; set; }
        public double Percentuale { get; set; }
    }

    // Modelli per performance e analisi
    public class PerformanceImmobile
    {
        public int ImmobileId { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public int NumeroVisite { get; set; }
        public int NumeroInteressati { get; set; }
        public int GiorniSulMercato { get; set; }
        public decimal Prezzo { get; set; }
        public string StatoVendita { get; set; } = string.Empty;
        public double TassoInteresse => NumeroVisite > 0 ? (double)NumeroInteressati / NumeroVisite * 100 : 0;
    }

    public class PerformanceAgente
    {
        public string NomeAgente { get; set; } = string.Empty;
        public int NumeroClienti { get; set; }
        public int NumeroAppuntamenti { get; set; }
        public int NumeroVendite { get; set; }
        public decimal ValoreVendite { get; set; }
        public double TassoChiusura => NumeroClienti > 0 ? (double)NumeroVendite / NumeroClienti * 100 : 0;
    }

    // Modelli per il calendario
    public class EventoCalendario
    {
        public int Id { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public DateTime Inizio { get; set; }
        public DateTime Fine { get; set; }
        public string Colore { get; set; } = "#2196F3";
        public string Tipo { get; set; } = string.Empty;
        public bool TuttoIlGiorno { get; set; }
        public string? ClienteNome { get; set; }
        public string? ImmobileTitolo { get; set; }
    }

    // Modelli per obiettivi e KPI
    public class ObiettivoPeriodo
    {
        public int Id { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public decimal Target { get; set; }
        public decimal Realizzato { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public string Tipologia { get; set; } = string.Empty; // Vendite, Clienti, Appuntamenti
        public double PercentualeRaggiungimento => Target > 0 ? (double)(Realizzato / Target) * 100 : 0;
        public bool IsRaggiunto => Realizzato >= Target;
    }

    // Modelli per analisi di mercato
    public class AnalisiMercato
    {
        public string Zona { get; set; } = string.Empty;
        public decimal PrezzoMedioMq { get; set; }
        public int NumeroImmobiliDisponibili { get; set; }
        public int TempoMedioVendita { get; set; } // in giorni
        public double VariazionePrezzoPeriodo { get; set; } // percentuale
        public string TendenzaMercato { get; set; } = string.Empty; // "Crescita", "Stabile", "Calo"
    }
}