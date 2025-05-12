using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace Kvalif
{
    public partial class OrderPage : Page
    {
        private readonly Frame _mainFrame;
        private readonly Tables _table;
        public ObservableCollection<OrderItemViewModel> OrderItems { get; set; }
        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                FilterItems();
            }
        }

        public OrderPage(Frame mainFrame, Tables table)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
            _table = table;
            OrderItems = new ObservableCollection<OrderItemViewModel>();
            LoadOrderData();
            DataContext = this;

            Find.TextChanged += (s, e) => SearchText = Find.Text;
        }

        private void LoadOrderData()
        {
            using (var context = new OreroMenuEntities())
            {
                var allOrders = context.Orders.ToList();
                var openOrder = context.Orders
                    .Include(o => o.OrderDetails.Select(od => od.Dishes))
                    .FirstOrDefault(o => o.TableID == _table.TableID && o.Status == "Открыт");

                if (openOrder != null && openOrder.OrderDetails != null)
                {
                    foreach (var detail in openOrder.OrderDetails)
                    {
                        OrderItems.Add(new OrderItemViewModel
                        {
                            DishName = detail.Dishes.Name,
                            Price = detail.Dishes.Price.ToString("F2") + " Р",
                            IsServed = detail.IsServedBool // Теперь используем IsServed из OrderDetails
                        });
                    }
                }
                else
                {
                    MessageBox.Show($"Заказ для стола с ID {_table.TableID} не найден или у него нет деталей.");
                }
                UpdateTotalPrice();
            }
        }

        private void FilterItems()
        {
            var filteredItems = string.IsNullOrWhiteSpace(_searchText)
                ? OrderItems
                : OrderItems.Where(i => i.DishName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            DishesListBox.ItemsSource = filteredItems;
            UpdateTotalPrice();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.DataContext as OrderItemViewModel;
            if (item != null)
            {
                using (var context = new OreroMenuEntities())
                {
                    var openOrder = context.Orders
                        .Include(o => o.OrderDetails)
                        .FirstOrDefault(o => o.TableID == _table.TableID && o.Status == "Открыт");
                    if (openOrder != null)
                    {
                        var detail = openOrder.OrderDetails.FirstOrDefault(d => d.Dishes.Name == item.DishName);
                        if (detail != null)
                        {
                            context.OrderDetails.Remove(detail);
                            context.SaveChanges();
                        }
                    }
                }
                OrderItems.Remove(item);
                UpdateTotalPrice();
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.DataContext as OrderItemViewModel;
            if (item != null && !item.IsServed)
            {
                item.IsServed = true;
                using (var context = new OreroMenuEntities())
                {
                    var openOrder = context.Orders
                        .Include(o => o.OrderDetails)
                        .FirstOrDefault(o => o.TableID == _table.TableID && o.Status == "Открыт");
                    if (openOrder != null)
                    {
                        var detail = openOrder.OrderDetails.FirstOrDefault(d => d.Dishes.Name == item.DishName);
                        if (detail != null)
                        {
                            detail.IsServedBool = true;
                            context.SaveChanges();
                        }
                    }
                }
                UpdateTotalPrice();
            }
        }

        private void UpdateTotalPrice()
        {
            if (OrderItems != null)
            {
                decimal total = OrderItems.Sum(item => decimal.TryParse(item.Price.Replace(" Р", ""), out decimal price) ? price : 0);
                TotalPrice.Text = total.ToString("F2") + " Р";
            }
        }
    }

    public class OrderItemViewModel
    {
        public string DishName { get; set; }
        public string Price { get; set; }
        public bool IsServed { get; set; }
    }
}