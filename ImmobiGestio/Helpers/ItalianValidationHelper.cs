using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ImmobiGestio.Helpers
{
    /// <summary>
    /// Helper per validazioni specifiche italiane
    /// </summary>
    public static class ItalianValidationHelper
    {
        // Province italiane con relative regioni
        public static readonly Dictionary<string, string> ProvinceItaliane = new()
        {
            // Piemonte
            {"TO", "Piemonte"}, {"VC", "Piemonte"}, {"NO", "Piemonte"}, {"CN", "Piemonte"},
            {"AT", "Piemonte"}, {"AL", "Piemonte"}, {"BI", "Piemonte"}, {"VB", "Piemonte"},
            
            // Valle d'Aosta
            {"AO", "Valle d'Aosta"},
            
            // Lombardia
            {"MI", "Lombardia"}, {"BG", "Lombardia"}, {"BS", "Lombardia"}, {"PV", "Lombardia"},
            {"CR", "Lombardia"}, {"MN", "Lombardia"}, {"CO", "Lombardia"}, {"VA", "Lombardia"},
            {"SO", "Lombardia"}, {"LC", "Lombardia"}, {"LO", "Lombardia"}, {"MB", "Lombardia"},
            
            // Trentino-Alto Adige
            {"TN", "Trentino-Alto Adige"}, {"BZ", "Trentino-Alto Adige"},
            
            // Veneto
            {"VE", "Veneto"}, {"VR", "Veneto"}, {"VI", "Veneto"}, {"BL", "Veneto"},
            {"PD", "Veneto"}, {"TV", "Veneto"}, {"RO", "Veneto"},
            
            // Friuli-Venezia Giulia
            {"UD", "Friuli-Venezia Giulia"}, {"GO", "Friuli-Venezia Giulia"},
            {"TS", "Friuli-Venezia Giulia"}, {"PN", "Friuli-Venezia Giulia"},
            
            // Liguria
            {"GE", "Liguria"}, {"SP", "Liguria"}, {"SV", "Liguria"}, {"IM", "Liguria"},
            
            // Emilia-Romagna
            {"BO", "Emilia-Romagna"}, {"FE", "Emilia-Romagna"}, {"FC", "Emilia-Romagna"},
            {"MO", "Emilia-Romagna"}, {"PR", "Emilia-Romagna"}, {"RA", "Emilia-Romagna"},
            {"RE", "Emilia-Romagna"}, {"RN", "Emilia-Romagna"}, {"PC", "Emilia-Romagna"},
            
            // Toscana
            {"FI", "Toscana"}, {"AR", "Toscana"}, {"SI", "Toscana"}, {"GR", "Toscana"},
            {"LI", "Toscana"}, {"LU", "Toscana"}, {"MS", "Toscana"}, {"PI", "Toscana"},
            {"PT", "Toscana"}, {"PO", "Toscana"},
            
            // Umbria
            {"PG", "Umbria"}, {"TR", "Umbria"},
            
            // Marche
            {"AN", "Marche"}, {"AP", "Marche"}, {"FM", "Marche"}, {"MC", "Marche"}, {"PU", "Marche"},
            
            // Lazio
            {"RM", "Lazio"}, {"VT", "Lazio"}, {"RI", "Lazio"}, {"LT", "Lazio"}, {"FR", "Lazio"},
            
            // Abruzzo
            {"AQ", "Abruzzo"}, {"CH", "Abruzzo"}, {"PE", "Abruzzo"}, {"TE", "Abruzzo"},
            
            // Molise
            {"CB", "Molise"}, {"IS", "Molise"},
            
            // Campania
            {"NA", "Campania"}, {"AV", "Campania"}, {"BN", "Campania"}, {"CE", "Campania"}, {"SA", "Campania"},
            
            // Puglia
            {"BA", "Puglia"}, {"FG", "Puglia"}, {"LE", "Puglia"}, {"BR", "Puglia"}, {"TA", "Puglia"}, {"BT", "Puglia"},
            
            // Basilicata
            {"PZ", "Basilicata"}, {"MT", "Basilicata"},
            
            // Calabria
            {"CS", "Calabria"}, {"CZ", "Calabria"}, {"RC", "Calabria"}, {"KR", "Calabria"}, {"VV", "Calabria"},
            
            // Sicilia
            {"PA", "Sicilia"}, {"AG", "Sicilia"}, {"CL", "Sicilia"}, {"CT", "Sicilia"},
            {"EN", "Sicilia"}, {"ME", "Sicilia"}, {"RG", "Sicilia"}, {"SR", "Sicilia"}, {"TP", "Sicilia"},
            
            // Sardegna
            {"CA", "Sardegna"}, {"NU", "Sardegna"}, {"OR", "Sardegna"}, {"SS", "Sardegna"},
            {"OT", "Sardegna"}, {"OG", "Sardegna"}, {"CI", "Sardegna"}, {"VS", "Sardegna"}
        };

        // Regioni con le relative province
        public static readonly Dictionary<string, string[]> RegioniProvincie = new()
        {
            {"Piemonte", new[] {"TO", "VC", "NO", "CN", "AT", "AL", "BI", "VB"}},
            {"Valle d'Aosta", new[] {"AO"}},
            {"Lombardia", new[] {"MI", "BG", "BS", "PV", "CR", "MN", "CO", "VA", "SO", "LC", "LO", "MB"}},
            {"Trentino-Alto Adige", new[] {"TN", "BZ"}},
            {"Veneto", new[] {"VE", "VR", "VI", "BL", "PD", "TV", "RO"}},
            {"Friuli-Venezia Giulia", new[] {"UD", "GO", "TS", "PN"}},
            {"Liguria", new[] {"GE", "SP", "SV", "IM"}},
            {"Emilia-Romagna", new[] {"BO", "FE", "FC", "MO", "PR", "RA", "RE", "RN", "PC"}},
            {"Toscana", new[] {"FI", "AR", "SI", "GR", "LI", "LU", "MS", "PI", "PT", "PO"}},
            {"Umbria", new[] {"PG", "TR"}},
            {"Marche", new[] {"AN", "AP", "FM", "MC", "PU"}},
            {"Lazio", new[] {"RM", "VT", "RI", "LT", "FR"}},
            {"Abruzzo", new[] {"AQ", "CH", "PE", "TE"}},
            {"Molise", new[] {"CB", "IS"}},
            {"Campania", new[] {"NA", "AV", "BN", "CE", "SA"}},
            {"Puglia", new[] {"BA", "FG", "LE", "BR", "TA", "BT"}},
            {"Basilicata", new[] {"PZ", "MT"}},
            {"Calabria", new[] {"CS", "CZ", "RC", "KR", "VV"}},
            {"Sicilia", new[] {"PA", "AG", "CL", "CT", "EN", "ME", "RG", "SR", "TP"}},
            {"Sardegna", new[] {"CA", "NU", "OR", "SS", "OT", "OG", "CI", "VS"}}
        };

        /// <summary>
        /// Valida un codice fiscale italiano
        /// </summary>
        public static bool IsValidCodiceFiscale(string? codiceFiscale)
        {
            if (string.IsNullOrWhiteSpace(codiceFiscale)) return false;

            codiceFiscale = codiceFiscale.Trim().ToUpper();

            // Verifica lunghezza
            if (codiceFiscale.Length != 16) return false;

            // Verifica formato con regex
            var regex = new Regex(@"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$");
            if (!regex.IsMatch(codiceFiscale)) return false;

            // Calcola e verifica carattere di controllo
            return VerifyCodiceFiscaleChecksum(codiceFiscale);
        }

        private static bool VerifyCodiceFiscaleChecksum(string codiceFiscale)
        {
            try
            {
                const string oddChars = "BAFHJNPRTVCESULDGIMOQKWZYX";
                const string evenChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string controlChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

                int sum = 0;

                // Somma caratteri in posizione dispari (1-based)
                for (int i = 0; i < 15; i += 2)
                {
                    char c = codiceFiscale[i];
                    if (char.IsDigit(c))
                    {
                        int digit = c - '0';
                        sum += new[] { 1, 0, 5, 7, 9, 13, 15, 17, 19, 21 }[digit];
                    }
                    else
                    {
                        sum += oddChars.IndexOf(c);
                    }
                }

                // Somma caratteri in posizione pari (1-based)
                for (int i = 1; i < 15; i += 2)
                {
                    char c = codiceFiscale[i];
                    if (char.IsDigit(c))
                    {
                        sum += c - '0';
                    }
                    else
                    {
                        sum += evenChars.IndexOf(c);
                    }
                }

                // Carattere di controllo
                char expectedControl = controlChars[sum % 26];
                return codiceFiscale[15] == expectedControl;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida un CAP italiano (5 cifre)
        /// </summary>
        public static bool IsValidItalianCAP(string? cap)
        {
            if (string.IsNullOrWhiteSpace(cap)) return false;

            cap = cap.Trim();
            return Regex.IsMatch(cap, @"^\d{5}$");
        }

        /// <summary>
        /// Valida un numero di telefono italiano
        /// </summary>
        public static bool IsValidItalianPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;

            // Rimuovi spazi e caratteri speciali
            phone = Regex.Replace(phone.Trim(), @"[\s\-\(\)\.\/]", "");

            // Patterns per telefoni italiani
            var patterns = new[]
            {
                @"^(\+39)?3\d{8,9}$",          // Cellulari (+39 opzionale)
                @"^(\+39)?0\d{8,10}$",         // Fissi (+39 opzionale)
                @"^(\+39)?\d{6,11}$"           // Generico (+39 opzionale)
            };

            return patterns.Any(pattern => Regex.IsMatch(phone, pattern));
        }

        /// <summary>
        /// Valida una sigla di provincia italiana
        /// </summary>
        public static bool IsValidItalianProvince(string? provincia)
        {
            if (string.IsNullOrWhiteSpace(provincia)) return false;

            return ProvinceItaliane.ContainsKey(provincia.Trim().ToUpper());
        }

        /// <summary>
        /// Valida un indirizzo email
        /// </summary>
        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                return emailRegex.IsMatch(email.Trim());
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Formatta un numero di telefono italiano
        /// </summary>
        public static string FormatItalianPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;

            // Rimuovi tutto eccetto numeri e +
            phone = Regex.Replace(phone.Trim(), @"[^\d\+]", "");

            // Se inizia con +39, rimuovilo per la formattazione
            if (phone.StartsWith("+39"))
                phone = phone.Substring(3);

            // Formattazione cellulari (3XX XXX XXXX)
            if (phone.Length == 10 && phone.StartsWith("3"))
            {
                return $"{phone.Substring(0, 3)} {phone.Substring(3, 3)} {phone.Substring(6)}";
            }

            // Formattazione fissi (0XX XXX XXXX)
            if (phone.Length >= 9 && phone.StartsWith("0"))
            {
                if (phone.Length == 9)
                    return $"{phone.Substring(0, 2)} {phone.Substring(2, 3)} {phone.Substring(5)}";
                if (phone.Length == 10)
                    return $"{phone.Substring(0, 3)} {phone.Substring(3, 3)} {phone.Substring(6)}";
            }

            return phone; // Restituisci il numero originale se non riconosciuto
        }

        /// <summary>
        /// Ottiene la regione da una provincia
        /// </summary>
        public static string? GetRegioneFromProvincia(string? provincia)
        {
            if (string.IsNullOrWhiteSpace(provincia)) return null;

            return ProvinceItaliane.TryGetValue(provincia.Trim().ToUpper(), out var regione)
                ? regione
                : null;
        }

        /// <summary>
        /// Ottiene tutte le province di una regione
        /// </summary>
        public static string[] GetProvinceFromRegione(string? regione)
        {
            if (string.IsNullOrWhiteSpace(regione)) return Array.Empty<string>();

            return RegioniProvincie.TryGetValue(regione.Trim(), out var province)
                ? province
                : Array.Empty<string>();
        }

        /// <summary>
        /// Valida un anno di costruzione ragionevole
        /// </summary>
        public static bool IsValidAnnoCostruzione(int? anno)
        {
            if (!anno.HasValue) return true; // Opzionale

            return anno >= 1800 && anno <= DateTime.Now.Year + 5; // Permetti costruzioni future planificate
        }

        /// <summary>
        /// Valida una superficie in metri quadri
        /// </summary>
        public static bool IsValidSuperficie(decimal? superficie)
        {
            if (!superficie.HasValue) return true; // Opzionale

            return superficie > 0 && superficie <= 50000; // Max 5 ettari (ragionevole per immobili)
        }

        /// <summary>
        /// Valida un prezzo
        /// </summary>
        public static bool IsValidPrezzo(decimal? prezzo)
        {
            if (!prezzo.HasValue) return true; // Opzionale

            return prezzo > 0 && prezzo <= 100_000_000; // Max 100 milioni (ragionevole)
        }

        /// <summary>
        /// Normalizza un CAP (rimuovi spazi e prefissi)
        /// </summary>
        public static string NormalizeCAP(string? cap)
        {
            if (string.IsNullOrWhiteSpace(cap)) return string.Empty;

            return Regex.Replace(cap.Trim(), @"[^\d]", "");
        }

        /// <summary>
        /// Normalizza una provincia (maiuscolo, senza spazi)
        /// </summary>
        public static string NormalizeProvincia(string? provincia)
        {
            if (string.IsNullOrWhiteSpace(provincia)) return string.Empty;

            return provincia.Trim().ToUpper();
        }

        /// <summary>
        /// Valida una partita IVA italiana
        /// </summary>
        public static bool IsValidPartitaIVA(string? partitaIva)
        {
            if (string.IsNullOrWhiteSpace(partitaIva)) return false;

            partitaIva = Regex.Replace(partitaIva.Trim(), @"[^\d]", "");

            if (partitaIva.Length != 11) return false;

            // Algoritmo di controllo Partita IVA
            try
            {
                int sum = 0;
                for (int i = 0; i < 10; i++)
                {
                    int digit = int.Parse(partitaIva[i].ToString());
                    if (i % 2 == 1) // Posizioni pari (0-indexed)
                    {
                        digit *= 2;
                        if (digit > 9) digit = digit / 10 + digit % 10;
                    }
                    sum += digit;
                }

                int checkDigit = (10 - (sum % 10)) % 10;
                return checkDigit == int.Parse(partitaIva[10].ToString());
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Fornisce suggerimenti di validazione per campi specifici
        /// </summary>
        /// <summary>
        /// Fornisce suggerimenti di validazione per più campi cliente
        /// </summary>
        public static string GetSuggerimentoValidazione(string? codiceFiscale, string? telefono, string? email, string? cap, string? provincia)
        {
            var suggerimenti = new List<string>();

            // Validazione Codice Fiscale
            if (!string.IsNullOrEmpty(codiceFiscale) && !IsValidCodiceFiscale(codiceFiscale))
            {
                suggerimenti.Add("• Codice Fiscale: Inserisci nel formato RSSMRA85M01H501Z");
            }

            // Validazione Telefono
            if (!string.IsNullOrEmpty(telefono) && !IsValidItalianPhone(telefono))
            {
                suggerimenti.Add("• Telefono: Formato italiano (es: 06 1234567 o 333 1234567)");
            }

            // Validazione Email
            if (!string.IsNullOrEmpty(email) && !IsValidEmail(email))
            {
                suggerimenti.Add("• Email: Formato valido (es: nome@dominio.it)");
            }

            // Validazione CAP
            if (!string.IsNullOrEmpty(cap) && !IsValidItalianCAP(cap))
            {
                suggerimenti.Add("• CAP: 5 cifre (es: 00100)");
            }

            // Validazione Provincia
            if (!string.IsNullOrEmpty(provincia) && !IsValidItalianProvince(provincia))
            {
                suggerimenti.Add("• Provincia: Sigla italiana valida (es: RM, MI, NA)");
            }

            return suggerimenti.Count > 0
                ? $"Correzioni suggerite:\n{string.Join("\n", suggerimenti)}"
                : "✅ Tutti i campi sono validi!";
        }

        /// <summary>
        /// Fornisce suggerimento per un singolo campo
        /// </summary>
        public static string GetSuggerimentoValidazione(string fieldName)
        {
            return fieldName.ToLower() switch
            {
                "codicefiscale" => "Inserisci il codice fiscale nel formato: RSSMRA85M01H501Z",
                "cap" => "Inserisci un CAP valido di 5 cifre (es: 00100)",
                "provincia" => "Seleziona una provincia italiana valida (es: RM, MI, NA)",
                "telefono" => "Inserisci un numero di telefono italiano (es: 06 1234567 o 333 1234567)",
                "cellulare" => "Inserisci un numero di cellulare italiano (es: 333 1234567)",
                "email" => "Inserisci un indirizzo email valido (es: nome@dominio.it)",
                "partitaiva" => "Inserisci una partita IVA italiana di 11 cifre",
                "prezzo" => "Inserisci un prezzo valido (massimo €100.000.000)",
                "superficie" => "Inserisci una superficie in mq (massimo 50.000 mq)",
                "annocostruzione" => "Inserisci un anno tra 1800 e " + (DateTime.Now.Year + 5),
                _ => "Verifica che il valore inserito sia corretto"
            };
        }
    }
}