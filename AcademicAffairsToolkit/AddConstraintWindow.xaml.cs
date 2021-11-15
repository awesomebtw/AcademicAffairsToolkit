using System;
using System.Windows;

namespace AcademicAffairsToolkit
{
    /// <summary>
    /// AddConstraintWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddConstraintWindow
    {
        public AddConstraintWindow()
        {
            InitializeComponent();
        }

        private void OKButtonClick(object sender, RoutedEventArgs e)
        {
            if (trOfficePicker.SelectedIndex != -1
                && fromDateTimePicker.Value is DateTime from
                && toDateTimePicker.Value is DateTime to)
            {
                if (from >= to)
                {
                    MessageBox.Show("The first date must be earlier than the second date.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Session.Constraints.Add(new InvigilateConstraint(
                        from, to, trOfficePicker.SelectedItem as TROfficeRecordEntry));
                    Close();
                }
            }
            else
            {
                MessageBox.Show("Please fill in all fields before pressing OK.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
