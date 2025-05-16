using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class AutorizationPage : Page
    {
        private readonly Frame _mainFrame;
        private readonly MainWindow _mainWindow;
        private readonly OreroMenuEntities _entities;

        public AutorizationPage(Frame mainFrame, MainWindow mainWindow)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
            _mainWindow = mainWindow;
            _entities = new OreroMenuEntities();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string code = txtCode.Text.Trim();

            var user = _entities.Users.FirstOrDefault(u => u.Code == code);

            if (user != null)
            {
                SessionData.Instance.UserID = user.UserID;
                SessionData.Instance.UserName = user.Username;
                SessionData.Instance.UserRole = user.Role;
                SessionData.Instance.UserActivity = (int)user.Activity;

                if (user.Activity == 1)
                {
                    switch (user.Role)
                    {
                        case "Admin":
                            _mainFrame.Navigate(new AdminPage(_mainFrame));
                            break;
                        case "Waiter":
                            _mainFrame.Navigate(new WaiterPage(_mainFrame));
                            break;
                        case "Manager":
                            _mainFrame.Navigate(new ManagerPage());
                            break;
                    }
                }
                else
                {
                    MessageBoxResult dr = MessageBox.Show("Открыть личную смену?", $"{user.Username}", MessageBoxButton.YesNo);
                    if (dr == MessageBoxResult.Yes)
                    {
                        user.Activity = 1;
                        _entities.SaveChanges();
                        SessionData.Instance.UserActivity = 1;
                        switch (user.Role)
                        {
                            case "Admin":
                                _mainFrame.Navigate(new AdminPage(_mainFrame));
                                break;
                            case "Waiter":
                                _mainFrame.Navigate(new WaiterPage(_mainFrame));
                                break;
                            case "Manager":
                                _mainFrame.Navigate(new ManagerPage());
                                break;
                        }
                    }
                    else if (dr == MessageBoxResult.No)
                    {
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Неверный код", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}