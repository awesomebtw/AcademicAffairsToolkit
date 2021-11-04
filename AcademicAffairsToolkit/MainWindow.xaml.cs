using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AcademicAffairsToolkit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static RoutedUICommand ToggleView = new RoutedUICommand();

        public static OpenFileDialog OpenExcelDialog = new OpenFileDialog()
            {
                Filter = "XLSX files|*.xlsx|XLS files|*.xls|All Excel files|*.xlsx;*.xls",
                InitialDirectory = Environment.CurrentDirectory
            };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RibbonWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] path)
            {
                recentlyOpenedGallery.Items.Add(path.First());
                // todo: start parsing excel file
            }
        }

        private void OpenCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (OpenExcelDialog.ShowDialog(this) == true)
            {
                recentlyOpenedGallery.Items.Add(OpenExcelDialog.FileName);
                var entries = ExcelProcessor.ReadInvigilateTable(OpenExcelDialog.FileName, "", Session.InvigilateFilePolicy);
                Session.InvigilateRecords = new ObservableCollection<InvigilateRecordEntry>(entries);
                ToggleView.Execute("/OriginalFileViewPage.xaml", this);
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
            // todo: can execute if auto arrangement finished
            e.CanExecute = Session.InvigilateRecords != null && Session.InvigilateRecords.Count() != 0;
        }

        private void ToggleViewCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is string uriString &&
                Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                mainViewFrame.Navigate(uri);
            }
        }
    }
}
