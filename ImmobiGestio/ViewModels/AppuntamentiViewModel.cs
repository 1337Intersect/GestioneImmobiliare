using ImmobiGestio.Commands;
using ImmobiGestio.Data;
using ImmobiGestio.Models;
using ImmobiGestio.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ImmobiGestio.ViewModels
{
    public class AppuntamentiViewModel : BaseViewModel
    {
        private readonly ImmobiliContext _context;
        private readonly OutlookService _outlookService;
        private readonly StatisticheService _statisticheService;
        private Appuntamento? _selectedAppuntamento;
        private string _searchText = string.Empty;
        private string _filtroStato = "Tutti";
        private string _filtroTipo = "Tutti";
        private DateTime _selectedDate = DateTime.Today;
        private string _vistaCalendario = "Mese";
        private bool _isDeleting = false;
        private bool _isOutlookConnected = false;

        public ObservableCollection<Appuntamento> Appuntamenti { get; set; } = new();
        public ObservableCollection<EventoCalendario> EventiCalendario { get; set; } = new();
        public ObservableCollection<Cliente> ClientiDisponibili { get; set; } = new();
        public ObservableCollection<Immobile> ImmobiliDisponibili { get; set; } = new();

        // Combo boxes data - SEMPLIFICATI
        public ObservableCollection<string> TipiAppuntamento { get; set; } = new();
        public ObservableCollection<string> StatiAppuntamento { get; set; } = new();
        public ObservableCollection<string> PrioritaAppuntamento { get; set; } = new();
        public ObservableCollection<string> TipiAppuntamentoFiltro { get; set; } = new();
        public ObservableCollection<string> StatiAppuntamentoFiltro { get; set; } = new();
        public ObservableCollection<CalendarDay> CalendarDays { get; set; } = new();

        public Appuntamento? SelectedAppuntamento
        {
            get => _selectedAppuntamento;
            set
            {
                if (_selectedAppuntamento != null && !_isDeleting)
                {
                    SaveCurrentAppuntamento();
                }

                SetProperty(ref _selectedAppuntamento, value);
                OnPropertyChanged(nameof(SelectedAppuntamento));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterAppuntamenti();
            }
        }

        public string FiltroStato
        {
            get => _filtroStato;
            set
            {
                SetProperty(ref _filtroStato, value);
                FilterAppuntamenti();
            }
        }

        public string FiltroTipo
        {
            get => _filtroTipo;
            set
            {
                SetProperty(ref _filtroTipo, value);
                FilterAppuntamenti();
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                SetProperty(ref _selectedDate, value);
                LoadEventiCalendario();
                GenerateMiniCalendarDays();
                GenerateMainCalendar(); // Aggiungi questa riga
                OnPropertyChanged(nameof(TitoloVista));
            }
        }

        public string VistaCalendario
        {
            get => _vistaCalendario;
            set
            {
                SetProperty(ref _vistaCalendario, value);
                LoadEventiCalendario();
                OnPropertyChanged(nameof(TitoloVista));
            }
        }

        public bool IsOutlookConnected
        {
            get => _isOutlookConnected;
            set => SetProperty(ref _isOutlookConnected, value);
        }

        // Proprietà calcolate per statistiche
        public int AppuntamentiCompletati => Appuntamenti?.Count(a => a.StatoAppuntamento == "Completato") ?? 0;
        public int AppuntamentiOggi => Appuntamenti?.Count(a => a.DataInizio.Date == DateTime.Today) ?? 0;

        // NUOVE PROPRIETÀ PER VISTA OUTLOOK
        // Proprietà per gli appuntamenti di oggi (per la sidebar)
        public ObservableCollection<Appuntamento> AppuntamentiOggiCollection
        {
            get
            {
                var oggi = DateTime.Today;
                var appuntamentiOggi = new ObservableCollection<Appuntamento>();

                foreach (var app in Appuntamenti.Where(a => a.DataInizio.Date == oggi).OrderBy(a => a.DataInizio))
                {
                    appuntamentiOggi.Add(app);
                }

                return appuntamentiOggi;
            }
        }

        // Proprietà per il titolo della vista
        public string TitoloVista
        {
            get
            {
                return VistaCalendario switch
                {
                    "Giorno" => $"{SelectedDate:dddd, dd MMMM yyyy}",
                    "Settimana" => GetTitoloSettimana(),
                    "Mese" => $"{SelectedDate:MMMM yyyy}",
                    _ => SelectedDate.ToString("MMMM yyyy")
                };
            }
        }

        // Commands
        public ICommand? AddAppuntamentoCommand { get; set; }
        public ICommand? SaveAppuntamentoCommand { get; set; }
        public ICommand? DeleteAppuntamentoCommand { get; set; }
        public ICommand? ConfermaAppuntamentoCommand { get; set; }
        public ICommand? CompletaAppuntamentoCommand { get; set; }
        public ICommand? AnnullaAppuntamentoCommand { get; set; }
        public ICommand? ConnectOutlookCommand { get; set; }
        public ICommand? SyncOutlookCommand { get; set; }
        public ICommand? CambiaVistaCommand { get; set; }
        public ICommand? PreviousPeriodCommand { get; set; }
        public ICommand? NextPeriodCommand { get; set; }
        public ICommand? TodayCommand { get; set; }
        public ICommand? EsportaCalendarioCommand { get; set; }
        public ICommand? SelectAppuntamentoCommand { get; set; }
        public ICommand? ClearFiltriCommand { get; set; }
        public ICommand? SetFiltroTipoCommand { get; set; }
        public ICommand? SetFiltroStatoCommand { get; set; }
        public ICommand? SelectDateCommand { get; set; }
        public ICommand? RefreshCommand { get; set; }
        public ObservableCollection<MiniCalendarDay> MiniCalendarDays { get; set; } = new();


        public AppuntamentiViewModel(ImmobiliContext context)
        {
            _context = context;
            _outlookService = new OutlookService();
            _statisticheService = new StatisticheService(context);

            InitializeCollections();
            InitializeCommands();
            LoadAllData();

            // Inizializza le notifiche per le nuove proprietà
            OnPropertyChanged(nameof(AppuntamentiOggiCollection));
            OnPropertyChanged(nameof(TitoloVista));
        }

        private void InitializeCollections()
        {
            try
            {
                // CORRETTO - usa le classi statiche invece delle ObservableCollection
                TipiAppuntamento.Clear();
                TipiAppuntamentoFiltro.Clear();
                TipiAppuntamentoFiltro.Add("Tutti");

                // USA LE CLASSI STATICHE - CORRETTO!
                foreach (var tipo in Models.TipiAppuntamento.GetAll())
                {
                    TipiAppuntamento.Add(tipo);
                    TipiAppuntamentoFiltro.Add(tipo);
                }

                StatiAppuntamento.Clear();
                StatiAppuntamentoFiltro.Clear();
                StatiAppuntamentoFiltro.Add("Tutti");

                // USA LE CLASSI STATICHE - CORRETTO!
                foreach (var stato in Models.StatiAppuntamento.GetAll())
                {
                    StatiAppuntamento.Add(stato);
                    StatiAppuntamentoFiltro.Add(stato);
                }

                PrioritaAppuntamento.Clear();
                // USA LE CLASSI STATICHE - CORRETTO!
                foreach (var priorita in Models.PrioritaAppuntamento.GetAll())
                {
                    PrioritaAppuntamento.Add(priorita);
                }

                System.Diagnostics.Debug.WriteLine($"Collezioni inizializzate: Tipi={TipiAppuntamento.Count}, Stati={StatiAppuntamento.Count}, Priorità={PrioritaAppuntamento.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore InitializeCollections: {ex.Message}");
            }
        }

        private void InitializeCommands()
        {
            AddAppuntamentoCommand = new RelayCommand(AddAppuntamento);
            SaveAppuntamentoCommand = new RelayCommand(SaveAppuntamento, _ => SelectedAppuntamento != null);
            DeleteAppuntamentoCommand = new RelayCommand(DeleteAppuntamento, _ => SelectedAppuntamento != null);
            ConfermaAppuntamentoCommand = new RelayCommand(ConfermaAppuntamento, _ => SelectedAppuntamento?.StatoAppuntamento == "Programmato");
            CompletaAppuntamentoCommand = new RelayCommand(CompletaAppuntamento, _ => SelectedAppuntamento?.StatoAppuntamento == "Confermato" || SelectedAppuntamento?.StatoAppuntamento == "Programmato");
            AnnullaAppuntamentoCommand = new RelayCommand(AnnullaAppuntamento, _ => SelectedAppuntamento?.PuoEssereCancellato() == true);
            ConnectOutlookCommand = new RelayCommand(async _ => await ConnectOutlook());
            SyncOutlookCommand = new RelayCommand(async _ => await SyncWithOutlook(), _ => IsOutlookConnected);
            CambiaVistaCommand = new RelayCommand(CambiaVista);
            PreviousPeriodCommand = new RelayCommand(PreviousPeriod);
            NextPeriodCommand = new RelayCommand(NextPeriod);
            TodayCommand = new RelayCommand(_ => { SelectedDate = DateTime.Today; });
            EsportaCalendarioCommand = new RelayCommand(EsportaCalendario);
            SetFiltroTipoCommand = new RelayCommand(param => {
                if (param is string tipo)
                {
                    FiltroTipo = tipo;
                }
            });

            SetFiltroStatoCommand = new RelayCommand(param => {
                if (param is string stato)
                {
                    FiltroStato = stato;
                }
            });

            SelectDateCommand = new RelayCommand(param => {
                if (param is DateTime date)
                {
                    SelectedDate = date;
                }
            });

            RefreshCommand = new RelayCommand(_ => LoadAllData());
            SelectAppuntamentoCommand = new RelayCommand(param => {
                if (param is Appuntamento app)
                {
                    SelectedAppuntamento = app;
                }
            });
            ClearFiltriCommand = new RelayCommand(_ => ClearFiltri());
        }

        public void LoadAllData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CARICAMENTO COMPLETO DATI APPUNTAMENTI ===");

                LoadClientiEImmobiliDisponibili();
                LoadAppuntamenti();
                LoadEventiCalendario();

                System.Diagnostics.Debug.WriteLine("Caricamento completo dati completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore LoadAllData: {ex.Message}");
                MessageBox.Show($"Errore nel caricamento dei dati: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadAppuntamenti()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CARICAMENTO APPUNTAMENTI ===");

                var appuntamenti = _context.Appuntamenti
                    .AsNoTracking()
                    .Include(a => a.Cliente)
                    .Include(a => a.Immobile)
                    .OrderBy(a => a.DataInizio)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Caricati {appuntamenti.Count} appuntamenti dal database");

                Appuntamenti.Clear();
                foreach (var appuntamento in appuntamenti)
                {
                    // Fix automatico dei valori mancanti
                    if (string.IsNullOrEmpty(appuntamento.TipoAppuntamento))
                        appuntamento.TipoAppuntamento = Models.TipiAppuntamento.Visita;
                    if (string.IsNullOrEmpty(appuntamento.StatoAppuntamento))
                        appuntamento.StatoAppuntamento = Models.StatiAppuntamento.Programmato;
                    if (string.IsNullOrEmpty(appuntamento.Priorita))
                        appuntamento.Priorita = Models.PrioritaAppuntamento.Media;

                    Appuntamenti.Add(appuntamento);
                }

                OnPropertyChanged(nameof(AppuntamentiCompletati));
                OnPropertyChanged(nameof(AppuntamentiOggi));
                OnPropertyChanged(nameof(AppuntamentiOggiCollection));

                System.Diagnostics.Debug.WriteLine($"LoadAppuntamenti completato: {Appuntamenti.Count} appuntamenti caricati");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore LoadAppuntamenti: {ex}");
                MessageBox.Show($"Errore nel caricamento degli appuntamenti: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            GenerateMiniCalendarDays();
            GenerateMainCalendar();
        }

        public void LoadClientiEImmobiliDisponibili()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CARICAMENTO CLIENTI E IMMOBILI DISPONIBILI ===");

                // Carica TUTTI i clienti
                var clienti = _context.Clienti
                    .AsNoTracking()
                    .OrderBy(c => c.Nome)
                    .ThenBy(c => c.Cognome)
                    .ToList();

                ClientiDisponibili.Clear();
                foreach (var cliente in clienti)
                {
                    ClientiDisponibili.Add(cliente);
                }
                System.Diagnostics.Debug.WriteLine($"Caricati {ClientiDisponibili.Count} clienti");

                // Carica TUTTI gli immobili
                var immobili = _context.Immobili
                    .AsNoTracking()
                    .OrderBy(i => i.Titolo)
                    .ToList();

                ImmobiliDisponibili.Clear();
                foreach (var immobile in immobili)
                {
                    ImmobiliDisponibili.Add(immobile);
                }
                System.Diagnostics.Debug.WriteLine($"Caricati {ImmobiliDisponibili.Count} immobili");

                OnPropertyChanged(nameof(ClientiDisponibili));
                OnPropertyChanged(nameof(ImmobiliDisponibili));

                System.Diagnostics.Debug.WriteLine("Caricamento clienti e immobili completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore LoadClientiEImmobiliDisponibili: {ex.Message}");
                MessageBox.Show($"Errore nel caricamento di clienti e immobili: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterAppuntamenti()
        {
            try
            {
                var query = _context.Appuntamenti
                    .AsNoTracking()
                    .Include(a => a.Cliente)
                    .Include(a => a.Immobile)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(a =>
                        a.Titolo.Contains(SearchText) ||
                        a.Descrizione.Contains(SearchText) ||
                        a.Luogo.Contains(SearchText) ||
                        (a.Cliente != null && (a.Cliente.Nome.Contains(SearchText) || a.Cliente.Cognome.Contains(SearchText))));
                }

                if (FiltroStato != "Tutti")
                {
                    query = query.Where(a => a.StatoAppuntamento == FiltroStato);
                }

                if (FiltroTipo != "Tutti")
                {
                    query = query.Where(a => a.TipoAppuntamento == FiltroTipo);
                }

                var filtered = query.OrderBy(a => a.DataInizio).ToList();

                Appuntamenti.Clear();
                foreach (var appuntamento in filtered)
                {
                    Appuntamenti.Add(appuntamento);
                }

                OnPropertyChanged(nameof(AppuntamentiCompletati));
                OnPropertyChanged(nameof(AppuntamentiOggi));
                OnPropertyChanged(nameof(AppuntamentiOggiCollection));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella ricerca appuntamenti: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadEventiCalendario()
        {
            try
            {
                DateTime start, end;

                switch (VistaCalendario)
                {
                    case "Giorno":
                        start = SelectedDate.Date;
                        end = start.AddDays(1);
                        break;
                    case "Settimana":
                        start = GetLunediSettimana(SelectedDate);
                        end = start.AddDays(7);
                        break;
                    case "Mese":
                        start = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
                        end = start.AddMonths(1);
                        break;
                    default:
                        start = SelectedDate.Date;
                        end = SelectedDate.Date.AddDays(7);
                        break;
                }

                var eventi = _statisticheService.GetEventiCalendario(start, end);

                EventiCalendario.Clear();
                foreach (var evento in eventi)
                {
                    EventiCalendario.Add(evento);
                }

                // Aggiorna anche il calendario principale
                GenerateMainCalendar();

                // Notifica il cambiamento degli eventi di oggi
                OnPropertyChanged(nameof(AppuntamentiOggiCollection));
                OnPropertyChanged(nameof(TitoloVista));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento del calendario: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DateTime GetLunediSettimana(DateTime data)
        {
            var dayOfWeek = (int)data.DayOfWeek;
            // In C#, Sunday = 0, Monday = 1, ..., Saturday = 6
            // Vogliamo che Monday = 0
            var daysFromMonday = (dayOfWeek == 0) ? 6 : dayOfWeek - 1;
            return data.AddDays(-daysFromMonday);
        }

        private string GetTitoloSettimana()
        {
            var lunedi = SelectedDate.AddDays(-(int)SelectedDate.DayOfWeek + 1);
            var domenica = lunedi.AddDays(6);

            if (lunedi.Month == domenica.Month)
            {
                return $"{lunedi:dd} - {domenica:dd} {lunedi:MMMM yyyy}";
            }
            else if (lunedi.Year == domenica.Year)
            {
                return $"{lunedi:dd MMM} - {domenica:dd MMM yyyy}";
            }
            else
            {
                return $"{lunedi:dd MMM yyyy} - {domenica:dd MMM yyyy}";
            }
        }

        private void AddAppuntamento(object? parameter)
        {
            try
            {
                var newAppuntamento = new Appuntamento();

                System.Diagnostics.Debug.WriteLine($"=== CREAZIONE NUOVO APPUNTAMENTO ===");
                System.Diagnostics.Debug.WriteLine($"TipoAppuntamento: '{newAppuntamento.TipoAppuntamento}'");
                System.Diagnostics.Debug.WriteLine($"StatoAppuntamento: '{newAppuntamento.StatoAppuntamento}'");
                System.Diagnostics.Debug.WriteLine($"Priorita: '{newAppuntamento.Priorita}'");

                if (!newAppuntamento.IsValid())
                {
                    MessageBox.Show("Errore: L'appuntamento non è valido.",
                        "Errore Validazione", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (var newContext = new ImmobiliContext())
                {
                    newContext.Appuntamenti.Add(newAppuntamento);
                    var saved = newContext.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"Appuntamento creato con ID: {newAppuntamento.Id}");
                }

                LoadAppuntamenti();

                var createdAppuntamento = Appuntamenti.FirstOrDefault(a => a.Id == newAppuntamento.Id);
                if (createdAppuntamento != null)
                {
                    SelectedAppuntamento = createdAppuntamento;
                }

                MessageBox.Show("Nuovo appuntamento creato con successo!",
                    "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella creazione dell'appuntamento: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Errore AddAppuntamento: {ex}");
            }
        }

        private void SaveCurrentAppuntamento()
        {
            if (SelectedAppuntamento == null) return;

            try
            {
                if (!SelectedAppuntamento.IsValid())
                {
                    System.Diagnostics.Debug.WriteLine("Appuntamento non valido, salto il salvataggio");
                    return;
                }

                SelectedAppuntamento.DataUltimaModifica = DateTime.Now;

                using (var saveContext = new ImmobiliContext())
                {
                    var existingAppuntamento = saveContext.Appuntamenti
                        .FirstOrDefault(a => a.Id == SelectedAppuntamento.Id);

                    if (existingAppuntamento != null)
                    {
                        // AGGIORNAMENTO
                        existingAppuntamento.Titolo = SelectedAppuntamento.Titolo;
                        existingAppuntamento.Descrizione = SelectedAppuntamento.Descrizione;
                        existingAppuntamento.DataInizio = SelectedAppuntamento.DataInizio;
                        existingAppuntamento.DataFine = SelectedAppuntamento.DataFine;
                        existingAppuntamento.Luogo = SelectedAppuntamento.Luogo;
                        existingAppuntamento.TipoAppuntamento = SelectedAppuntamento.TipoAppuntamento;
                        existingAppuntamento.StatoAppuntamento = SelectedAppuntamento.StatoAppuntamento;
                        existingAppuntamento.Priorita = SelectedAppuntamento.Priorita;
                        existingAppuntamento.ClienteId = SelectedAppuntamento.ClienteId;
                        existingAppuntamento.ImmobileId = SelectedAppuntamento.ImmobileId;
                        existingAppuntamento.NotePrivate = SelectedAppuntamento.NotePrivate;
                        existingAppuntamento.EsitoIncontro = SelectedAppuntamento.EsitoIncontro;
                        existingAppuntamento.RichiedeConferma = SelectedAppuntamento.RichiedeConferma;
                        existingAppuntamento.DataUltimaModifica = DateTime.Now;

                        saveContext.Update(existingAppuntamento);
                        System.Diagnostics.Debug.WriteLine($"Aggiornamento appuntamento ID {SelectedAppuntamento.Id}");
                    }
                    else
                    {
                        saveContext.Appuntamenti.Add(SelectedAppuntamento);
                        System.Diagnostics.Debug.WriteLine($"Nuovo appuntamento in salvataggio");
                    }

                    var saved = saveContext.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"SaveCurrentAppuntamento completato: {saved} record salvati");
                }

                if (IsOutlookConnected && SelectedAppuntamento.SincronizzatoOutlook)
                {
                    Task.Run(async () => await _outlookService.UpdateAppuntamentoAsync(SelectedAppuntamento));
                }
            }
            catch (Exception ex)
            {
                if (!_isDeleting)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore SaveCurrentAppuntamento: {ex}");
                    MessageBox.Show($"Errore nel salvataggio dell'appuntamento: {ex.Message}",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveAppuntamento(object? parameter)
        {
            if (SelectedAppuntamento == null)
            {
                MessageBox.Show("Nessun appuntamento selezionato da salvare.",
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (!SelectedAppuntamento.IsValid())
                {
                    MessageBox.Show("L'appuntamento contiene dati non validi.\n" +
                                   "Controlla che tutti i campi obbligatori siano compilati.",
                        "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveCurrentAppuntamento();
                LoadEventiCalendario();

                MessageBox.Show("Appuntamento salvato con successo!",
                    "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel salvataggio dell'appuntamento: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Errore SaveAppuntamento: {ex}");
            }
        }

        private void DeleteAppuntamento(object? parameter)
        {
            if (SelectedAppuntamento == null) return;

            var result = MessageBox.Show(
                $"Sei sicuro di voler eliminare l'appuntamento '{SelectedAppuntamento.Titolo}'?\n\n" +
                $"Data: {SelectedAppuntamento.DataInizio:dd/MM/yyyy HH:mm}\n" +
                $"Questa operazione non può essere annullata.",
                "Conferma Eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _isDeleting = true;
                    var appuntamentoId = SelectedAppuntamento.Id;
                    var appuntamentoTitolo = SelectedAppuntamento.Titolo;
                    var outlookEventId = SelectedAppuntamento.OutlookEventId;

                    using (var deleteContext = new ImmobiliContext())
                    {
                        var appuntamentoToDelete = deleteContext.Appuntamenti
                            .FirstOrDefault(a => a.Id == appuntamentoId);

                        if (appuntamentoToDelete != null)
                        {
                            deleteContext.Appuntamenti.Remove(appuntamentoToDelete);
                            var deletedRecords = deleteContext.SaveChanges();
                            System.Diagnostics.Debug.WriteLine($"Eliminazione completata: {deletedRecords} record eliminati");
                        }
                    }

                    if (IsOutlookConnected && !string.IsNullOrEmpty(outlookEventId))
                    {
                        Task.Run(async () => await _outlookService.DeleteAppuntamentoAsync(outlookEventId));
                    }

                    var uiAppuntamento = Appuntamenti.FirstOrDefault(a => a.Id == appuntamentoId);
                    if (uiAppuntamento != null)
                    {
                        Appuntamenti.Remove(uiAppuntamento);
                    }

                    SelectedAppuntamento = Appuntamenti.FirstOrDefault();
                    LoadEventiCalendario();

                    OnPropertyChanged(nameof(AppuntamentiCompletati));
                    OnPropertyChanged(nameof(AppuntamentiOggi));
                    OnPropertyChanged(nameof(AppuntamentiOggiCollection));

                    MessageBox.Show($"Appuntamento '{appuntamentoTitolo}' eliminato con successo!",
                        "Eliminazione Completata", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nell'eliminazione dell'appuntamento: {ex.Message}",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoadAppuntamenti();
                }
                finally
                {
                    _isDeleting = false;
                }
            }
        }

        private void ConfermaAppuntamento(object? parameter)
        {
            if (SelectedAppuntamento != null && SelectedAppuntamento.StatoAppuntamento == "Programmato")
            {
                SelectedAppuntamento.Conferma();
                SaveCurrentAppuntamento();
                LoadEventiCalendario();

                MessageBox.Show("Appuntamento confermato con successo!",
                    "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CompletaAppuntamento(object? parameter)
        {
            if (SelectedAppuntamento != null && SelectedAppuntamento.StatoAppuntamento != "Completato")
            {
                var result = MessageBox.Show(
                    "Vuoi aggiungere delle note per questo appuntamento completato?",
                    "Completa Appuntamento",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                string esito = "";
                if (result == MessageBoxResult.Yes)
                {
                    esito = "Appuntamento completato con successo";
                }

                SelectedAppuntamento.Completa(esito);
                SaveCurrentAppuntamento();
                LoadEventiCalendario();

                OnPropertyChanged(nameof(AppuntamentiCompletati));
                OnPropertyChanged(nameof(AppuntamentiOggi));
                OnPropertyChanged(nameof(AppuntamentiOggiCollection));

                MessageBox.Show("Appuntamento completato con successo!",
                    "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AnnullaAppuntamento(object? parameter)
        {
            if (SelectedAppuntamento != null && SelectedAppuntamento.PuoEssereCancellato())
            {
                var result = MessageBox.Show(
                    "Sei sicuro di voler annullare questo appuntamento?",
                    "Conferma Annullamento",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SelectedAppuntamento.Annulla();
                    SaveCurrentAppuntamento();
                    LoadEventiCalendario();

                    MessageBox.Show("Appuntamento annullato!",
                        "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async Task ConnectOutlook()
        {
            try
            {
                var connected = await _outlookService.InitializeAsync();
                IsOutlookConnected = connected;

                if (connected)
                {
                    MessageBox.Show("Connessione con Outlook stabilita con successo!",
                        "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await SyncWithOutlook();
                }
                else
                {
                    MessageBox.Show("Impossibile connettersi a Outlook. Verifica le credenziali.",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella connessione a Outlook: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SyncWithOutlook()
        {
            if (!IsOutlookConnected) return;

            try
            {
                var appuntamentiDaSincronizzare = _context.Appuntamenti
                    .Where(a => !a.SincronizzatoOutlook && a.StatoAppuntamento != "Annullato")
                    .ToList();

                int sincronizzati = 0;
                foreach (var appuntamento in appuntamentiDaSincronizzare)
                {
                    var outlookEventId = await _outlookService.CreateAppuntamentoAsync(appuntamento);
                    if (!string.IsNullOrEmpty(outlookEventId))
                    {
                        appuntamento.OutlookEventId = outlookEventId;
                        appuntamento.SincronizzatoOutlook = true;
                        sincronizzati++;
                    }
                }

                if (sincronizzati > 0)
                {
                    _context.SaveChanges();
                }

                MessageBox.Show($"Sincronizzazione completata: {sincronizzati} appuntamenti sincronizzati.",
                    "Sincronizzazione", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadEventiCalendario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella sincronizzazione: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CambiaVista(object? parameter)
        {
            if (parameter is string vista)
            {
                VistaCalendario = vista;
            }
        }

        private void PreviousPeriod(object? parameter)
        {
            SelectedDate = VistaCalendario switch
            {
                "Giorno" => SelectedDate.AddDays(-1),
                "Settimana" => SelectedDate.AddDays(-7),
                "Mese" => SelectedDate.AddMonths(-1),
                _ => SelectedDate.AddDays(-7)
            };
        }

        private void NextPeriod(object? parameter)
        {
            SelectedDate = VistaCalendario switch
            {
                "Giorno" => SelectedDate.AddDays(1),
                "Settimana" => SelectedDate.AddDays(7),
                "Mese" => SelectedDate.AddMonths(1),
                _ => SelectedDate.AddDays(7)
            };
        }

        private void EsportaCalendario(object? parameter)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"Calendario_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = "Titolo,Data Inizio,Data Fine,Cliente,Immobile,Tipo,Stato,Luogo\n";

                    foreach (var appuntamento in Appuntamenti)
                    {
                        csv += $"{appuntamento.Titolo},{appuntamento.DataInizio:yyyy-MM-dd HH:mm}," +
                               $"{appuntamento.DataFine:yyyy-MM-dd HH:mm}," +
                               $"{appuntamento.Cliente?.NomeCompleto ?? "N/A"}," +
                               $"{appuntamento.Immobile?.Titolo ?? "N/A"}," +
                               $"{appuntamento.TipoAppuntamento},{appuntamento.StatoAppuntamento}," +
                               $"{appuntamento.Luogo}\n";
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, csv);

                    MessageBox.Show($"Calendario esportato con successo in {saveFileDialog.FileName}",
                        "Esportazione Completata", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'esportazione: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearFiltri()
        {
            SearchText = string.Empty;
            FiltroStato = "Tutti";
            FiltroTipo = "Tutti";
            SelectedDate = DateTime.Today;
        }

        public void RefreshAll()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== REFRESH COMPLETO APPUNTAMENTI ===");
                LoadAllData();
                System.Diagnostics.Debug.WriteLine("Refresh completo completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore RefreshAll: {ex.Message}");
            }
        }

        public void OnApplicationClosing()
        {
            try
            {
                if (!_isDeleting && SelectedAppuntamento != null)
                {
                    SaveCurrentAppuntamento();
                }
                _outlookService?.Disconnect();

                System.Diagnostics.Debug.WriteLine("AppuntamentiViewModel: Cleanup completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel salvataggio finale appuntamento: {ex.Message}");
            }
        }

        // METODI HELPER PER VISTA OUTLOOK
        public string GetColorePerTipo(string tipo)
        {
            return tipo switch
            {
                "Visita" => "#0078D4",      // Blu Outlook
                "Incontro" => "#107C10",    // Verde
                "Firma" => "#D83B01",       // Arancione/Rosso
                "Valutazione" => "#8764B8", // Viola
                "Sopralluogo" => "#E3008C", // Magenta
                "Chiamata" => "#00BCF2",    // Azzurro
                _ => "#0078D4"
            };
        }

        public string GetColorePerStato(string stato)
        {
            return stato switch
            {
                "Programmato" => "#0078D4", // Blu
                "Confermato" => "#107C10",  // Verde
                "Completato" => "#8A8886",  // Grigio
                "Annullato" => "#D83B01",   // Rosso
                _ => "#0078D4"
            };
        }
        private void GenerateMiniCalendarDays()
        {
            try
            {
                MiniCalendarDays.Clear();

                var firstDayOfMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // Inizia dalla domenica precedente al primo giorno del mese
                var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);

                // 42 giorni = 6 settimane per coprire tutti i casi
                for (int i = 0; i < 42; i++)
                {
                    var currentDate = startDate.AddDays(i);

                    var day = new MiniCalendarDay
                    {
                        Date = currentDate,
                        Day = currentDate.Day,
                        IsToday = currentDate.Date == DateTime.Today,
                        IsSelected = currentDate.Date == SelectedDate.Date,
                        IsCurrentMonth = currentDate.Month == SelectedDate.Month,
                        IsWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday ||
                                   currentDate.DayOfWeek == DayOfWeek.Sunday
                    };

                    // Controlla se ci sono eventi in questo giorno
                    day.HasEvents = Appuntamenti.Any(a => a.DataInizio.Date == currentDate.Date);
                    day.EventCount = Appuntamenti.Count(a => a.DataInizio.Date == currentDate.Date);

                    MiniCalendarDays.Add(day);
                }

                OnPropertyChanged(nameof(MiniCalendarDays));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore GenerateMiniCalendarDays: {ex.Message}");
            }
        }
        public class CalendarDay : INotifyPropertyChanged
        {
            private DateTime _date;
            private int _dayNumber;
            private bool _isToday = false;
            private bool _isCurrentMonth = true;
            private ObservableCollection<Appuntamento> _eventi = new();

            public DateTime Date
            {
                get => _date;
                set => SetProperty(ref _date, value);
            }

            public int DayNumber
            {
                get => _dayNumber;
                set => SetProperty(ref _dayNumber, value);
            }

            public bool IsToday
            {
                get => _isToday;
                set => SetProperty(ref _isToday, value);
            }

            public bool IsCurrentMonth
            {
                get => _isCurrentMonth;
                set => SetProperty(ref _isCurrentMonth, value);
            }

            public ObservableCollection<Appuntamento> Eventi
            {
                get => _eventi;
                set => SetProperty(ref _eventi, value);
            }

            public string BackgroundColor
            {
                get
                {
                    if (IsToday) return "#E3F2FD";
                    return "White";
                }
            }

            public string TextColor
            {
                get
                {
                    if (!IsCurrentMonth) return "#A19F9D";
                    if (IsToday) return "#0078D4";
                    return "#323130";
                }
            }

            public string FontWeight
            {
                get
                {
                    if (IsToday) return "Bold";
                    return "Normal";
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
            {
                if (Equals(storage, value)) return false;
                storage = value;
                OnPropertyChanged(propertyName);
                return true;
            }
        }
        private void GenerateMainCalendar()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== GENERAZIONE CALENDARIO PRINCIPALE ===");

                CalendarDays.Clear();

                var firstDayOfMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // Inizia dalla domenica precedente al primo giorno del mese
                var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);

                // 42 giorni = 6 settimane per coprire tutti i casi
                for (int i = 0; i < 42; i++)
                {
                    var currentDate = startDate.AddDays(i);

                    var calendarDay = new CalendarDay
                    {
                        Date = currentDate,
                        DayNumber = currentDate.Day,
                        IsToday = currentDate.Date == DateTime.Today,
                        IsCurrentMonth = currentDate.Month == SelectedDate.Month
                    };

                    // Trova gli eventi per questo giorno
                    var eventiDelGiorno = Appuntamenti
                        .Where(a => a.DataInizio.Date == currentDate.Date)
                        .OrderBy(a => a.DataInizio)
                        .Take(3) // Massimo 3 eventi visibili per giorno
                        .ToList();

                    foreach (var evento in eventiDelGiorno)
                    {
                        calendarDay.Eventi.Add(evento);
                    }

                    CalendarDays.Add(calendarDay);

                    System.Diagnostics.Debug.WriteLine($"Giorno {currentDate:dd/MM}: {eventiDelGiorno.Count} eventi");
                }

                OnPropertyChanged(nameof(CalendarDays));
                System.Diagnostics.Debug.WriteLine($"Calendario principale generato: {CalendarDays.Count} giorni");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore GenerateMainCalendar: {ex.Message}");
            }
        }
    }
}