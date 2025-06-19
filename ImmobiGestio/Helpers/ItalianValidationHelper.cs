using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ImmobiGestio.Helpers
{
    /// <summary>
    /// Helper per validazioni specifiche del mercato italiano immobiliare
    /// </summary>
    public static class ItalianValidationHelper
    {
        // PROVINCE ITALIANE COMPLETE (aggiornate 2024)
        public static readonly Dictionary<string, string> ProvinceItaliane = new()
        {
            {"AG", "Agrigento"}, {"AL", "Alessandria"}, {"AN", "Ancona"}, {"AO", "Aosta"},
            {"AR", "Arezzo"}, {"AP", "Ascoli Piceno"}, {"AT", "Asti"}, {"AV", "Avellino"},
            {"BA", "Bari"}, {"BT", "Barletta-Andria-Trani"}, {"BL", "Belluno"}, {"BN", "Benevento"},
            {"BG", "Bergamo"}, {"BI", "Biella"}, {"BO", "Bologna"}, {"BZ", "Bolzano"},
            {"BS", "Brescia"}, {"BR", "Brindisi"}, {"CA", "Cagliari"}, {"CL", "Caltanissetta"},
            {"CB", "Campobasso"}, {"CI", "Carbonia-Iglesias"}, {"CE", "Caserta"}, {"CT", "Catania"},
            {"CZ", "Catanzaro"}, {"CH", "Chieti"}, {"CO", "Como"}, {"CS", "Cosenza"},
            {"CR", "Cremona"}, {"KR", "Crotone"}, {"CN", "Cuneo"}, {"EN", "Enna"},
            {"FM", "Fermo"}, {"FE", "Ferrara"}, {"FI", "Firenze"}, {"FG", "Foggia"},
            {"FC", "Forlì-Cesena"}, {"FR", "Frosinone"}, {"GE", "Genova"}, {"GO", "Gorizia"},
            {"GR", "Grosseto"}, {"IM", "Imperia"}, {"IS", "Isernia"}, {"SP", "La Spezia"},
            {"AQ", "L'Aquila"}, {"LT", "Latina"}, {"LE", "Lecce"}, {"LC", "Lecco"},
            {"LI", "Livorno"}, {"LO", "Lodi"}, {"LU", "Lucca"}, {"MC", "Macerata"},
            {"MN", "Mantova"}, {"MS", "Massa-Carrara"}, {"MT", "Matera"}, {"VS", "Medio Campidano"},
            {"ME", "Messina"}, {"MI", "Milano"}, {"MO", "Modena"}, {"MB", "Monza e Brianza"},
            {"NA", "Napoli"}, {"NO", "Novara"}, {"NU", "Nuoro"}, {"OG", "Ogliastra"},
            {"OT", "Olbia-Tempio"}, {"OR", "Oristano"}, {"PD", "Padova"}, {"PA", "Palermo"},
            {"PR", "Parma"}, {"PV", "Pavia"}, {"PG", "Perugia"}, {"PU", "Pesaro e Urbino"},
            {"PE", "Pescara"}, {"PC", "Piacenza"}, {"PI", "Pisa"}, {"PT", "Pistoia"},
            {"PN", "Pordenone"}, {"PZ", "Potenza"}, {"PO", "Prato"}, {"RG", "Ragusa"},
            {"RA", "Ravenna"}, {"RC", "Reggio Calabria"}, {"RE", "Reggio Emilia"}, {"RI", "Rieti"},
            {"RN", "Rimini"}, {"RM", "Roma"}, {"RO", "Rovigo"}, {"SA", "Salerno"},
            {"SS", "Sassari"}, {"SV", "Savona"}, {"SI", "Siena"}, {"SR", "Siracusa"},
            {"SO", "Sondrio"}, {"TA", "Taranto"}, {"TE", "Teramo"}, {"TR", "Terni"},
            {"TO", "Torino"}, {"TP", "Trapani"}, {"TN", "Trento"}, {"TV", "Treviso"},
            {"TS", "Trieste"}, {"UD", "Udine"}, {"VA", "Varese"}, {"VE", "Venezia"},
            {"VB", "Verbano-Cusio-Ossola"}, {"VC", "Vercelli"}, {"VR", "Verona"}, {"VV", "Vibo Valentia"},
            {"VI", "Vicenza"}, {"VT", "Viterbo"}
        };

        // REGIONI ITALIANE
        public static readonly Dictionary<string, List<string>> RegioniProvincie = new()
        {
            {"Abruzzo", new List<string> {"AQ", "CH", "PE", "TE"}},
            {"Basilicata", new List<string> {"MT", "PZ"}},
            {"Calabria", new List<string> {"CS", "CZ", "KR", "RC", "VV"}},
            {"Campania", new List<string> {"AV", "BN", "CE", "NA", "SA"}},
            {"Emilia-Romagna", new List<string> {"BO", "FE", "FC", "MO", "PR", "PC", "RA", "RE", "RN"}},
            {"Friuli-Venezia Giulia", new List<string> {"GO", "PN", "TS", "UD"}},
            {"Lazio", new List<string> {"FR", "LT", "RI", "RM", "VT"}},
            {"Liguria", new List<string> {"GE", "IM", "SP", "SV"}},
            {"Lombardia", new List<string> {"BG", "BS", "CO", "CR", "LO", "MN", "MI", "MB", "PV", "SO", "VA"}},
            {"Marche", new List<string> {"AN", "AP", "FM", "MC", "PU"}},
            {"Molise", new List<string> {"CB", "IS"}},
            {"Piemonte", new List<string> {"AL", "AT", "BI", "CN", "NO", "TO", "VB", "VC"}},
            {"Puglia", new List<string> {"BA", "BT", "BR", "FG", "LE", "TA"}},
            {"Sardegna", new List<string> {"CA", "CI", "VS", "NU", "OG", "OT", "OR", "SS"}},
            {"Sicilia", new List<string> {"AG", "CL", "CT", "EN", "ME", "PA", "RG", "SR", "TP"}},
            {"Toscana", new List<string> {"AR", "FI", "GR", "LI", "LU", "MS", "PI", "PO", "PT", "SI"}},
            {"Trentino-Alto Adige", new List<string> {"BZ", "TN"}},
            {"Umbria", new List<string> {"PG", "TR"}},
            {"Valle d'Aosta", new List<string> {"AO"}},
            {"Veneto", new List<string> {"BL", "PD", "RO", "TV", "VE", "VR", "VI"}}
        };

        /// <summary>
        /// Valida un codice fiscale italiano
        /// </summary>
        public static bool IsValidCodiceFiscale(string codiceFiscale)
        {
            if (string.IsNullOrWhiteSpace(codiceFiscale) || codiceFiscale.Length != 16)
                return false;

            codiceFiscale = codiceFiscale.ToUpper();

            // Controllo formato: 6 lettere + 2 cifre + 1 lettera + 2 cifre + 1 lettera + 3 cifre + 1 lettera
            if (!Regex.IsMatch(codiceFiscale, @"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$"))
                return false;

            // Controllo carattere di controllo
            return ValidateCodiceFiscaleChecksum(codiceFiscale);
        }

        private static bool ValidateCodiceFiscaleChecksum(string codiceFiscale)
        {
            const string oddChars = "BAFHJNPRTVCESULDGIMOQKWZYX";
            const string evenChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string checkChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            int sum = 0;
            for (int i = 0; i < 15; i++)
            {
                char c = codiceFiscale[i];
                int value;

                if (char.IsLetter(c))
                {
                    value = c - 'A';
                }
                else
                {
                    value = c - '0';
                }

                if (i % 2 == 0) // posizione dispari (1-based)
                {
                    // Per le posizioni dispari, usa la tabella "odd"
                    if (char.IsLetter(c))
                    {
                        sum += oddChars.IndexOf(c);
                    }
                    else
                    {
                        sum += new int[] { 1, 0, 5, 7, 9, 13, 15, 17, 19, 21 }[c - '0'];
                    }
                }
                else // posizione pari
                {
                    // Per le posizioni pari, usa la tabella "even"
                    if (char.IsLetter(c))
                    {
                        sum += c - 'A';
                    }
                    else
                    {
                        sum += c - '0';
                    }
                }
            }

            char expectedCheck = checkChars[sum % 26];
            return codiceFiscale[15] == expectedCheck;
        }

        /// <summary>
        /// Valida una partita IVA italiana
        /// </summary>
        public static bool IsValidPartitaIVA(string partitaIva)
        {
            if (string.IsNullOrWhiteSpace(partitaIva) || partitaIva.Length != 11)
                return false;

            // Deve essere composta solo da cifre
            if (!partitaIva.All(char.IsDigit))
                return false;

            // Controllo algoritmo di Luhn modificato
            int sum = 0;
            for (int i = 0; i < 10; i++)
            {
                int digit = int.Parse(partitaIva[i].ToString());
                if (i % 2 == 1)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit = digit / 10 + digit % 10;
                }
                sum += digit;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit == int.Parse(partitaIva[10].ToString());
        }

        /// <summary>
        /// Valida un CAP italiano
        /// </summary>
        public static bool IsValidCAP(string cap)
        {
            if (string.IsNullOrWhiteSpace(cap))
                return true; // CAP non obbligatorio

            return cap.Length == 5 && cap.All(char.IsDigit);
        }

        /// <summary>
        /// Valida una sigla provincia italiana
        /// </summary>
        public static bool IsValidProvincia(string provincia)
        {
            if (string.IsNullOrWhiteSpace(provincia))
                return true; // Provincia non obbligatoria

            return ProvinceItaliane.ContainsKey(provincia.ToUpper());
        }

        /// <summary>
        /// Ottiene il nome completo di una provincia dalla sigla
        /// </summary>
        public static string? GetNomeProvincia(string sigla)
        {
            return ProvinceItaliane.TryGetValue(sigla?.ToUpper() ?? "", out string? nome) ? nome : null;
        }

        /// <summary>
        /// Ottiene la regione di una provincia
        /// </summary>
        public static string? GetRegioneProvincia(string sigla)
        {
            if (string.IsNullOrWhiteSpace(sigla))
                return null;

            sigla = sigla.ToUpper();
            foreach (var regione in RegioniProvincie)
            {
                if (regione.Value.Contains(sigla))
                    return regione.Key;
            }
            return null;
        }

        /// <summary>
        /// Valida un numero di telefono italiano
        /// </summary>
        public static bool IsValidTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return true; // Telefono non obbligatorio

            // Rimuovi spazi, trattini e parentesi
            var cleaned = Regex.Replace(telefono, @"[\s\-\(\)]", "");

            // Pattern per numeri italiani
            // +39 seguito da numero, oppure numero che inizia con 0 o 3
            var patterns = new[]
            {
                @"^\+39[0-9]{6,11}$",           // +39 seguito da 6-11 cifre
                @"^0[0-9]{5,10}$",              // Fisso: inizia con 0, 6-11 cifre totali
                @"^3[0-9]{8,9}$",               // Mobile: inizia con 3, 9-10 cifre totali
                @"^00390[0-9]{5,10}$",          // Formato internazionale alternativo
                @"^003903[0-9]{8,9}$"           // Formato internazionale mobile alternativo
            };

            return patterns.Any(pattern => Regex.IsMatch(cleaned, pattern));
        }

        /// <summary>
        /// Formatta un numero di telefono italiano
        /// </summary>
        public static string FormatTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return "";

            var cleaned = Regex.Replace(telefono, @"[\s\-\(\)]", "");

            // Rimuove +39 o 0039 se presente
            if (cleaned.StartsWith("+39"))
                cleaned = cleaned.Substring(3);
            else if (cleaned.StartsWith("0039"))
                cleaned = cleaned.Substring(4);

            // Formattazione basata sul tipo
            if (cleaned.StartsWith("3") && (cleaned.Length == 9 || cleaned.Length == 10))
            {
                // Mobile: 3XX XXX XXXX
                return $"{cleaned.Substring(0, 3)} {cleaned.Substring(3, 3)} {cleaned.Substring(6)}";
            }
            else if (cleaned.StartsWith("0"))
            {
                // Fisso: formato varia per città
                if (cleaned.Length >= 6)
                {
                    var prefisso = cleaned.Substring(0, Math.Min(4, cleaned.Length - 4));
                    var numero = cleaned.Substring(prefisso.Length);

                    if (numero.Length <= 4)
                        return $"{prefisso} {numero}";
                    else
                        return $"{prefisso} {numero.Substring(0, numero.Length / 2)} {numero.Substring(numero.Length / 2)}";
                }
            }

            return telefono; // Ritorna originale se non riconosciuto
        }

        /// <summary>
        /// Valida un IBAN italiano
        /// </summary>
        public static bool IsValidIBAN(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return true; // IBAN non obbligatorio

            // Rimuove spazi
            iban = iban.Replace(" ", "").ToUpper();

            // IBAN italiano: IT + 2 cifre di controllo + 1 lettera + 10 cifre + 12 caratteri alfanumerici
            if (!Regex.IsMatch(iban, @"^IT[0-9]{2}[A-Z][0-9]{10}[A-Z0-9]{12}$"))
                return false;

            // Verifica checksum IBAN
            return ValidateIBANChecksum(iban);
        }

        private static bool ValidateIBANChecksum(string iban)
        {
            // Sposta i primi 4 caratteri alla fine
            var rearranged = iban.Substring(4) + iban.Substring(0, 4);

            // Converti lettere in numeri (A=10, B=11, ..., Z=35)
            var numeric = "";
            foreach (char c in rearranged)
            {
                if (char.IsLetter(c))
                    numeric += (c - 'A' + 10).ToString();
                else
                    numeric += c;
            }

            // Calcola mod 97
            return CalculateMod97(numeric) == 1;
        }

        private static int CalculateMod97(string number)
        {
            int remainder = 0;
            foreach (char digit in number)
            {
                remainder = (remainder * 10 + (digit - '0')) % 97;
            }
            return remainder;
        }

        /// <summary>
        /// Valida un indirizzo email
        /// </summary>
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

        /// <summary>
        /// Estrae il CAP da un indirizzo o restituisce informazioni sulla zona
        /// </summary>
        public static (bool IsValid, string? CAP, string? Info) AnalyzeCAP(string cap)
        {
            if (!IsValidCAP(cap))
                return (false, null, "CAP non valido");

            if (string.IsNullOrWhiteSpace(cap))
                return (true, null, null);

            // Analisi zone italiane basata su prime cifre
            var firstDigit = cap[0];
            var zone = firstDigit switch
            {
                '0' => "Piemonte, Valle d'Aosta, Liguria",
                '1' => "Lombardia",
                '2' => "Lombardia, Trentino-Alto Adige",
                '3' => "Veneto, Friuli-Venezia Giulia",
                '4' => "Emilia-Romagna",
                '5' => "Toscana",
                '6' => "Marche, Umbria",
                '7' => "Lazio, Abruzzo",
                '8' => "Campania, Molise, Puglia, Basilicata",
                '9' => "Calabria, Sicilia, Sardegna",
                _ => "Zona non riconosciuta"
            };

            return (true, cap, $"Zona: {zone}");
        }

        /// <summary>
        /// Valida e formatta un prezzo immobiliare
        /// </summary>
        public static (bool IsValid, decimal Value, string Formatted) ValidatePrezzo(string prezzo)
        {
            if (string.IsNullOrWhiteSpace(prezzo))
                return (true, 0, "€ 0");

            // Rimuove caratteri non numerici eccetto virgola e punto
            var cleaned = Regex.Replace(prezzo, @"[^0-9,.]", "");

            // Sostituisce virgola con punto per parsing
            cleaned = cleaned.Replace(',', '.');

            if (decimal.TryParse(cleaned, out decimal value))
            {
                if (value < 0)
                    return (false, 0, "Il prezzo non può essere negativo");

                if (value > 999999999)
                    return (false, 0, "Il prezzo è troppo alto");

                var formatted = value > 0 ? $"€ {value:N0}" : "Prezzo da definire";
                return (true, value, formatted);
            }

            return (false, 0, "Formato prezzo non valido");
        }

        /// <summary>
        /// Calcola e valida il prezzo al metro quadro
        /// </summary>
        public static (bool IsValid, decimal PrezzoMq, string Formatted, string Categoria)
            CalculatePrezzoMetroQuadro(decimal prezzo, int? superficie, string? provincia = null)
        {
            if (!superficie.HasValue || superficie <= 0 || prezzo <= 0)
                return (false, 0, "N/D", "Dati insufficienti");

            var prezzoMq = prezzo / superficie.Value;
            var formatted = $"€ {prezzoMq:N0}/mq";

            // Categorizzazione basata su medie italiane (semplificata)
            var categoria = prezzoMq switch
            {
                < 1000 => "Molto conveniente",
                < 2000 => "Conveniente",
                < 3000 => "Nella media",
                < 5000 => "Sopra la media",
                < 8000 => "Alto",
                _ => "Molto alto"
            };

            // Aggiusta per zone specifiche se provincia fornita
            if (!string.IsNullOrEmpty(provincia))
            {
                categoria = AdjustCategoriaByProvincia(prezzoMq, provincia, categoria);
            }

            return (true, prezzoMq, formatted, categoria);
        }

        private static string AdjustCategoriaByProvincia(decimal prezzoMq, string provincia, string categoriaBase)
        {
            // Zone ad alto costo (Milano, Roma, Venezia, Firenze, etc.)
            var zoneAltoValore = new[] { "MI", "RM", "VE", "FI", "BO", "TO", "GE", "NA" };

            if (zoneAltoValore.Contains(provincia.ToUpper()))
            {
                return prezzoMq switch
                {
                    < 2000 => "Molto conveniente",
                    < 4000 => "Conveniente",
                    < 6000 => "Nella media",
                    < 8000 => "Sopra la media",
                    < 12000 => "Alto",
                    _ => "Molto alto"
                };
            }

            return categoriaBase;
        }

        /// <summary>
        /// Genera suggerimenti per migliorare la validazione dei dati
        /// </summary>
        public static List<string> GetSuggerimentiValidazione(
            string? codiceFiscale, string? telefono, string? email,
            string? cap, string? provincia)
        {
            var suggerimenti = new List<string>();

            if (!string.IsNullOrEmpty(codiceFiscale) && !IsValidCodiceFiscale(codiceFiscale))
                suggerimenti.Add("Il codice fiscale non è corretto. Verificare l'inserimento.");

            if (!string.IsNullOrEmpty(telefono) && !IsValidTelefono(telefono))
                suggerimenti.Add("Il numero di telefono non sembra valido. Includere il prefisso (es. 02, 06, 347).");

            if (!string.IsNullOrEmpty(email) && !IsValidEmail(email))
                suggerimenti.Add("L'indirizzo email non è corretto.");

            if (!string.IsNullOrEmpty(cap) && !IsValidCAP(cap))
                suggerimenti.Add("Il CAP deve essere composto da 5 cifre.");

            if (!string.IsNullOrEmpty(provincia) && !IsValidProvincia(provincia))
                suggerimenti.Add("La provincia deve essere una sigla italiana valida (es. MI, RM, NA).");

            return suggerimenti;
        }
    }
}