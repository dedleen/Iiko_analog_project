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
    /// Логика взаимодействия для AutorizationPage.xaml
    /// </summary>
    public partial class AutorizationPage : Page
    {
        private Frame _mainFrame;
        OreroMenuEntities entities = new OreroMenuEntities();
        public AutorizationPage(Frame MainFrame)
        {
            InitializeComponent();
            _mainFrame = MainFrame;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string code = txtCode.Text.Trim();

           
                var user = entities.Users.FirstOrDefault(u => u.Code == code);

                if(user != null)
                {

                    SessionData.UserID = user.UserID;
                    SessionData.UserName = user.Username;
                    SessionData.UserRole = user.Role;
                    SessionData.UserActivity = (int)user.Activity;
                    
                    if(user.Activity == 1)
                    {
                        switch(user.Role)
                        {
                            case "Admin":
                                 _mainFrame.Navigate(new AdminPage());
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
                        MessageBoxResult dr =  MessageBox.Show("Открыть личную смену?", $"{user.Username}", MessageBoxButton.YesNo);
                       if(dr == MessageBoxResult.Yes)
                       {
                            user.Activity = 1;
                            entities.SaveChanges();

                            switch (user.Role)
                            {
                                case "Admin":
                                    _mainFrame.Navigate(new AdminPage());
                                    break;

                                case "Waiter":
                                    _mainFrame.Navigate(new WaiterPage(_mainFrame));
                                    break;

                                case "Manager":
                                    _mainFrame.Navigate(new ManagerPage());
                                    break;
                            }
                       }
                       if(dr == MessageBoxResult.No)
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
