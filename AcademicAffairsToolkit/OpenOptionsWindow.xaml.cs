using System.Windows;
using System.Windows.Controls.Primitives;

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
            DialogResult = sender is ButtonBase b && b.Tag?.ToString() == "OK";
            Close();
        }

        private void OpenAsRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            if (sender is Fluent.RadioButton rb && rb.Tag is SelectedFileType type)
            {
                SelectedFileType = type;
                if (parseOptionsPanel != null)
                {
                    parseOptionsPanel.DataContext = type switch
                    {
                        SelectedFileType.InvigilateFile => Session.InvigilateFilePolicy,
                        SelectedFileType.TROfficeFile => Session.TROfficeFilePolicy,
                        _ => null
                    };
                }
            }
        }
    }
}
