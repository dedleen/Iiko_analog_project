using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Kvalif.Converters
{
    class OrderDetailsToListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection<Orders> orders)
            {
                return orders.Where(o => o.Status == "Открыт")
                    .SelectMany(o => o.OrderDetails)
                    .Select(od => new
                    {
                        DishName = od.Dishes?.Name ?? "Unknown",
                        od.Quantity,
                        Price = od.Dishes?.Price ?? 0m
                    })
                    .ToList();
            }

            return new List<object>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
