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
    /// Логика взаимодействия для ChangeQuantityWindow.xaml
    /// </summary>
    public partial class ChangeQuantityWindow : Window
    {
        public int? NewQuantity { get; private set; }

        public ChangeQuantityWindow(int currentQuantity)
        {
            InitializeComponent();
            QuantityTextBox.Text = currentQuantity.ToString();
            QuantityTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out int qty) && qty > 0)
            {
                NewQuantity = qty;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Введите положительное число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}