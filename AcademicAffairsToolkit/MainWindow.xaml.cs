using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void backstageOpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Filter = "XLSX files|*.xlsx|XLS files|*.xls|All Excel files|*.xlsx;*.xls",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)
            };

            if (fileDialog.ShowDialog(this) == true)
            {
                MessageBox.Show(fileDialog.FileName, "Selected file",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                recentlyOpenedGallery.Items.Add(fileDialog.FileName);
            }
            else
            {
                MessageBox.Show("You clicked cancel");
            }
        }

        private void RibbonWindow_DragEnter(object sender, DragEventArgs e)
        {
            // todo: process drag and drop event
            e.Handled = true;
        }

        private void backStageExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Exit",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                Close();
        }
    }
}
