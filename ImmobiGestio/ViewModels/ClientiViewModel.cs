using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ImmobiGestio.Commands;
using ImmobiGestio.Data;
using ImmobiGestio.Models;
using System.Windows;

namespace ImmobiGestio.ViewModels
{
    public class ClientiViewModel : BaseViewModel
    {
        private readonly ImmobiliContext _context;
        private Cliente? _selectedCliente;
        private string _searchText = string.Empty;
        private string _filtroTipoCliente = "Tutti";
        private string _filtroStato = "Tutti";
        private bool _isDeleting = false;

        public ObservableCollection<Cliente> Clienti { get; set; } = new();
        public ObservableCollection<Appuntamento> AppuntamentiCliente { get; set; } = new();
        public ObservableCollection<ClienteImmobile> ImmobiliInteresse { get; set; } = new();

        // Combo boxes data SOLO per i clienti (NON per appuntamenti)
        public ObservableCollection<string> TipiCliente { get; set; } = new();
        public ObservableCollection<string> StatiCliente { get; set; } = new();
        public ObservableCollection<string> FontiContatto { get; set; } = new();
        public ObservableCollection<string> TipiClienteFiltro { get; set; } = new();
        public ObservableCollection<string> StatiClienteFiltro { get; set; } = new();

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

        // Commands
        public ICommand? AddClienteCommand { get; set; }
        public ICommand? SaveClienteCommand { get; set; }
        public ICommand? DeleteClienteCommand { get; set; }
        public ICommand? AddAppuntamentoCommand { get; set; }
        public ICommand? AddInteresseImmobileCommand { get; set; }
        public ICommand? DeleteAppuntamentoCommand { get; set; }
        public ICommand? DeleteInteresseCommand { get; set; }
        public ICommand? InviaEmailCommand { get; set; }
        public ICommand? ChiamaClienteCommand { get; set; }
        public ICommand? EsportaClientiCommand { get; set; }

        public ClientiViewModel(ImmobiliContext context)
        {
            _context = context;
            InitializeCollections();
            InitializeCommands();
            LoadClienti();
        }

        private void InitializeCollections()
        {
            // Tipi cliente (SOLO per clienti, NON appuntamenti)
            TipiCliente.Clear();
            TipiClienteFiltro.Clear();
            TipiClienteFiltro.Add("Tutti");

            foreach (var tipo in new[] { "Acquirente", "Venditore", "Locatario", "Locatore" })
            {
                TipiCliente.Add(tipo);
                TipiClienteFiltro.Add(tipo);
            }

            // Stati cliente (SOLO per clienti, NON appuntamenti)
            StatiCliente.Clear();
            StatiClienteFiltro.Clear();
            StatiClienteFiltro.Add("Tutti");

            foreach (var stato in new[] { "Attivo", "Inattivo", "Prospect", "Concluso" })
            {
                StatiCliente.Add(stato);
                StatiClienteFiltro.Add(stato);
            }

            // Fonti contatto
            FontiContatto.Clear();
            foreach (var fonte in new[] { "Web", "Telefono", "Email", "Referral", "Social Media", "Cartellone", "Passaparola", "Altro" })
            {
                FontiContatto.Add(fonte);
            }
        }

        private void InitializeCommands()
        {
            AddClienteCommand = new RelayCommand(AddCliente);
            SaveClienteCommand = new RelayCommand(SaveCliente, _ => SelectedCliente != null);
            DeleteClienteCommand = new RelayCommand(DeleteCliente, _ => SelectedCliente != null);
            AddAppuntamentoCommand = new RelayCommand(AddAppuntamento, _ => SelectedCliente != null);
            AddInteresseImmobileCommand = new RelayCommand(AddInteresseImmobile, _ => SelectedCliente != null);
            DeleteAppuntamentoCommand = new RelayCommand(DeleteAppuntamento);
            DeleteInteresseCommand = new RelayCommand(DeleteInteresse);
            InviaEmailCommand = new RelayCommand(InviaEmail, _ => SelectedCliente != null && !string.IsNullOrEmpty(SelectedCliente.Email));
            ChiamaClienteCommand = new RelayCommand(ChiamaCliente, _ => SelectedCliente != null && (!string.IsNullOrEmpty(SelectedCliente.Telefono) || !string.IsNullOrEmpty(SelectedCliente.Cellulare)));
            EsportaClientiCommand = new RelayCommand(EsportaClienti);
        }

        public void LoadClienti()
        {
            try
            {
                var clienti = _context.Clienti
                    .AsNoTracking()
                    .Include(c => c.Appuntamenti)
                    .Include(c => c.ImmobiliDiInteresse)
                    .OrderByDescending(c => c.DataInserimento)
                    .ToList();

                Clienti.Clear();
                foreach (var cliente in clienti)
                {
                    Clienti.Add(cliente);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento dei clienti: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
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

                // Filtro per testo di ricerca
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(c =>
                        c.Nome.Contains(SearchText) ||
                        c.Cognome.Contains(SearchText) ||
                        c.Email.Contains(SearchText) ||
                        c.Telefono.Contains(SearchText) ||
                        c.Cellulare.Contains(SearchText));
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

                var filtered = query
                    .OrderByDescending(c => c.DataInserimento)
                    .ToList();

                Clienti.Clear();
                foreach (var cliente in filtered)
                {
                    Clienti.Add(cliente);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella ricerca clienti: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RefreshCurrentCollections()
        {
            AppuntamentiCliente.Clear();
            ImmobiliInteresse.Clear();

            if (SelectedCliente != null)
            {
                try
                {
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nel caricamento dei dati del cliente: {ex.Message}",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddCliente(object? parameter)
        {
            var newCliente = new Cliente
            {
                Nome = "Nuovo",
                Cognome = "Cliente",
                TipoCliente = "Acquirente",
                StatoCliente = "Prospect",
                FonteContatto = "Web",
                Email = "",
                Telefono = "",
                Cellulare = "",
                CodiceFiscale = "",
                Indirizzo = "",
                Citta = "",
                CAP = "",
                Provincia = "",
                Note = "",
                PreferenzeTipologia = "",
                PreferenzeZone = "",
                BudgetMin = 0,
                BudgetMax = 0,
                DataNascita = DateTime.Today.AddYears(-30),
                DataInserimento = DateTime.Now
            };

            try
            {
                _context.Clienti.Add(newCliente);
                _context.SaveChanges();

                Clienti.Insert(0, newCliente);
                SelectedCliente = newCliente;

                MessageBox.Show("Nuovo cliente creato con successo!",
                    "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nella creazione del cliente: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCurrentCliente()
        {
            if (SelectedCliente != null)
            {
                try
                {
                    var exists = _context.Clienti.Any(c => c.Id == SelectedCliente.Id);
                    if (!exists) return;

                    SelectedCliente.DataUltimaModifica = DateTime.Now;

                    // Detach eventuali entità già tracciate
                    var tracked = _context.ChangeTracker.Entries<Cliente>()
                        .FirstOrDefault(e => e.Entity.Id == SelectedCliente.Id);
                    if (tracked != null)
                    {
                        _context.Entry(tracked.Entity).State = EntityState.Detached;
                    }

                    _context.Update(SelectedCliente);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    if (!_isDeleting)
                    {
                        MessageBox.Show($"Errore nel salvataggio del cliente: {ex.Message}",
                            "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SaveCliente(object? parameter)
        {
            SaveCurrentCliente();
            MessageBox.Show("Cliente salvato con successo!",
                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteCliente(object? parameter)
        {
            if (SelectedCliente == null) return;

            var result = MessageBox.Show(
                $"Sei sicuro di voler eliminare il cliente '{SelectedCliente.NomeCompleto}'?\n" +
                "Questa operazione eliminerà anche tutti gli appuntamenti associati.",
                "Conferma Eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _isDeleting = true;
                    var clienteId = SelectedCliente.Id;
                    var clienteNome = SelectedCliente.NomeCompleto;

                    var clienteToDelete = _context.Clienti
                        .Include(c => c.Appuntamenti)
                        .Include(c => c.ImmobiliDiInteresse)
                        .FirstOrDefault(c => c.Id == clienteId);

                    if (clienteToDelete != null)
                    {
                        _context.Clienti.Remove(clienteToDelete);
                        _context.SaveChanges();

                        var uiCliente = Clienti.FirstOrDefault(c => c.Id == clienteId);
                        if (uiCliente != null)
                        {
                            Clienti.Remove(uiCliente);
                        }

                        SelectedCliente = Clienti.FirstOrDefault();

                        MessageBox.Show($"Cliente '{clienteNome}' eliminato con successo!",
                            "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nell'eliminazione del cliente: {ex.Message}",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoadClienti();
                }
                finally
                {
                    _isDeleting = false;
                }
            }
        }

        // METODO FIX: AddAppuntamento corretto per ClientiViewModel
        private void AddAppuntamento(object? parameter)
        {
            if (SelectedCliente == null)
            {
                MessageBox.Show("Nessun cliente selezionato per creare l'appuntamento.",
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Salva prima il cliente corrente
                SaveCurrentCliente();

                // Crea l'appuntamento con valori diretti
                var newAppuntamento = new Appuntamento
                {
                    ClienteId = SelectedCliente.Id,
                    Titolo = $"Appuntamento con {SelectedCliente.NomeCompleto}",
                    Descrizione = $"Incontro con il cliente {SelectedCliente.NomeCompleto}",
                    DataInizio = DateTime.Now.AddDays(1),
                    DataFine = DateTime.Now.AddDays(1).AddHours(1),
                    TipoAppuntamento = "Incontro",
                    StatoAppuntamento = "Programmato",
                    Priorita = "Media",
                    Luogo = "Ufficio",
                    NotePrivate = $"Appuntamento creato dalla scheda cliente: {SelectedCliente.NomeCompleto}",
                    EsitoIncontro = "",
                    OutlookEventId = "",
                    CreatoDa = "Sistema",
                    DataCreazione = DateTime.Now,
                    SincronizzatoOutlook = false,
                    NotificaInviata = false,
                    RichiedeConferma = true
                };

                // Log per debug
                System.Diagnostics.Debug.WriteLine($"=== CREAZIONE APPUNTAMENTO DA CLIENTE ===");
                System.Diagnostics.Debug.WriteLine($"ClienteId: {newAppuntamento.ClienteId}");
                System.Diagnostics.Debug.WriteLine($"Cliente: {SelectedCliente.NomeCompleto}");

                // Usa un nuovo contesto per evitare conflitti
                using (var newContext = new ImmobiliContext())
                {
                    var clienteExists = newContext.Clienti.Any(c => c.Id == SelectedCliente.Id);
                    if (!clienteExists)
                    {
                        MessageBox.Show("Errore: Il cliente selezionato non esiste nel database!",
                            "Errore Database", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    newContext.Appuntamenti.Add(newAppuntamento);
                    newContext.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"Appuntamento salvato con ID: {newAppuntamento.Id}");
                }

                // Ricarica gli appuntamenti del cliente
                RefreshCurrentCollections();

                // Notifica agli altri ViewModels
                AppuntamentoCreated?.Invoke();

                MessageBox.Show($"Appuntamento creato con successo per {SelectedCliente.NomeCompleto}!\n\n" +
                               $"Data: {newAppuntamento.DataInizio:dd/MM/yyyy HH:mm}",
                    "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                var message = $"Errore nella creazione dell'appuntamento:\n\n{ex.Message}";
                if (ex.InnerException != null)
                {
                    message += $"\n\nDettagli: {ex.InnerException.Message}";
                }

                MessageBox.Show(message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Errore AddAppuntamento: {ex}");
            }
        }

        private void AddInteresseImmobile(object? parameter)
        {
            if (SelectedCliente == null) return;

            try
            {
                // Prendi il primo immobile disponibile per esempio
                var immobile = _context.Immobili
                    .FirstOrDefault(i => i.StatoVendita == "Disponibile");

                if (immobile != null)
                {
                    var interesse = new ClienteImmobile
                    {
                        ClienteId = SelectedCliente.Id,
                        ImmobileId = immobile.Id,
                        StatoInteresse = "Interessato",
                        Note = "Interesse aggiunto manualmente",
                        DataInteresse = DateTime.Now
                    };

                    _context.ClientiImmobili.Add(interesse);
                    _context.SaveChanges();

                    RefreshCurrentCollections();

                    MessageBox.Show($"Interesse per l'immobile '{immobile.Titolo}' aggiunto!",
                        "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Nessun immobile disponibile per aggiungere interesse.",
                        "Avviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'aggiunta dell'interesse: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteAppuntamento(object? parameter)
        {
            if (parameter is Appuntamento appuntamento)
            {
                var result = MessageBox.Show(
                    $"Sei sicuro di voler eliminare l'appuntamento '{appuntamento.Titolo}'?",
                    "Conferma Eliminazione",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var appToDelete = _context.Appuntamenti.Find(appuntamento.Id);
                        if (appToDelete != null)
                        {
                            _context.Appuntamenti.Remove(appToDelete);
                            _context.SaveChanges();

                            AppuntamentiCliente.Remove(appuntamento);

                            // Notifica agli altri ViewModels
                            AppuntamentoCreated?.Invoke();

                            MessageBox.Show("Appuntamento eliminato con successo!",
                                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore nell'eliminazione dell'appuntamento: {ex.Message}",
                            "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteInteresse(object? parameter)
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

                            ImmobiliInteresse.Remove(interesse);

                            MessageBox.Show("Interesse rimosso con successo!",
                                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore nella rimozione dell'interesse: {ex.Message}",
                            "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void InviaEmail(object? parameter)
        {
            if (SelectedCliente != null && !string.IsNullOrEmpty(SelectedCliente.Email))
            {
                try
                {
                    var mailto = $"mailto:{SelectedCliente.Email}?subject=Contatto da ImmobiGestio";
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = mailto,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nell'apertura del client email: {ex.Message}",
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ChiamaCliente(object? parameter)
        {
            if (SelectedCliente != null)
            {
                var telefono = !string.IsNullOrEmpty(SelectedCliente.Cellulare)
                    ? SelectedCliente.Cellulare
                    : SelectedCliente.Telefono;

                if (!string.IsNullOrEmpty(telefono))
                {
                    try
                    {
                        var telUri = $"tel:{telefono}";
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = telUri,
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        MessageBox.Show($"Numero di telefono: {telefono}",
                            "Chiama Cliente", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

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
                    var csv = "Nome,Cognome,Email,Telefono,TipoCliente,StatoCliente,Budget Min,Budget Max,Data Inserimento\n";

                    foreach (var cliente in Clienti)
                    {
                        csv += $"{cliente.Nome},{cliente.Cognome},{cliente.Email},{cliente.Telefono}," +
                               $"{cliente.TipoCliente},{cliente.StatoCliente},{cliente.BudgetMin},{cliente.BudgetMax}," +
                               $"{cliente.DataInserimento:yyyy-MM-dd}\n";
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, csv);

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

        public void OnApplicationClosing()
        {
            try
            {
                if (!_isDeleting)
                {
                    SaveCurrentCliente();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel salvataggio finale cliente: {ex.Message}");
            }
        }
    }
}