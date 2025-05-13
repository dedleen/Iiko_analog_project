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
        private readonly int _waiterId;
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

        public OrderPage(Frame mainFrame, Tables table, int waiterId)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
            _table = table;
            _waiterId = waiterId;
            OrderItems = new ObservableCollection<OrderItemViewModel>();
            LoadOrderData();
            DataContext = this;

            Find.TextChanged += (s, e) => SearchText = Find.Text;
            LoadCategories();
        }

        private void LoadOrderData()
        {
            using (var context = new OreroMenuEntities())
            {
                var openOrder = context.Orders
                    .Include(o => o.OrderDetails.Select(od => od.Dishes))
                    .FirstOrDefault(o => o.TableID == _table.TableID && o.Status == "Открыт");

                if (openOrder == null)
                {
                    openOrder = new Orders
                    {
                        TableID = _table.TableID,
                        Status = "Открыт",
                        DateCreated = DateTime.Now,
                        TotalSum = 0m,
                        WaiterID = _waiterId
                    };
                    context.Orders.Add(openOrder);
                    context.SaveChanges();
                }

                if (openOrder.OrderDetails != null && openOrder.OrderDetails.Any())
                {
                    foreach (var detail in openOrder.OrderDetails)
                    {
                        OrderItems.Add(new OrderItemViewModel
                        {
                            DishName = detail.Dishes?.Name ?? "Не указано",
                            Price = detail.Dishes?.Price.ToString("F2") + " Р" ?? "0.00 Р",
                            IsServed = detail.IsServed ?? "0"
                        });
                    }
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
                        openOrder.TotalSum = openOrder.OrderDetails
                            .Sum(d => (d.Dishes?.Price ?? 0m) * d.Quantity);
                        context.SaveChanges();
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
            if (item != null && item.IsServed == "0")
            {
                item.IsServed = "1";
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
                            detail.IsServed = "1";
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

                using (var context = new OreroMenuEntities())
                {
                    var openOrder = context.Orders
                        .FirstOrDefault(o => o.TableID == _table.TableID && o.Status == "Открыт");
                    if (openOrder != null)
                    {
                        openOrder.TotalSum = total;
                        context.SaveChanges();
                    }
                }
            }
        }

        private void LoadCategories()
        {
            using (var context = new OreroMenuEntities())
            {
                var categories = context.Categories.ToList();

                var categoryPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                foreach (var category in categories)
                {
                    var button = new Button
                    {
                        Content = category.Name,
                        Width = 150,
                        Height = 30,
                        Margin = new Thickness(5),
                        Tag = category.CategoryID
                    };
                    button.Click += (s, e) => LoadDishes((int)button.Tag);
                    categoryPanel.Children.Add(button);
                }
                CategoryFrame.Navigate(categoryPanel);
            }
        }

        private void LoadDishes(int categoryId)
        {
            using (var context = new OreroMenuEntities())
            {
                var dishes = context.Dishes
                    .Where(d => d.CategoryID == categoryId)
                    .ToList();

                var dishPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var backButton = new Button
                {
                    Content = "Вернуться",
                    Width = 90,
                    Height = 30,
                    Margin = new Thickness(5)
                };
                backButton.Click += (s, e) =>
                {
                    if (CategoryFrame.CanGoBack) CategoryFrame.GoBack();
                };
                dishPanel.Children.Add(backButton);

                foreach (var dish in dishes)
                {
                    var button = new Button
                    {
                        Content = $"{dish.Name} - {dish.Price} Р",
                        Width = 150,
                        Height = 30,
                        Margin = new Thickness(5),
                        Tag = dish
                    };
                    button.Click += (s, e) => AddToOrder((Dishes)button.Tag);
                    dishPanel.Children.Add(button);
                }
                CategoryFrame.Navigate(dishPanel);
            }
        }

        private void AddToOrder(Dishes dish)
        {
            using (var context = new OreroMenuEntities())
            {
                var openOrder = context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefault(o => o.TableID == _table.TableID && o.Status == "Открыт");

                if (openOrder == null)
                {
                    openOrder = new Orders
                    {
                        TableID = _table.TableID,
                        Status = "Открыт",
                        DateCreated = DateTime.Now,
                        TotalSum = 0m,
                        WaiterID = _waiterId
                    };
                    context.Orders.Add(openOrder);
                    context.SaveChanges();
                }

                var existingDetail = openOrder.OrderDetails.FirstOrDefault(d => d.DishID == dish.DishID);
                if (existingDetail != null)
                {
                    existingDetail.Quantity += 1;
                }
                else
                {
                    var newDetail = new OrderDetails
                    {
                        OrderID = openOrder.OrderID,
                        DishID = dish.DishID,
                        Quantity = 1,
                        IsServed = "0"
                    };
                    context.OrderDetails.Add(newDetail);
                }
                context.SaveChanges();

                OrderItems.Add(new OrderItemViewModel
                {
                    DishName = dish.Name,
                    Price = dish.Price.ToString("F2") + " Р",
                    IsServed = "0"
                });
                UpdateTotalPrice();
            }
        }
    }

    public class OrderItemViewModel
    {
        public string DishName { get; set; }
        public string Price { get; set; }
        public string IsServed { get; set; }
    }
}