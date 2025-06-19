using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;

namespace ImmobiGestio.Models
{
    // COSTANTI ITALIANE per Cliente
    public static class TipiCliente
    {
        public const string Acquirente = "Acquirente";
        public const string Venditore = "Venditore";
        public const string Locatario = "Locatario";
        public const string Locatore = "Locatore";
        public const string Investitore = "Investitore";

        public static readonly string[] All = { Acquirente, Venditore, Locatario, Locatore, Investitore };
    }

    public static class StatiCliente
    {
        public const string Attivo = "Attivo";
        public const string Inattivo = "Inattivo";
        public const string Prospect = "Prospect";
        public const string Concluso = "Concluso";
        public const string Sospeso = "Sospeso";

        public static readonly string[] All = { Attivo, Inattivo, Prospect, Concluso, Sospeso };
    }

    public static class FontiContatto
    {
        public const string Web = "Web";
        public const string Telefono = "Telefono";
        public const string Email = "Email";
        public const string Referral = "Referral";
        public const string PassaParola = "Passa Parola";
        public const string Pubblicita = "Pubblicità";
        public const string SocialMedia = "Social Media";
        public const string Fiera = "Fiera/Evento";
        public const string Altro = "Altro";

        public static readonly string[] All = { Web, Telefono, Email, Referral, PassaParola, Pubblicita, SocialMedia, Fiera, Altro };
    }

    // PROVINCE ITALIANE più comuni nel settore immobiliare
    public static class ProvinceItaliane
    {
        public static readonly string[] All = {
            "AG", "AL", "AN", "AO", "AR", "AP", "AT", "AV", "BA", "BT", "BL", "BN", "BG", "BI", "BO", "BZ", "BS", "BR",
            "CA", "CL", "CB", "CI", "CE", "CT", "CZ", "CH", "CO", "CS", "CR", "KR", "CN", "EN", "FM", "FE", "FI", "FG",
            "FC", "FR", "GE", "GO", "GR", "IM", "IS", "SP", "AQ", "LT", "LE", "LC", "LI", "LO", "LU", "MC", "MN", "MS",
            "MT", "VS", "ME", "MI", "MO", "MB", "NA", "NO", "NU", "OG", "OT", "OR", "PD", "PA", "PR", "PV", "PG", "PU",
            "PE", "PC", "PI", "PT", "PN", "PZ", "PO", "RG", "RA", "RC", "RE", "RI", "RN", "RM", "RO", "SA", "SS", "SV",
            "SI", "SR", "SO", "TA", "TE", "TR", "TO", "TP", "TN", "TV", "TS", "UD", "VA", "VE", "VB", "VC", "VR", "VV", "VI", "VT"
        };
    }

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
        public string TipoCliente { get; set; } = TipiCliente.Acquirente;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BudgetMin { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BudgetMax { get; set; } = 0;

        [StringLength(500)]
        public string PreferenzeTipologia { get; set; } = string.Empty;

        [StringLength(500)]
        public string PreferenzeZone { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Note { get; set; } = string.Empty;

        [StringLength(50)]
        public string StatoCliente { get; set; } = StatiCliente.Attivo;

        public DateTime DataInserimento { get; set; } = DateTime.Now;
        public DateTime? DataUltimaModifica { get; set; }
        public DateTime? DataUltimoContatto { get; set; }

        [StringLength(100)]
        public string FonteContatto { get; set; } = string.Empty;

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
        public string BudgetFormattato
        {
            get
            {
                if (BudgetMax > 0 && BudgetMin > 0)
                {
                    if (BudgetMax == BudgetMin)
                        return $"€ {BudgetMin:N0}";
                    else
                        return $"€ {BudgetMin:N0} - € {BudgetMax:N0}";
                }
                else if (BudgetMin > 0)
                    return $"Da € {BudgetMin:N0}";
                else if (BudgetMax > 0)
                    return $"Fino a € {BudgetMax:N0}";
                else
                    return "Budget non specificato";
            }
        }

        [NotMapped]
        public string ContattoCompleto
        {
            get
            {
                var contatti = new List<string>();
                if (!string.IsNullOrEmpty(Telefono)) contatti.Add($"Tel: {Telefono}");
                if (!string.IsNullOrEmpty(Cellulare)) contatti.Add($"Cell: {Cellulare}");
                if (!string.IsNullOrEmpty(Email)) contatti.Add($"Email: {Email}");
                return string.Join(" | ", contatti);
            }
        }

        [NotMapped]
        public string ContattoPreferito
        {
            get
            {
                if (!string.IsNullOrEmpty(Cellulare)) return Cellulare;
                if (!string.IsNullOrEmpty(Telefono)) return Telefono;
                if (!string.IsNullOrEmpty(Email)) return Email;
                return "Nessun contatto";
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

        [NotMapped]
        public string EtaFormattata => $"{Eta} anni";

        [NotMapped]
        public string DataNascitaFormattata => DataNascita.ToString("dd/MM/yyyy");

        [NotMapped]
        public string DataInserimentoFormattata => DataInserimento.ToString("dd/MM/yyyy");

        [NotMapped]
        public string DataUltimaModificaFormattata => DataUltimaModifica?.ToString("dd/MM/yyyy HH:mm") ?? "Mai modificato";

        [NotMapped]
        public string DataUltimoContattoFormattata => DataUltimoContatto?.ToString("dd/MM/yyyy") ?? "Nessun contatto";

        [NotMapped]
        public bool HasValidCodiceFiscale => IsValidCodiceFiscale(CodiceFiscale);

        [NotMapped]
        public bool HasValidEmail => IsValidEmail(Email);

        [NotMapped]
        public bool HasValidCAP => IsValidItalianCAP(CAP);

        [NotMapped]
        public bool HasValidPhone => IsValidItalianPhone(Telefono) || IsValidItalianPhone(Cellulare);

        [NotMapped]
        public string StatoColore
        {
            get
            {
                return StatoCliente switch
                {
                    StatiCliente.Attivo => "#4CAF50",
                    StatiCliente.Prospect => "#2196F3",
                    StatiCliente.Concluso => "#8BC34A",
                    StatiCliente.Inattivo => "#9E9E9E",
                    StatiCliente.Sospeso => "#FF9800",
                    _ => "#9E9E9E"
                };
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
            StatoCliente = StatiCliente.Attivo;
            TipoCliente = TipiCliente.Acquirente;
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

        // METODI DI VALIDAZIONE ITALIANI
        public static bool IsValidCodiceFiscale(string codiceFiscale)
        {
            if (string.IsNullOrWhiteSpace(codiceFiscale) || codiceFiscale.Length != 16)
                return false;

            codiceFiscale = codiceFiscale.ToUpper();

            // Controllo caratteri validi
            if (!Regex.IsMatch(codiceFiscale, @"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$"))
                return false;

            // Controllo carattere di controllo
            const string oddChars = "BAFHJNPRTVCESULDGIMOQKWZYX";
            const string evenChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            int sum = 0;
            for (int i = 0; i < 15; i++)
            {
                char c = codiceFiscale[i];
                int value = char.IsLetter(c) ? c - 'A' : c - '0' + 26;

                if (i % 2 == 0) // posizione dispari (1-based)
                    sum += oddChars.IndexOf(char.IsLetter(c) ? c : (char)('A' + (c - '0')));
                else // posizione pari
                    sum += char.IsLetter(c) ? c - 'A' : c - '0';
            }

            char checkChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[sum % 26];
            return codiceFiscale[15] == checkChar;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true; // Email non obbligatoria

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidItalianCAP(string cap)
        {
            if (string.IsNullOrWhiteSpace(cap))
                return true; // CAP non obbligatorio

            return cap.Length == 5 && cap.All(char.IsDigit);
        }

        public static bool IsValidItalianPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // Telefono non obbligatorio

            // Rimuovi spazi, trattini e parentesi
            var cleaned = Regex.Replace(phone, @"[\s\-\(\)]", "");

            // Controllo pattern italiani
            return Regex.IsMatch(cleaned, @"^(\+39)?[0-9]{6,11}$") &&
                   (cleaned.StartsWith("+39") || cleaned.StartsWith("0") || cleaned.StartsWith("3"));
        }

        public static bool IsValidItalianProvince(string provincia)
        {
            if (string.IsNullOrWhiteSpace(provincia))
                return true; // Provincia non obbligatoria

            return ProvinceItaliane.All.Contains(provincia.ToUpper());
        }

        // METODI DI VALIDAZIONE COMPLETA
        public bool IsValid()
        {
            // Campi obbligatori
            if (string.IsNullOrWhiteSpace(Nome)) return false;
            if (string.IsNullOrWhiteSpace(Cognome)) return false;

            // Validazioni specifiche
            if (!string.IsNullOrEmpty(CodiceFiscale) && !IsValidCodiceFiscale(CodiceFiscale)) return false;
            if (!string.IsNullOrEmpty(Email) && !IsValidEmail(Email)) return false;
            if (!string.IsNullOrEmpty(CAP) && !IsValidItalianCAP(CAP)) return false;
            if (!string.IsNullOrEmpty(Telefono) && !IsValidItalianPhone(Telefono)) return false;
            if (!string.IsNullOrEmpty(Cellulare) && !IsValidItalianPhone(Cellulare)) return false;
            if (!string.IsNullOrEmpty(Provincia) && !IsValidItalianProvince(Provincia)) return false;

            // Validazioni logiche
            if (BudgetMin < 0 || BudgetMax < 0) return false;
            if (BudgetMax > 0 && BudgetMin > BudgetMax) return false;
            if (DataNascita > DateTime.Today) return false;
            if (DataNascita < DateTime.Today.AddYears(-120)) return false; // Età massima ragionevole

            // Validazioni enum
            if (!Array.Exists(TipiCliente.All, t => t == TipoCliente)) return false;
            if (!Array.Exists(StatiCliente.All, s => s == StatoCliente)) return false;
            if (!string.IsNullOrEmpty(FonteContatto) && !Array.Exists(FontiContatto.All, f => f == FonteContatto)) return false;

            return true;
        }

        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            // Campi obbligatori
            if (string.IsNullOrWhiteSpace(Nome))
                errors.Add("Il nome è obbligatorio");

            if (string.IsNullOrWhiteSpace(Cognome))
                errors.Add("Il cognome è obbligatorio");

            // Validazioni specifiche italiane
            if (!string.IsNullOrEmpty(CodiceFiscale) && !IsValidCodiceFiscale(CodiceFiscale))
                errors.Add("Il codice fiscale non è valido");

            if (!string.IsNullOrEmpty(Email) && !IsValidEmail(Email))
                errors.Add("L'indirizzo email non è valido");

            if (!string.IsNullOrEmpty(CAP) && !IsValidItalianCAP(CAP))
                errors.Add("Il CAP deve essere di 5 cifre");

            if (!string.IsNullOrEmpty(Telefono) && !IsValidItalianPhone(Telefono))
                errors.Add("Il numero di telefono non è valido per l'Italia");

            if (!string.IsNullOrEmpty(Cellulare) && !IsValidItalianPhone(Cellulare))
                errors.Add("Il numero di cellulare non è valido per l'Italia");

            if (!string.IsNullOrEmpty(Provincia) && !IsValidItalianProvince(Provincia))
                errors.Add("La provincia deve essere una sigla italiana valida (es. MI, RM, NA)");

            // Validazioni logiche
            if (BudgetMin < 0)
                errors.Add("Il budget minimo non può essere negativo");

            if (BudgetMax < 0)
                errors.Add("Il budget massimo non può essere negativo");

            if (BudgetMax > 0 && BudgetMin > BudgetMax)
                errors.Add("Il budget minimo non può essere maggiore del budget massimo");

            if (DataNascita > DateTime.Today)
                errors.Add("La data di nascita non può essere nel futuro");

            if (DataNascita < DateTime.Today.AddYears(-120))
                errors.Add("La data di nascita non è realistica");

            // Validazioni enum
            if (!Array.Exists(TipiCliente.All, t => t == TipoCliente))
                errors.Add($"Tipo cliente '{TipoCliente}' non valido");

            if (!Array.Exists(StatiCliente.All, s => s == StatoCliente))
                errors.Add($"Stato cliente '{StatoCliente}' non valido");

            if (!string.IsNullOrEmpty(FonteContatto) && !Array.Exists(FontiContatto.All, f => f == FonteContatto))
                errors.Add($"Fonte contatto '{FonteContatto}' non valida");

            return errors;
        }

        // METODI DI UTILITÀ
        public void MarkAsContacted()
        {
            DataUltimoContatto = DateTime.Now;
            DataUltimaModifica = DateTime.Now;
        }

        public void UpdateStatus(string newStatus)
        {
            if (Array.Exists(StatiCliente.All, s => s == newStatus))
            {
                StatoCliente = newStatus;
                DataUltimaModifica = DateTime.Now;
            }
        }

        public void AddNote(string newNote)
        {
            if (!string.IsNullOrWhiteSpace(newNote))
            {
                var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                var separator = string.IsNullOrEmpty(Note) ? "" : "\n\n";
                Note += $"{separator}[{timestamp}] {newNote}";
                DataUltimaModifica = DateTime.Now;
            }
        }

        // OVERRIDE PER DEBUG
        public override string ToString()
        {
            return $"Cliente {Id}: {NomeCompleto} - {StatoCliente} ({TipoCliente})";
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

        [NotMapped]
        public string DataInteresseFormattata => DataInteresse.ToString("dd/MM/yyyy");

        [NotMapped]
        public string DataOffertaFormattata => DataOfferta?.ToString("dd/MM/yyyy") ?? "Nessuna offerta";

        [NotMapped]
        public string OffertaFormattata => OffertaProposta?.ToString("C0") ?? "Nessuna offerta";

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