// ===== APP CODE-BEHIND - App.xaml.cs =====
using System.Windows;
using System;

namespace ImmobiGestio
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== AVVIO APPLICAZIONE ===");

                // Inizializzazione globale se necessaria
                // Ad esempio, configurazione cultura, logging, etc.

                System.Diagnostics.Debug.WriteLine("Applicazione avviata correttamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante l'avvio: {ex.Message}");
                MessageBox.Show($"Errore critico durante l'avvio dell'applicazione:\n\n{ex.Message}",
                    "Errore Avvio", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CHIUSURA APPLICAZIONE ===");

                // Cleanup globale se necessario
                // Ad esempio, salvataggio configurazioni, chiusura risorse, etc.

                System.Diagnostics.Debug.WriteLine("Applicazione chiusa correttamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante la chiusura: {ex.Message}");
                // Non mostrare messaggi durante la chiusura per evitare di bloccare
            }
        }
    }
}