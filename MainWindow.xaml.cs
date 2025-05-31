
using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;


namespace Kvalif
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private OreroMenuEntities _entities;
        readonly MainWindow mainWindow;
        public MainWindow()
        {
            InitializeComponent();
            _entities = new OreroMenuEntities();
            MainFrame.Navigate(new AutorizationPage(MainFrame, this));
            DataContext = this; 
            SessionData.Instance.PropertyChanged += SessionData_PropertyChanged;
        }

        // cвойство для привязки имени пользователя
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
                MenuOutButton.Visibility = Visibility.Collapsed;
                LogOutButton.Visibility = Visibility.Collapsed;
                
            }
            else
            {
                MenuOutButton.Visibility = Visibility.Visible;
                LogOutButton.Visibility = Visibility.Visible;
                
            }
        }

        private void Click_LogOut(object sender, RoutedEventArgs e)
        {
            SessionData.Instance.UserID = 0;
            SessionData.Instance.UserName = null;
            SessionData.Instance.UserRole = null;
            SessionData.Instance.UserActivity = 0;
            OnPropertyChanged(nameof(UserName));
            MainFrame.Navigate(new AutorizationPage(MainFrame, this));
        }

        private void Click_MenuOut(object sender, RoutedEventArgs e)
        {
            if (SessionData.Instance.UserRole == "Admin" || SessionData.Instance.UserRole == "Manager")
            {
                MainFrame.Navigate(new AdminPage(MainFrame));
            }
            else
            {
                MainFrame.Navigate(new CloseSW(MainFrame, mainWindow));
            }

        }

        

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}