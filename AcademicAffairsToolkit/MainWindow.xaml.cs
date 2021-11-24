using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Threading;

namespace AcademicAffairsToolkit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static readonly RoutedUICommand ToggleView = new RoutedUICommand();

        public static readonly RoutedUICommand StartArrangement = new RoutedUICommand();

        public static readonly RoutedUICommand StopArrangement = new RoutedUICommand();

        private static readonly OpenFileDialog openExcelDialog = new OpenFileDialog
        {
            DereferenceLinks = true,
            Filter = $"XLSX {Resource.File}|*.xlsx|XLS {Resource.File}|*.xls|Excel {Resource.File}|*.xlsx;*.xls",
            InitialDirectory = Environment.CurrentDirectory
        };

        private IArrangementAlgorithm alg;

        private CancellationTokenSource cancellationTokenSource;

        public ObservableCollection<Tuple<string, SelectedFileType>> RecentlyOpenedFiles { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            RecentlyOpenedFiles = new ObservableCollection<Tuple<string, SelectedFileType>>();
            openExcelDialog.FileOk += OpenExcelDialogFileOk;
        }

        private async Task OpenFileAsync(string fileName, SelectedFileType selectedFileType)
        {
            switch (selectedFileType)
            {
                case SelectedFileType.InvigilateFile:
                    {
                        Session.InvigilateRecords.Clear();
                        foreach (var item in await ExcelProcessor.ReadInvigilateTableAsync(
                            fileName, Session.InvigilateFilePolicy))
                        {
                            Session.InvigilateRecords.Add(item);
                        }
                        ToggleView.Execute("/InvigilateFileViewPage.xaml", this);
                        invigilateFileViewButton.IsChecked = true;
                    }
                    break;
                case SelectedFileType.TROfficeFile:
                    {
                        Session.TROffices.Clear();
                        foreach (var item in await ExcelProcessor.ReadTROfficeTableAsync(
                            fileName, Session.TROfficeFilePolicy))
                        {
                            Session.TROffices.Add(item);
                        }
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

            try
            {
                await OpenFileAsync(openExcelDialog.FileName, openOptionsWindow.SelectedFileType);
                RecentlyOpenedFiles.Add(Tuple.Create(openExcelDialog.FileName, openOptionsWindow.SelectedFileType));
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message + "\n" + Resource.FileFormatErrorTip,
                    Resource.UnableToOpenFile, MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
                return;
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    ex.Message + "\n" + Resource.FileIOErrorTip,
                    Resource.UnableToOpenFile, MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resource.UnableToOpenFile, MessageBoxButton.OK, MessageBoxImage.Error);
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
            e.CanExecute = Session.AutoArrangementFinished();
        }

        private async void SaveCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog()
            {
                AddExtension = true,
                Filter = $"XLSX {Resource.File}|*.xlsx|XLS {Resource.File}|*.xls",
                InitialDirectory = Environment.CurrentDirectory
            };

            if (fileDialog.ShowDialog(this) == true)
            {
                try
                {
                    await ExcelProcessor.SaveFileAsync(
                        Session.Arrangements, fileDialog.FileName, fileDialog.FilterIndex != 1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Resource.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                MessageBox.Show($"{Resource.SheetsWrittenPrefix} {Session.Arrangements.Count} {Resource.SheetsWrittenPostfix}",
                    Resource.Save, MessageBoxButton.OK, MessageBoxImage.Information);
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

        private async void StartArrangementExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();

            alg = new GeneticAlgorithm(
                Session.InvigilateRecords, Session.TROffices, Session.Constraints,
                (int)iterationsSpinner.Value, (int)populationSpinner.Value, (int)solutionsSpinner.Value,
                cancellationTokenSource.Token);

            stopArrangementButton.IsEnabled = true;
            startArrangementButton.IsEnabled = false;
            iterationsSpinner.IsEnabled = false;
            populationSpinner.IsEnabled = false;
            solutionsSpinner.IsEnabled = false;

            arrangementProgessBar.Visibility = Visibility.Visible;
            statusText.Text = Resource.ArrangementIsInProgress + "...";

            alg.ArrangementStepForward += AlgArrangementStepForward;
            alg.ArrangementTerminated += AlgArrangementTerminated;

            try
            {
                await alg.StartArrangementAsync();
            }
            catch (Exception ex)
            {
                arrangementProgessBar.Visibility = Visibility.Collapsed;
                statusText.Text = Resource.ArrangementErrorTip;
                MessageBox.Show(ex.Message, Resource.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AlgArrangementStepForward(object sender, ArrangementStepForwardEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                arrangementProgessBar.Value = e.CurrentIteration;
            }));
        }

        private void AlgArrangementTerminated(object sender, ArrangementTerminatedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Session.Arrangements?.Clear();
                foreach (var result in e.Result)
                {
                    Session.Arrangements?.Add(
                        e.InvigilateRecords.Select(
                            (p, i) => new ArrangementResultEntry(p, result[i], e.PeopleNeeded[i])).ToArray());
                }

                startArrangementButton.IsEnabled = true;
                iterationsSpinner.IsEnabled = true;
                populationSpinner.IsEnabled = true;
                solutionsSpinner.IsEnabled = true;

                selectedSolutionGallery.SelectedIndex = 0;

                arrangementProgessBar.Visibility = Visibility.Collapsed;
                arrangementProgessBar.Value = 0;
                ToggleView.Execute("/TableViewPage.xaml", this);
                tableViewButton.IsChecked = true;
                statusText.Text = e.Cancelled ? Resource.ArrangementCanceled : Resource.ArrangementFinished;

                if (e.PotentialUnableToArrange)
                {
                    MessageBox.Show(Resource.PotentialUnableToArrangeTip, Resource.Warning,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }));
        }

        private void StopCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Session.CanStartArrange();
        }

        private void StopCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            stopArrangementButton.IsEnabled = false;
        }

        private void AddConstraintButtonClick(object sender, RoutedEventArgs e)
        {
            new AddConstraintWindow() { Owner = this }.ShowDialog();
        }

        private void ManageConstraintButtonClick(object sender, RoutedEventArgs e)
        {
            if (Session.Constraints.Count > 0)
            {
                new ManageConstraintsWindow() { Owner = this }.ShowDialog();
            }
            else
            {
                var result = MessageBox.Show(Resource.ConstraintListEmptyTip,
                    Resource.ConstraintListEmpty, MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                    new AddConstraintWindow() { Owner = this }.ShowDialog();
            }
        }

        private async void Gallery_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Fluent.Gallery gallery && gallery.SelectedItem is Tuple<string, SelectedFileType> tuple)
            {
                await OpenFileAsync(tuple.Item1, tuple.Item2);
            }
        }
    }
}
