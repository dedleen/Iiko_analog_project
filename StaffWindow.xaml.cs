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
    /// Логика взаимодействия для StaffWindow.xaml
    /// </summary>
    public partial class StaffWindow : Window
    {
        public StaffWindow()
        {
            InitializeComponent();
            LoadStaff();
        }

        private void LoadStaff()
        {
            using (var context = new OreroMenuEntities())
            {
                UserDataGrid.ItemsSource = context.Users.ToList();
            }
        }


        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as FrameworkElement)?.DataContext as Users;

            if (user == null) return;

            var result = MessageBox.Show($"Удалить пользователя {user.Username}?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                using (var context = new OreroMenuEntities())
                {
                    var userToRemove = context.Users.FirstOrDefault(u => u.UserID == user.UserID);
                    if (userToRemove != null)
                    {
                        context.Users.Remove(userToRemove);
                        context.SaveChanges();
                        MessageBox.Show("Пользователь удалён.");
                        LoadStaff(); 
                    }
                }
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new OreroMenuEntities())
            {
                var users = context.Users.ToList();

                var workbook = new ClosedXML.Excel.XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Персонал");

                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Имя пользователя";
                worksheet.Cell(1, 3).Value = "Код";
                worksheet.Cell(1, 4).Value = "Роль";
                worksheet.Cell(1, 5).Value = "Активен";

                for (int i = 0; i < users.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = users[i].UserID;
                    worksheet.Cell(i + 2, 2).Value = users[i].Username;
                    worksheet.Cell(i + 2, 3).Value = users[i].Code;
                    worksheet.Cell(i + 2, 4).Value = users[i].Role;
                    worksheet.Cell(i + 2, 5).Value = users[i].Activity == 1 ? "Да" : "Нет";
                }

                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = "Персонал",
                    DefaultExt = ".xlsx",
                    Filter = "Excel файлы (*.xlsx)|*.xlsx"
                };

                if (dialog.ShowDialog() == true)
                {
                    workbook.SaveAs(dialog.FileName);
                    MessageBox.Show("Экспорт выполнен успешно.");
                }
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new OreroMenuEntities())
            {
                foreach (var item in UserDataGrid.ItemsSource)
                {
                    if (item is Users user)
                    {

                        if (string.IsNullOrWhiteSpace(user.Username) ||
                            string.IsNullOrWhiteSpace(user.Code) ||
                            string.IsNullOrWhiteSpace(user.Role) ||
                            user.Activity == null)
                        {
                            MessageBox.Show($"Ошибка: Не все поля заполнены у пользователя с ID {user.UserID}.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var existing = context.Users.Find(user.UserID);
                        if (existing != null)
                        {
                            existing.Username = user.Username;
                            existing.Code = user.Code;
                            existing.Role = user.Role;
                            existing.Activity = user.Activity;
                        }
                        else
                        {
                            context.Users.Add(user);
                        }
                    }
                }
                context.SaveChanges();
                MessageBox.Show("Изменения сохранены!");
            }
        }
    }
}