using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.ComponentModel;
using Kvalif.ViewModels;
using System.Windows.Media;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using System.Diagnostics;

namespace Kvalif
{
    public partial class OrderPage : Page
    {
        private Orders currentOrder;

        private readonly System.Windows.Controls.Frame _mainFrame;
        private readonly Tables _table;
        private readonly int _waiterId;

        public OrderPage(Orders order)
        {
            InitializeComponent();
            currentOrder = order;

            LoadWaiters();
            LoadTables();

            comboBoxWaiters.SelectedValue = currentOrder.WaiterID;
            comboBoxTables.SelectedValue = currentOrder.TableID;
        }
        
        
        private void LoadWaiters()
        {
            using (var db = new OreroMenuEntities())
            {
                var waiters = db.Users
                    .Where(u => u.Role == "Waiter")
                    .ToList();

                MessageBox.Show($"Официантов загружено: {waiters.Count}");
                comboBoxWaiters.ItemsSource = waiters;
            }
        }

        private void LoadTables()
        {
            using(var db = new OreroMenuEntities())
            {
                var tables = db.Tables.ToList();
                MessageBox.Show($"Столов загружено: {tables.Count}");
                comboBoxTables.ItemsSource = tables;
            }
        }

        private void comboBoxWaiters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxWaiters.SelectedValue is int newWaiterID && newWaiterID != currentOrder.WaiterID)
            {
                var selectedWaiter = (Users)comboBoxWaiters.SelectedItem;

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите изменить официанта на {selectedWaiter.Username}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var db = new OreroMenuEntities())
                    {
                        var orderToUpdate = db.Orders.FirstOrDefault(o => o.OrderID == currentOrder.OrderID);
                        if (orderToUpdate != null)
                        {
                            orderToUpdate.WaiterID = newWaiterID;
                            db.SaveChanges();
                            currentOrder.WaiterID = newWaiterID;
                        }
                    }
                }
                else
                {
                    comboBoxWaiters.SelectedValue = currentOrder.WaiterID;
                }
            }
        }

        private void comboBoxTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxTables.SelectedValue is int newTableID && newTableID != currentOrder.TableID)
            {
                var selectedTable = (Tables)comboBoxTables.SelectedItem;

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите изменить стол на №{selectedTable.Number}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var db = new OreroMenuEntities())
                    {
                        var orderToUpdate = db.Orders.FirstOrDefault(o => o.OrderID == currentOrder.OrderID);
                        if (orderToUpdate != null)
                        {
                            orderToUpdate.TableID = newTableID;
                            db.SaveChanges();
                            currentOrder.TableID = newTableID;
                        }
                    }
                }
                else
                {
                    comboBoxTables.SelectedValue = currentOrder.TableID;
                }
            }
        }


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
                    .Include("OrderDetails.Dishes")
                    .FirstOrDefault(o => o.TableID == _table.TableID && o.Status == "Открыт");

                if (order != null)
                {
                    string fileName = $"Пречек_Стол_{_table.Number}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
                    {
                        MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                        mainPart.Document = new Document();
                        Body body = new Body();

                        Paragraph header = new Paragraph(
                            new Run(
                                new Text($"Стол №{_table.Number} — Пречек"))
                        );
                        header.ParagraphProperties = new ParagraphProperties(
                            new Justification() { Val = JustificationValues.Center },
                            new SpacingBetweenLines() { After = "200" }
                        );
                        body.AppendChild(header);

                        Table table = new Table();

                        TableProperties tblProps = new TableProperties(
                            new TableBorders(
                                new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                            )
                        );
                        table.AppendChild(tblProps);

                        TableRow headerRow = new TableRow();
                        headerRow.Append(
                            CreateCell("Блюдо"),
                            CreateCell("Кол-во"),
                            CreateCell("Цена")
                        );
                        table.Append(headerRow);

                        decimal total = 0;

                        foreach (var item in order.OrderDetails)
                        {
                            decimal price = item.Quantity * item.Dishes.Price;
                            total += price;

                            TableRow row = new TableRow();
                            row.Append(
                                CreateCell(item.Dishes.Name),
                                CreateCell(item.Quantity.ToString()),
                                CreateCell($"{price} ₽")
                            );
                            table.Append(row);
                        }

                        body.AppendChild(table);

                        Paragraph totalParagraph = new Paragraph(
                            new Run(
                                new Text($"\nИтого: {total} ₽"))
                        );
                        totalParagraph.ParagraphProperties = new ParagraphProperties(
                            new Justification() { Val = JustificationValues.Right },
                            new SpacingBetweenLines() { Before = "200" }
                        );
                        body.AppendChild(totalParagraph);

                        mainPart.Document.Append(body);
                        mainPart.Document.Save();
                    }

                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    MessageBox.Show($"Пречек сохранён в документ: {filePath}", "Готово");
                }
                else
                {
                    MessageBox.Show("Нет открытых заказов для этого стола.");
                }
            }
        }


        private TableCell CreateCell(string text)
        {
            return new TableCell(
                new Paragraph(
                    new Run(
                        new Text(text)
                    )
                )
            );
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

        public OrderPage(System.Windows.Controls.Frame mainFrame, Tables table, int waiterId)
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
                    Orientation = System.Windows.Controls.Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top
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
                    Orientation = System.Windows.Controls.Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top
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

                    if(dish.InStopList)
                    {
                        button.Foreground = Brushes.Red;
                        button.Background = Brushes.LightCoral;
                    }
                    button.Click += (s, e) => AddToOrder((Dishes)button.Tag);
                    dishPanel.Children.Add(button);
                }
                CategoryFrame.Navigate(dishPanel);
            }
        }

        private void AddToOrder(Dishes dish)
        {
            if (dish.InStopList)
            {
                MessageBox.Show("Это блюдо находится в стоп-листе и не может быть добавлено в заказ.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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

