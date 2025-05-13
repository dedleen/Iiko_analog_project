using System;
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
using System.Windows.Shapes;

namespace Kvalif
{
    /// <summary>
    /// Логика взаимодействия для Tables.xaml
    /// </summary>
    public partial class TablesWindow : Window
    {
        private readonly Frame _mainFrame;
        public TablesWindow(Frame mainFrame)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
        }

        private void OneButton_Click(object sender, RoutedEventArgs e)
        {
            TableBox.Text += "1";
        }
        private void TwoButton_Click(object sender, RoutedEventArgs e)
        {
            TableBox.Text += "2";
        }
        private void ThreeButton_Click(object sender, RoutedEventArgs e)
        {
            TableBox.Text += "3";
        }
        private void FourButton_Click(object sender, RoutedEventArgs e)
        {
            TableBox.Text += "4";
        }
        private void FiveButton_Click(object sender, RoutedEventArgs e)
        {
            TableBox.Text += "5";
        }
        private void SixButton_Click(object sender, RoutedEventArgs e)
        {
            TableBox.Text += "6";
        }
        private void SevenButton_Click(object sender, RoutedEventArgs e)
        {
            TableBox.Text += "7";
        }
        private void EightButton_Click(object sender, RoutedEventArgs e)
        {
            TableBox.Text += "8";
        }
        private void NineButton_Click(object sender, RoutedEventArgs e)
        {
            TableBox.Text += "9";
        }

        private void ZeroButton_Click(object sender, RoutedEventArgs e)
        {
            TableBox.Text += "0";
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            TableBox.Text = "";
        }
        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TableBox.Text) || !int.TryParse(TableBox.Text, out int tableNumber))
            {
                MessageBox.Show("Пожалуйста, введите корректный номер стола.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new OreroMenuEntities())
                {
                    var table = context.Tables.FirstOrDefault(t => t.Number == tableNumber);
                    if (table == null)
                    {
                        MessageBox.Show($"Стол с номером {tableNumber} не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (SessionData.Instance.UserID <= 0)
                    {
                        MessageBox.Show("Официант не авторизован.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var order = new Orders
                    {
                        TableID = table.TableID,
                        WaiterID = SessionData.Instance.UserID,
                        DateCreated = DateTime.Now,
                        Status = "Открыт",
                        TotalSum = 0m
                    };

                    context.Orders.Add(order);
                    context.SaveChanges();

                    _mainFrame.Navigate(new OrderPage(_mainFrame, table, SessionData.Instance.UserID));

                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
