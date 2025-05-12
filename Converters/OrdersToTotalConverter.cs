using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Kvalif.Converters
{
    class OrdersToTotalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection<Orders> orders)
                return orders.Where(o => o.Status == "Открыт").Sum(o => o.TotalSum);
            return 0m;
        }

        public object ConvertBack(object value, Type targetType, object  parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
