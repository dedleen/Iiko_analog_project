using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kvalif
{
    /// <summary>
    /// Логика взаимодействия для WaiterPage.xaml
    /// </summary>
    public partial class WaiterPage : Page
    {
        public class WaiterInfo
        {
            public string ShowWaiter { get; set; }
        }

        private Frame _mainFrame;
        private bool showAllOrders = false;
        public WaiterPage(Frame frame, bool showAllOrders = false)
        {
            InitializeComponent();
            _mainFrame = frame;
            this.showAllOrders = showAllOrders;
            LoadWaiters();
            GenerateTableCells();
        }


        private void LoadTables()
        {
            using (var context = new OreroMenuEntities())
            {
                var openOrders = context.Orders
                    .Where(o => o.Status == "Открыт")
                    .Select(o => new
                    {
                        TableNumber = o.Tables.Number,
                        WaiterName = o.Users.Username,
                        TotalSum = o.TotalSum
                    })
                    .ToList();

                WLV.ItemsSource = openOrders;
            }
        }


        private void LoadWaiters()
        {
            using (var context = new OreroMenuEntities())
            {
                var activeWaiters = context.Users
                    .Where(u => u.Activity == 1)
                    .Select(u => new WaiterInfo
                    {
                        ShowWaiter = u.Username
                    })
                    .ToList();

                WLV.ItemsSource = activeWaiters;
            }
        }

        private void GenerateTableCells()
        {
            TableGrid.Children.Clear();

            using (var context = new OreroMenuEntities())
            {
                var allTables = context.Tables.OrderBy(t => t.Number).ToList();

                foreach (var table in allTables)
                {
                    var existingOrder = context.Orders.FirstOrDefault(o =>
                        o.TableID == table.TableID &&
                        o.Status == "Открыт");

                    Button btn = new Button
                    {
                        FontSize = 16,
                        Tag = table.TableID,
                        Padding = new Thickness(5),
                        Margin = new Thickness(5),
                        Height = 100,
                        Width = 150,
                        Background = Brushes.White,    
                        BorderThickness = new Thickness(3)
                    };

                    if (existingOrder != null)
                    {
                        var waiterName = context.Users.FirstOrDefault(u => u.UserID == existingOrder.WaiterID)?.Username ?? "Неизвестно";

                        if (existingOrder.WaiterID == SessionData.Instance.UserID)
                        {
                           
                            btn.BorderBrush = Brushes.LimeGreen;

                            var dishNames = (from d in context.Dishes
                                             join od in context.OrderDetails on d.DishID equals od.DishID
                                             where od.OrderID == existingOrder.OrderID
                                             select d.Name).ToList();

                            string dishes = string.Join(", ", dishNames);
                            string content = $"Стол {table.Number}\nБлюда: {dishes}\nСумма: {existingOrder.TotalSum} ₽";

                            btn.Content = new TextBlock
                            {
                                Text = content,
                                TextWrapping = TextWrapping.Wrap,
                                FontSize = 14
                            };

                            var capturedTable = table;
                            btn.Click += (s, e) =>
                            {
                                _mainFrame.Navigate(new OrderPage(_mainFrame, capturedTable, SessionData.Instance.UserID));
                            };
                        }
                        else
                        {
                           
                            btn.BorderBrush = Brushes.Gray;

                            string content = $"Стол {table.Number}\nОфициант: {waiterName}";

                            btn.Content = new TextBlock
                            {
                                Text = content,
                                TextWrapping = TextWrapping.Wrap,
                                FontSize = 14,
                                Foreground = Brushes.DarkGray,
                                FontStyle = FontStyles.Italic
                            };
                            if (SessionData.Instance.UserRole == "Waiter")
                            { 
                                btn.Click += (s, e) =>
                                {
                                    MessageBox.Show($"Этот стол занят другим официантом: {waiterName}");
                                };
                            
                            }
                            else
                            {
                                btn.Click += (s, e) =>
                                {
                                    _mainFrame.Navigate(new OrderPage(_mainFrame, table, SessionData.Instance.UserID));
                                };
                            }
                        }
                    }
                    else
                    {
                        
                        btn.BorderBrush = Brushes.Gray;
                        btn.BorderThickness = new Thickness(1);

                        btn.Content = new TextBlock
                        {
                            Text = "+",
                            FontSize = 40,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Foreground = Brushes.Black
                        };

                        var capturedTable = table;

                        btn.Click += (s, e) =>
                        {
                            using (var contextInner = new OreroMenuEntities())
                            {
                                var otherOrder = contextInner.Orders.FirstOrDefault(o =>
                                    o.TableID == capturedTable.TableID &&
                                    o.Status == "Открыт");

                                if (otherOrder != null)
                                {
                                    var otherWaiter = contextInner.Users.FirstOrDefault(u => u.UserID == otherOrder.WaiterID)?.Username ?? "Неизвестно";
                                    MessageBox.Show($"Этот стол занят другим официантом: {otherWaiter}");
                                    return;
                                }

                                TablesWindow tablesWindow = new TablesWindow(_mainFrame);
                                tablesWindow.ShowDialog();
                                GenerateTableCells();
                            }
                        };
                    }

                    TableGrid.Children.Add(btn);
                }
            }
        }
    }
}