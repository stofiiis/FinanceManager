using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace PersonalFinanceManager
{
    public partial class ReportsView : UserControl
    {
        public ReportsView()
        {
            InitializeComponent();
        }
    }

    public class PercentageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is decimal) || !(values[1] is decimal))
                return 0;

            decimal value = (decimal)values[0];
            decimal total = (decimal)values[1];

            if (total == 0)
                return 0;

            // Calculate percentage of the total width
            double percentage = (double)(value / total);

            // Get the parent control's actual width
            if (targetType == typeof(double))
                return percentage * 100; // Use percentage of 100 for testing

            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
