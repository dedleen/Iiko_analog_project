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
                for (int i = 1; i <= 9; i++)
                {
                    var existingOrder = context.Orders.FirstOrDefault(o =>
                        o.TableID == i &&
                        o.Status == "Открыт" &&
                       (showAllOrders || o.WaiterID == SessionData.Instance.UserID));

                    Button btn = new Button
                    {
                        FontSize = 16,
                        Tag = i,
                        Padding = new Thickness(5),
                        Margin = new Thickness(5),
                        Height = 100,
                        Width = 150
                    };

                    if (existingOrder != null)
                    {
                        var dishNames = (from d in context.Dishes
                                         join od in context.OrderDetails on d.DishID equals od.DishId
                                         where od.OrderID == existingOrder.OrderID
                                         select d.Name).ToList();

                        string dishes = string.Join(", ", dishNames);
                        string content = $"Стол {i}\nБлюда: {dishes}\nСумма: {existingOrder.TotalSum} ₽";

                        btn.Content = new TextBlock
                        {
                            Text = content,
                            TextWrapping = TextWrapping.Wrap,
                            FontSize = 14
                        };

                        int capturedTableId = i;
                        var table = context.Tables.FirstOrDefault(t => t.TableID == existingOrder.TableID);
                        if (table != null)
                        {
                            var capturedTable = table;
                            btn.Click += (s, e) =>
                            {
                                _mainFrame.Navigate(new OrderPage(_mainFrame, capturedTable, SessionData.Instance.UserID));
                            };
                        }
                    }
                    else
                    {
                        btn.Content = new TextBlock
                        {
                            Text = "+",
                            FontSize = 40,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        int capturedTableId = i;
                        btn.Click += (s, e) =>
                        {
                            using (var contextInner = new OreroMenuEntities())
                            {
                                var existingOrderInner = contextInner.Orders.FirstOrDefault(o =>
                                    o.TableID == capturedTableId &&
                                    o.Status == "Открыт");

                                if (existingOrderInner != null)
                                {
                                    var table = contextInner.Tables.FirstOrDefault(t => t.TableID == capturedTableId);
                                    if (table != null)
                                    {
                                        _mainFrame.Navigate(new OrderPage(_mainFrame, table, SessionData.Instance.UserID));
                                    }
                                }
                                else
                                {
                                    TablesWindow tablesWindow = new TablesWindow(_mainFrame);
                                    tablesWindow.ShowDialog();
                                    GenerateTableCells();
                                }
                            }
                        };
                    }

                    TableGrid.Children.Add(btn);
                }
            }
        }
    }
}