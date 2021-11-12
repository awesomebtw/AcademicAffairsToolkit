using System.Windows;
using System.Windows.Controls;

namespace AcademicAffairsToolkit
{
    public enum SelectedFileType
    {
        Unknown,
        InvigilateFile,
        TROfficeFile
    }

    /// <summary>
    /// OpenOptionsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OpenOptionsWindow
    {
        public string FileName { get; set; }

        public string Password { get => filePasswordBox.Password; }

        public SelectedFileType SelectedFileType { get; private set; }

        public OpenOptionsWindow()
        {
            InitializeComponent();
        }

        public bool? ShowDialog(Window owner)
        {
            Owner = owner;
            return ShowDialog();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = sender is Fluent.Button b && b.Header.ToString() == "OK";
            Close();
        }

        private void OpenAsRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            if (sender is Fluent.RadioButton rb && rb.Tag is SelectedFileType type)
            {
                SelectedFileType = type;
            }
        }
    }
}
