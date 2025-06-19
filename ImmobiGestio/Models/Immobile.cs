using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;

namespace ImmobiGestio.Models
{
    // COSTANTI ITALIANE per Immobili
    public static class TipiImmobile
    {
        public const string Appartamento = "Appartamento";
        public const string Villa = "Villa";
        public const string VillettaSchiera = "Villetta a schiera";
        public const string Attico = "Attico";
        public const string Penthouse = "Penthouse";
        public const string Loft = "Loft";
        public const string CasaIndipendente = "Casa indipendente";
        public const string Rustico = "Rustico";
        public const string Casale = "Casale";
        public const string Palazzo = "Palazzo";
        public const string Mansarda = "Mansarda";
        public const string Monolocale = "Monolocale";
        public const string Bilocale = "Bilocale";
        public const string Trilocale = "Trilocale";
        public const string Quadrilocale = "Quadrilocale";
        public const string Ufficio = "Ufficio";
        public const string Negozio = "Negozio";
        public const string Capannone = "Capannone";
        public const string Terreno = "Terreno";
        public const string BoxGarage = "Box/Garage";
        public const string Altro = "Altro";

        public static readonly string[] All = {
            Appartamento, Villa, VillettaSchiera, Attico, Penthouse, Loft, CasaIndipendente,
            Rustico, Casale, Palazzo, Mansarda, Monolocale, Bilocale, Trilocale, Quadrilocale,
            Ufficio, Negozio, Capannone, Terreno, BoxGarage, Altro
        };
    }

    public static class StatiConservazione
    {
        public const string NuovoCostruito = "Nuovo/Appena costruito";
        public const string Ottimo = "Ottimo";
        public const string Buono = "Buono";
        public const string Discreto = "Discreto";
        public const string DaRistrutturare = "Da ristrutturare";
        public const string DaRistrutturareCompletamente = "Da ristrutturare completamente";
        public const string InCostruzione = "In costruzione";
        public const string Grezzo = "Grezzo";

        public static readonly string[] All = {
            NuovoCostruito, Ottimo, Buono, Discreto, DaRistrutturare,
            DaRistrutturareCompletamente, InCostruzione, Grezzo
        };
    }

    public static class ClassiEnergetiche
    {
        public const string A4 = "A4";
        public const string A3 = "A3";
        public const string A2 = "A2";
        public const string A1 = "A1";
        public const string B = "B";
        public const string C = "C";
        public const string D = "D";
        public const string E = "E";
        public const string F = "F";
        public const string G = "G";
        public const string Esente = "Esente";

        public static readonly string[] All = { A4, A3, A2, A1, B, C, D, E, F, G, Esente };
    }

    public static class StatiVendita
    {
        public const string Disponibile = "Disponibile";
        public const string InTrattativa = "In trattativa";
        public const string Prenotato = "Prenotato";
        public const string Compromesso = "Compromesso";
        public const string Venduto = "Venduto";
        public const string RitiratoMercato = "Ritirato dal mercato";
        public const string Sospeso = "Sospeso";

        public static readonly string[] All = {
            Disponibile, InTrattativa, Prenotato, Compromesso, Venduto, RitiratoMercato, Sospeso
        };
    }

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

        [Required]
        [StringLength(50)]
        public string TipoImmobile { get; set; } = TipiImmobile.Appartamento;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Prezzo { get; set; } = 0;

        public int? Superficie { get; set; } // metri quadri

        public int? NumeroLocali { get; set; }

        public int? NumeroBagni { get; set; }

        public int? Piano { get; set; } // Piano dell'immobile (0 = terra, -1 = seminterrato, etc.)

        [StringLength(2000)]
        public string Descrizione { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string StatoConservazione { get; set; } = StatiConservazione.Buono;

        [Required]
        [StringLength(50)]
        public string ClasseEnergetica { get; set; } = ClassiEnergetiche.G;

        [Required]
        [StringLength(50)]
        public string StatoVendita { get; set; } = StatiVendita.Disponibile;

        [Required]
        public DateTime DataInserimento { get; set; } = DateTime.Now;

        public DateTime? DataUltimaModifica { get; set; }

        // Proprietà aggiuntive per il mercato italiano
        public bool HasTerrazzo { get; set; } = false;
        public bool HasGiardino { get; set; } = false;
        public bool HasBox { get; set; } = false;
        public bool HasCantina { get; set; } = false;
        public bool HasAscensore { get; set; } = false;
        public bool HasRiscaldamentoAutonomo { get; set; } = false;
        public bool HasAriaCondizionata { get; set; } = false;
        public bool HasCamino { get; set; } = false;
        public bool HasPiscina { get; set; } = false;
        public bool HasPortineria { get; set; } = false;

        [StringLength(100)]
        public string TipoRiscaldamento { get; set; } = string.Empty; // Autonomo, Centralizzato, Radiatori, Pavimento

        [StringLength(100)]
        public string Orientamento { get; set; } = string.Empty; // Nord, Sud, Est, Ovest, Sud-Est, etc.

        public int? AnnoCostruzione { get; set; }

        [StringLength(100)]
        public string TipoProprietà { get; set; } = "Piena proprietà"; // Piena proprietà, Nuda proprietà, Usufrutto

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SpeseCondominiali { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? IPU { get; set; } = 0; // Imposta su immobili (ex IMU)

        [StringLength(50)]
        public string CodiceImmobile { get; set; } = string.Empty; // Codice identificativo interno

        // Proprietà calcolate
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

        [NotMapped]
        public string DescrizioneBreve
        {
            get
            {
                if (string.IsNullOrEmpty(Descrizione)) return "";
                return Descrizione.Length > 100 ? Descrizione.Substring(0, 100) + "..." : Descrizione;
            }
        }

        [NotMapped]
        public string CaratteristichePrincipali
        {
            get
            {
                var caratteristiche = new List<string>();

                if (Superficie.HasValue) caratteristiche.Add($"{Superficie} mq");
                if (NumeroLocali.HasValue) caratteristiche.Add($"{NumeroLocali} locali");
                if (NumeroBagni.HasValue) caratteristiche.Add($"{NumeroBagni} bagni");
                if (Piano.HasValue)
                {
                    if (Piano == 0) caratteristiche.Add("Piano terra");
                    else if (Piano > 0) caratteristiche.Add($"{Piano}° piano");
                    else caratteristiche.Add("Seminterrato");
                }

                return string.Join(" • ", caratteristiche);
            }
        }

        [NotMapped]
        public string CaratteristicheExtra
        {
            get
            {
                var extra = new List<string>();

                if (HasTerrazzo) extra.Add("Terrazzo");
                if (HasGiardino) extra.Add("Giardino");
                if (HasBox) extra.Add("Box/Garage");
                if (HasCantina) extra.Add("Cantina");
                if (HasAscensore) extra.Add("Ascensore");
                if (HasRiscaldamentoAutonomo) extra.Add("Riscaldamento autonomo");
                if (HasAriaCondizionata) extra.Add("Aria condizionata");
                if (HasCamino) extra.Add("Camino");
                if (HasPiscina) extra.Add("Piscina");
                if (HasPortineria) extra.Add("Portineria");

                return string.Join(" • ", extra);
            }
        }

        [NotMapped]
        public string PrezzoAlMetro
        {
            get
            {
                if (Superficie.HasValue && Superficie > 0 && Prezzo > 0)
                {
                    var prezzoMq = Prezzo / Superficie.Value;
                    return $"€ {prezzoMq:N0}/mq";
                }
                return "N/D";
            }
        }

        [NotMapped]
        public string DataInserimentoFormattata => DataInserimento.ToString("dd/MM/yyyy");

        [NotMapped]
        public string DataUltimaModificaFormattata => DataUltimaModifica?.ToString("dd/MM/yyyy HH:mm") ?? "Mai modificato";

        [NotMapped]
        public int GiorniSulMercato => (DateTime.Now - DataInserimento).Days;

        [NotMapped]
        public string GiorniSulMercatoFormattato
        {
            get
            {
                var giorni = GiorniSulMercato;
                if (giorni == 0) return "Oggi";
                if (giorni == 1) return "1 giorno";
                if (giorni < 30) return $"{giorni} giorni";
                if (giorni < 365) return $"{giorni / 30} mesi";
                return $"{giorni / 365} anni";
            }
        }

        [NotMapped]
        public string StatoVenditaColore
        {
            get
            {
                return StatoVendita switch
                {
                    StatiVendita.Disponibile => "#4CAF50",
                    StatiVendita.InTrattativa => "#2196F3",
                    StatiVendita.Prenotato => "#FF9800",
                    StatiVendita.Compromesso => "#9C27B0",
                    StatiVendita.Venduto => "#8BC34A",
                    StatiVendita.RitiratoMercato => "#9E9E9E",
                    StatiVendita.Sospeso => "#F44336",
                    _ => "#9E9E9E"
                };
            }
        }

        [NotMapped]
        public string ClasseEnergeticaColore
        {
            get
            {
                return ClasseEnergetica switch
                {
                    ClassiEnergetiche.A4 => "#1B5E20",
                    ClassiEnergetiche.A3 => "#2E7D32",
                    ClassiEnergetiche.A2 => "#388E3C",
                    ClassiEnergetiche.A1 => "#43A047",
                    ClassiEnergetiche.B => "#66BB6A",
                    ClassiEnergetiche.C => "#8BC34A",
                    ClassiEnergetiche.D => "#CDDC39",
                    ClassiEnergetiche.E => "#FFEB3B",
                    ClassiEnergetiche.F => "#FF9800",
                    ClassiEnergetiche.G => "#F44336",
                    ClassiEnergetiche.Esente => "#9E9E9E",
                    _ => "#9E9E9E"
                };
            }
        }

        [NotMapped]
        public bool IsDisponibile => StatoVendita == StatiVendita.Disponibile;

        [NotMapped]
        public bool IsVenduto => StatoVendita == StatiVendita.Venduto;

        [NotMapped]
        public bool HasValidCAP => IsValidItalianCAP(CAP);

        [NotMapped]
        public bool HasValidProvincia => IsValidItalianProvincia(Provincia);

        // Relazioni
        public virtual ICollection<DocumentoImmobile> Documenti { get; set; }
        public virtual ICollection<FotoImmobile> Foto { get; set; }

        public Immobile()
        {
            Documenti = new HashSet<DocumentoImmobile>();
            Foto = new HashSet<FotoImmobile>();
            DataInserimento = DateTime.Now;
            StatoVendita = StatiVendita.Disponibile;
            TipoImmobile = TipiImmobile.Appartamento;
            StatoConservazione = StatiConservazione.Buono;
            ClasseEnergetica = ClassiEnergetiche.G;

            // Inizializza le stringhe per evitare null
            Titolo = string.Empty;
            Indirizzo = string.Empty;
            Citta = string.Empty;
            CAP = string.Empty;
            Provincia = string.Empty;
            Descrizione = string.Empty;
            TipoRiscaldamento = string.Empty;
            Orientamento = string.Empty;
            TipoProprietà = "Piena proprietà";
            CodiceImmobile = string.Empty;
        }

        // METODI DI VALIDAZIONE ITALIANI
        public static bool IsValidItalianCAP(string cap)
        {
            if (string.IsNullOrWhiteSpace(cap))
                return true; // CAP non obbligatorio

            return cap.Length == 5 && cap.All(char.IsDigit);
        }

        public static bool IsValidItalianProvincia(string provincia)
        {
            if (string.IsNullOrWhiteSpace(provincia))
                return true; // Provincia non obbligatoria

            return ProvinceItaliane.All.Contains(provincia.ToUpper());
        }

        public static bool IsValidAnnoCostruzione(int? anno)
        {
            if (!anno.HasValue)
                return true; // Anno non obbligatorio

            return anno >= 1000 && anno <= DateTime.Now.Year + 5; // Permette costruzioni future pianificate
        }

        public static bool IsValidSuperficie(int? superficie)
        {
            if (!superficie.HasValue)
                return true; // Superficie non obbligatoria

            return superficie > 0 && superficie <= 10000; // Limite ragionevole
        }

        public static bool IsValidPrezzo(decimal prezzo)
        {
            return prezzo >= 0 && prezzo <= 999999999; // Limite ragionevole
        }

        // METODI DI VALIDAZIONE COMPLETA
        public bool IsValid()
        {
            // Campi obbligatori
            if (string.IsNullOrWhiteSpace(Titolo)) return false;
            if (string.IsNullOrWhiteSpace(Indirizzo)) return false;

            // Validazioni specifiche
            if (!string.IsNullOrEmpty(CAP) && !IsValidItalianCAP(CAP)) return false;
            if (!string.IsNullOrEmpty(Provincia) && !IsValidItalianProvincia(Provincia)) return false;
            if (!IsValidAnnoCostruzione(AnnoCostruzione)) return false;
            if (!IsValidSuperficie(Superficie)) return false;
            if (!IsValidPrezzo(Prezzo)) return false;

            // Validazioni logiche
            if (NumeroLocali.HasValue && NumeroLocali < 0) return false;
            if (NumeroBagni.HasValue && NumeroBagni < 0) return false;
            if (SpeseCondominiali.HasValue && SpeseCondominiali < 0) return false;
            if (IPU.HasValue && IPU < 0) return false;

            // Validazioni enum
            if (!Array.Exists(TipiImmobile.All, t => t == TipoImmobile)) return false;
            if (!Array.Exists(StatiConservazione.All, s => s == StatoConservazione)) return false;
            if (!Array.Exists(ClassiEnergetiche.All, c => c == ClasseEnergetica)) return false;
            if (!Array.Exists(StatiVendita.All, s => s == StatoVendita)) return false;

            return true;
        }

        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            // Campi obbligatori
            if (string.IsNullOrWhiteSpace(Titolo))
                errors.Add("Il titolo è obbligatorio");

            if (string.IsNullOrWhiteSpace(Indirizzo))
                errors.Add("L'indirizzo è obbligatorio");

            // Validazioni specifiche italiane
            if (!string.IsNullOrEmpty(CAP) && !IsValidItalianCAP(CAP))
                errors.Add("Il CAP deve essere di 5 cifre");

            if (!string.IsNullOrEmpty(Provincia) && !IsValidItalianProvincia(Provincia))
                errors.Add("La provincia deve essere una sigla italiana valida (es. MI, RM, NA)");

            if (!IsValidAnnoCostruzione(AnnoCostruzione))
                errors.Add("L'anno di costruzione non è valido");

            if (!IsValidSuperficie(Superficie))
                errors.Add("La superficie deve essere un valore positivo ragionevole");

            if (!IsValidPrezzo(Prezzo))
                errors.Add("Il prezzo deve essere un valore positivo ragionevole");

            // Validazioni logiche
            if (NumeroLocali.HasValue && NumeroLocali < 0)
                errors.Add("Il numero di locali non può essere negativo");

            if (NumeroBagni.HasValue && NumeroBagni < 0)
                errors.Add("Il numero di bagni non può essere negativo");

            if (SpeseCondominiali.HasValue && SpeseCondominiali < 0)
                errors.Add("Le spese condominiali non possono essere negative");

            if (IPU.HasValue && IPU < 0)
                errors.Add("L'IPU non può essere negativa");

            // Validazioni enum
            if (!Array.Exists(TipiImmobile.All, t => t == TipoImmobile))
                errors.Add($"Tipo immobile '{TipoImmobile}' non valido");

            if (!Array.Exists(StatiConservazione.All, s => s == StatoConservazione))
                errors.Add($"Stato conservazione '{StatoConservazione}' non valido");

            if (!Array.Exists(ClassiEnergetiche.All, c => c == ClasseEnergetica))
                errors.Add($"Classe energetica '{ClasseEnergetica}' non valida");

            if (!Array.Exists(StatiVendita.All, s => s == StatoVendita))
                errors.Add($"Stato vendita '{StatoVendita}' non valido");

            return errors;
        }

        // METODI DI UTILITÀ
        public void MarkAsVenduto(decimal prezzoVendita = 0)
        {
            StatoVendita = StatiVendita.Venduto;
            if (prezzoVendita > 0)
                Prezzo = prezzoVendita;
            DataUltimaModifica = DateTime.Now;
        }

        public void MarkAsPrenotato()
        {
            StatoVendita = StatiVendita.Prenotato;
            DataUltimaModifica = DateTime.Now;
        }

        public void SetCompromesso()
        {
            StatoVendita = StatiVendita.Compromesso;
            DataUltimaModifica = DateTime.Now;
        }

        public void RitiraDalMercato(string motivo = "")
        {
            StatoVendita = StatiVendita.RitiratoMercato;
            if (!string.IsNullOrEmpty(motivo))
            {
                var separator = string.IsNullOrEmpty(Descrizione) ? "" : "\n\n";
                Descrizione += $"{separator}[Ritirato il {DateTime.Now:dd/MM/yyyy}] {motivo}";
            }
            DataUltimaModifica = DateTime.Now;
        }

        public void UpdatePrezzo(decimal nuovoPrezzo)
        {
            if (nuovoPrezzo >= 0)
            {
                Prezzo = nuovoPrezzo;
                DataUltimaModifica = DateTime.Now;
            }
        }

        public void GenerateCodiceImmobile()
        {
            if (string.IsNullOrEmpty(CodiceImmobile))
            {
                var tipoCodice = TipoImmobile.Substring(0, Math.Min(3, TipoImmobile.Length)).ToUpper();
                var cittaCodice = string.IsNullOrEmpty(Citta) ? "XXX" : Citta.Substring(0, Math.Min(3, Citta.Length)).ToUpper();
                var timestamp = DateTime.Now.ToString("yyyyMMdd");
                CodiceImmobile = $"{tipoCodice}-{cittaCodice}-{timestamp}-{Id:D4}";
            }
        }

        // OVERRIDE PER DEBUG
        public override string ToString()
        {
            return $"Immobile {Id}: {Titolo} - {IndirizzoCompleto} ({StatoVendita}) - {PrezzoFormattato}";
        }
    }

    // MODELLI CORRELATI
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