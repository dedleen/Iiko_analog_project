using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace Kvalif.ViewModels
{
    public class OrderViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Users> Users { get; set; }
        public ObservableCollection<Tables> Tables { get; set; }

        private int selectedWaiterID;
        public int SelectedWaiterID
        {
            get => selectedWaiterID;
            set
            {
                if (selectedWaiterID != value)
                {
                    ConfirmWaiterChange(value);
                }
            }
        }

        private int selectedTableID;
        public int SelectedTableID
        {
            get => selectedTableID;
            set
            {
                if (selectedTableID != value)
                {
                    ConfirmTableChange(value);
                }
            }
        }

        public Orders CurrentOrder { get; set; }

        public OrderViewModel(Orders order)
        {
            CurrentOrder = order;
            selectedWaiterID = order.WaiterID;
            selectedTableID = order.TableID;

            LoadWaiters();
            LoadTables();
        }

        private void LoadWaiters()
        {
            using (var db = new OreroMenuEntities())
            {
                Users = new ObservableCollection<Users>(db.Users.Where(u => u.Role == "Официант").ToList());
            }
        }

        private void LoadTables()
        {
            using (var db = new OreroMenuEntities())
            {
                Tables = new ObservableCollection<Tables>(db.Tables.ToList());
            }
        }

        private void ConfirmWaiterChange(int newWaiterID)
        {
            var selectedWaiter = Users.FirstOrDefault(w => w.UserID == newWaiterID);
            var result = MessageBox.Show(
                $"Вы уверены, что хотите изменить официанта на {selectedWaiter?.Username}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using (var db = new OreroMenuEntities())
                {
                    var orderToUpdate = db.Orders.FirstOrDefault(o => o.OrderID == CurrentOrder.OrderID);
                    if (orderToUpdate != null)
                    {
                        orderToUpdate.WaiterID = newWaiterID;
                        db.SaveChanges();
                        CurrentOrder.WaiterID = newWaiterID;
                        selectedWaiterID = newWaiterID;
                        OnPropertyChanged(nameof(SelectedWaiterID));
                    }
                }
            }
            else
            {
                OnPropertyChanged(nameof(SelectedWaiterID));
            }
        }

        private void ConfirmTableChange(int newTableID)
        {
            var selectedTable = Tables.FirstOrDefault(t => t.TableID == newTableID);
            var result = MessageBox.Show(
                $"Вы уверены, что хотите изменить стол на №{selectedTable?.Number}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using (var db = new OreroMenuEntities())
                {
                    var orderToUpdate = db.Orders.FirstOrDefault(o => o.OrderID == CurrentOrder.OrderID);
                    if (orderToUpdate != null)
                    {
                        orderToUpdate.TableID = newTableID;
                        db.SaveChanges();
                        CurrentOrder.TableID = newTableID;
                        selectedTableID = newTableID;
                        OnPropertyChanged(nameof(SelectedTableID));
                    }
                }
            }
            else
            {
                OnPropertyChanged(nameof(SelectedTableID));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
