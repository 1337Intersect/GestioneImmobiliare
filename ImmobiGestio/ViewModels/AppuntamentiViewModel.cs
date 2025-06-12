using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ImmobiGestio.Commands;
using ImmobiGestio.Data;
using ImmobiGestio.Models;
using ImmobiGestio.Services;
using System.Windows;
using System.Threading.Tasks;

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
        private string _vistaCalendario = "Settimana"; // Giorno, Settimana, Mese
        private bool _isDeleting = false;
        private bool _isOutlookConnected = false;

        public ObservableCollection<Appuntamento> Appuntamenti { get; set; } = new();
        public ObservableCollection<EventoCalendario> EventiCalendario { get; set; } = new();
        public ObservableCollection<Cliente> ClientiDisponibili { get; set; } = new();
        public ObservableCollection<Immobile> ImmobiliDisponibili { get; set; } = new();

        // Combo boxes data
        public ObservableCollection<string> TipiAppuntamento { get; set; } = new();
        public ObservableCollection<string> StatiAppuntamento { get; set; } = new();
        public ObservableCollection<string> PrioritaAppuntamento { get; set; } = new();
        public ObservableCollection<string> TipiAppuntamentoFiltro { get; set; } = new();
        public ObservableCollection<string> StatiAppuntamentoFiltro { get; set; } = new();

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
                LoadClientiEImmobiliDisponibili();
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
            }
        }

        public string VistaCalendario
        {
            get => _vistaCalendario;
            set
            {
                SetProperty(ref _vistaCalendario, value);
                LoadEventiCalendario();
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

        public AppuntamentiViewModel(ImmobiliContext context)
        {
            _context = context;
            _outlookService = new OutlookService();
            _statisticheService = new StatisticheService(context);

            InitializeCollections();
            InitializeCommands();
            LoadAppuntamenti();
            LoadEventiCalendario();
            LoadClientiEImmobiliDisponibili();
        }

        private void InitializeCollections()
        {
            // Tipi appuntamento
            TipiAppuntamento.Clear();
            TipiAppuntamentoFiltro.Clear();
            TipiAppuntamentoFiltro.Add("Tutti");

            foreach (var tipo in Models.TipiAppuntamento.GetAll())
            {
                TipiAppuntamento.Add(tipo);
                TipiAppuntamentoFiltro.Add(tipo);
            }

            // Stati appuntamento
            StatiAppuntamento.Clear();
            StatiAppuntamentoFiltro.Clear();
            StatiAppuntamentoFiltro.Add("Tutti");

            foreach (var stato in Models.StatiAppuntamento.GetAll())
            {
                StatiAppuntamento.Add(stato);
                StatiAppuntamentoFiltro.Add(stato);
            }

            // Priorità
            PrioritaAppuntamento.Clear();
            foreach (var priorita in Models.PrioritaAppuntamento.GetAll())
            {
                PrioritaAppuntamento.Add(priorita);
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
            TodayCommand = new RelayCommand(_ => SelectedDate = DateTime.Today);
            EsportaCalendarioCommand = new RelayCommand(EsportaCalendario);

            // Nuovi comandi aggiunti
            SelectAppuntamentoCommand = new RelayCommand(param => {
                if (param is Appuntamento app)
                {
                    SelectedAppuntamento = app;
                }
            });

            ClearFiltriCommand = new RelayCommand(_ => ClearFiltri());
        }

        public void LoadAppuntamenti()
        {
            try
            {
                var appuntamenti = _context.Appuntamenti
                    .AsNoTracking()
                    .Include(a => a.Cliente)
                    .Include(a => a.Immobile)
                    .OrderBy(a => a.DataInizio)
                    .ToList();

                Appuntamenti.Clear();
                foreach (var appuntamento in appuntamenti)
                {
                    Appuntamenti.Add(appuntamento);
                }

                // Notifica le proprietà calcolate
                OnPropertyChanged(nameof(AppuntamentiCompletati));
                OnPropertyChanged(nameof(AppuntamentiOggi));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento degli appuntamenti: {ex.Message}",
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

                // Filtro per testo di ricerca
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(a =>
                        a.Titolo.Contains(SearchText) ||
                        a.Descrizione.Contains(SearchText) ||
                        a.Luogo.Contains(SearchText) ||
                        (a.Cliente != null && (a.Cliente.Nome.Contains(SearchText) || a.Cliente.Cognome.Contains(SearchText))));
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

                var filtered = query
                    .OrderBy(a => a.DataInizio)
                    .ToList();

                Appuntamenti.Clear();
                foreach (var appuntamento in filtered)
                {
                    Appuntamenti.Add(appuntamento);
                }

                // Notifica le proprietà calcolate
                OnPropertyChanged(nameof(AppuntamentiCompletati));
                OnPropertyChanged(nameof(AppuntamentiOggi));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella ricerca appuntamenti: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadEventiCalendario()
        {
            try
            {
                DateTime start, end;

                switch (VistaCalendario)
                {
                    case "Giorno":
                        start = SelectedDate.Date;
                        end = SelectedDate.Date.AddDays(1);
                        break;
                    case "Settimana":
                        start = SelectedDate.AddDays(-(int)SelectedDate.DayOfWeek);
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

                // Carica eventi dal database
                var eventi = _statisticheService.GetEventiCalendario(start, end);

                EventiCalendario.Clear();
                foreach (var evento in eventi)
                {
                    EventiCalendario.Add(evento);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento del calendario: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadClientiEImmobiliDisponibili()
        {
            try
            {
                // Carica clienti attivi
                var clienti = _context.Clienti
                    .Where(c => c.StatoCliente == "Attivo")
                    .OrderBy(c => c.Nome)
                    .ThenBy(c => c.Cognome)
                    .ToList();

                ClientiDisponibili.Clear();
                foreach (var cliente in clienti)
                {
                    ClientiDisponibili.Add(cliente);
                }

                // Carica immobili disponibili
                var immobili = _context.Immobili
                    .Where(i => i.StatoVendita == "Disponibile")
                    .OrderBy(i => i.Titolo)
                    .ToList();

                ImmobiliDisponibili.Clear();
                foreach (var immobile in immobili)
                {
                    ImmobiliDisponibili.Add(immobile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento di clienti e immobili: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddAppuntamento(object? parameter)
        {
            var newAppuntamento = new Appuntamento
            {
                Titolo = "Nuovo Appuntamento",
                DataInizio = DateTime.Now.AddHours(1),
                DataFine = DateTime.Now.AddHours(2),
                TipoAppuntamento = "Visita",
                StatoAppuntamento = "Programmato",
                Priorita = "Media",
                Luogo = "Ufficio"
            };

            try
            {
                _context.Appuntamenti.Add(newAppuntamento);
                _context.SaveChanges();

                LoadAppuntamenti();
                SelectedAppuntamento = newAppuntamento;

                MessageBox.Show("Nuovo appuntamento creato con successo!",
                    "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella creazione dell'appuntamento: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCurrentAppuntamento()
        {
            if (SelectedAppuntamento != null)
            {
                try
                {
                    var exists = _context.Appuntamenti.Any(a => a.Id == SelectedAppuntamento.Id);
                    if (!exists) return;

                    SelectedAppuntamento.DataUltimaModifica = DateTime.Now;

                    var tracked = _context.ChangeTracker.Entries<Appuntamento>()
                        .FirstOrDefault(e => e.Entity.Id == SelectedAppuntamento.Id);
                    if (tracked != null)
                    {
                        _context.Entry(tracked.Entity).State = EntityState.Detached;
                    }

                    _context.Update(SelectedAppuntamento);
                    _context.SaveChanges();

                    // Sync con Outlook se connesso
                    if (IsOutlookConnected && SelectedAppuntamento.SincronizzatoOutlook)
                    {
                        Task.Run(async () => await _outlookService.UpdateAppuntamentoAsync(SelectedAppuntamento));
                    }
                }
                catch (Exception ex)
                {
                    if (!_isDeleting)
                    {
                        MessageBox.Show($"Errore nel salvataggio dell'appuntamento: {ex.Message}",
                            "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SaveAppuntamento(object? parameter)
        {
            SaveCurrentAppuntamento();
            LoadEventiCalendario(); // Ricarica il calendario
            MessageBox.Show("Appuntamento salvato con successo!",
                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteAppuntamento(object? parameter)
        {
            if (SelectedAppuntamento == null) return;

            var result = MessageBox.Show(
                $"Sei sicuro di voler eliminare l'appuntamento '{SelectedAppuntamento.Titolo}'?",
                "Conferma Eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _isDeleting = true;
                    var appuntamentoId = SelectedAppuntamento.Id;
                    var outlookEventId = SelectedAppuntamento.OutlookEventId;

                    var appuntamentoToDelete = _context.Appuntamenti.Find(appuntamentoId);
                    if (appuntamentoToDelete != null)
                    {
                        _context.Appuntamenti.Remove(appuntamentoToDelete);
                        _context.SaveChanges();

                        // Elimina da Outlook se sincronizzato
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

                        // Notifica le proprietà calcolate
                        OnPropertyChanged(nameof(AppuntamentiCompletati));
                        OnPropertyChanged(nameof(AppuntamentiOggi));

                        MessageBox.Show("Appuntamento eliminato con successo!",
                            "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
                // Usa una finestra semplice per l'input
                var result = MessageBox.Show(
                    "Vuoi aggiungere delle note per questo appuntamento completato?",
                    "Completa Appuntamento",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                string esito = "";
                if (result == MessageBoxResult.Yes)
                {
                    // Per ora usa un esito predefinito, in futuro si può implementare una finestra custom
                    esito = "Appuntamento completato con successo";
                }

                SelectedAppuntamento.Completa(esito);
                SaveCurrentAppuntamento();
                LoadEventiCalendario();

                // Notifica le proprietà calcolate
                OnPropertyChanged(nameof(AppuntamentiCompletati));
                OnPropertyChanged(nameof(AppuntamentiOggi));

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
                // Sincronizza appuntamenti non ancora sincronizzati
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

        // Nuovo metodo per pulire i filtri
        private void ClearFiltri()
        {
            SearchText = string.Empty;
            FiltroStato = "Tutti";
            FiltroTipo = "Tutti";
            SelectedDate = DateTime.Today;
        }

        // Proprietà per la vista calendario
        public string TitoloVista
        {
            get
            {
                return VistaCalendario switch
                {
                    "Giorno" => SelectedDate.ToString("dddd, dd MMMM yyyy"),
                    "Settimana" => $"Settimana del {SelectedDate.AddDays(-(int)SelectedDate.DayOfWeek):dd MMM} - {SelectedDate.AddDays(6 - (int)SelectedDate.DayOfWeek):dd MMM yyyy}",
                    "Mese" => SelectedDate.ToString("MMMM yyyy"),
                    _ => SelectedDate.ToString("dd MMMM yyyy")
                };
            }
        }

        public void OnApplicationClosing()
        {
            try
            {
                if (!_isDeleting)
                {
                    SaveCurrentAppuntamento();
                }
                _outlookService?.Disconnect();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel salvataggio finale appuntamento: {ex.Message}");
            }
        }
    }
}