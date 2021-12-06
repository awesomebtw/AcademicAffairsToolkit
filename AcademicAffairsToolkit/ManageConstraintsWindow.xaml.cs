using System.Windows;
using System.Windows.Controls;

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
            currentItemBindingGroup?.BeginEdit();
        }

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (itemsGallery.SelectedIndex != -1)
                Session.Constraints.RemoveAt(itemsGallery.SelectedIndex);
        }

        private void ItemsGallerySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentItemBindingGroup?.CommitEdit() == true)
                currentItemBindingGroup?.BeginEdit();
            else
                currentItemBindingGroup?.CancelEdit();
        }

        private void DateTimePickerLostFocus(object sender, RoutedEventArgs e)
        {
            if (currentItemBindingGroup?.CommitEdit() == true)
                currentItemBindingGroup?.BeginEdit();
            else
                currentItemBindingGroup?.CancelEdit();
        }

        private void Grid_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                MessageBox.Show(e.Error.ErrorContent.ToString(), Resource.Error,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            e.Handled = true;
        }
    }
}
