using System;
using System.ComponentModel;
using System.Windows;
using ImmobiGestio.ViewModels;

namespace ImmobiGestio.Views
{
    public partial class SettingsWindow : Window
    {
        private SettingsViewModel? _viewModel;

        public SettingsWindow()
        {
            try
            {
                InitializeComponent();
                InitializeViewModel();

                System.Diagnostics.Debug.WriteLine("SettingsWindow inizializzata correttamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore inizializzazione SettingsWindow: {ex.Message}");
                MessageBox.Show($"Errore nell'inizializzazione della finestra impostazioni: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeViewModel()
        {
            try
            {
                _viewModel = new SettingsViewModel();
                DataContext = _viewModel;

                // Sottoscrivi agli eventi del ViewModel
                _viewModel.CloseRequested += OnCloseRequested;
                _viewModel.SettingsSaved += OnSettingsSaved;

                System.Diagnostics.Debug.WriteLine("SettingsViewModel collegato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore InitializeViewModel: {ex.Message}");
                throw;
            }
        }

        private void OnCloseRequested()
        {
            try
            {
                DialogResult = _viewModel?.HasChanges == false;
                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnCloseRequested: {ex.Message}");
                Close();
            }
        }

        private void OnSettingsSaved()
        {
            try
            {
                DialogResult = true;
                System.Diagnostics.Debug.WriteLine("Impostazioni salvate - finestra chiusa con successo");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnSettingsSaved: {ex.Message}");
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                // Se ci sono modifiche non salvate, chiedi conferma
                if (_viewModel?.HasChanges == true)
                {
                    var result = MessageBox.Show(
                        "Ci sono modifiche non salvate. Vuoi scartarle?",
                        "Conferma Chiusura",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                // Disconnetti gli eventi
                if (_viewModel != null)
                {
                    _viewModel.CloseRequested -= OnCloseRequested;
                    _viewModel.SettingsSaved -= OnSettingsSaved;
                }

                base.OnClosing(e);
                System.Diagnostics.Debug.WriteLine("SettingsWindow chiusa correttamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnClosing: {ex.Message}");
                base.OnClosing(e);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                base.OnClosed(e);

                // Cleanup finale
                _viewModel = null;
                DataContext = null;

                System.Diagnostics.Debug.WriteLine("SettingsWindow cleanup completato");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnClosed: {ex.Message}");
            }
        }

        // Proprietà per accedere al ViewModel dall'esterno
        public SettingsViewModel? ViewModel => _viewModel;

        // Metodo statico per aprire la finestra delle impostazioni
        public static bool? ShowSettingsDialog(Window? owner = null)
        {
            try
            {
                var settingsWindow = new SettingsWindow();

                if (owner != null)
                {
                    settingsWindow.Owner = owner;
                }

                return settingsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore ShowSettingsDialog: {ex.Message}");
                MessageBox.Show($"Errore nell'apertura della finestra impostazioni: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Metodo per forzare il focus su un tab specifico
        public void FocusOnTab(string tabName)
        {
            try
            {
                if (_viewModel != null)
                {
                    _viewModel.SelectedTab = tabName;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore FocusOnTab: {ex.Message}");
            }
        }

        // Event handler per gestire i tasti rapidi
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                // Ctrl+S per salvare
                if (e.Key == System.Windows.Input.Key.S &&
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                {
                    if (_viewModel?.SaveCommand?.CanExecute(null) == true)
                    {
                        _viewModel.SaveCommand.Execute(null);
                        e.Handled = true;
                        return;
                    }
                }

                // Escape per annullare
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    if (_viewModel?.CancelCommand?.CanExecute(null) == true)
                    {
                        _viewModel.CancelCommand.Execute(null);
                        e.Handled = true;
                        return;
                    }
                }

                base.OnKeyDown(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnKeyDown: {ex.Message}");
                base.OnKeyDown(e);
            }
        }

        // Metodo per validare e mostrare errori specifici
        private void ShowValidationError(string fieldName, string error)
        {
            try
            {
                MessageBox.Show($"Errore nel campo '{fieldName}':\n\n{error}",
                    "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore ShowValidationError: {ex.Message}");
            }
        }

        // Metodo per mostrare informazioni sui cambiamenti che richiedono riavvio
        public static void ShowRestartRequiredMessage()
        {
            try
            {
                MessageBox.Show("Alcune modifiche alle impostazioni potrebbero richiedere il riavvio dell'applicazione per essere applicate completamente.\n\n" +
                               "Modifiche che richiedono riavvio:\n" +
                               "• Configurazioni di logging\n" +
                               "• Percorsi di sistema\n" +
                               "• Configurazioni email e Outlook",
                    "Riavvio Richiesto", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore ShowRestartRequiredMessage: {ex.Message}");
            }
        }
    }
}