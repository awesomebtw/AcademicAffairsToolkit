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

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            // properties in datetime are read-only and can't bind to datetime
            // so we have to update it manually
            if (trOfficePicker.SelectedIndex != -1)
                Session.Constraints.Add(new InvigilateConstraint(
                    fromDatePicker.DisplayDate.Date.AddHours(fromHourPicker.Value).AddMinutes(fromMinutePicker.Value),
                    toDatePicker.DisplayDate.Date.AddHours(toHourPicker.Value).AddMinutes(toMinutePicker.Value),
                    trOfficePicker.SelectedItem as TROfficeRecordEntry));
            else
                MessageBox.Show("Please select an office before adding constraint.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (itemsGallery.SelectedIndex != -1)
                Session.Constraints.RemoveAt(itemsGallery.SelectedIndex);
        }

        private void TimePickerValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // properties in datetime are read-only and can't bind to datetime
            // so we have to update it manually
        }
    }
}
