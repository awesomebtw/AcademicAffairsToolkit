using System.Windows;

namespace AcademicAffairsToolkit
{
    /// <summary>
    /// ManageConstraintsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ManageConstraintsWindow
    {
        public ManageConstraintsWindow()
        {
            InitializeComponent();
        }

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (itemsGallery.SelectedIndex != -1)
                Session.Constraints.RemoveAt(itemsGallery.SelectedIndex);
        }
    }
}
