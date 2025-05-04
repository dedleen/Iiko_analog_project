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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new AutorizationPage(MainFrame));
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            
            if(e.Content is AutorizationPage)
            {
                LogOutButton.Visibility = Visibility.Collapsed;
            }

            else
            {
                LogOutButton.Visibility = Visibility.Visible;
            }
        }
        private void Click_LogOut(object sender, RoutedEventArgs e)
        {
            SessionData.UserID = 0;
            SessionData.UserName = null;
            SessionData.UserRole = null;

            MainFrame.Navigate(new AutorizationPage(MainFrame));
        }



    }
}
