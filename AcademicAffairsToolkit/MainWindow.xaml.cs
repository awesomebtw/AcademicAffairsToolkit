using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AcademicAffairsToolkit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static readonly RoutedUICommand ToggleView = new RoutedUICommand();

        public static readonly RoutedUICommand StartArrangement = new RoutedUICommand();

        private static readonly OpenFileDialog openExcelDialog = new OpenFileDialog
        {
            DereferenceLinks = true,
            Filter = "XLSX files|*.xlsx|XLS files|*.xls|All Excel files|*.xlsx;*.xls",
            InitialDirectory = Environment.CurrentDirectory
        };

        public ObservableCollection<Tuple<string, string>> RecentlyOpenedFiles { get; set; } = new ObservableCollection<Tuple<string, string>>();

        public MainWindow()
        {
            InitializeComponent();
            openExcelDialog.FileOk += OpenExcelDialogFileOk;
        }

        private async Task OpenFileAsync(string fileName, string password, SelectedFileType selectedFileType)
        {
            switch (selectedFileType)
            {
                case SelectedFileType.InvigilateFile:
                    {
                        Session.InvigilateRecords = (await ExcelProcessor.ReadInvigilateTableAsync(
                                fileName, password, Session.InvigilateFilePolicy)).ToArray();
                        ToggleView.Execute("/InvigilateFileViewPage.xaml", this);
                        invigilateFileViewButton.IsChecked = true;
                    }
                    break;
                case SelectedFileType.TROfficeFile:
                    {
                        Session.TROffices = (await ExcelProcessor.ReadTROfficeTableAsync(
                                fileName, password, Session.TROfficeFilePolicy)).ToArray();
                        ToggleView.Execute("/TRFileViewPage.xaml", this);
                        trOfficeFileViewButton.IsChecked = true;
                    }
                    break;
                case SelectedFileType.Unknown:
                    break;
                default:
                    break;
            }
        }

        private async void OpenExcelDialogFileOk(object sender, CancelEventArgs e)
        {
            var openOptionsWindow = new OpenOptionsWindow { FileName = openExcelDialog.FileName };
            if (openOptionsWindow.ShowDialog(this) != true)
            {
                e.Cancel = true;
                return;
            }

            var openFileTask = OpenFileAsync(
                openExcelDialog.FileName, openOptionsWindow.Password, openOptionsWindow.SelectedFileType);
            try
            {
                await openFileTask;
                RecentlyOpenedFiles.Add(
                    new Tuple<string, string>(
                        openExcelDialog.FileName, openOptionsWindow.SelectedFileType.ToString()));
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message + "\nCheck if file type and/or parse policies settings is incorrect.",
                    "Unable to open file", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
                return;
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    ex.Message + "\nCheck if the file is being used or does not exist.",
                    "Unable to open file", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to open file", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
                return;
            }
            if (openFileTask.Exception != null)
            {
                MessageBox.Show("exception");
                e.Cancel = true;
                return;
            }
        }

        private void OpenCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            openExcelDialog.ShowDialog(this);
        }

        private void CloseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void SaveCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog()
            {
                Filter = "JSON file|*.json",
                InitialDirectory = Environment.CurrentDirectory
            };

            if (fileDialog.ShowDialog(this) == true)
            {
                // todo: save file
                MessageBox.Show(fileDialog.FileName, "Saved (TODO)");
            }
        }

        private void ToggleViewCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Session.AnyFileLoaded();
        }

        private void ToggleViewCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is string uriString &&
                Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                mainViewFrame.Navigate(uri);
            }
        }

        private void StartArrangementCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Session.CanStartArrange();
        }

        private void StartArrangementExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var ga = new GeneticAlgorithmScheduler(
                Session.InvigilateRecords, Session.TROffices, Session.Constraints, 1000);
            ga.StartArrangement();
            // todo: show result
            MessageBox.Show(ga.Result.ToString());
        }

        private void ArrangementPolicyButtonClick(object sender, RoutedEventArgs e)
        {
            new ArrangementPolicyWindow().Show();
        }

        private void AddConstraintButtonClick(object sender, RoutedEventArgs e)
        {
            new AddConstraintWindow() { Owner = this }.ShowDialog();
        }

        private void ManageConstraintButtonClick(object sender, RoutedEventArgs e)
        {
            if (Session.Constraints.Count > 0)
                new ManageConstraintsWindow() { Owner = this }.ShowDialog();
            else
                MessageBox.Show("There's no constraints added. To add one, use \"add constraint\".",
                    "Constraint list empty", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
