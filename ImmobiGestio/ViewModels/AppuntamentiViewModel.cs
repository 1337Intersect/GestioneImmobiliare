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
using System.Collections.Generic;

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
        public bool IsListView { get; set; } = false;
        public bool IsCalendarView { get; set; } = true;

        public ObservableCollection<Appuntamento> Appuntamenti { get; set; } = new();
        public ObservableCollection<EventoCalendario> EventiCalendario { get; set; } = new();
        public ObservableCollection<Cliente> ClientiDisponibili { get; set; } = new();
        public ObservableCollection<Immobile> ImmobiliDisponibili { get; set; } = new();

        // Combo boxes data - CORRETTI CON COSTANTI
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
                GenerateMainCalendar();
                FilterAppuntamenti();
            }
        }

        public string VistaCalendario
        {
            get => _vistaCalendario;
            set
            {
                SetProperty(ref _vistaCalendario, value);
                GenerateMainCalendar();
            }
        }

        public bool IsOutlookConnected
        {
            get => _isOutlookConnected;
            set => SetProperty(ref _isOutlookConnected, value);
        }

        // Proprietà calcolate
        public int AppuntamentiCompletati => Appuntamenti.Count(a => a.StatoAppuntamento == Models.StatiAppuntamento.Completato);
        public int AppuntamentiOggi => Appuntamenti.Count(a => a.DataInizio.Date == DateTime.Today);
        public ObservableCollection<Appuntamento> AppuntamentiOggiCollection =>
            new(Appuntamenti.Where(a => a.DataInizio.Date == DateTime.Today).OrderBy(a => a.DataInizio));

        // Commands
        public ICommand? AddAppuntamentoCommand { get; set; }
        public ICommand? SaveAppuntamentoCommand { get; set; }
        public ICommand? DeleteAppuntamentoCommand { get; set; }
        public ICommand? MarkAsCompletedCommand { get; set; }
        public ICommand? MarkAsConfirmedCommand { get; set; }
        public ICommand? CancelAppuntamentoCommand { get; set; }
        public ICommand? SyncOutlookCommand { get; set; }
        public ICommand? ConnectOutlookCommand { get; set; }
        public ICommand? PreviousMonthCommand { get; set; }
        public ICommand? NextMonthCommand { get; set; }
        public ICommand? TodayCommand { get; set; }
        public ICommand? ToggleViewCommand { get; set; }
        public ICommand? ClearFiltriCommand { get; set; }
        public ICommand? EsportaAppuntamentiCommand { get; set; }

        public AppuntamentiViewModel(ImmobiliContext context)
        {
            _context = context;
            _outlookService = new OutlookService();
            _statisticheService = new StatisticheService(context);

            InitializeItalianCollections();
            InitializeCommands();
            LoadAllData();
            InitializeOutlook();
        }

        private void InitializeItalianCollections()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INIZIALIZZAZIONE COLLEZIONI APPUNTAMENTI ===");

                // USA LE COSTANTI dal Model Appuntamento
                TipiAppuntamento.Clear();
                TipiAppuntamentoFiltro.Clear();
                TipiAppuntamentoFiltro.Add("Tutti");

                foreach (var tipo in Models.TipiAppuntamento.All)
                {
                    TipiAppuntamento.Add(tipo);
                    TipiAppuntamentoFiltro.Add(tipo);
                }

                StatiAppuntamento.Clear();
                StatiAppuntamentoFiltro.Clear();
                StatiAppuntamentoFiltro.Add("Tutti");

                foreach (var stato in Models.StatiAppuntamento.All)
                {
                    StatiAppuntamento.Add(stato);
                    StatiAppuntamentoFiltro.Add(stato);
                }

                PrioritaAppuntamento.Clear();
                foreach (var priorita in Models.PrioritaAppuntamento.All)
                {
                    PrioritaAppuntamento.Add(priorita);
                }

                System.Diagnostics.Debug.WriteLine("Collezioni appuntamenti inizializzate con costanti");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore InitializeItalianCollections appuntamenti: {ex.Message}");
            }
        }

        private void InitializeCommands()
        {
            try
            {
                AddAppuntamentoCommand = new RelayCommand(AddAppuntamento);
                SaveAppuntamentoCommand = new RelayCommand(SaveAppuntamento, _ => SelectedAppuntamento != null);
                DeleteAppuntamentoCommand = new RelayCommand(DeleteAppuntamento, _ => SelectedAppuntamento != null);
                MarkAsCompletedCommand = new RelayCommand(MarkAsCompleted, _ => SelectedAppuntamento != null);
                MarkAsConfirmedCommand = new RelayCommand(MarkAsConfirmed, _ => SelectedAppuntamento != null);
                CancelAppuntamentoCommand = new RelayCommand(CancelAppuntamento, _ => SelectedAppuntamento != null);
                SyncOutlookCommand = new RelayCommand(SyncOutlook);
                ConnectOutlookCommand = new RelayCommand(ConnectOutlook);
                PreviousMonthCommand = new RelayCommand(PreviousMonth);
                NextMonthCommand = new RelayCommand(NextMonth);
                TodayCommand = new RelayCommand(GoToToday);
                ToggleViewCommand = new RelayCommand(ToggleView);
                ClearFiltriCommand = new RelayCommand(ClearFiltri);
                EsportaAppuntamentiCommand = new RelayCommand(EsportaAppuntamenti);

                System.Diagnostics.Debug.WriteLine("Comandi appuntamenti inizializzati");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore InitializeCommands appuntamenti: {ex.Message}");
            }
        }

        public void LoadAllData()
        {
            LoadAppuntamenti();
            LoadClientiEImmobiliDisponibili();
            GenerateMiniCalendarDays();
            GenerateMainCalendar();
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
                    // FIX AUTOMATICO per appuntamenti con valori non validi
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

                System.Diagnostics.Debug.WriteLine("Caricamento clienti e immobili completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore LoadClientiEImmobiliDisponibili: {ex}");
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

                // Filtro per testo
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(a =>
                        a.Titolo.Contains(SearchText) ||
                        a.Descrizione.Contains(SearchText) ||
                        a.Luogo.Contains(SearchText) ||
                        (a.Cliente != null && (a.Cliente.Nome.Contains(SearchText) || a.Cliente.Cognome.Contains(SearchText))) ||
                        (a.Immobile != null && a.Immobile.Titolo.Contains(SearchText)));
                }

                // Filtro per stato
                if (FiltroStato != "Tutti")
                {
                    query = query.Where(a => a.StatoAppuntamento == FiltroStato);
                }

                // Filtro per tipo
                if (FiltroTipo != "Tutti")
                {
                    query = query.Where(a => a.TipoAppuntamento == FiltroTipo);
                }

                // Filtro per data (mostra solo il mese selezionato)
                if (VistaCalendario == "Mese")
                {
                    var startOfMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                    query = query.Where(a => a.DataInizio.Date >= startOfMonth && a.DataInizio.Date <= endOfMonth);
                }

                var risultati = query.OrderBy(a => a.DataInizio).ToList();

                Appuntamenti.Clear();
                foreach (var appuntamento in risultati)
                {
                    // Fix automatico come nel LoadAppuntamenti
                    if (string.IsNullOrEmpty(appuntamento.TipoAppuntamento))
                        appuntamento.TipoAppuntamento = Models.TipiAppuntamento.Visita;
                    if (string.IsNullOrEmpty(appuntamento.StatoAppuntamento))
                        appuntamento.StatoAppuntamento = Models.StatiAppuntamento.Programmato;
                    if (string.IsNullOrEmpty(appuntamento.Priorita))
                        appuntamento.Priorita = Models.PrioritaAppuntamento.Media;

                    Appuntamenti.Add(appuntamento);
                }

                // Aggiorna proprietà calcolate
                OnPropertyChanged(nameof(AppuntamentiCompletati));
                OnPropertyChanged(nameof(AppuntamentiOggi));
                OnPropertyChanged(nameof(AppuntamentiOggiCollection));

                System.Diagnostics.Debug.WriteLine($"Filtri applicati: {risultati.Count} appuntamenti trovati");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore FilterAppuntamenti: {ex}");
                MessageBox.Show($"Errore nel filtro degli appuntamenti: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // METODI CRUD - CORRETTI
        private void AddAppuntamento(object? parameter)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CREAZIONE NUOVO APPUNTAMENTO ===");

                // USA IL COSTRUTTORE STANDARD che è già corretto
                var newAppuntamento = new Appuntamento();

                // Log per debug
                System.Diagnostics.Debug.WriteLine($"Nuovo appuntamento creato:");
                System.Diagnostics.Debug.WriteLine($"  TipoAppuntamento: '{newAppuntamento.TipoAppuntamento}'");
                System.Diagnostics.Debug.WriteLine($"  StatoAppuntamento: '{newAppuntamento.StatoAppuntamento}'");
                System.Diagnostics.Debug.WriteLine($"  Priorita: '{newAppuntamento.Priorita}'");
                System.Diagnostics.Debug.WriteLine($"  Titolo: '{newAppuntamento.Titolo}'");

                // Validazione PRIMA del salvataggio
                if (!newAppuntamento.IsValid())
                {
                    var errors = newAppuntamento.GetValidationErrors();
                    MessageBox.Show($"Errore nella validazione dell'appuntamento:\n\n{errors}",
                        "Errore Validazione", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Usa un nuovo contesto per evitare conflitti
                using (var newContext = new ImmobiliContext())
                {
                    newContext.Appuntamenti.Add(newAppuntamento);
                    newContext.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"Appuntamento creato con ID: {newAppuntamento.Id}");
                }

                // Ricarica e seleziona il nuovo appuntamento
                LoadAppuntamenti();
                var createdAppuntamento = Appuntamenti.FirstOrDefault(a => a.Id == newAppuntamento.Id);
                if (createdAppuntamento != null)
                {
                    SelectedAppuntamento = createdAppuntamento;
                    System.Diagnostics.Debug.WriteLine($"Appuntamento selezionato: ID {createdAppuntamento.Id}");
                }

                MessageBox.Show("Nuovo appuntamento creato con successo!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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
                        // AGGIORNAMENTO SICURO
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
                        saveContext.SaveChanges();

                        System.Diagnostics.Debug.WriteLine($"Appuntamento ID {SelectedAppuntamento.Id} salvato");
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_isDeleting)
                {
                    MessageBox.Show($"Errore nel salvataggio dell'appuntamento: {ex.Message}",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                System.Diagnostics.Debug.WriteLine($"Errore SaveCurrentAppuntamento: {ex}");
            }
        }

        private void SaveAppuntamento(object? parameter)
        {
            SaveCurrentAppuntamento();
            MessageBox.Show("Appuntamento salvato con successo!", "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteAppuntamento(object? parameter)
        {
            if (SelectedAppuntamento == null) return;

            var result = MessageBox.Show(
                $"Sei sicuro di voler eliminare l'appuntamento '{SelectedAppuntamento.Titolo}'?",
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
                        var appuntamentoToDelete = deleteContext.Appuntamenti
                            .FirstOrDefault(a => a.Id == SelectedAppuntamento.Id);

                        if (appuntamentoToDelete != null)
                        {
                            deleteContext.Appuntamenti.Remove(appuntamentoToDelete);
                            deleteContext.SaveChanges();
                        }
                    }

                    SelectedAppuntamento = null;
                    LoadAppuntamenti();

                    MessageBox.Show("Appuntamento eliminato con successo!", "Successo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nell'eliminazione dell'appuntamento: {ex.Message}",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    _isDeleting = false;
                }
            }
        }

        // METODI DI GESTIONE STATO
        private void MarkAsCompleted(object? parameter)
        {
            if (SelectedAppuntamento != null)
            {
                SelectedAppuntamento.MarkAsCompleted("Completato dall'utente");
                SaveCurrentAppuntamento();
                LoadAppuntamenti();

                MessageBox.Show("Appuntamento marcato come completato!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MarkAsConfirmed(object? parameter)
        {
            if (SelectedAppuntamento != null)
            {
                SelectedAppuntamento.MarkAsConfirmed();
                SaveCurrentAppuntamento();
                LoadAppuntamenti();

                MessageBox.Show("Appuntamento confermato!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelAppuntamento(object? parameter)
        {
            if (SelectedAppuntamento != null)
            {
                var motivo = Microsoft.VisualBasic.Interaction.InputBox(
                    "Inserisci il motivo della cancellazione (opzionale):",
                    "Cancella Appuntamento",
                    "");

                SelectedAppuntamento.Cancel(motivo);
                SaveCurrentAppuntamento();
                LoadAppuntamenti();

                MessageBox.Show("Appuntamento cancellato!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // METODI DI NAVIGAZIONE CALENDARIO
        private void PreviousMonth(object? parameter)
        {
            SelectedDate = SelectedDate.AddMonths(-1);
        }

        private void NextMonth(object? parameter)
        {
            SelectedDate = SelectedDate.AddMonths(1);
        }

        private void GoToToday(object? parameter)
        {
            SelectedDate = DateTime.Today;
        }

        private void ToggleView(object? parameter)
        {
            IsListView = !IsListView;
            IsCalendarView = !IsCalendarView;
            OnPropertyChanged(nameof(IsListView));
            OnPropertyChanged(nameof(IsCalendarView));
        }

        private void ClearFiltri(object? parameter)
        {
            SearchText = string.Empty;
            FiltroStato = "Tutti";
            FiltroTipo = "Tutti";
            LoadAppuntamenti();
        }

        // INTEGRAZIONE OUTLOOK
        private async void InitializeOutlook()
        {
            try
            {
                IsOutlookConnected = await _outlookService.InitializeAsync();
                System.Diagnostics.Debug.WriteLine($"Outlook inizializzato: {IsOutlookConnected}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore inizializzazione Outlook: {ex.Message}");
                IsOutlookConnected = false;
            }
        }

        private async void ConnectOutlook(object? parameter)
        {
            try
            {
                IsOutlookConnected = await _outlookService.InitializeAsync();

                if (IsOutlookConnected)
                {
                    MessageBox.Show("Connessione a Outlook stabilita con successo!", "Outlook",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Impossibile connettersi a Outlook.", "Outlook",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella connessione a Outlook: {ex.Message}", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SyncOutlook(object? parameter)
        {
            if (!IsOutlookConnected)
            {
                MessageBox.Show("Outlook non è connesso. Connetti prima Outlook.", "Outlook",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var appuntamentiDaSincronizzare = Appuntamenti
                    .Where(a => !a.SincronizzatoOutlook && string.IsNullOrEmpty(a.OutlookEventId))
                    .ToList();

                var sincronizzati = 0;
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

                // Salva le modifiche
                _context.SaveChanges();

                MessageBox.Show($"Sincronizzazione completata: {sincronizzati} appuntamenti sincronizzati.",
                    "Sincronizzazione Outlook", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella sincronizzazione: {ex.Message}", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // GENERAZIONE CALENDARIO
        private void GenerateMiniCalendarDays()
        {
            CalendarDays.Clear();

            var firstDayOfMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Giorni del mese precedente per completare la prima settimana
            var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            var startDate = firstDayOfMonth.AddDays(-firstDayOfWeek);

            // Genera 42 giorni (6 settimane)
            for (int i = 0; i < 42; i++)
            {
                var date = startDate.AddDays(i);
                var appuntamentiDelGiorno = Appuntamenti.Count(a => a.DataInizio.Date == date.Date);

                CalendarDays.Add(new CalendarDay
                {
                    Date = date,
                    IsCurrentMonth = date.Month == SelectedDate.Month,
                    IsToday = date.Date == DateTime.Today,
                    AppuntamentiCount = appuntamentiDelGiorno,
                    HasAppuntamenti = appuntamentiDelGiorno > 0
                });
            }
        }

        private void GenerateMainCalendar()
        {
            EventiCalendario.Clear();

            var appuntamentiMese = Appuntamenti
                .Where(a => a.DataInizio.Month == SelectedDate.Month && a.DataInizio.Year == SelectedDate.Year)
                .ToList();

            foreach (var appuntamento in appuntamentiMese)
            {
                EventiCalendario.Add(new EventoCalendario
                {
                    Id = appuntamento.Id,
                    Titolo = appuntamento.Titolo,
                    Inizio = appuntamento.DataInizio,
                    Fine = appuntamento.DataFine,
                    Colore = appuntamento.StatoColore,
                    Tipo = appuntamento.TipoAppuntamento,
                    TuttoIlGiorno = false,
                    ClienteNome = appuntamento.Cliente?.NomeCompleto,
                    ImmobileTitolo = appuntamento.Immobile?.Titolo
                });
            }
        }

        // EXPORT
        private void EsportaAppuntamenti(object? parameter)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"Appuntamenti_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = "Data,Ora,Titolo,Tipo,Stato,Cliente,Immobile,Luogo,Priorità\n";

                    foreach (var app in Appuntamenti)
                    {
                        csv += $"{app.DataInizio:dd/MM/yyyy},{app.DataInizio:HH:mm}," +
                               $"\"{app.Titolo}\",\"{app.TipoAppuntamento}\",\"{app.StatoAppuntamento}\"," +
                               $"\"{app.Cliente?.NomeCompleto ?? ""}\",\"{app.Immobile?.Titolo ?? ""}\"," +
                               $"\"{app.Luogo}\",\"{app.Priorita}\"\n";
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, csv, System.Text.Encoding.UTF8);

                    MessageBox.Show($"Appuntamenti esportati con successo in {saveFileDialog.FileName}",
                        "Esportazione Completata", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'esportazione: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MarkAsCompletedInternal(object? parameter)
        {
            try
            {
                if (SelectedAppuntamento == null) return;

                // Use the correct static constant reference
                SelectedAppuntamento.StatoAppuntamento = Models.StatiAppuntamento.Completato;
                SelectedAppuntamento.DataUltimaModifica = DateTime.Now;

                // Save changes
                _context.SaveChanges();

                // Refresh the collections
                LoadAppuntamenti();
                GenerateMainCalendar();

                System.Diagnostics.Debug.WriteLine($"Appuntamento {SelectedAppuntamento.Id} marcato come completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore MarkAsCompleted: {ex.Message}");
                MessageBox.Show($"Errore nel marcare l'appuntamento come completato: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ALSO ADD THESE HELPER METHODS FOR OTHER STATUS CHANGES:

        private void MarkAsConfirmedInternal(object? parameter)
        {
            try
            {
                if (SelectedAppuntamento == null) return;

                SelectedAppuntamento.StatoAppuntamento = Models.StatiAppuntamento.Confermato;
                SelectedAppuntamento.DataUltimaModifica = DateTime.Now;

                _context.SaveChanges();
                LoadAppuntamenti();
                GenerateMainCalendar();

                System.Diagnostics.Debug.WriteLine($"Appuntamento {SelectedAppuntamento.Id} confermato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore MarkAsConfirmed: {ex.Message}");
                MessageBox.Show($"Errore nella conferma dell'appuntamento: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelAppuntamentoInternal(object? parameter)
        {
            try
            {
                if (SelectedAppuntamento == null) return;

                var result = MessageBox.Show($"Sei sicuro di voler annullare l'appuntamento '{SelectedAppuntamento.Titolo}'?",
                    "Conferma Annullamento", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SelectedAppuntamento.StatoAppuntamento = Models.StatiAppuntamento.Cancellato;
                    SelectedAppuntamento.DataUltimaModifica = DateTime.Now;

                    _context.SaveChanges();
                    LoadAppuntamenti();
                    GenerateMainCalendar();

                    System.Diagnostics.Debug.WriteLine($"Appuntamento {SelectedAppuntamento.Id} annullato");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore CancelAppuntamento: {ex.Message}");
                MessageBox.Show($"Errore nell'annullamento dell'appuntamento: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // CLEANUP
        public void OnApplicationClosing()
        {
            try
            {
                if (!_isDeleting && SelectedAppuntamento != null)
                {
                    SaveCurrentAppuntamento();
                }
                System.Diagnostics.Debug.WriteLine("AppuntamentiViewModel cleanup completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel salvataggio finale appuntamento: {ex.Message}");
            }
        }
    }

    // CLASSI HELPER PER IL CALENDARIO
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public int AppuntamentiCount { get; set; }
        public bool HasAppuntamenti { get; set; }
        public string DayNumber => Date.Day.ToString();
        public string CssClass
        {
            get
            {
                var classes = new List<string>();
                if (!IsCurrentMonth) classes.Add("other-month");
                if (IsToday) classes.Add("today");
                if (HasAppuntamenti) classes.Add("has-events");
                return string.Join(" ", classes);
            }
        }
    }
}