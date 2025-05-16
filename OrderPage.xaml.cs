using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Diagnostics.Eventing.Reader;
using System.Text;

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

        private void GeneratePrecheck_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new OreroMenuEntities())
            {
                var order = context.Orders
                    .Include("OrderDetails")
                    .FirstOrDefault(o => o.TableID == _table.TableID && o.Status == "Открыт");

                if (order != null)
                {
                    StringBuilder receipt = new StringBuilder();
                    receipt.AppendLine($"Стол №{_table.Number} — Пречек");
                    receipt.AppendLine("Блюдо\t\tКол-во\tЦена");

                    decimal total = 0;

                    foreach (var item in order.OrderDetails)
                    {
                        receipt.AppendLine($"{item.Dishes.Name}\t{item.Quantity}\t{item.Quantity * item.Dishes.Price}₽");
                        total += item.Quantity * item.Dishes.Price;
                    }

                    receipt.AppendLine($"\nИтого: {total}₽");

                    MessageBox.Show(receipt.ToString(), "Пречек");


                }
            }
        }

        private void ChangeQuantity_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DishesListBox.SelectedItem as OrderDetails;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите блюдо.");
                return;
            }

            var window = new ChangeQuantityWindow(selectedItem.Quantity);
            window.Owner = Window.GetWindow(this); // чтобы окно было поверх текущего

            if (window.ShowDialog() == true && window.NewQuantity.HasValue)
            {
                using (var context = new OreroMenuEntities())
                {
                    var item = context.OrderDetails.FirstOrDefault(i => i.DetailID == selectedItem.DetailID);
                    if (item != null)
                    {
                        item.Quantity = window.NewQuantity.Value;
                        context.SaveChanges();
                    }
                }

                RefreshOrderList();
            }
        }

        private void RefreshOrderList()
        {
            using (var context = new OreroMenuEntities())
            {
                var order = context.Orders
                    .Include("OrderDetails.Dishes")
                    .FirstOrDefault(o => o.TableID == _table.TableID && o.Status == "Открыт");

                if (order != null)
                {
                    DishesListBox.ItemsSource = order.OrderDetails.ToList();
                }
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(new WaiterPage(_mainFrame));
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

        private void CloseOrder_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите закрыть заказ?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes && (SessionData.Instance.UserRole=="Admin" || SessionData.Instance.UserRole == "Manager"))
            {
                using (var context = new OreroMenuEntities())
                {
                    var openOrder = context.Orders
                        .FirstOrDefault(o => o.TableID == _table.TableID && o.Status == "Открыт");

                    if (openOrder != null)
                    {
                        openOrder.Status = "Закрыт";
                        context.SaveChanges();
                    }
                }
            
            

                    MessageBox.Show("Заказ закрыт!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Переход назад на страницу официанта
                _mainFrame.Navigate(new WaiterPage(_mainFrame));
            }
            else
            {
                MessageBox.Show("У вас недостаточно прав для выполнения данного действия", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                            IsServed = (detail.IsServed == "1")
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
            if (!item.IsServed)
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
                    VerticalAlignment = VerticalAlignment.Top
                };

                foreach (var category in categories)
                {
                    var button = new Button
                    {
                        Content = category.Name,
                        Width = 200,
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
                    VerticalAlignment = VerticalAlignment.Top
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
                    LoadCategories();
                };
                dishPanel.Children.Add(backButton);

                foreach (var dish in dishes)
                {
                    var button = new Button
                    {
                        Content = $"{dish.Name} - {dish.Price} Р",
                        Width = 200,
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
                    openOrder.TotalSum = openOrder.OrderDetails.Sum(d => (d.Dishes?.Price ?? 0m) * d.Quantity);
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
                    IsServed = false
                });
                UpdateTotalPrice();
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