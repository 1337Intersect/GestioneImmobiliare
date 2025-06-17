using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using ImmobiGestio.Models;
using ImmobiGestio.Data;
using ImmobiGestio.Commands;
using ImmobiGestio.Services;
using System.IO;
using System;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Collections.Generic;
using ImmobiGestio.Helpers;

namespace ImmobiGestio.ViewModels
{
    public class ImmobiliViewModel : BaseViewModel
    {
        private readonly ImmobiliContext _context;
        private readonly StatisticheService _statisticheService;
        private Immobile? _selectedImmobile;
        private string _searchText = string.Empty;
        private string _filtroTipo = "Tutti";
        private string _filtroStato = "Tutti";
        private string _filtroCitta = "Tutti";
        private decimal _filtroPrezzoDa = 0;
        private decimal _filtroPrezzoA = 0;
        private bool _isDeleting = false;


        // ObservableCollection separate per l'UI
        public ObservableCollection<Immobile> Immobili { get; set; } = new();
        public ObservableCollection<DocumentoImmobile> DocumentiCorrente { get; set; } = new();
        public ObservableCollection<FotoImmobile> FotoCorrente { get; set; } = new();
        public ObservableCollection<ClienteImmobile> ClientiInteressati { get; set; } = new();
        public ObservableCollection<Appuntamento> AppuntamentiImmobile { get; set; } = new();

        public ObservableCollection<string> TipiDocumento { get; set; } = new();
        public ObservableCollection<string> TipiImmobile { get; set; } = new();
        public ObservableCollection<string> StatiConservazione { get; set; } = new();
        public ObservableCollection<string> ClassiEnergetiche { get; set; } = new();
        public ObservableCollection<string> StatiVendita { get; set; } = new();

        // Filtri
        public ObservableCollection<string> TipiImmobileFiltro { get; set; } = new();
        public ObservableCollection<string> StatiVenditaFiltro { get; set; } = new();
        public ObservableCollection<string> CittaFiltro { get; set; } = new();

        // Eventi per comunicare con altri ViewModels
        public event Action? AppuntamentoCreated;

        public Immobile? SelectedImmobile
        {
            get => _selectedImmobile;
            set
            {
                if (_selectedImmobile != null && !_isDeleting)
                {
                    SaveCurrentImmobile();
                }
                SetProperty(ref _selectedImmobile, value);
                RefreshCurrentCollections();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterImmobili();
            }
        }

        public string FiltroTipo
        {
            get => _filtroTipo;
            set
            {
                SetProperty(ref _filtroTipo, value);
                FilterImmobili();
            }
        }

        public string FiltroStato
        {
            get => _filtroStato;
            set
            {
                SetProperty(ref _filtroStato, value);
                FilterImmobili();
            }
        }

        public string FiltroCitta
        {
            get => _filtroCitta;
            set
            {
                SetProperty(ref _filtroCitta, value);
                FilterImmobili();
            }
        }

        public decimal FiltroPrezzoDa
        {
            get => _filtroPrezzoDa;
            set
            {
                SetProperty(ref _filtroPrezzoDa, value);
                FilterImmobili();
            }
        }

        public decimal FiltroPrezzoA
        {
            get => _filtroPrezzoA;
            set
            {
                SetProperty(ref _filtroPrezzoA, value);
                FilterImmobili();
            }
        }

        public string MaxDocumentSizeFormatted => SettingsIntegrationHelper.CurrentSettings.MaxDocumentSizeFormatted;
        public string MaxPhotoSizeFormatted => SettingsIntegrationHelper.CurrentSettings.MaxPhotoSizeFormatted;

        // Commands
        public ICommand? AddImmobileCommand { get; set; }
        public ICommand? SaveImmobileCommand { get; set; }
        public ICommand? DeleteImmobileCommand { get; set; }
        public ICommand? AddDocumentCommand { get; set; }
        public ICommand? AddPhotoCommand { get; set; }
        public ICommand? OpenDocumentCommand { get; set; }
        public ICommand? DeleteDocumentCommand { get; set; }
        public ICommand? DeletePhotoCommand { get; set; }
        public ICommand? AddClienteInteressatoCommand { get; set; }
        public ICommand? DeleteClienteInteressatoCommand { get; set; }
        public ICommand? CreaAppuntamentoCommand { get; set; }
        public ICommand? EsportaImmobiliCommand { get; set; }
        public ICommand? ImportaImmobiliCommand { get; set; }
        public ICommand? GeneraReportCommand { get; set; }
        public ICommand? ClearFiltriCommand { get; set; }

        // Eventi per navigazione
        public event Action<int>? NavigateToCliente;
        public event Action<int>? NavigateToAppuntamento;

        public ImmobiliViewModel(ImmobiliContext context)
        {
            _context = context;
            _statisticheService = new StatisticheService(context);

            InitializeCollections();
            InitializeCommands();
            LoadImmobili();
            LoadFiltriData();

            // NUOVO: Registra per ricevere notifiche cambio impostazioni
            SettingsIntegrationHelper.RegisterSettingsChangedCallback(OnSettingsChanged);
        }
        private void OnSettingsChanged(SettingsModel newSettings)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== IMPOSTAZIONI CAMBIATE IN IMMOBILI ===");

                // Aggiorna i limiti di dimensione file nei messaggi di errore
                UpdateFileLimitsInUI();

                // Aggiorna la validazione dei file
                RefreshFileValidation();

                System.Diagnostics.Debug.WriteLine("ImmobiliViewModel: Impostazioni aggiornate");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnSettingsChanged in ImmobiliViewModel: {ex.Message}");
            }
        }
        private void UpdateFileLimitsInUI()
        {
            // Notifica le proprietà che dipendono dalle impostazioni
            OnPropertyChanged(nameof(MaxDocumentSizeFormatted));
            OnPropertyChanged(nameof(MaxPhotoSizeFormatted));
        }
        private void InitializeCollections()
        {
            // Popolamento TipiDocumento
            TipiDocumento.Clear();
            foreach (var tipo in new[] { "Planimetria", "Visura Catastale", "APE", "Certificazione Urbanistica", "Atto di Proprietà", "Certificato di Agibilità", "Altro" })
            {
                TipiDocumento.Add(tipo);
            }

            // Popolamento TipiImmobile
            TipiImmobile.Clear();
            TipiImmobileFiltro.Clear();
            TipiImmobileFiltro.Add("Tutti");

            foreach (var tipo in new[] { "Appartamento", "Villa", "Villetta", "Attico", "Loft", "Ufficio", "Negozio", "Capannone", "Terreno", "Box/Garage" })
            {
                TipiImmobile.Add(tipo);
                TipiImmobileFiltro.Add(tipo);
            }

            // Popolamento StatiConservazione
            StatiConservazione.Clear();
            foreach (var stato in new[] { "Nuovo", "Ottimo", "Buono", "Da ristrutturare", "In costruzione" })
            {
                StatiConservazione.Add(stato);
            }

            // Popolamento ClassiEnergetiche
            ClassiEnergetiche.Clear();
            foreach (var classe in new[] { "A4", "A3", "A2", "A1", "B", "C", "D", "E", "F", "G" })
            {
                ClassiEnergetiche.Add(classe);
            }

            // Stati vendita
            StatiVendita.Clear();
            StatiVenditaFiltro.Clear();
            StatiVenditaFiltro.Add("Tutti");

            foreach (var stato in new[] { "Disponibile", "Prenotato", "Venduto", "Ritirato" })
            {
                StatiVendita.Add(stato);
                StatiVenditaFiltro.Add(stato);
            }
        }

        private void InitializeCommands()
        {
            AddImmobileCommand = new RelayCommand(AddImmobile);
            SaveImmobileCommand = new RelayCommand(SaveImmobile, _ => SelectedImmobile != null);
            DeleteImmobileCommand = new RelayCommand(DeleteImmobile, _ => SelectedImmobile != null);
            AddDocumentCommand = new RelayCommand(AddDocument, _ => SelectedImmobile != null);
            AddPhotoCommand = new RelayCommand(AddPhoto, _ => SelectedImmobile != null);
            OpenDocumentCommand = new RelayCommand(OpenDocument);
            DeleteDocumentCommand = new RelayCommand(DeleteDocument);
            DeletePhotoCommand = new RelayCommand(DeletePhoto);
            AddClienteInteressatoCommand = new RelayCommand(AddClienteInteressato, _ => SelectedImmobile != null);
            DeleteClienteInteressatoCommand = new RelayCommand(DeleteClienteInteressato);
            CreaAppuntamentoCommand = new RelayCommand(CreaAppuntamento, _ => SelectedImmobile != null);
            EsportaImmobiliCommand = new RelayCommand(EsportaImmobili);
            ImportaImmobiliCommand = new RelayCommand(ImportaImmobili);
            GeneraReportCommand = new RelayCommand(GeneraReport);
            ClearFiltriCommand = new RelayCommand(ClearFiltri);
        }

        private void LoadFiltriData()
        {
            try
            {
                // Carica città per filtro
                CittaFiltro.Clear();
                CittaFiltro.Add("Tutti");

                var citta = _context.Immobili
                    .Where(i => !string.IsNullOrEmpty(i.Citta))
                    .Select(i => i.Citta)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                foreach (var c in citta)
                {
                    CittaFiltro.Add(c);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel caricamento filtri: {ex.Message}");
            }
        }

        public void LoadImmobili()
        {
            try
            {
                var immobili = _context.Immobili
                    .AsNoTracking()
                    .Include(i => i.Documenti)
                    .Include(i => i.Foto)
                    .OrderByDescending(i => i.DataInserimento)
                    .ToList();

                Immobili.Clear();
                foreach (var immobile in immobili)
                {
                    Immobili.Add(immobile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento degli immobili: {ex.Message}", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterImmobili()
        {
            try
            {
                var query = _context.Immobili
                    .AsNoTracking()
                    .Include(i => i.Documenti)
                    .Include(i => i.Foto)
                    .AsQueryable();

                // Filtro per testo di ricerca
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(i => i.Titolo.Contains(SearchText) ||
                                           i.Indirizzo.Contains(SearchText) ||
                                           i.Citta.Contains(SearchText) ||
                                           i.Descrizione.Contains(SearchText));
                }

                // Filtro per tipo
                if (FiltroTipo != "Tutti")
                {
                    query = query.Where(i => i.TipoImmobile == FiltroTipo);
                }

                // Filtro per stato
                if (FiltroStato != "Tutti")
                {
                    query = query.Where(i => i.StatoVendita == FiltroStato);
                }

                // Filtro per città
                if (FiltroCitta != "Tutti")
                {
                    query = query.Where(i => i.Citta == FiltroCitta);
                }

                // Filtro per prezzo
                if (FiltroPrezzoDa > 0)
                {
                    query = query.Where(i => i.Prezzo >= FiltroPrezzoDa);
                }

                if (FiltroPrezzoA > 0)
                {
                    query = query.Where(i => i.Prezzo <= FiltroPrezzoA);
                }

                var filtered = query
                    .OrderByDescending(i => i.DataInserimento)
                    .ToList();

                Immobili.Clear();
                foreach (var immobile in filtered)
                {
                    Immobili.Add(immobile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella ricerca: {ex.Message}", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshCurrentCollections()
        {
            DocumentiCorrente.Clear();
            FotoCorrente.Clear();
            ClientiInteressati.Clear();
            AppuntamentiImmobile.Clear();

            if (SelectedImmobile != null)
            {
                try
                {
                    var immobileWithData = _context.Immobili
                        .AsNoTracking()
                        .Include(i => i.Documenti)
                        .Include(i => i.Foto)
                        .FirstOrDefault(i => i.Id == SelectedImmobile.Id);

                    if (immobileWithData != null)
                    {
                        foreach (var doc in immobileWithData.Documenti)
                        {
                            DocumentiCorrente.Add(doc);
                        }

                        foreach (var foto in immobileWithData.Foto)
                        {
                            FotoCorrente.Add(foto);
                        }
                    }

                    // Carica clienti interessati
                    var interessati = _context.ClientiImmobili
                        .AsNoTracking()
                        .Include(ci => ci.Cliente)
                        .Where(ci => ci.ImmobileId == SelectedImmobile.Id)
                        .OrderByDescending(ci => ci.DataInteresse)
                        .ToList();

                    foreach (var interessato in interessati)
                    {
                        ClientiInteressati.Add(interessato);
                    }

                    // Carica appuntamenti per questo immobile
                    var appuntamenti = _context.Appuntamenti
                        .AsNoTracking()
                        .Include(a => a.Cliente)
                        .Where(a => a.ImmobileId == SelectedImmobile.Id)
                        .OrderByDescending(a => a.DataInizio)
                        .ToList();

                    foreach (var app in appuntamenti)
                    {
                        AppuntamentiImmobile.Add(app);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nel caricamento dei dati dell'immobile: {ex.Message}",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // FIX: Metodo AddImmobile corretto con inizializzazione delle stringhe
        private void AddImmobile(object? parameter)
        {
            try
            {
                // Crea l'immobile con TUTTI i valori richiesti impostati correttamente
                var newImmobile = new Immobile
                {
                    Titolo = "Nuovo Immobile",
                    Indirizzo = "Inserisci indirizzo",
                    Citta = "",
                    CAP = "",
                    Provincia = "",
                    Descrizione = "",

                    // Proprietà numeriche inizializzate correttamente
                    Prezzo = 0, // decimal not null
                    Superficie = null, // int nullable
                    NumeroLocali = null, // int nullable  
                    NumeroBagni = null, // int nullable
                    Piano = null, // int nullable

                    // Proprietà con valori di default RICHIESTI
                    TipoImmobile = "Appartamento", // NOT NULL
                    StatoConservazione = "Buono", // NOT NULL
                    ClasseEnergetica = "G", // NOT NULL
                    StatoVendita = "Disponibile", // NOT NULL

                    // Date RICHIESTE
                    DataInserimento = DateTime.Now,
                    DataUltimaModifica = null // nullable
                };

                // Verifica che il contesto sia valido
                if (_context == null)
                {
                    MessageBox.Show("Errore: Database non inizializzato correttamente.",
                        "Errore Database", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Log per debug - verifica che tutti i campi siano impostati
                System.Diagnostics.Debug.WriteLine($"=== CREAZIONE IMMOBILE ===");
                System.Diagnostics.Debug.WriteLine($"Titolo: '{newImmobile.Titolo}'");
                System.Diagnostics.Debug.WriteLine($"Indirizzo: '{newImmobile.Indirizzo}'");
                System.Diagnostics.Debug.WriteLine($"TipoImmobile: '{newImmobile.TipoImmobile}'");
                System.Diagnostics.Debug.WriteLine($"StatoConservazione: '{newImmobile.StatoConservazione}'");
                System.Diagnostics.Debug.WriteLine($"ClasseEnergetica: '{newImmobile.ClasseEnergetica}'");
                System.Diagnostics.Debug.WriteLine($"StatoVendita: '{newImmobile.StatoVendita}'");
                System.Diagnostics.Debug.WriteLine($"Prezzo: {newImmobile.Prezzo}");
                System.Diagnostics.Debug.WriteLine($"DataInserimento: {newImmobile.DataInserimento}");

                // Verifica che tutti i campi obbligatori siano valorizzati
                if (string.IsNullOrEmpty(newImmobile.Titolo))
                {
                    MessageBox.Show("Errore: Titolo è vuoto!", "Errore Validazione",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(newImmobile.Indirizzo))
                {
                    MessageBox.Show("Errore: Indirizzo è vuoto!", "Errore Validazione",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(newImmobile.TipoImmobile))
                {
                    MessageBox.Show("Errore: TipoImmobile è vuoto!", "Errore Validazione",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Usa un nuovo contesto per evitare conflitti di tracking
                using (var newContext = new ImmobiliContext())
                {
                    newContext.Immobili.Add(newImmobile);
                    newContext.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"Immobile creato con ID: {newImmobile.Id}");
                }

                // Ricarica tutti gli immobili per aggiornare la UI
                LoadImmobili();

                // Seleziona il nuovo immobile creato
                var createdImmobile = Immobili.FirstOrDefault(i => i.Id == newImmobile.Id);
                if (createdImmobile != null)
                {
                    SelectedImmobile = createdImmobile;
                    System.Diagnostics.Debug.WriteLine($"Immobile selezionato: ID {createdImmobile.Id}");
                }

                MessageBox.Show("Nuovo immobile creato con successo!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException?.Message ?? "Nessun dettaglio disponibile";
                var message = $"Errore di database nella creazione dell'immobile:\n\n" +
                             $"Errore principale: {dbEx.Message}\n\n" +
                             $"Dettagli: {innerException}";

                MessageBox.Show(message, "Errore Database", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"DbUpdateException immobile: {dbEx}");
            }
            catch (Exception ex)
            {
                var message = $"Errore imprevisto nella creazione dell'immobile:\n\n" +
                             $"Tipo: {ex.GetType().Name}\n" +
                             $"Messaggio: {ex.Message}";

                if (ex.InnerException != null)
                {
                    message += $"\n\nErrore interno: {ex.InnerException.Message}";
                }

                MessageBox.Show(message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Exception immobile: {ex}");
            }
        }

        private void SaveCurrentImmobile()
        {
            if (SelectedImmobile != null)
            {
                try
                {
                    var exists = _context.Immobili.Any(i => i.Id == SelectedImmobile.Id);
                    if (!exists) return;

                    SelectedImmobile.DataUltimaModifica = DateTime.Now;

                    var tracked = _context.ChangeTracker.Entries<Immobile>()
                        .FirstOrDefault(e => e.Entity.Id == SelectedImmobile.Id);
                    if (tracked != null)
                    {
                        _context.Entry(tracked.Entity).State = EntityState.Detached;
                    }

                    _context.Update(SelectedImmobile);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    if (!_isDeleting)
                    {
                        MessageBox.Show($"Errore nel salvataggio: {ex.Message}", "Errore",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SaveImmobile(object? parameter)
        {
            SaveCurrentImmobile();
            MessageBox.Show("Immobile salvato con successo!", "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteImmobile(object? parameter)
        {
            if (SelectedImmobile == null) return;

            var result = MessageBox.Show(
                $"Sei sicuro di voler eliminare l'immobile '{SelectedImmobile.Titolo}'?\n" +
                "Questa operazione eliminerà anche tutti i documenti, le foto e gli interessi associati.",
                "Conferma Eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _isDeleting = true;
                    var immobileId = SelectedImmobile.Id;
                    var immobileTitolo = SelectedImmobile.Titolo;

                    DeletePhysicalFiles(SelectedImmobile);

                    var immobileToDelete = _context.Immobili
                        .Include(i => i.Documenti)
                        .Include(i => i.Foto)
                        .FirstOrDefault(i => i.Id == immobileId);

                    if (immobileToDelete != null)
                    {
                        _context.Immobili.Remove(immobileToDelete);
                        _context.SaveChanges();

                        var uiImmobile = Immobili.FirstOrDefault(i => i.Id == immobileId);
                        if (uiImmobile != null)
                        {
                            Immobili.Remove(uiImmobile);
                        }

                        SelectedImmobile = Immobili.FirstOrDefault();

                        MessageBox.Show($"Immobile '{immobileTitolo}' eliminato con successo!",
                            "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nell'eliminazione: {ex.Message}", "Errore",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    LoadImmobili();
                }
                finally
                {
                    _isDeleting = false;
                }
            }
        }

        private void DeletePhysicalFiles(Immobile immobile)
        {
            try
            {
                FileManagerService.DeleteDirectory(immobile.Id, "Documenti");
                FileManagerService.DeleteDirectory(immobile.Id, "Foto");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'eliminazione dei file: {ex.Message}", "Avviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreaAppuntamento(object? parameter)
        {
            if (SelectedImmobile == null)
            {
                MessageBox.Show("Nessun immobile selezionato per creare l'appuntamento.",
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Salva prima l'immobile corrente
                SaveCurrentImmobile();

                // USA IL FACTORY METHOD - MOLTO PIÙ SICURO!
                var newAppuntamento = Appuntamento.CreaPerImmobile(
                    SelectedImmobile.Id,
                    SelectedImmobile.Titolo,
                    SelectedImmobile.Indirizzo
                );

                // Log per debug
                System.Diagnostics.Debug.WriteLine($"=== CREAZIONE APPUNTAMENTO DA IMMOBILE ===");
                System.Diagnostics.Debug.WriteLine($"ImmobileId: {newAppuntamento.ImmobileId}");
                System.Diagnostics.Debug.WriteLine($"Immobile: {SelectedImmobile.Titolo}");
                System.Diagnostics.Debug.WriteLine($"TipoAppuntamento: '{newAppuntamento.TipoAppuntamento}'");
                System.Diagnostics.Debug.WriteLine($"StatoAppuntamento: '{newAppuntamento.StatoAppuntamento}'");
                System.Diagnostics.Debug.WriteLine($"Priorita: '{newAppuntamento.Priorita}'");

                // Valida prima di salvare
                if (!newAppuntamento.IsValid())
                {
                    MessageBox.Show("Errore: L'appuntamento creato non è valido. Controlla i dati.",
                        "Errore Validazione", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Usa un nuovo contesto per evitare conflitti di tracking
                using (var newContext = new ImmobiliContext())
                {
                    // Verifica che l'immobile esista ancora
                    var immobileExists = newContext.Immobili.Any(i => i.Id == SelectedImmobile.Id);
                    if (!immobileExists)
                    {
                        MessageBox.Show("Errore: L'immobile selezionato non esiste nel database!",
                            "Errore Database", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Aggiungi e salva
                    newContext.Appuntamenti.Add(newAppuntamento);
                    var saved = newContext.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"Appuntamento salvato con ID: {newAppuntamento.Id}, Records salvati: {saved}");
                }

                // Ricarica le collezioni correnti
                RefreshCurrentCollections();

                // Notifica agli altri ViewModels
                AppuntamentoCreated?.Invoke();

                MessageBox.Show($"Appuntamento creato con successo per l'immobile!\n\n" +
                               $"Immobile: {SelectedImmobile.Titolo}\n" +
                               $"Data: {newAppuntamento.DataInizio:dd/MM/yyyy HH:mm}\n" +
                               $"Tipo: {newAppuntamento.TipoAppuntamento}",
                    "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? "Nessun dettaglio";
                var message = $"Errore di database nella creazione dell'appuntamento:\n\n" +
                             $"Errore: {dbEx.Message}\n\n" +
                             $"Dettagli: {innerMessage}";

                MessageBox.Show(message, "Errore Database", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"DbUpdateException CreaAppuntamento: {dbEx}");
            }
            catch (Exception ex)
            {
                var message = $"Errore imprevisto nella creazione dell'appuntamento:\n\n" +
                             $"Tipo: {ex.GetType().Name}\n" +
                             $"Messaggio: {ex.Message}";

                if (ex.InnerException != null)
                {
                    message += $"\n\nErrore interno: {ex.InnerException.Message}";
                }

                MessageBox.Show(message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Exception CreaAppuntamento: {ex}");
            }
        }

        // Altri metodi rimangono uguali...
        private void AddDocument(object? parameter)
        {
            if (SelectedImmobile == null) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf|Word files (*.doc;*.docx)|*.doc;*.docx|Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*",
                Multiselect = false,
                Title = "Seleziona documento da aggiungere"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var tipoDocumento = parameter?.ToString() ?? "Altro";
                    var sourceFile = openFileDialog.FileName;
                    var fileExtension = System.IO.Path.GetExtension(sourceFile);

                    // NUOVO: Usa le impostazioni per validare
                    if (!SettingsIntegrationHelper.IsDocumentFormatSupported(fileExtension))
                    {
                        MessageBox.Show($"Tipo di file '{fileExtension}' non supportato per i documenti.\n\n" +
                                       $"Formati supportati: {SettingsIntegrationHelper.CurrentSettings.SupportedDocumentFormats}",
                            "Formato Non Supportato", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var fileSize = FileManagerService.GetFileSize(sourceFile);
                    var maxSize = SettingsIntegrationHelper.GetMaxDocumentSize();

                    if (fileSize > maxSize)
                    {
                        MessageBox.Show($"Il file è troppo grande ({FileManagerService.FormatFileSize(fileSize)}).\n\n" +
                                       $"Dimensione massima consentita: {SettingsIntegrationHelper.CurrentSettings.MaxDocumentSizeFormatted}",
                            "File Troppo Grande", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // ... resto del metodo invariato ...
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nell'aggiunta del documento: {ex.Message}", "Errore",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddPhoto(object? parameter)
        {
            if (SelectedImmobile == null) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif",
                Multiselect = true,
                Title = "Seleziona foto da aggiungere"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var photosPath = FileManagerService.GetPhotosPath(SelectedImmobile.Id);
                    int fotoAggiunte = 0;
                    var errori = new List<string>();

                    foreach (var sourceFile in openFileDialog.FileNames)
                    {
                        try
                        {
                            var fileExtension = System.IO.Path.GetExtension(sourceFile);

                            // NUOVO: Usa le impostazioni per validare
                            if (!SettingsIntegrationHelper.IsImageFormatSupported(fileExtension))
                            {
                                errori.Add($"{Path.GetFileName(sourceFile)}: Formato '{fileExtension}' non supportato");
                                continue;
                            }

                            var fileSize = FileManagerService.GetFileSize(sourceFile);
                            var maxSize = SettingsIntegrationHelper.GetMaxPhotoSize();

                            if (fileSize > maxSize)
                            {
                                errori.Add($"{Path.GetFileName(sourceFile)}: File troppo grande ({FileManagerService.FormatFileSize(fileSize)})");
                                continue;
                            }

                            // ... resto della logica di copia file ...

                            fotoAggiunte++;
                        }
                        catch (Exception ex)
                        {
                            errori.Add($"{Path.GetFileName(sourceFile)}: {ex.Message}");
                        }
                    }

                    // ... resto del metodo invariato ...
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore imprevisto: {ex.Message}", "Errore",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenDocument(object? parameter)
        {
            if (parameter is DocumentoImmobile documento)
            {
                try
                {
                    if (File.Exists(documento.PercorsoFile))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = documento.PercorsoFile,
                            UseShellExecute = true
                        });
                        return;
                    }

                    var foundPath = FileManagerService.FindFile(documento.NomeFile, documento.ImmobileId, "Documenti");

                    if (!string.IsNullOrEmpty(foundPath))
                    {
                        var docToUpdate = _context.Documenti.Find(documento.Id);
                        if (docToUpdate != null)
                        {
                            docToUpdate.PercorsoFile = foundPath;
                            _context.SaveChanges();
                            documento.PercorsoFile = foundPath;
                        }

                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = foundPath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        var result = MessageBox.Show(
                            $"File '{documento.NomeFile}' non trovato!\n\nVuoi rimuovere questo documento dall'elenco?",
                            "File non trovato",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            DeleteDocument(documento);
                        }
                    }
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    MessageBox.Show("Impossibile aprire il file. Assicurati di avere un'applicazione associata per questo tipo di file.",
                        "Errore apertura file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nell'apertura del documento: {ex.Message}", "Errore",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteDocument(object? parameter)
        {
            if (parameter is DocumentoImmobile documento)
            {
                var result = MessageBox.Show(
                    $"Sei sicuro di voler eliminare il documento '{documento.NomeFile}'?",
                    "Conferma Eliminazione",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var docToDelete = _context.Documenti.Find(documento.Id);
                        if (docToDelete != null)
                        {
                            if (File.Exists(docToDelete.PercorsoFile))
                            {
                                File.Delete(docToDelete.PercorsoFile);
                            }

                            _context.Documenti.Remove(docToDelete);
                            _context.SaveChanges();

                            DocumentiCorrente.Remove(documento);

                            MessageBox.Show("Documento eliminato con successo!", "Successo",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore nell'eliminazione del documento: {ex.Message}", "Errore",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeletePhoto(object? parameter)
        {
            if (parameter is FotoImmobile foto)
            {
                var result = MessageBox.Show(
                    $"Sei sicuro di voler eliminare la foto '{foto.NomeFile}'?",
                    "Conferma Eliminazione",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var fotoToDelete = _context.Foto.Find(foto.Id);
                        if (fotoToDelete != null)
                        {
                            if (File.Exists(fotoToDelete.PercorsoFile))
                            {
                                File.Delete(fotoToDelete.PercorsoFile);
                            }

                            _context.Foto.Remove(fotoToDelete);
                            _context.SaveChanges();

                            FotoCorrente.Remove(foto);

                            MessageBox.Show("Foto eliminata con successo!", "Successo",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore nell'eliminazione della foto: {ex.Message}", "Errore",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void AddClienteInteressato(object? parameter)
        {
            MessageBox.Show("Funzionalità in sviluppo: Aggiungi Cliente Interessato", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteClienteInteressato(object? parameter)
        {
            if (parameter is ClienteImmobile interesse)
            {
                var result = MessageBox.Show(
                    "Sei sicuro di voler rimuovere questo interesse?",
                    "Conferma Rimozione",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var interesseToDelete = _context.ClientiImmobili.Find(interesse.Id);
                        if (interesseToDelete != null)
                        {
                            _context.ClientiImmobili.Remove(interesseToDelete);
                            _context.SaveChanges();

                            ClientiInteressati.Remove(interesse);

                            MessageBox.Show("Interesse rimosso con successo!", "Successo",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore nella rimozione dell'interesse: {ex.Message}", "Errore",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void EsportaImmobili(object? parameter)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"Immobili_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = "Titolo,Indirizzo,Città,Prezzo,Tipo,Stato,Superficie,Locali,Classe Energetica,Data Inserimento\n";

                    foreach (var immobile in Immobili)
                    {
                        csv += $"{immobile.Titolo},{immobile.Indirizzo},{immobile.Citta},{immobile.Prezzo}," +
                               $"{immobile.TipoImmobile},{immobile.StatoVendita},{immobile.Superficie}," +
                               $"{immobile.NumeroLocali},{immobile.ClasseEnergetica},{immobile.DataInserimento:yyyy-MM-dd}\n";
                    }

                    File.WriteAllText(saveFileDialog.FileName, csv);

                    MessageBox.Show($"Immobili esportati con successo in {saveFileDialog.FileName}",
                        "Esportazione Completata", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'esportazione: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportaImmobili(object? parameter)
        {
            MessageBox.Show("Funzionalità in sviluppo: Importa Immobili da CSV", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GeneraReport(object? parameter)
        {
            try
            {
                var performance = _statisticheService.GetPerformanceImmobili();
                var analisiMercato = _statisticheService.GetAnalisiMercato();

                var report = "REPORT IMMOBILI\n";
                report += $"Generato il: {DateTime.Now:dd/MM/yyyy HH:mm}\n\n";
                report += $"Totale immobili: {Immobili.Count}\n";
                report += $"Immobili disponibili: {Immobili.Count(i => i.StatoVendita == "Disponibile")}\n";
                report += $"Immobili venduti: {Immobili.Count(i => i.StatoVendita == "Venduto")}\n";
                report += $"Valore totale portafoglio: € {Immobili.Where(i => i.StatoVendita == "Disponibile").Sum(i => i.Prezzo):N0}\n\n";

                if (performance.Any())
                {
                    report += "TOP 5 IMMOBILI PER VISITE:\n";
                    foreach (var p in performance.Take(5))
                    {
                        report += $"- {p.Titolo}: {p.NumeroVisite} visite, {p.NumeroInteressati} interessati\n";
                    }
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    FileName = $"Report_Immobili_{DateTime.Now:yyyyMMdd}.txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, report);
                    MessageBox.Show($"Report generato: {saveFileDialog.FileName}", "Report Generato",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella generazione del report: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearFiltri(object? parameter)
        {
            SearchText = string.Empty;
            FiltroTipo = "Tutti";
            FiltroStato = "Tutti";
            FiltroCitta = "Tutti";
            FiltroPrezzoDa = 0;
            FiltroPrezzoA = 0;
        }

        // Proprietà per statistiche rapide
        public int TotaleImmobili => Immobili.Count;
        public int ImmobiliDisponibili => Immobili.Count(i => i.StatoVendita == "Disponibile");
        public decimal ValoreTotale => Immobili.Where(i => i.StatoVendita == "Disponibile").Sum(i => i.Prezzo);
        public decimal PrezzoMedio => ImmobiliDisponibili > 0 ? ValoreTotale / ImmobiliDisponibili : 0;


        public void OnApplicationClosing()
        {
            try
            {
                if (!_isDeleting && SelectedImmobile != null)
                {
                    SaveCurrentImmobile();
                }

                // NUOVO: Rimuovi il callback delle impostazioni
                SettingsIntegrationHelper.UnregisterSettingsChangedCallback(OnSettingsChanged);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel salvataggio finale immobile: {ex.Message}");
            }
        }
    }

}