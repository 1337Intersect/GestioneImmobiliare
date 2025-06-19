using ImmobiGestio.Commands;
using ImmobiGestio.Data;
using ImmobiGestio.Helpers;
using ImmobiGestio.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImmobiGestio.ViewModels
{
    public class ClientiViewModel : BaseViewModel
    {
        private readonly ImmobiliContext _context;
        private Cliente? _selectedCliente;
        private string _searchText = string.Empty;
        private string _filtroTipoCliente = "Tutti";
        private string _filtroStato = "Tutti";
        private string _filtroRegione = "Tutti";
        private bool _isDeleting = false;

        public ObservableCollection<Cliente> Clienti { get; set; } = new();
        public ObservableCollection<Appuntamento> AppuntamentiCliente { get; set; } = new();
        public ObservableCollection<ClienteImmobile> ImmobiliInteresse { get; set; } = new();
        public ObservableCollection<Immobile> ImmobiliDisponibili { get; set; } = new();

        // LISTE ITALIANIZZATE
        public ObservableCollection<string> TipiCliente { get; set; } = new();
        public ObservableCollection<string> StatiCliente { get; set; } = new();
        public ObservableCollection<string> FontiContatto { get; set; } = new();
        public ObservableCollection<string> ProvinceItaliane { get; set; } = new();
        public ObservableCollection<string> RegioniItaliane { get; set; } = new();
        public ObservableCollection<string> TipiClienteFiltro { get; set; } = new();
        public ObservableCollection<string> StatiClienteFiltro { get; set; } = new();
        public ObservableCollection<string> RegioniFiltro { get; set; } = new();

        // Eventi per comunicare con altri ViewModels
        public event Action? AppuntamentoCreated;

        public Cliente? SelectedCliente
        {
            get => _selectedCliente;
            set
            {
                if (_selectedCliente != null && !_isDeleting)
                {
                    SaveCurrentCliente();
                }
                SetProperty(ref _selectedCliente, value);
                RefreshCurrentCollections();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterClienti();
            }
        }

        public string FiltroTipoCliente
        {
            get => _filtroTipoCliente;
            set
            {
                SetProperty(ref _filtroTipoCliente, value);
                FilterClienti();
            }
        }

        public string FiltroStato
        {
            get => _filtroStato;
            set
            {
                SetProperty(ref _filtroStato, value);
                FilterClienti();
            }
        }

        public string FiltroRegione
        {
            get => _filtroRegione;
            set
            {
                SetProperty(ref _filtroRegione, value);
                FilterClienti();
            }
        }

        // Commands
        public ICommand? AddClienteCommand { get; set; }
        public ICommand? SaveClienteCommand { get; set; }
        public ICommand? DeleteClienteCommand { get; set; }
        public ICommand? AddAppuntamentoCommand { get; set; }
        public ICommand? AddInteresseImmobileCommand { get; set; }
        public ICommand? DeleteInteresseImmobileCommand { get; set; }
        public ICommand? ValidateClienteCommand { get; set; }
        public ICommand? FormatTelefonoCommand { get; set; }
        public ICommand? CercaProvinciaCommand { get; set; }
        public ICommand? EsportaClientiCommand { get; set; }
        public ICommand? ImportaClientiCommand { get; set; }
        public ICommand? ClearFiltriCommand { get; set; }
        public ICommand? CreaAppuntamentoCommand { get; set; }

        public ClientiViewModel(ImmobiliContext context)
        {
            _context = context;

            InitializeItalianCollections();
            InitializeCommands();
            LoadClienti();
            LoadImmobiliDisponibili();
        }

        private void InitializeItalianCollections()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INIZIALIZZAZIONE COLLEZIONI CLIENTI ITALIANE ===");

                // Tipi Cliente
                TipiCliente.Clear();
                TipiClienteFiltro.Clear();
                TipiClienteFiltro.Add("Tutti");

                foreach (var tipo in Models.TipiCliente.All)
                {
                    TipiCliente.Add(tipo);
                    TipiClienteFiltro.Add(tipo);
                }

                // Stati Cliente
                StatiCliente.Clear();
                StatiClienteFiltro.Clear();
                StatiClienteFiltro.Add("Tutti");

                foreach (var stato in Models.StatiCliente.All)
                {
                    StatiCliente.Add(stato);
                    StatiClienteFiltro.Add(stato);
                }

                // Fonti Contatto
                FontiContatto.Clear();
                foreach (var fonte in Models.FontiContatto.All)
                {
                    FontiContatto.Add(fonte);
                }

                // Province italiane
                ProvinceItaliane.Clear();
                foreach (var provincia in ItalianValidationHelper.ProvinceItaliane.Keys.OrderBy(p => p))
                {
                    ProvinceItaliane.Add(provincia);
                }

                // Regioni italiane
                RegioniItaliane.Clear();
                RegioniFiltro.Clear();
                RegioniFiltro.Add("Tutti");

                foreach (var regione in ItalianValidationHelper.RegioniProvincie.Keys.OrderBy(r => r))
                {
                    RegioniItaliane.Add(regione);
                    RegioniFiltro.Add(regione);
                }

                System.Diagnostics.Debug.WriteLine("Collezioni clienti italiane inizializzate");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore InitializeItalianCollections clienti: {ex.Message}");
            }
        }

        private void InitializeCommands()
        {
            try
            {
                AddClienteCommand = new RelayCommand(AddCliente);
                SaveClienteCommand = new RelayCommand(SaveCliente, _ => SelectedCliente != null);
                DeleteClienteCommand = new RelayCommand(DeleteCliente, _ => SelectedCliente != null);
                AddAppuntamentoCommand = new RelayCommand(AddAppuntamento, _ => SelectedCliente != null);
                AddInteresseImmobileCommand = new RelayCommand(AddInteresseImmobile, _ => SelectedCliente != null);
                DeleteInteresseImmobileCommand = new RelayCommand(DeleteInteresseImmobile);
                ValidateClienteCommand = new RelayCommand(ValidateCliente, _ => SelectedCliente != null);
                FormatTelefonoCommand = new RelayCommand(FormatTelefono, _ => SelectedCliente != null);
                CercaProvinciaCommand = new RelayCommand(CercaProvincia);
                EsportaClientiCommand = new RelayCommand(EsportaClienti);
                ImportaClientiCommand = new RelayCommand(ImportaClienti);
                ClearFiltriCommand = new RelayCommand(ClearFiltri);
                CreaAppuntamentoCommand = new RelayCommand(CreaAppuntamento, _ => SelectedCliente != null);

                System.Diagnostics.Debug.WriteLine("Comandi clienti inizializzati");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore InitializeCommands clienti: {ex.Message}");
            }
        }

        public void LoadClienti()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CARICAMENTO CLIENTI ===");

                var clienti = _context.Clienti
                    .AsNoTracking()
                    .Include(c => c.Appuntamenti)
                    .Include(c => c.ImmobiliDiInteresse)
                    .OrderByDescending(c => c.DataInserimento)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Caricati {clienti.Count} clienti dal database");

                Clienti.Clear();
                foreach (var cliente in clienti)
                {
                    Clienti.Add(cliente);
                }

                System.Diagnostics.Debug.WriteLine($"LoadClienti completato: {Clienti.Count} clienti nella UI");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore LoadClienti: {ex}");
                MessageBox.Show($"Errore nel caricamento dei clienti: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadImmobiliDisponibili()
        {
            try
            {
                var immobili = _context.Immobili
                    .AsNoTracking()
                    .Where(i => i.StatoVendita == "Disponibile")
                    .OrderBy(i => i.Titolo)
                    .ToList();

                ImmobiliDisponibili.Clear();
                foreach (var immobile in immobili)
                {
                    ImmobiliDisponibili.Add(immobile);
                }

                System.Diagnostics.Debug.WriteLine($"Caricati {ImmobiliDisponibili.Count} immobili disponibili");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore LoadImmobiliDisponibili: {ex.Message}");
            }
        }

        private void FilterClienti()
        {
            try
            {
                var query = _context.Clienti
                    .AsNoTracking()
                    .Include(c => c.Appuntamenti)
                    .Include(c => c.ImmobiliDiInteresse)
                    .AsQueryable();

                // Filtro per testo
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(c =>
                        c.Nome.Contains(SearchText) ||
                        c.Cognome.Contains(SearchText) ||
                        c.Email.Contains(SearchText) ||
                        c.Telefono.Contains(SearchText) ||
                        c.Cellulare.Contains(SearchText) ||
                        c.CodiceFiscale.Contains(SearchText) ||
                        c.Citta.Contains(SearchText));
                }

                // Filtro per tipo cliente
                if (FiltroTipoCliente != "Tutti")
                {
                    query = query.Where(c => c.TipoCliente == FiltroTipoCliente);
                }

                // Filtro per stato
                if (FiltroStato != "Tutti")
                {
                    query = query.Where(c => c.StatoCliente == FiltroStato);
                }

                // Filtro per regione
                if (FiltroRegione != "Tutti")
                {
                    var provinceRegione = ItalianValidationHelper.RegioniProvincie
                        .Where(r => r.Key == FiltroRegione)
                        .SelectMany(r => r.Value)
                        .ToList();

                    query = query.Where(c => provinceRegione.Contains(c.Provincia));
                }

                var risultati = query.OrderByDescending(c => c.DataInserimento).ToList();

                Clienti.Clear();
                foreach (var cliente in risultati)
                {
                    Clienti.Add(cliente);
                }

                System.Diagnostics.Debug.WriteLine($"Filtri applicati: {risultati.Count} clienti trovati");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore FilterClienti: {ex}");
                MessageBox.Show($"Errore nel filtro dei clienti: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshCurrentCollections()
        {
            try
            {
                AppuntamentiCliente.Clear();
                ImmobiliInteresse.Clear();

                if (SelectedCliente == null) return;

                // Carica appuntamenti del cliente
                var appuntamenti = _context.Appuntamenti
                    .AsNoTracking()
                    .Include(a => a.Immobile)
                    .Where(a => a.ClienteId == SelectedCliente.Id)
                    .OrderByDescending(a => a.DataInizio)
                    .ToList();

                foreach (var app in appuntamenti)
                {
                    AppuntamentiCliente.Add(app);
                }

                // Carica immobili di interesse
                var interessi = _context.ClientiImmobili
                    .AsNoTracking()
                    .Include(ci => ci.Immobile)
                    .Where(ci => ci.ClienteId == SelectedCliente.Id)
                    .OrderByDescending(ci => ci.DataInteresse)
                    .ToList();

                foreach (var interesse in interessi)
                {
                    ImmobiliInteresse.Add(interesse);
                }

                System.Diagnostics.Debug.WriteLine($"Caricati {appuntamenti.Count} appuntamenti e {interessi.Count} interessi per cliente {SelectedCliente.Id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore RefreshCurrentCollections: {ex.Message}");
                MessageBox.Show($"Errore nel caricamento dei dati del cliente: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // METODI CRUD
        private void AddCliente(object? parameter)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CREAZIONE NUOVO CLIENTE ===");

                var newCliente = new Cliente();

                // Log per debug
                System.Diagnostics.Debug.WriteLine($"Nuovo cliente - Nome: '{newCliente.Nome}'");
                System.Diagnostics.Debug.WriteLine($"Nuovo cliente - TipoCliente: '{newCliente.TipoCliente}'");
                System.Diagnostics.Debug.WriteLine($"Nuovo cliente - StatoCliente: '{newCliente.StatoCliente}'");

                // Validazione base
                if (!newCliente.IsValid())
                {
                    var errors = newCliente.GetValidationErrors();
                    MessageBox.Show($"Errore nella validazione del cliente:\n\n{string.Join("\n", errors)}",
                        "Errore Validazione", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Usa un nuovo contesto per evitare conflitti
                using (var newContext = new ImmobiliContext())
                {
                    newContext.Clienti.Add(newCliente);
                    newContext.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"Cliente creato con ID: {newCliente.Id}");
                }

                // Ricarica e seleziona il nuovo cliente
                LoadClienti();
                var createdCliente = Clienti.FirstOrDefault(c => c.Id == newCliente.Id);
                if (createdCliente != null)
                {
                    SelectedCliente = createdCliente;
                    System.Diagnostics.Debug.WriteLine($"Cliente selezionato: ID {createdCliente.Id}");
                }

                MessageBox.Show("Nuovo cliente creato con successo!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella creazione del cliente: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Errore AddCliente: {ex}");
            }
        }

        private void SaveCurrentCliente()
        {
            if (SelectedCliente == null) return;

            try
            {
                if (!SelectedCliente.IsValid())
                {
                    System.Diagnostics.Debug.WriteLine("Cliente non valido, salto il salvataggio");
                    return;
                }

                SelectedCliente.DataUltimaModifica = DateTime.Now;

                using (var saveContext = new ImmobiliContext())
                {
                    var existingCliente = saveContext.Clienti
                        .FirstOrDefault(c => c.Id == SelectedCliente.Id);

                    if (existingCliente != null)
                    {
                        // AGGIORNAMENTO SICURO
                        existingCliente.Nome = SelectedCliente.Nome;
                        existingCliente.Cognome = SelectedCliente.Cognome;
                        existingCliente.CodiceFiscale = SelectedCliente.CodiceFiscale;
                        existingCliente.Telefono = SelectedCliente.Telefono;
                        existingCliente.Cellulare = SelectedCliente.Cellulare;
                        existingCliente.Email = SelectedCliente.Email;
                        existingCliente.Indirizzo = SelectedCliente.Indirizzo;
                        existingCliente.Citta = SelectedCliente.Citta;
                        existingCliente.CAP = SelectedCliente.CAP;
                        existingCliente.Provincia = SelectedCliente.Provincia;
                        existingCliente.DataNascita = SelectedCliente.DataNascita;
                        existingCliente.TipoCliente = SelectedCliente.TipoCliente;
                        existingCliente.BudgetMin = SelectedCliente.BudgetMin;
                        existingCliente.BudgetMax = SelectedCliente.BudgetMax;
                        existingCliente.PreferenzeTipologia = SelectedCliente.PreferenzeTipologia;
                        existingCliente.PreferenzeZone = SelectedCliente.PreferenzeZone;
                        existingCliente.Note = SelectedCliente.Note;
                        existingCliente.StatoCliente = SelectedCliente.StatoCliente;
                        existingCliente.FonteContatto = SelectedCliente.FonteContatto;
                        existingCliente.DataUltimaModifica = DateTime.Now;

                        saveContext.Update(existingCliente);
                        saveContext.SaveChanges();

                        System.Diagnostics.Debug.WriteLine($"Cliente ID {SelectedCliente.Id} salvato");
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_isDeleting)
                {
                    MessageBox.Show($"Errore nel salvataggio del cliente: {ex.Message}",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                System.Diagnostics.Debug.WriteLine($"Errore SaveCurrentCliente: {ex}");
            }
        }

        private void SaveCliente(object? parameter)
        {
            SaveCurrentCliente();
            MessageBox.Show("Cliente salvato con successo!", "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteCliente(object? parameter)
        {
            if (SelectedCliente == null) return;

            var result = MessageBox.Show(
                $"Sei sicuro di voler eliminare il cliente '{SelectedCliente.NomeCompleto}'?\n\n" +
                "Questa operazione eliminerà anche tutti gli appuntamenti e interessi associati.",
                "Conferma Eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _isDeleting = true;

                    using (var deleteContext = new ImmobiliContext())
                    {
                        var clienteToDelete = deleteContext.Clienti
                            .Include(c => c.Appuntamenti)
                            .Include(c => c.ImmobiliDiInteresse)
                            .FirstOrDefault(c => c.Id == SelectedCliente.Id);

                        if (clienteToDelete != null)
                        {
                            deleteContext.Clienti.Remove(clienteToDelete);
                            deleteContext.SaveChanges();
                        }
                    }

                    SelectedCliente = null;
                    LoadClienti();

                    MessageBox.Show("Cliente eliminato con successo!", "Successo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nell'eliminazione del cliente: {ex.Message}",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    _isDeleting = false;
                }
            }
        }

        // METODI DI VALIDAZIONE E FORMATTAZIONE ITALIANA
        private void ValidateCliente(object? parameter)
        {
            if (SelectedCliente == null) return;

            try
            {
                var suggerimenti = ItalianValidationHelper.GetSuggerimentiValidazione(
                    SelectedCliente.CodiceFiscale,
                    SelectedCliente.Telefono,
                    SelectedCliente.Email,
                    SelectedCliente.CAP,
                    SelectedCliente.Provincia
                );

                if (suggerimenti.Any())
                {
                    var message = "Suggerimenti per migliorare i dati:\n\n" + string.Join("\n• ", suggerimenti);
                    MessageBox.Show(message, "Validazione Dati", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Tutti i dati del cliente sono corretti!", "Validazione Dati",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella validazione: {ex.Message}", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FormatTelefono(object? parameter)
        {
            if (SelectedCliente == null) return;

            try
            {
                if (!string.IsNullOrEmpty(SelectedCliente.Telefono))
                {
                    SelectedCliente.Telefono = ItalianValidationHelper.FormatItalianPhone(SelectedCliente.Telefono);
                }

                if (!string.IsNullOrEmpty(SelectedCliente.Cellulare))
                {
                    SelectedCliente.Cellulare = ItalianValidationHelper.FormatItalianPhone(SelectedCliente.Cellulare);
                }

                OnPropertyChanged(nameof(SelectedCliente));
                MessageBox.Show("Numeri di telefono formattati!", "Formattazione",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella formattazione: {ex.Message}", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CercaProvincia(object? parameter)
        {
            if (parameter is string sigla)
            {
                var nomeProvincia = ItalianValidationHelper.GetProvinceFromRegione(sigla);
                var regione = ItalianValidationHelper.GetRegioneFromProvincia(sigla);

                if (nomeProvincia != null)
                {
                    MessageBox.Show($"Provincia: {nomeProvincia}\nRegione: {regione}",
                        "Informazioni Provincia", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Sigla provincia non riconosciuta.",
                        "Provincia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // GESTIONE APPUNTAMENTI E INTERESSI
        private void AddAppuntamento(object? parameter)
        {
            if (SelectedCliente == null) return;

            try
            {
                // Salva prima il cliente corrente
                SaveCurrentCliente();

                // USA IL FACTORY METHOD
                var newAppuntamento = Appuntamento.CreaPerCliente(
                    SelectedCliente.Id,
                    SelectedCliente.NomeCompleto
                );

                // Validazione
                if (!newAppuntamento.IsValid())
                {
                    MessageBox.Show("Errore: L'appuntamento creato non è valido.",
                        "Errore Validazione", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (var newContext = new ImmobiliContext())
                {
                    newContext.Appuntamenti.Add(newAppuntamento);
                    newContext.SaveChanges();
                }

                RefreshCurrentCollections();
                AppuntamentoCreated?.Invoke();

                MessageBox.Show("Appuntamento creato con successo!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella creazione dell'appuntamento: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreaAppuntamento(object? parameter)
        {
            AddAppuntamento(parameter);
        }

        private void AddInteresseImmobile(object? parameter)
        {
            if (SelectedCliente == null) return;

            try
            {
                // Mostra dialog per selezione immobile
                var immobiliWindow = new ImmobiliSelectionWindow(ImmobiliDisponibili.ToList());
                if (immobiliWindow.ShowDialog() == true && immobiliWindow.SelectedImmobile != null)
                {
                    var selectedImmobile = immobiliWindow.SelectedImmobile;

                    // Verifica che non esista già questo interesse
                    var esisteGia = _context.ClientiImmobili
                        .Any(ci => ci.ClienteId == SelectedCliente.Id && ci.ImmobileId == selectedImmobile.Id);

                    if (esisteGia)
                    {
                        MessageBox.Show("Il cliente ha già mostrato interesse per questo immobile.",
                            "Interesse Esistente", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Crea nuovo interesse
                    var interesse = new ClienteImmobile
                    {
                        ClienteId = SelectedCliente.Id,
                        ImmobileId = selectedImmobile.Id,
                        DataInteresse = DateTime.Now,
                        StatoInteresse = "Interessato",
                        Note = ""
                    };

                    _context.ClientiImmobili.Add(interesse);
                    _context.SaveChanges();

                    RefreshCurrentCollections();

                    MessageBox.Show("Interesse per l'immobile aggiunto con successo!", "Successo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'aggiunta dell'interesse: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteInteresseImmobile(object? parameter)
        {
            if (parameter is ClienteImmobile interesse)
            {
                var result = MessageBox.Show(
                    $"Sei sicuro di voler rimuovere l'interesse per '{interesse.Immobile?.Titolo}'?",
                    "Conferma Rimozione",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.ClientiImmobili.Remove(interesse);
                        _context.SaveChanges();

                        RefreshCurrentCollections();

                        MessageBox.Show("Interesse rimosso con successo!", "Successo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore nella rimozione dell'interesse: {ex.Message}",
                            "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // EXPORT/IMPORT
        private void EsportaClienti(object? parameter)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"Clienti_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = "Nome,Cognome,Email,Telefono,Cellulare,TipoCliente,StatoCliente,Budget Min,Budget Max,Città,Provincia,Data Inserimento\n";

                    foreach (var cliente in Clienti)
                    {
                        csv += $"\"{cliente.Nome}\",\"{cliente.Cognome}\",\"{cliente.Email}\"," +
                               $"\"{cliente.Telefono}\",\"{cliente.Cellulare}\",\"{cliente.TipoCliente}\"," +
                               $"\"{cliente.StatoCliente}\",{cliente.BudgetMin},{cliente.BudgetMax}," +
                               $"\"{cliente.Citta}\",\"{cliente.Provincia}\",{cliente.DataInserimento:yyyy-MM-dd}\n";
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, csv, System.Text.Encoding.UTF8);

                    MessageBox.Show($"Clienti esportati con successo in {saveFileDialog.FileName}",
                        "Esportazione Completata", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'esportazione: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportaClienti(object? parameter)
        {
            MessageBox.Show("Funzionalità Import CSV - In sviluppo", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearFiltri(object? parameter)
        {
            SearchText = string.Empty;
            FiltroTipoCliente = "Tutti";
            FiltroStato = "Tutti";
            FiltroRegione = "Tutti";

            LoadClienti();
        }

        // CLEANUP
        public void OnApplicationClosing()
        {
            try
            {
                if (!_isDeleting && SelectedCliente != null)
                {
                    SaveCurrentCliente();
                }
                System.Diagnostics.Debug.WriteLine("ClientiViewModel cleanup completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel salvataggio finale cliente: {ex.Message}");
            }
        }
    }

    // CLASSE HELPER PER SELEZIONE IMMOBILI
    public class ImmobiliSelectionWindow : Window
    {
        public Immobile? SelectedImmobile { get; private set; }

        public ImmobiliSelectionWindow(List<Immobile> immobili)
        {
            Title = "Seleziona Immobile";
            Width = 600;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var listBox = new ListBox
            {
                ItemsSource = immobili,
                DisplayMemberPath = "Titolo"
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 75,
                Height = 25,
                Margin = new Thickness(5)
            };

            var cancelButton = new Button
            {
                Content = "Annulla",
                Width = 75,
                Height = 25,
                Margin = new Thickness(5)
            };

            okButton.Click += (s, e) =>
            {
                SelectedImmobile = listBox.SelectedItem as Immobile;
                DialogResult = SelectedImmobile != null;
            };

            cancelButton.Click += (s, e) =>
            {
                DialogResult = false;
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            var mainPanel = new DockPanel();
            DockPanel.SetDock(buttonPanel, Dock.Bottom);
            mainPanel.Children.Add(buttonPanel);
            mainPanel.Children.Add(listBox);

            Content = mainPanel;
        }
    }
}