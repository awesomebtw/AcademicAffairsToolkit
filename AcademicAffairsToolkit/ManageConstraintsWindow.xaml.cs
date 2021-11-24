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

        private void DateTimePickerValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (fromDateTimePicker != null && toDateTimePicker != null &&
                fromDateTimePicker.Value >= toDateTimePicker.Value)
            {
                MessageBox.Show(Resource.EndTimeEarlierThanStartTimeErrorTip, Resource.Error,
                    MessageBoxButton.OK, MessageBoxImage.Error);

                (sender as Xceed.Wpf.Toolkit.DateTimeUpDown).Value = (System.DateTime)e.OldValue;
                e.Handled = true;
            }
        }
    }
}
