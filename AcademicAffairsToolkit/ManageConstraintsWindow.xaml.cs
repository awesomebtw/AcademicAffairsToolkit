using System;
using System.Windows;

namespace AcademicAffairsToolkit
{
    enum ChangedItem
    {
        FromHour,
        FromMinute,
        ToHour,
        ToMinute
    }

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
            if (trOfficePicker.SelectedIndex != -1
                && fromDateTimePicker.Value is DateTime from
                && toDateTimePicker.Value is DateTime to)
            {
                Session.Constraints.Add(new InvigilateConstraint(
                    from, to, trOfficePicker.SelectedItem as TROfficeRecordEntry));
                itemsGallery.SelectedIndex = Session.Constraints.Count - 1;
            }
            else
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (itemsGallery.SelectedIndex != -1)
                Session.Constraints.RemoveAt(itemsGallery.SelectedIndex);
        }
    }
}
