using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ImmobiGestio.Data;
using ImmobiGestio.Models;

namespace ImmobiGestio.Services
{
    public class StatisticheService
    {
        private readonly ImmobiliContext _context;

        public StatisticheService(ImmobiliContext context)
        {
            _context = context;
        }

        public StatisticheDashboard GetStatisticheDashboard()
        {
            var oggi = DateTime.Today;
            var inizioMese = new DateTime(oggi.Year, oggi.Month, 1);
            var prossimaSettimana = oggi.AddDays(7);

            var statistiche = new StatisticheDashboard
            {
                // Conteggi base
                TotaleImmobili = _context.Immobili.Count(),
                ImmobiliDisponibili = _context.Immobili.Count(i => i.StatoVendita == "Disponibile"),
                ImmobiliVenduti = _context.Immobili.Count(i => i.StatoVendita == "Venduto"),
                TotaleClienti = _context.Clienti.Count(),
                ClientiAttivi = _context.Clienti.Count(c => c.StatoCliente == "Attivo"),
                AppuntamentiOggi = _context.Appuntamenti.Count(a => a.DataInizio.Date == oggi),
                AppuntamentiProssimaSettimana = _context.Appuntamenti
                    .Count(a => a.DataInizio.Date >= oggi && a.DataInizio.Date <= prossimaSettimana),

                // Valori finanziari
                ValoreTotalePortafoglio = _context.Immobili
                    .Where(i => i.StatoVendita == "Disponibile")
                    .Sum(i => i.Prezzo),
                ValoreVenditeUltimoMese = _context.Immobili
                    .Where(i => i.StatoVendita == "Venduto" &&
                               i.DataUltimaModifica >= inizioMese)
                    .Sum(i => i.Prezzo),

                // Grafici e distribuzioni
                VenditeMensili = GetVenditeMensili(),
                DistribuzioneTipologie = GetDistribuzioneTipologie(),
                AttivitaRecenti = GetAttivitaRecenti(),
                ClientiPerTipologie = GetClientiPerTipologie()
            };

            // Calcolo ticket medio
            var venditeUltimoMese = _context.Immobili
                .Count(i => i.StatoVendita == "Venduto" && i.DataUltimaModifica >= inizioMese);

            statistiche.TicketMedio = venditeUltimoMese > 0
                ? statistiche.ValoreVenditeUltimoMese / venditeUltimoMese
                : 0;

            return statistiche;
        }

        private List<VenditeMensili> GetVenditeMensili()
        {
            var ultimi12Mesi = DateTime.Today.AddMonths(-12);

            return _context.Immobili
                .Where(i => i.StatoVendita == "Venduto" &&
                           i.DataUltimaModifica >= ultimi12Mesi)
                .GroupBy(i => new {
                    Anno = i.DataUltimaModifica!.Value.Year,
                    Mese = i.DataUltimaModifica!.Value.Month
                })
                .Select(g => new VenditeMensili
                {
                    Anno = g.Key.Anno,
                    Mese = GetNomeMese(g.Key.Mese),
                    DataMese = new DateTime(g.Key.Anno, g.Key.Mese, 1),
                    Valore = g.Sum(i => i.Prezzo),
                    NumeroVendite = g.Count()
                })
                .OrderBy(v => v.DataMese)
                .ToList();
        }

        private List<TipologiaImmobili> GetDistribuzioneTipologie()
        {
            var totaleImmobili = _context.Immobili.Count();

            var tipologie = _context.Immobili
                .GroupBy(i => i.TipoImmobile)
                .Select(g => new TipologiaImmobili
                {
                    Tipologia = g.Key,
                    Quantita = g.Count(),
                    ValoreTotale = g.Sum(i => i.Prezzo),
                    Percentuale = totaleImmobili > 0 ? (double)g.Count() / totaleImmobili * 100 : 0
                })
                .OrderByDescending(t => t.Quantita)
                .ToList();

            return tipologie;
        }

        private List<AttivitaRecente> GetAttivitaRecenti()
        {
            var attivita = new List<AttivitaRecente>();
            var ultimi7Giorni = DateTime.Today.AddDays(-7);

            // Nuovi clienti
            var nuoviClienti = _context.Clienti
                .Where(c => c.DataInserimento >= ultimi7Giorni)
                .OrderByDescending(c => c.DataInserimento)
                .Take(5)
                .Select(c => new AttivitaRecente
                {
                    Data = c.DataInserimento,
                    Tipo = "Nuovo Cliente",
                    Descrizione = $"Nuovo cliente: {c.NomeCompleto}",
                    Icona = "👤",
                    Colore = "#4CAF50",
                    RelatedId = c.Id
                })
                .ToList();

            // Nuovi immobili
            var nuoviImmobili = _context.Immobili
                .Where(i => i.DataInserimento >= ultimi7Giorni)
                .OrderByDescending(i => i.DataInserimento)
                .Take(5)
                .Select(i => new AttivitaRecente
                {
                    Data = i.DataInserimento,
                    Tipo = "Nuovo Immobile",
                    Descrizione = $"Nuovo immobile: {i.Titolo}",
                    Icona = "🏠",
                    Colore = "#2196F3",
                    RelatedId = i.Id
                })
                .ToList();

            // Appuntamenti completati
            var appuntamentiCompletati = _context.Appuntamenti
                .Include(a => a.Cliente)
                .Where(a => a.StatoAppuntamento == "Completato" &&
                           a.DataUltimaModifica >= ultimi7Giorni)
                .OrderByDescending(a => a.DataUltimaModifica)
                .Take(5)
                .Select(a => new AttivitaRecente
                {
                    Data = a.DataUltimaModifica ?? a.DataCreazione,
                    Tipo = "Appuntamento Completato",
                    Descrizione = $"Completato: {a.Titolo}" +
                                 (a.Cliente != null ? $" con {a.Cliente.NomeCompleto}" : ""),
                    Icona = "✅",
                    Colore = "#FF9800",
                    RelatedId = a.Id
                })
                .ToList();

            // Vendite
            var venditeRecenti = _context.Immobili
                .Where(i => i.StatoVendita == "Venduto" &&
                           i.DataUltimaModifica >= ultimi7Giorni)
                .OrderByDescending(i => i.DataUltimaModifica)
                .Take(3)
                .Select(i => new AttivitaRecente
                {
                    Data = i.DataUltimaModifica ?? i.DataInserimento,
                    Tipo = "Vendita",
                    Descrizione = $"Venduto: {i.Titolo} - € {i.Prezzo:N0}",
                    Icona = "💰",
                    Colore = "#4CAF50",
                    RelatedId = i.Id
                })
                .ToList();

            attivita.AddRange(nuoviClienti);
            attivita.AddRange(nuoviImmobili);
            attivita.AddRange(appuntamentiCompletati);
            attivita.AddRange(venditeRecenti);

            return attivita
                .OrderByDescending(a => a.Data)
                .Take(10)
                .ToList();
        }

        private List<ClientiPerTipologia> GetClientiPerTipologie()
        {
            var totaleClienti = _context.Clienti.Count();

            return _context.Clienti
                .GroupBy(c => c.TipoCliente)
                .Select(g => new ClientiPerTipologia
                {
                    Tipologia = g.Key,
                    Quantita = g.Count(),
                    Percentuale = totaleClienti > 0 ? (double)g.Count() / totaleClienti * 100 : 0
                })
                .OrderByDescending(c => c.Quantita)
                .ToList();
        }

        public List<PerformanceImmobile> GetPerformanceImmobili()
        {
            return _context.Immobili
                .Where(i => i.StatoVendita == "Disponibile")
                .Select(i => new PerformanceImmobile
                {
                    ImmobileId = i.Id,
                    Titolo = i.Titolo,
                    NumeroVisite = _context.Appuntamenti
                        .Count(a => a.ImmobileId == i.Id &&
                                   a.StatoAppuntamento == "Completato"),
                    NumeroInteressati = _context.ClientiImmobili
                        .Count(ci => ci.ImmobileId == i.Id),
                    GiorniSulMercato = (DateTime.Today - i.DataInserimento).Days,
                    Prezzo = i.Prezzo,
                    StatoVendita = i.StatoVendita
                })
                .OrderByDescending(p => p.NumeroVisite)
                .ToList();
        }

        public List<EventoCalendario> GetEventiCalendario(DateTime start, DateTime end)
        {
            return _context.Appuntamenti
                .Include(a => a.Cliente)
                .Include(a => a.Immobile)
                .Where(a => a.DataInizio >= start && a.DataInizio <= end)
                .Select(a => new EventoCalendario
                {
                    Id = a.Id,
                    Titolo = a.Titolo,
                    Inizio = a.DataInizio,
                    Fine = a.DataFine,
                    Colore = a.StatoColore,
                    Tipo = a.TipoAppuntamento,
                    TuttoIlGiorno = false,
                    ClienteNome = a.Cliente != null ? a.Cliente.NomeCompleto : null,
                    ImmobileTitolo = a.Immobile != null ? a.Immobile.Titolo : null
                })
                .ToList();
        }

        public AnalisiMercato GetAnalisiMercato(string? zona = null)
        {
            var query = _context.Immobili.AsQueryable();

            if (!string.IsNullOrEmpty(zona))
            {
                query = query.Where(i => i.Citta == zona);
            }

            var immobiliZona = query.ToList();
            var immobiliDisponibili = immobiliZona.Where(i => i.StatoVendita == "Disponibile").ToList();
            var immobiliVenduti = immobiliZona.Where(i => i.StatoVendita == "Venduto").ToList();

            var prezzoMedioMq = immobiliDisponibili.Any() && immobiliDisponibili.Any(i => i.Superficie > 0)
                ? immobiliDisponibili.Where(i => i.Superficie > 0)
                    .Average(i => i.Prezzo / i.Superficie.Value)
                : 0;

            var tempoMedioVendita = immobiliVenduti.Any() && immobiliVenduti.Any(i => i.DataUltimaModifica.HasValue)
                ? (int)immobiliVenduti.Where(i => i.DataUltimaModifica.HasValue)
                    .Average(i => (i.DataUltimaModifica!.Value - i.DataInserimento).TotalDays)
                : 0;

            return new AnalisiMercato
            {
                Zona = zona ?? "Tutte le zone",
                PrezzoMedioMq = prezzoMedioMq,
                NumeroImmobiliDisponibili = immobiliDisponibili.Count,
                TempoMedioVendita = tempoMedioVendita,
                VariazionePrezzoPeriodo = 0, // Calcolo complesso - da implementare con dati storici
                TendenzaMercato = DeterminaTendenzaMercato(immobiliZona)
            };
        }

        private string DeterminaTendenzaMercato(List<Immobile> immobili)
        {
            var ultimi3Mesi = DateTime.Today.AddMonths(-3);
            var venditeRecenti = immobili.Count(i => i.StatoVendita == "Venduto" &&
                                                    i.DataUltimaModifica >= ultimi3Mesi);
            var totaleVendite = immobili.Count(i => i.StatoVendita == "Venduto");

            if (totaleVendite == 0) return "Stabile";

            var percentualeVenditeRecenti = (double)venditeRecenti / totaleVendite;

            return percentualeVenditeRecenti switch
            {
                > 0.5 => "Crescita",
                < 0.2 => "Calo",
                _ => "Stabile"
            };
        }

        private string GetNomeMese(int mese)
        {
            return mese switch
            {
                1 => "Gen",
                2 => "Feb",
                3 => "Mar",
                4 => "Apr",
                5 => "Mag",
                6 => "Giu",
                7 => "Lug",
                8 => "Ago",
                9 => "Set",
                10 => "Ott",
                11 => "Nov",
                12 => "Dic",
                _ => "N/A"
            };
        }
    }
}