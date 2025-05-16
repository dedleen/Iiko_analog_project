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
    /// Логика взаимодействия для CloseSW.xaml
    /// </summary>
    public partial class CloseSW : Page
    {
        private readonly OreroMenuEntities _entities;
        private readonly Frame _mainFrame;
        private readonly MainWindow _mainWindow;

        public CloseSW(Frame frame, MainWindow mainWindow)
        {
            InitializeComponent();
            _entities = new OreroMenuEntities();
            _mainFrame = frame;
            _mainWindow = mainWindow;
        }

        private void CloseWSButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionData.Instance.UserID != 0 && SessionData.Instance.UserActivity == 1)
            {
                var user = _entities.Users.FirstOrDefault(u => u.UserID == SessionData.Instance.UserID);
                if (user != null)
                {
                    user.Activity = 0;
                    _entities.SaveChanges();

                    MessageBox.Show($"Смена закрыта для {SessionData.Instance.UserName}");

                    
                    SessionData.Instance.UserID = 0;
                    SessionData.Instance.UserName = null;
                    SessionData.Instance.UserRole = null;
                    SessionData.Instance.UserActivity = 0;

                    
                    _mainFrame.Navigate(new AutorizationPage(_mainFrame, _mainWindow));
                }
            }
        }
    }
}