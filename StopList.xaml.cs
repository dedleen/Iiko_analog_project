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
    /// Логика взаимодействия для StopList.xaml
    /// </summary>
    public partial class StopList : Window
    {
        public StopList()
        {
            InitializeComponent();
            LoadDishes();
        }

        private List<Dishes> _dishes;

        private void LoadDishes()
        {
            using (var context = new OreroMenuEntities())
            {
                _dishes = context.Dishes.ToList();
                dishesDataGrid.ItemsSource = _dishes;
            }
        }

        private void SaveStopListChanges_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new OreroMenuEntities())
            {
                foreach (var dish in _dishes)
                {
                    var dishInDb = context.Dishes.Find(dish.DishID);
                    if (dishInDb != null)
                    {
                        dishInDb.InStopList = dish.InStopList;
                    }
                }
                context.SaveChanges();
            }

            MessageBox.Show("Изменения сохранены");
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                dishesDataGrid.ItemsSource = _dishes;
            }
            else
            {
                var filtered = _dishes
                    .Where(d => d.Name != null && d.Name.ToLower().Contains(searchText))
                    .ToList();
                dishesDataGrid.ItemsSource = filtered;
            }
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            dishesDataGrid.ItemsSource = _dishes;
        }
    }
}
