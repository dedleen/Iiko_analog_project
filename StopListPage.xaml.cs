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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kvalif
{
    /// <summary>
    /// Логика взаимодействия для StopListPage.xaml
    /// </summary>
    public partial class StopListPage : Page
    {
        private List<Dishes> allDishes;

        public StopListPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDishes();
        }

        private void LoadDishes()
        {
            using (var context = new OreroMenuEntities())
            {
                allDishes = context.Dishes.ToList(); // сохраняем полный список
                DishesDataGrid.ItemsSource = allDishes;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new OreroMenuEntities())
            {
                foreach (Dishes updatedDish in DishesDataGrid.ItemsSource)
                {
                    var dish = context.Dishes.FirstOrDefault(d => d.DishID == updatedDish.DishID);
                    if (dish != null)
                    {
                        dish.InStopList = updatedDish.InStopList;
                    }
                }
                context.SaveChanges();
                MessageBox.Show("Изменения стоп-листа сохранены.");
            }

            LoadDishes(); // обновим интерфейс
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = SearchBox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(filter))
            {
                DishesDataGrid.ItemsSource = allDishes;
            }
            else
            {
                var filtered = allDishes
                    .Where(d => d.Name != null && d.Name.ToLower().Contains(filter))
                    .ToList();

                DishesDataGrid.ItemsSource = filtered;
            }
        }
    }
}