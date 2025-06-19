using ImmobiGestio.Commands;
using ImmobiGestio.Data;
using ImmobiGestio.Helpers;
using ImmobiGestio.Models;
using ImmobiGestio.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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

        // LISTE ITALIANIZZATE
        public ObservableCollection<string> TipiDocumento { get; set; } = new();
        public ObservableCollection<string> TipiImmobile { get; set; } = new();
        public ObservableCollection<string> StatiConservazione { get; set; } = new();
        public ObservableCollection<string> ClassiEnergetiche { get; set; } = new();
        public ObservableCollection<string> StatiVendita { get; set; } = new();
        public ObservableCollection<string> ProvinceItaliane { get; set; } = new();

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

            InitializeItalianCollections();
            InitializeCommands();
            LoadImmobili();
            LoadFiltriData();

            // NUOVO: Registra per ricevere notifiche cambio impostazioni
            try
            {
                SettingsIntegrationHelper.RegisterSettingsChangedCallback(OnSettingsChanged);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore registrazione settings callback: {ex.Message}");
            }
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

        private void RefreshFileValidation()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== REFRESH FILE VALIDATION ===");

                // Get current settings
                var currentSettings = SettingsIntegrationHelper.CurrentSettings;

                // If there's a selected property, validate its existing files against new settings
                if (SelectedImmobile != null)
                {
                    // Validate existing documents
                    if (SelectedImmobile.Documenti?.Any() == true)
                    {
                        var invalidDocs = SelectedImmobile.Documenti
                            .Where(d => new FileInfo(d.PercorsoFile).Exists &&
                                       new FileInfo(d.PercorsoFile).Length > currentSettings.MaxDocumentSize)
                            .ToList();

                        if (invalidDocs.Any())
                        {
                            System.Diagnostics.Debug.WriteLine($"Trovati {invalidDocs.Count} documenti che superano il nuovo limite");
                        }
                    }

                    // Validate existing photos
                    if (SelectedImmobile.Foto?.Any() == true)
                    {
                        var invalidPhotos = SelectedImmobile.Foto
                            .Where(f => new FileInfo(f.PercorsoFile).Exists &&
                                       new FileInfo(f.PercorsoFile).Length > currentSettings.MaxPhotoSize)
                            .ToList();

                        if (invalidPhotos.Any())
                        {
                            System.Diagnostics.Debug.WriteLine($"Trovate {invalidPhotos.Count} foto che superano il nuovo limite");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("File validation refresh completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore RefreshFileValidation: {ex.Message}");
            }
        }

        [NotMapped]
        public string MaxDocumentSizeFormatted
        {
            get
            {
                try
                {
                    var settings = SettingsIntegrationHelper.CurrentSettings;
                    var sizeMB = settings.MaxDocumentSize / (1024 * 1024);
                    return $"{sizeMB} MB";
                }
                catch
                {
                    return "50 MB"; // Fallback
                }
            }
        }

        [NotMapped]
        public string MaxPhotoSizeFormatted
        {
            get
            {
                try
                {
                    var settings = SettingsIntegrationHelper.CurrentSettings;
                    var sizeMB = settings.MaxPhotoSize / (1024 * 1024);
                    return $"{sizeMB} MB";
                }
                catch
                {
                    return "10 MB"; // Fallback
                }
            }
        }

        private void InitializeItalianCollections()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INIZIALIZZAZIONE COLLEZIONI ITALIANE ===");

                // Tipi documento italiani
                TipiDocumento.Clear();
                foreach (var tipo in new[] {
                    "APE - Certificato Energetico",
                    "Planimetria Catastale",
                    "Visura Catastale",
                    "Contratto di Compravendita",
                    "Rogito Notarile",
                    "Certificato di Conformità",
                    "Relazione Tecnica",
                    "Perizia di Stima",
                    "Documenti Condominiali",
                    "Altri Documenti" })
                {
                    TipiDocumento.Add(tipo);
                }

                // Province italiane
                ProvinceItaliane.Clear();
                foreach (var provincia in Models.ProvinceItaliane.All.OrderBy(p => p))
                {
                    ProvinceItaliane.Add(provincia);
                }

                // Popolamento TipiImmobile ITALIANI
                TipiImmobile.Clear();
                TipiImmobileFiltro.Clear();
                TipiImmobileFiltro.Add("Tutti");

                foreach (var tipo in new[] {
                    "Appartamento",
                    "Villa",
                    "Villetta a schiera",
                    "Attico",
                    "Penthouse",
                    "Loft",
                    "Casa indipendente",
                    "Rustico",
                    "Casale",
                    "Palazzo",
                    "Mansarda",
                    "Monolocale",
                    "Bilocale",
                    "Trilocale",
                    "Quadrilocale",
                    "Ufficio",
                    "Negozio",
                    "Capannone",
                    "Terreno",
                    "Box/Garage",
                    "Altro" })
                {
                    TipiImmobile.Add(tipo);
                    TipiImmobileFiltro.Add(tipo);
                }

                // Popolamento StatiConservazione ITALIANI
                StatiConservazione.Clear();
                foreach (var stato in new[] {
                    "Nuovo/Appena costruito",
                    "Ottimo",
                    "Buono",
                    "Discreto",
                    "Da ristrutturare",
                    "Da ristrutturare completamente",
                    "In costruzione",
                    "Grezzo" })
                {
                    StatiConservazione.Add(stato);
                }

                // Popolamento ClassiEnergetiche ITALIANE
                ClassiEnergetiche.Clear();
                foreach (var classe in new[] { "A4", "A3", "A2", "A1", "B", "C", "D", "E", "F", "G", "Esente" })
                {
                    ClassiEnergetiche.Add(classe);
                }

                // Stati vendita ITALIANI
                StatiVendita.Clear();
                StatiVenditaFiltro.Clear();
                StatiVenditaFiltro.Add("Tutti");

                foreach (var stato in new[] {
                    "Disponibile",
                    "In trattativa",
                    "Prenotato",
                    "Compromesso",
                    "Venduto",
                    "Ritirato dal mercato",
                    "Sospeso" })
                {
                    StatiVendita.Add(stato);
                    StatiVenditaFiltro.Add(stato);
                }

                System.Diagnostics.Debug.WriteLine("Collezioni italiane inizializzate");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore InitializeItalianCollections: {ex.Message}");
            }
        }

        private void InitializeCommands()
        {
            try
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

                System.Diagnostics.Debug.WriteLine("Comandi immobili inizializzati");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore InitializeCommands immobili: {ex.Message}");
            }
        }

        public void LoadImmobili()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CARICAMENTO IMMOBILI ===");

                var immobili = _context.Immobili
                    .AsNoTracking()
                    .Include(i => i.Documenti)
                    .Include(i => i.Foto)
                    .OrderByDescending(i => i.DataInserimento)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Caricati {immobili.Count} immobili dal database");

                Immobili.Clear();
                foreach (var immobile in immobili)
                {
                    Immobili.Add(immobile);
                }

                LoadFiltriData();
                System.Diagnostics.Debug.WriteLine($"LoadImmobili completato: {Immobili.Count} immobili nella UI");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore LoadImmobili: {ex}");
                MessageBox.Show($"Errore nel caricamento degli immobili: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFiltriData()
        {
            try
            {
                // Carica città uniche per il filtro
                CittaFiltro.Clear();
                CittaFiltro.Add("Tutti");

                var cittaUniche = _context.Immobili
                    .Where(i => !string.IsNullOrEmpty(i.Citta))
                    .Select(i => i.Citta)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                foreach (var citta in cittaUniche)
                {
                    CittaFiltro.Add(citta);
                }

                System.Diagnostics.Debug.WriteLine($"Caricati filtri: {cittaUniche.Count} città");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore LoadFiltriData: {ex.Message}");
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

                // Filtro per testo
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(i =>
                        i.Titolo.Contains(SearchText) ||
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

                var risultati = query.OrderByDescending(i => i.DataInserimento).ToList();

                Immobili.Clear();
                foreach (var immobile in risultati)
                {
                    Immobili.Add(immobile);
                }

                System.Diagnostics.Debug.WriteLine($"Filtri applicati: {risultati.Count} immobili trovati");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore FilterImmobili: {ex}");
                MessageBox.Show($"Errore nel filtro degli immobili: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshCurrentCollections()
        {
            try
            {
                DocumentiCorrente.Clear();
                FotoCorrente.Clear();
                ClientiInteressati.Clear();
                AppuntamentiImmobile.Clear();

                if (SelectedImmobile == null) return;

                // Carica documenti e foto
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

        // FIX: Metodo AddImmobile corretto con inizializzazione delle stringhe
        private void AddImmobile(object? parameter)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CREAZIONE NUOVO IMMOBILE ===");

                var newImmobile = new Immobile();

                // Log dei valori iniziali per debug
                System.Diagnostics.Debug.WriteLine($"Nuovo immobile - Titolo: '{newImmobile.Titolo}'");
                System.Diagnostics.Debug.WriteLine($"Nuovo immobile - Indirizzo: '{newImmobile.Indirizzo}'");
                System.Diagnostics.Debug.WriteLine($"Nuovo immobile - TipoImmobile: '{newImmobile.TipoImmobile}'");

                // Validazione PRIMA del salvataggio
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
                $"Sei sicuro di voler eliminare l'immobile '{SelectedImmobile.Titolo}'?\n\n" +
                "Questa operazione eliminerà anche tutti i documenti, foto e relazioni associate.",
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
                        var immobileToDelete = deleteContext.Immobili
                            .Include(i => i.Documenti)
                            .Include(i => i.Foto)
                            .FirstOrDefault(i => i.Id == SelectedImmobile.Id);

                        if (immobileToDelete != null)
                        {
                            deleteContext.Immobili.Remove(immobileToDelete);
                            deleteContext.SaveChanges();
                        }
                    }

                    SelectedImmobile = null;
                    LoadImmobili();

                    MessageBox.Show("Immobile eliminato con successo!", "Successo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nell'eliminazione dell'immobile: {ex.Message}",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    _isDeleting = false;
                }
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
                            "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    newContext.Appuntamenti.Add(newAppuntamento);
                    newContext.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"Appuntamento creato con ID: {newAppuntamento.Id}");
                }

                // Aggiorna la UI
                RefreshCurrentCollections();

                // Notifica la creazione dell'appuntamento agli altri ViewModels
                AppuntamentoCreated?.Invoke();

                MessageBox.Show("Appuntamento creato con successo!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Opzionale: naviga all'appuntamento appena creato
                NavigateToAppuntamento?.Invoke(newAppuntamento.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella creazione dell'appuntamento: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Errore CreaAppuntamento: {ex}");
            }
        }

        private void AddDocument(object? parameter)
        {
            if (SelectedImmobile == null) return;

            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Seleziona documento da aggiungere",
                    Filter = "Documenti|*.pdf;*.doc;*.docx;*.jpg;*.jpeg;*.png;*.bmp;*.tiff|" +
                            "PDF|*.pdf|" +
                            "Word|*.doc;*.docx|" +
                            "Immagini|*.jpg;*.jpeg;*.png;*.bmp;*.tiff|" +
                            "Tutti i file|*.*",
                    Multiselect = true
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var currentSettings = SettingsIntegrationHelper.CurrentSettings;

                    foreach (var fileName in openFileDialog.FileNames)
                    {
                        var fileInfo = new FileInfo(fileName);

                        // Controllo dimensione file
                        if (fileInfo.Length > currentSettings.MaxDocumentSize)
                        {
                            MessageBox.Show($"Il file '{fileInfo.Name}' supera la dimensione massima consentita ({MaxDocumentSizeFormatted}).",
                                "File troppo grande", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }

                        // Crea cartella documenti se non esiste
                        var documentsPath = Path.Combine(Environment.CurrentDirectory, "Documenti");
                        Directory.CreateDirectory(documentsPath);

                        // Copia il file
                        var newFileName = $"{SelectedImmobile.Id}_{DateTime.Now:yyyyMMdd_HHmmss}_{fileInfo.Name}";
                        var destinationPath = Path.Combine(documentsPath, newFileName);
                        File.Copy(fileName, destinationPath, true);

                        // Crea record nel database
                        var documento = new DocumentoImmobile
                        {
                            ImmobileId = SelectedImmobile.Id,
                            NomeFile = fileInfo.Name,
                            PercorsoFile = destinationPath,
                            TipoDocumento = "Documento Generico", // Potrà essere cambiato dall'utente
                            DataCaricamento = DateTime.Now,
                            Descrizione = ""
                        };

                        _context.Documenti.Add(documento);
                    }

                    _context.SaveChanges();
                    RefreshCurrentCollections();

                    MessageBox.Show($"Documenti aggiunti con successo!", "Successo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'aggiunta del documento: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddPhoto(object? parameter)
        {
            if (SelectedImmobile == null) return;

            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Seleziona foto da aggiungere",
                    Filter = "Immagini|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.gif;*.webp|Tutti i file|*.*",
                    Multiselect = true
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var currentSettings = SettingsIntegrationHelper.CurrentSettings;

                    foreach (var fileName in openFileDialog.FileNames)
                    {
                        var fileInfo = new FileInfo(fileName);

                        // Controllo dimensione file
                        if (fileInfo.Length > currentSettings.MaxPhotoSize)
                        {
                            MessageBox.Show($"Il file '{fileInfo.Name}' supera la dimensione massima consentita ({MaxPhotoSizeFormatted}).",
                                "File troppo grande", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }

                        // Crea cartella foto se non esiste
                        var photosPath = Path.Combine(Environment.CurrentDirectory, "Foto");
                        Directory.CreateDirectory(photosPath);

                        // Copia il file
                        var newFileName = $"{SelectedImmobile.Id}_{DateTime.Now:yyyyMMdd_HHmmss}_{fileInfo.Name}";
                        var destinationPath = Path.Combine(photosPath, newFileName);
                        File.Copy(fileName, destinationPath, true);

                        // Determina se è la prima foto (principale)
                        var isPrincipale = !FotoCorrente.Any();

                        // Crea record nel database
                        var foto = new FotoImmobile
                        {
                            ImmobileId = SelectedImmobile.Id,
                            NomeFile = fileInfo.Name,
                            PercorsoFile = destinationPath,
                            IsPrincipale = isPrincipale,
                            DataCaricamento = DateTime.Now,
                            Descrizione = ""
                        };

                        _context.Foto.Add(foto);
                    }

                    _context.SaveChanges();
                    RefreshCurrentCollections();

                    MessageBox.Show($"Foto aggiunte con successo!", "Successo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'aggiunta della foto: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    }
                    else
                    {
                        MessageBox.Show("File non trovato. Potrebbe essere stato spostato o eliminato.",
                            "File non trovato", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nell'apertura del documento: {ex.Message}",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteDocument(object? parameter)
        {
            if (parameter is DocumentoImmobile documento)
            {
                var result = MessageBox.Show($"Sei sicuro di voler eliminare il documento '{documento.NomeFile}'?",
                    "Conferma Eliminazione", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Elimina dal database
                        _context.Documenti.Remove(documento);
                        _context.SaveChanges();

                        // Elimina il file fisico
                        if (File.Exists(documento.PercorsoFile))
                        {
                            File.Delete(documento.PercorsoFile);
                        }

                        RefreshCurrentCollections();

                        MessageBox.Show("Documento eliminato con successo!", "Successo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore nell'eliminazione del documento: {ex.Message}",
                            "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeletePhoto(object? parameter)
        {
            if (parameter is FotoImmobile foto)
            {
                var result = MessageBox.Show($"Sei sicuro di voler eliminare la foto '{foto.NomeFile}'?",
                    "Conferma Eliminazione", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Elimina dal database
                        _context.Foto.Remove(foto);
                        _context.SaveChanges();

                        // Elimina il file fisico
                        if (File.Exists(foto.PercorsoFile))
                        {
                            File.Delete(foto.PercorsoFile);
                        }

                        RefreshCurrentCollections();

                        MessageBox.Show("Foto eliminata con successo!", "Successo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore nell'eliminazione della foto: {ex.Message}",
                            "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void AddClienteInteressato(object? parameter)
        {
            // Implementazione per aggiungere cliente interessato
            MessageBox.Show("Funzionalità in sviluppo", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteClienteInteressato(object? parameter)
        {
            // Implementazione per rimuovere cliente interessato
            MessageBox.Show("Funzionalità in sviluppo", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
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
                    var csv = "Titolo,Tipo,Indirizzo,Città,CAP,Provincia,Prezzo,Superficie,Locali,Stato Vendita,Classe Energetica,Data Inserimento\n";

                    foreach (var immobile in Immobili)
                    {
                        csv += $"\"{immobile.Titolo}\",\"{immobile.TipoImmobile}\",\"{immobile.Indirizzo}\"," +
                               $"\"{immobile.Citta}\",\"{immobile.CAP}\",\"{immobile.Provincia}\"," +
                               $"{immobile.Prezzo},{immobile.Superficie},{immobile.NumeroLocali}," +
                               $"\"{immobile.StatoVendita}\",\"{immobile.ClasseEnergetica}\"," +
                               $"{immobile.DataInserimento:yyyy-MM-dd}\n";
                    }

                    File.WriteAllText(saveFileDialog.FileName, csv, System.Text.Encoding.UTF8);

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
            MessageBox.Show("Funzionalità Import CSV - In sviluppo", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GeneraReport(object? parameter)
        {
            MessageBox.Show("Funzionalità Genera Report - In sviluppo", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearFiltri(object? parameter)
        {
            SearchText = string.Empty;
            FiltroTipo = "Tutti";
            FiltroStato = "Tutti";
            FiltroCitta = "Tutti";
            FiltroPrezzoDa = 0;
            FiltroPrezzoA = 0;

            LoadImmobili();
        }

        // METODI PER IL CLEANUP
        public void OnApplicationClosing()
        {
            try
            {
                if (!_isDeleting && SelectedImmobile != null)
                {
                    SaveCurrentImmobile();
                }
                System.Diagnostics.Debug.WriteLine("ImmobiliViewModel cleanup completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel salvataggio finale immobile: {ex.Message}");
            }
        }
    }
}