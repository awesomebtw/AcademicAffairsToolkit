﻿using Microsoft.Win32;
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

        private IArrangementAlgorithm alg;

        public ObservableCollection<Tuple<string, SelectedFileType>> RecentlyOpenedFiles { get; set; } = new ObservableCollection<Tuple<string, SelectedFileType>>();

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
                        Session.InvigilateRecords = await ExcelProcessor.ReadInvigilateTableAsync(
                                fileName, password, Session.InvigilateFilePolicy);
                        ToggleView.Execute("/InvigilateFileViewPage.xaml", this);
                        invigilateFileViewButton.IsChecked = true;
                    }
                    break;
                case SelectedFileType.TROfficeFile:
                    {
                        Session.TROffices = await ExcelProcessor.ReadTROfficeTableAsync(
                                fileName, password, Session.TROfficeFilePolicy);
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
                await OpenFileAsync(
                    openExcelDialog.FileName, openOptionsWindow.Password, openOptionsWindow.SelectedFileType);
                RecentlyOpenedFiles.Add(Tuple.Create(openExcelDialog.FileName, openOptionsWindow.SelectedFileType));
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
                Filter = "XLSX file|*.xlsx|XLS file|*.xls",
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
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }

                MessageBox.Show($"{Session.Arrangements.Count} sheets written", "Save successfully",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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
            alg = new GeneticAlgorithm(
                Session.InvigilateRecords, Session.TROffices, Session.Constraints,
                (int)iterationsSpinner.Value, (int)populationSpinner.Value);
            arrangementProgessBar.Visibility = Visibility.Visible;
            statusText.Text = "Arrangement in progress...";

            alg.ArrangementStepForward += AlgArrangementStepForward;
            alg.ArrangementTerminated += AlgArrangementTerminated;

            try
            {
                await alg.StartArrangementAsync();
            }
            catch (Exception ex)
            {
                arrangementProgessBar.Visibility = Visibility.Collapsed;
                statusText.Text = "An error occurred during arrangement. Please try again.";
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                arrangementProgessBar.Visibility = Visibility.Collapsed;
                arrangementProgessBar.Value = 0;
                ToggleView.Execute("/TableViewPage.xaml", this);
                tableViewButton.IsChecked = true;
                statusText.Text = "Arrangement finished";
            }));
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
                var result = MessageBox.Show("There's no constraints added. Add one?",
                    "Constraint list empty", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                    new AddConstraintWindow() { Owner = this }.ShowDialog();
            }
        }

        private async void Gallery_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Fluent.Gallery gallery && gallery.SelectedItem is Tuple<string, SelectedFileType> tuple)
            {
                await OpenFileAsync(tuple.Item1, "", tuple.Item2);
            }
        }
    }
}
