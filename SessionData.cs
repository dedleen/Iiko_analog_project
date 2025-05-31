using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Kvalif
{
    public class SessionData : INotifyPropertyChanged
    {
        private static readonly SessionData _instance = new SessionData();
        private int _userID;
        private string _userName;
        private string _userRole;
        private int _userActivity;

        
        private SessionData() { }

       
        public static SessionData Instance => _instance;

        public int UserID
        {
            get => _userID;
            set
            {
                if (_userID != value)
                {
                    _userID = value;
                    OnPropertyChanged(nameof(UserID));
                }
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        public string UserRole
        {
            get => _userRole;
            set
            {
                if (_userRole != value)
                {
                    _userRole = value;
                    OnPropertyChanged(nameof(UserRole));
                }
            }
        }

        public int UserActivity
        {
            get => _userActivity;
            set
            {
                if (_userActivity != value)
                {
                    _userActivity = value;
                    OnPropertyChanged(nameof(UserActivity));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}