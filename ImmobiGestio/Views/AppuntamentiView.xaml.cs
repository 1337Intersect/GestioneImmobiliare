using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using ImmobiGestio.ViewModels;
using ImmobiGestio.Models;
using System;
using System.ComponentModel;

namespace ImmobiGestio.Views
{
    public partial class AppuntamentiView : UserControl
    {
        private AppuntamentiViewModel? ViewModel => DataContext as AppuntamentiViewModel;

        public AppuntamentiView()
        {
            InitializeComponent();
            Loaded += AppuntamentiView_Loaded;
        }

        private void AppuntamentiView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Configura il mini calendario quando la vista è caricata
                SetupMiniCalendar();

                // Configura il calendario principale
                SetupMainCalendar();

                System.Diagnostics.Debug.WriteLine("AppuntamentiView caricato correttamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore AppuntamentiView_Loaded: {ex.Message}");
            }
        }

        private void SetupMiniCalendar()
        {
            try
            {
                if (ViewModel != null)
                {
                    // Forza la generazione del mini calendario
                    ViewModel.LoadAllData();

                    // Il binding è già impostato nel XAML
                    System.Diagnostics.Debug.WriteLine("MiniCalendar setup completato");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore SetupMiniCalendar: {ex.Message}");
            }
        }

        private void SetupMainCalendar()
        {
            try
            {
                if (ViewModel != null)
                {
                    // Il binding è già impostato nel XAML
                    // Qui possiamo aggiungere gestori di eventi aggiuntivi se necessario
                    UpdateCalendarVisibility();
                    System.Diagnostics.Debug.WriteLine("MainCalendar setup completato");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore SetupMainCalendar: {ex.Message}");
            }
        }

        private void UpdateCalendarVisibility()
        {
            try
            {
                if (ViewModel != null)
                {
                    // La visibilità è gestita dai trigger nel XAML
                    // Qui possiamo solo fare log o logica aggiuntiva se necessario
                    System.Diagnostics.Debug.WriteLine($"Vista calendario: {ViewModel.VistaCalendario}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore UpdateCalendarVisibility: {ex.Message}");
            }
        }

        // Gestori di eventi per interazioni personalizzate
        private void OnCalendarCellDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2 && ViewModel != null)
                {
                    // Crea un nuovo appuntamento nella data selezionata
                    ViewModel.AddAppuntamentoCommand?.Execute(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnCalendarCellDoubleClick: {ex.Message}");
            }
        }

        private void OnAppuntamentoDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2 && sender is FrameworkElement element)
                {
                    if (element.DataContext is Appuntamento appuntamento && ViewModel != null)
                    {
                        // Seleziona l'appuntamento per la modifica
                        ViewModel.SelectedAppuntamento = appuntamento;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnAppuntamentoDoubleClick: {ex.Message}");
            }
        }

        private void OnMiniCalendarDayClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && ViewModel != null)
                {
                    // Il CommandParameter è già impostato correttamente nel XAML
                    // Non c'è bisogno di gestire manualmente questo evento
                    System.Diagnostics.Debug.WriteLine("MiniCalendar day clicked");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnMiniCalendarDayClick: {ex.Message}");
            }
        }

        // Gestione del cambio di vista
        private void OnVistaCalendarioChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UpdateCalendarVisibility();

                if (ViewModel != null)
                {
                    // Ricarica gli eventi per la nuova vista
                    ViewModel.LoadEventiCalendario();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnVistaCalendarioChanged: {ex.Message}");
            }
        }

        // Gestione scroll del mouse per navigazione rapida
        private void OnCalendarMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                if (ViewModel != null && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    // Ctrl + scroll per navigare tra i periodi
                    if (e.Delta > 0)
                    {
                        ViewModel.PreviousPeriodCommand?.Execute(null);
                    }
                    else
                    {
                        ViewModel.NextPeriodCommand?.Execute(null);
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnCalendarMouseWheel: {ex.Message}");
            }
        }

        // Gestione tasti di scelta rapida
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (ViewModel != null)
                {
                    switch (e.Key)
                    {
                        case Key.T when Keyboard.Modifiers == ModifierKeys.Control:
                            // Ctrl+T = Oggi
                            ViewModel.TodayCommand?.Execute(null);
                            e.Handled = true;
                            break;

                        case Key.N when Keyboard.Modifiers == ModifierKeys.Control:
                            // Ctrl+N = Nuovo appuntamento
                            ViewModel.AddAppuntamentoCommand?.Execute(null);
                            e.Handled = true;
                            break;

                        case Key.S when Keyboard.Modifiers == ModifierKeys.Control:
                            // Ctrl+S = Salva
                            ViewModel.SaveAppuntamentoCommand?.Execute(null);
                            e.Handled = true;
                            break;

                        case Key.F5:
                            // F5 = Aggiorna
                            ViewModel.RefreshCommand?.Execute(null);
                            e.Handled = true;
                            break;

                        case Key.Left when Keyboard.Modifiers == ModifierKeys.Alt:
                            // Alt+Left = Periodo precedente
                            ViewModel.PreviousPeriodCommand?.Execute(null);
                            e.Handled = true;
                            break;

                        case Key.Right when Keyboard.Modifiers == ModifierKeys.Alt:
                            // Alt+Right = Periodo successivo
                            ViewModel.NextPeriodCommand?.Execute(null);
                            e.Handled = true;
                            break;

                        case Key.D1 when Keyboard.Modifiers == ModifierKeys.Control:
                            // Ctrl+1 = Vista giorno
                            ViewModel.VistaCalendario = "Giorno";
                            e.Handled = true;
                            break;

                        case Key.D2 when Keyboard.Modifiers == ModifierKeys.Control:
                            // Ctrl+2 = Vista settimana
                            ViewModel.VistaCalendario = "Settimana";
                            e.Handled = true;
                            break;

                        case Key.D3 when Keyboard.Modifiers == ModifierKeys.Control:
                            // Ctrl+3 = Vista mese
                            ViewModel.VistaCalendario = "Mese";
                            e.Handled = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnKeyDown: {ex.Message}");
            }
        }

        // Aggiorna la visibilità quando cambia il DataContext
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (e.NewValue is AppuntamentiViewModel viewModel)
                {
                    // Setup quando il ViewModel cambia
                    SetupMiniCalendar();
                    SetupMainCalendar();

                    // Sottoscrivi agli eventi del ViewModel se necessario
                    viewModel.PropertyChanged += ViewModel_PropertyChanged;
                }

                if (e.OldValue is AppuntamentiViewModel oldViewModel)
                {
                    // Cleanup del vecchio ViewModel
                    oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnDataContextChanged: {ex.Message}");
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                // Reagisci ai cambiamenti del ViewModel
                switch (e.PropertyName)
                {
                    case nameof(AppuntamentiViewModel.VistaCalendario):
                        UpdateCalendarVisibility();
                        break;

                    case nameof(AppuntamentiViewModel.SelectedDate):
                        // Aggiorna il mini calendario quando cambia la data
                        SetupMiniCalendar();
                        break;

                    case nameof(AppuntamentiViewModel.Appuntamenti):
                        // Rigenera il mini calendario quando cambiano gli appuntamenti
                        SetupMiniCalendar();
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore ViewModel_PropertyChanged: {ex.Message}");
            }
        }

        // Cleanup quando la vista viene scaricata
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel != null)
                {
                    ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore OnUnloaded: {ex.Message}");
            }
        }
    }
}