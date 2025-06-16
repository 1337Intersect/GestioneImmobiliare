using System.Windows;
using System.Windows.Controls;
using ImmobiGestio.ViewModels;

namespace ImmobiGestio.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
            
            // Imposta il tema combobox
            var viewModel = DataContext as SettingsViewModel;
            if (viewModel != null)
            {
                SetThemeComboBoxSelection(viewModel.CurrentTheme);
                viewModel.PropertyChanged += (s, e) => {
                    if (e.PropertyName == nameof(SettingsViewModel.CurrentTheme))
                    {
                        SetThemeComboBoxSelection(viewModel.CurrentTheme);
                    }
                };
            }
        }

        private void SetThemeComboBoxSelection(Services.AppTheme theme)
        {
            ThemeComboBox.SelectedIndex = theme switch
            {
                Services.AppTheme.System => 0,
                Services.AppTheme.Light => 1,
                Services.AppTheme.Dark => 2,
                _ => 0
            };
        }

        private void ThemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel && ThemeComboBox.SelectedItem is ComboBoxItem item)
            {
                var theme = item.Tag.ToString() switch
                {
                    "System" => Services.AppTheme.System,
                    "Light" => Services.AppTheme.Light,
                    "Dark" => Services.AppTheme.Dark,
                    _ => Services.AppTheme.System
                };
                
                viewModel.CurrentTheme = theme;
            }
        }
    }
}