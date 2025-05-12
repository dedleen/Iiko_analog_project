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
using static Kvalif.WaiterPage;

namespace Kvalif
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private OreroMenuEntities _entities;

        public MainWindow()
        {
            InitializeComponent();
            _entities = new OreroMenuEntities();
            MainFrame.Navigate(new AutorizationPage(MainFrame, this));
            DataContext = this; // Для привязки данных
            SessionData.Instance.PropertyChanged += SessionData_PropertyChanged; // Подписка на изменения
        }

        // Свойство для привязки имени пользователя
        public string UserName
        {
            get => SessionData.Instance.UserName ?? "Не авторизирован";
            set
            {
                if (SessionData.Instance.UserName != value)
                {
                    SessionData.Instance.UserName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        private void SessionData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SessionData.Instance.UserName))
            {
                OnPropertyChanged(nameof(UserName));
            }
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is AutorizationPage)
            {
                LogOutButton.Visibility = Visibility.Collapsed;
                CloseWSButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                LogOutButton.Visibility = Visibility.Visible;
                CloseWSButton.Visibility = Visibility.Visible;
            }
        }

        private void Click_LogOut(object sender, RoutedEventArgs e)
        {
            SessionData.Instance.UserID = 0;
            SessionData.Instance.UserName = null;
            SessionData.Instance.UserRole = null;
            SessionData.Instance.UserActivity = 0;
            OnPropertyChanged(nameof(UserName)); // Явно обновляем привязку
            MainFrame.Navigate(new AutorizationPage(MainFrame, this));
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
                    OnPropertyChanged(nameof(UserName)); // Явно обновляем привязку
                    MainFrame.Navigate(new AutorizationPage(MainFrame, this));
                }
            }
        }

        // Реализация INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}