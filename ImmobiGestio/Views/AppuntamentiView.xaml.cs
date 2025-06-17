using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using ImmobiGestio.ViewModels;
using ImmobiGestio.Models;

namespace ImmobiGestio.Views
{
    /// <summary>
    /// Logica di interazione per AppuntamentiView.xaml
    /// </summary>
    public partial class AppuntamentiView : UserControl
    {
        public AppuntamentiView()
        {
            InitializeComponent();

            // Inizializza il mini calendario con i giorni
            Loaded += AppuntamentiView_Loaded;
        }

        private void AppuntamentiView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Aggiorna il mini calendario quando la vista viene caricata
                if (DataContext is AppuntamentiViewModel viewModel)
                {
                    MiniCalendarDays.ItemsSource = viewModel.MiniCalendarDays;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore nel caricamento AppuntamentiView: {ex.Message}");
            }
        }

        // Gestisce il click sui giorni del calendario principale per evitare conflitti con eventi
        private void CalendarDay_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement element)
                {
                    // Se il click è su un evento specifico, non creare un nuovo appuntamento
                    var hitTest = element.InputHitTest(e.GetPosition(element));

                    // Controlla se l'hit test ha colpito un evento (border con evento)
                    var eventBorder = FindParent<Border>(hitTest as DependencyObject);
                    if (eventBorder?.DataContext is Appuntamento)
                    {
                        // È un click su un evento esistente, non procedere con la creazione
                        e.Handled = true;
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore CalendarDay_PreviewMouseLeftButtonDown: {ex.Message}");
            }
        }

        // Gestisce il double click su eventi esistenti
        private void AppuntamentoEvent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                e.Handled = true; // CRITICO: Previene la propagazione

                if (sender is FrameworkElement element &&
                    element.DataContext is Appuntamento appuntamento &&
                    DataContext is AppuntamentiViewModel viewModel)
                {
                    // Seleziona l'appuntamento senza creare uno nuovo
                    viewModel.SelectedAppuntamento = appuntamento;
                    System.Diagnostics.Debug.WriteLine($"Appuntamento selezionato: {appuntamento.Titolo}");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore AppuntamentoEvent_MouseDoubleClick: {ex.Message}");
            }
        }

        // Gestisce il double click su celle vuote del calendario
        private void CalendarCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Solo se non è stato gestito da un evento figlio
                if (!e.Handled && DataContext is AppuntamentiViewModel viewModel)
                {
                    viewModel.AddAppuntamentoCommand?.Execute(null);
                    System.Diagnostics.Debug.WriteLine("Nuovo appuntamento creato da cella vuota");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore CalendarCell_MouseDoubleClick: {ex.Message}");
            }
        }

        // Gestisce l'hover effect per gli eventi
        private void AppuntamentoEvent_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is Border border)
                {
                    // Salva il colore originale se non già salvato
                    if (!border.Tag?.ToString()?.StartsWith("OriginalColor:") == true)
                    {
                        var originalBrush = border.Background;
                        border.Tag = $"OriginalColor:{originalBrush}";
                    }

                    // Applica l'effetto hover - colore più scuro
                    if (border.Background is System.Windows.Media.SolidColorBrush brush)
                    {
                        var color = brush.Color;
                        var darkerColor = System.Windows.Media.Color.FromArgb(
                            color.A,
                            (byte)(color.R * 0.8),
                            (byte)(color.G * 0.8),
                            (byte)(color.B * 0.8)
                        );
                        border.Background = new System.Windows.Media.SolidColorBrush(darkerColor);
                    }

                    // Aggiungi ombra o bordo per evidenziare
                    border.BorderThickness = new Thickness(2);
                    border.BorderBrush = System.Windows.Media.Brushes.White;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore AppuntamentoEvent_MouseEnter: {ex.Message}");
            }
        }

        // Rimuove l'hover effect
        private void AppuntamentoEvent_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is Border border && border.Tag?.ToString()?.StartsWith("OriginalColor:") == true)
                {
                    // Ripristina il colore originale
                    var tagString = border.Tag.ToString();
                    var originalBrushString = tagString.Substring("OriginalColor:".Length);

                    // Per semplicità, ripristiniamo dal DataContext
                    if (border.DataContext is Appuntamento appuntamento)
                    {
                        var colorString = appuntamento.StatoColore;
                        if (System.Windows.Media.ColorConverter.ConvertFromString(colorString) is System.Windows.Media.Color color)
                        {
                            border.Background = new System.Windows.Media.SolidColorBrush(color);
                        }
                    }

                    // Rimuovi il bordo
                    border.BorderThickness = new Thickness(0);
                    border.BorderBrush = null;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore AppuntamentoEvent_MouseLeave: {ex.Message}");
            }
        }

        // Helper method per trovare il parent di un tipo specifico
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        // Gestisce il cambio di selezione nel mini calendario
        private void MiniCalendarDay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button &&
                    button.DataContext is MiniCalendarDay day &&
                    DataContext is AppuntamentiViewModel viewModel)
                {
                    viewModel.SelectedDate = day.Date;
                    System.Diagnostics.Debug.WriteLine($"Data selezionata: {day.Date:dd/MM/yyyy}");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore MiniCalendarDay_Click: {ex.Message}");
            }
        }

        // Previene il double click su eventi dalla lista di propagare al container
        private void EventListItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Questo gestisce il double click nella vista lista (giorno/settimana)
                if (sender is FrameworkElement element && element.DataContext is Appuntamento appuntamento)
                {
                    e.Handled = true; // Previene la propagazione

                    if (DataContext is AppuntamentiViewModel viewModel)
                    {
                        viewModel.SelectedAppuntamento = appuntamento;
                        System.Diagnostics.Debug.WriteLine($"Appuntamento selezionato dalla lista: {appuntamento.Titolo}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore EventListItem_PreviewMouseDoubleClick: {ex.Message}");
            }
        }
    }
}