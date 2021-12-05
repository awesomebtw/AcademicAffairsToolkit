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
        }

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (itemsGallery.SelectedIndex != -1)
                Session.Constraints.RemoveAt(itemsGallery.SelectedIndex);
        }

        private void ItemsGallerySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindingGroup.CommitEdit();
        }

        private void DateTimePickerGotFocus(object sender, RoutedEventArgs e)
        {
            BindingGroup.BeginEdit();
        }

        private void DateTimePickerLostFocus(object sender, RoutedEventArgs e)
        {
            if (fromDateTimePicker != null && toDateTimePicker != null)
            {
                if (fromDateTimePicker.Value >= toDateTimePicker.Value)
                {
                    BindingGroup.CancelEdit();

                    MessageBox.Show(Resource.EndTimeEarlierThanStartTimeErrorTip, Resource.Error,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    BindingGroup.CommitEdit();
                }
            }
            e.Handled = true;
        }
    }
}
