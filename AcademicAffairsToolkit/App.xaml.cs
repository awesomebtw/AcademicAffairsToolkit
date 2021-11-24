using ControlzEx.Theming;
using System.Windows;
using System.Windows.Threading;

namespace AcademicAffairsToolkit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // synchronize themes with current windows settings
            ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncAll);

            base.OnStartup(e);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"{e.Exception.Message}\n{e.Exception.StackTrace}",
                Resource.UnhandledError, MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
