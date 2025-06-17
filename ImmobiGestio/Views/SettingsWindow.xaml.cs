using System;
using System.Windows;
using System.Windows.Controls;
using ImmobiGestio.ViewModels;
using ImmobiGestio.Services;

namespace ImmobiGestio.Views
{
    public partial class SettingsWindow : Window
    {
        private SettingsViewModel? _viewModel;

        public SettingsWindow()
        {
            InitializeComponent();
            Loaded += SettingsWindow_Loaded;
        }

        public static bool? ShowSettingsDialog(Window? owner = null)
        {
            try
            {
                var window = new SettingsWindow();
                if (owner != null)
                {
                    window.Owner = owner;
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }

                window.DataContext = new SettingsViewModel();
                return window.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ShowSettingsDialog: {ex.Message}");
                MessageBox.Show($"Errore: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as SettingsViewModel;
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                SwitchToTab(_viewModel.SelectedTab);
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsViewModel.SelectedTab) && _viewModel != null)
            {
                SwitchToTab(_viewModel.SelectedTab);
            }
        }

        public void FocusOnTab(string tabName)
        {
            if (_viewModel != null)
            {
                _viewModel.SelectedTab = tabName;
            }
            SwitchToTab(tabName);
        }

        private void SwitchToTab(string tabName)
        {
            try
            {
                // Hide all tabs first
                if (GeneraleTab != null) GeneraleTab.Visibility = Visibility.Collapsed;
                if (FindName("TemaTab") is StackPanel temaTab)
                    temaTab.Visibility = Visibility.Collapsed;
                if (PercorsiTab != null) PercorsiTab.Visibility = Visibility.Collapsed;
                if (FileTab != null) FileTab.Visibility = Visibility.Collapsed;
                if (BackupTab != null) BackupTab.Visibility = Visibility.Collapsed;
                if (EmailTab != null) EmailTab.Visibility = Visibility.Collapsed;
                if (OutlookTab != null) OutlookTab.Visibility = Visibility.Collapsed;
                if (LoggingTab != null) LoggingTab.Visibility = Visibility.Collapsed;

                // Show selected tab
                switch (tabName)
                {
                    case "Generale":
                        if (GeneraleTab != null) GeneraleTab.Visibility = Visibility.Visible;
                        break;
                    case "Tema":
                        if (FindName("TemaTab") is StackPanel temaTabShow)
                            temaTabShow.Visibility = Visibility.Visible;
                        break;
                    case "Percorsi":
                        if (PercorsiTab != null) PercorsiTab.Visibility = Visibility.Visible;
                        break;
                    case "File":
                        if (FileTab != null) FileTab.Visibility = Visibility.Visible;
                        break;
                    case "Backup":
                        if (BackupTab != null) BackupTab.Visibility = Visibility.Visible;
                        break;
                    case "Email":
                        if (EmailTab != null) EmailTab.Visibility = Visibility.Visible;
                        break;
                    case "Outlook":
                        if (OutlookTab != null) OutlookTab.Visibility = Visibility.Visible;
                        break;
                    case "Logging":
                        if (LoggingTab != null) LoggingTab.Visibility = Visibility.Visible;
                        break;
                    default:
                        if (GeneraleTab != null) GeneraleTab.Visibility = Visibility.Visible;
                        break;
                }

                // Update button styles
                UpdateTabButtonStyles(tabName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SwitchToTab: {ex.Message}");
            }
        }

        // FIXED: Much safer method to update tab button styles
        private void UpdateTabButtonStyles(string activeTab)
        {
            try
            {
                // Use a more robust approach to find sidebar buttons
                var sidebarButtons = FindSidebarButtons();

                foreach (Button button in sidebarButtons)
                {
                    if (button.Content?.ToString() == activeTab)
                    {
                        // Set active style
                        if (FindResource("TabButtonActiveStyle") is Style activeStyle)
                            button.Style = activeStyle;
                    }
                    else
                    {
                        // Set normal style
                        if (FindResource("TabButtonStyle") is Style normalStyle)
                            button.Style = normalStyle;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating tab button styles: {ex.Message}");
            }
        }

        // FIXED: Safe method to find sidebar buttons without hard-coded casting
        private System.Collections.Generic.List<Button> FindSidebarButtons()
        {
            var buttons = new System.Collections.Generic.List<Button>();

            try
            {
                // Method 1: Try to find by name if the StackPanel has a name
                if (FindName("SidebarStackPanel") is StackPanel namedStackPanel)
                {
                    foreach (var child in namedStackPanel.Children)
                    {
                        if (child is Button button)
                            buttons.Add(button);
                    }
                    return buttons;
                }

                // Method 2: Search through the visual tree safely
                if (Content is Grid mainGrid)
                {
                    // Find the main content grid (first row)
                    foreach (var child in mainGrid.Children)
                    {
                        if (child is Grid contentGrid && Grid.GetRow(contentGrid) == 0)
                        {
                            // Find the sidebar border (first column)
                            foreach (var gridChild in contentGrid.Children)
                            {
                                if (gridChild is Border sidebarBorder && Grid.GetColumn(sidebarBorder) == 0)
                                {
                                    // Find the StackPanel inside the border
                                    if (sidebarBorder.Child is StackPanel stackPanel)
                                    {
                                        foreach (var stackChild in stackPanel.Children)
                                        {
                                            if (stackChild is Button button)
                                                buttons.Add(button);
                                        }
                                        return buttons;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding sidebar buttons: {ex.Message}");
            }

            return buttons;
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    var themeString = selectedItem.Tag?.ToString();
                    if (!string.IsNullOrEmpty(themeString) && Enum.TryParse<Theme>(themeString, out var theme))
                    {
                        ThemeManager.Instance.SetTheme(theme);

                        if (_viewModel != null)
                        {
                            _viewModel.Settings.AppTheme = themeString;
                        }

                        System.Diagnostics.Debug.WriteLine($"Theme changed to: {theme}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error changing theme: {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosed(e);
        }
    }
}