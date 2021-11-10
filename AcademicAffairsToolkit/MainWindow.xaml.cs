using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private static readonly OpenOptionsWindow openOptionsWindow = new OpenOptionsWindow();

        public ObservableCollection<Tuple<string, string>> RecentlyOpenedFiles { get; set; } = new ObservableCollection<Tuple<string, string>>();

        public MainWindow()
        {
            InitializeComponent();
            openExcelDialog.FileOk += OpenExcelDialogFileOk;
        }

        private void OpenExcelDialogFileOk(object sender, CancelEventArgs e)
        {
            if (openOptionsWindow.ShowDialog(this) != true)
            {
                e.Cancel = true;
            }
        }

        private void OpenCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private async void OpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // todo: coupling too much, needs refactoring

            if (openExcelDialog.ShowDialog(this) != true)
                return;

            RecentlyOpenedFiles.Add(new Tuple<string, string>(openExcelDialog.FileName, openOptionsWindow.SelectedFileType.ToString()));

            switch (openOptionsWindow.SelectedFileType)
            {
                case SelectedFileType.InvigilateFile:
                    {
                        Session.InvigilateRecords = await ExcelProcessor.ReadInvigilateTableAsync(
                            openExcelDialog.FileName,
                            openOptionsWindow.Password,
                            Session.InvigilateFilePolicy);
                        ToggleView.Execute("/InvigilateFileViewPage.xaml", this);
                        invigilateFileViewButton.IsChecked = true;
                    }
                    break;
                case SelectedFileType.TROfficeFile:
                    {
                        Session.TROffices = await ExcelProcessor.ReadTROfficeTableAsync(
                            openExcelDialog.FileName,
                            openOptionsWindow.Password,
                            Session.TROfficeFilePolicy);
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
            // todo: invoke start arrangement function
        }

        private void ArrangementPolicyButtonClick(object sender, RoutedEventArgs e)
        {
            new ArrangementPolicyWindow().Show();
        }
    }
}
