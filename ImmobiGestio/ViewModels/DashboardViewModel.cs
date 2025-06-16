using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ImmobiGestio.Commands;
using ImmobiGestio.Data;
using ImmobiGestio.Models;
using ImmobiGestio.Services;
using System.Windows;

namespace ImmobiGestio.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ImmobiliContext _context;
        private readonly StatisticheService _statisticheService;
        private StatisticheDashboard _statistiche;

        public StatisticheDashboard Statistiche
        {
            get => _statistiche;
            set => SetProperty(ref _statistiche, value);
        }

        public ObservableCollection<AttivitaRecente> AttivitaRecenti { get; set; } = new();
        public ObservableCollection<PerformanceImmobile> TopImmobili { get; set; } = new();
        public ObservableCollection<EventoCalendario> ProssimiAppuntamenti { get; set; } = new();

        // Commands
        public ICommand? RefreshCommand { get; set; }
        public ICommand? NuovoClienteCommand { get; set; }
        public ICommand? NuovoImmobileCommand { get; set; }
        public ICommand? NuovoAppuntamentoCommand { get; set; }
        public ICommand? VaiAImmobiliCommand { get; set; }
        public ICommand? VaiAClientiCommand { get; set; }
        public ICommand? VaiAAppuntamentiCommand { get; set; }

        // Eventi per navigazione
        public event Action? NavigateToClienti;
        public event Action? NavigateToImmobili;
        public event Action? NavigateToAppuntamenti;
        public event Action<int>? NavigateToCliente;
        public event Action<int>? NavigateToImmobile;
        public event Action<int>? NavigateToAppuntamento;

        public DashboardViewModel(ImmobiliContext context)
        {
            _context = context;
            _statisticheService = new StatisticheService(context);
            _statistiche = new StatisticheDashboard();

            InitializeCommands();
            LoadDashboardData();
        }

        private void InitializeCommands()
        {
            RefreshCommand = new RelayCommand(_ => LoadDashboardData());
            NuovoClienteCommand = new RelayCommand(_ => NavigateToClienti?.Invoke());
            NuovoImmobileCommand = new RelayCommand(_ => NavigateToImmobili?.Invoke());
            NuovoAppuntamentoCommand = new RelayCommand(_ => NavigateToAppuntamenti?.Invoke());
            VaiAImmobiliCommand = new RelayCommand(_ => NavigateToImmobili?.Invoke());
            VaiAClientiCommand = new RelayCommand(_ => NavigateToClienti?.Invoke());
            VaiAAppuntamentiCommand = new RelayCommand(_ => NavigateToAppuntamenti?.Invoke());
        }

        public void LoadDashboardData()
        {
            try
            {
                // Carica statistiche principali
                Statistiche = _statisticheService.GetStatisticheDashboard();

                // Carica attività recenti
                AttivitaRecenti.Clear();
                foreach (var attivita in Statistiche.AttivitaRecenti)
                {
                    AttivitaRecenti.Add(attivita);
                }

                // Carica top immobili per performance
                TopImmobili.Clear();
                var topPerformance = _statisticheService.GetPerformanceImmobili()
                    .Take(5)
                    .ToList();

                foreach (var performance in topPerformance)
                {
                    TopImmobili.Add(performance);
                }

                // Carica prossimi appuntamenti
                LoadProssimiAppuntamenti();

                OnPropertyChanged(nameof(Statistiche));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento dei dati dashboard: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProssimiAppuntamenti()
        {
            ProssimiAppuntamenti.Clear();

            var prossimi = _statisticheService.GetEventiCalendario(
                DateTime.Today,
                DateTime.Today.AddDays(7))
                .OrderBy(e => e.Inizio)
                .Take(5)
                .ToList();

            foreach (var evento in prossimi)
            {
                ProssimiAppuntamenti.Add(evento);
            }
        }

        // Metodi per gestire i click sulle attività
        public void OnAttivitaClick(AttivitaRecente attivita)
        {
            if (attivita.RelatedId.HasValue)
            {
                switch (attivita.Tipo)
                {
                    case "Nuovo Cliente":
                        NavigateToCliente?.Invoke(attivita.RelatedId.Value);
                        break;
                    case "Nuovo Immobile":
                        NavigateToImmobile?.Invoke(attivita.RelatedId.Value);
                        break;
                    case "Appuntamento Completato":
                        NavigateToAppuntamento?.Invoke(attivita.RelatedId.Value);
                        break;
                }
            }
        }

        public void OnPerformanceImmobileClick(PerformanceImmobile performance)
        {
            NavigateToImmobile?.Invoke(performance.ImmobileId);
        }

        public void OnAppuntamentoClick(EventoCalendario evento)
        {
            NavigateToAppuntamento?.Invoke(evento.Id);
        }

        // Proprietà per indicatori di performance
        public string IndicatoreVendite
        {
            get
            {
                if (Statistiche.VenditeMensili.Count >= 2)
                {
                    var ultimo = Statistiche.VenditeMensili.LastOrDefault();
                    var penultimo = Statistiche.VenditeMensili.Skip(Statistiche.VenditeMensili.Count - 2).FirstOrDefault();

                    if (ultimo != null && penultimo != null)
                    {
                        if (ultimo.Valore > penultimo.Valore)
                            return "📈 In crescita";
                        else if (ultimo.Valore < penultimo.Valore)
                            return "📉 In calo";
                    }
                }
                return "📊 Stabile";
            }
        }

        public string IndicatoreAttivita
        {
            get
            {
                var oggi = AttivitaRecenti.Count(a => a.Data.Date == DateTime.Today);
                return oggi switch
                {
                    0 => "😴 Giornata tranquilla",
                    1 or 2 => "😊 Attività normale",
                    > 2 => "🔥 Giornata intensa",
                    _ => "📊 Nessuna attività"
                };
            }
        }

        public string ProgresoVendite => $"{Statistiche.PercentualeVendite:F1}%";
        public string ProgresoClienti => $"{Statistiche.PercentualeClientiAttivi:F1}%";
    }
}