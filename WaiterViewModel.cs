using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Data.Entity;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Data.Entity.Core.Objects;

namespace Kvalif
{
    public class WaiterViewModel : INotifyPropertyChanged
    {
        private readonly Frame _mainFrame;
        public ObservableCollection<Tables> Tables { get; set; }
        public ObservableCollection<WaiterInfo> Waiters { get; set; }
        public ICommand OpenTableCommand { get; }
        public ICommand NavigateToOrdersCommand { get; }

        public WaiterViewModel(Frame mainFrame)
        {
            _mainFrame = mainFrame;
            Tables = new ObservableCollection<Tables>();
            Waiters = new ObservableCollection<WaiterInfo>();

            LoadData();

            OpenTableCommand = new RelayCommand<Tables>(OpenTable);
            NavigateToOrdersCommand = new RelayCommand<Tables>(NavigateToOrders);
        }

        private void LoadData()
        {
            using (var context = new OreroMenuEntities())
            {
                Console.WriteLine("Загружаем столы...");
                var tables = context.Tables
                    .Include(t => t.Orders.Select(o => o.OrderDetails.Select(od => od.Dishes)))
                    .ToList();
                Console.WriteLine($"Загружено столов: {tables.Count}");
                foreach (var table in tables)
                    Tables.Add(table);

                Console.WriteLine("Загружаем активных официантов...");
                var activeWaiters = context.Users
                    .Where(u => u.Activity == 1)
                    .Select(u => new WaiterInfo { ShowWaiter = u.Username })
                    .ToList();
                Console.WriteLine($"Загружено официантов: {activeWaiters.Count}");
                foreach (var waiter in activeWaiters)
                    Waiters.Add(waiter);
            }
        }

        private void OpenTable(Tables table)
        {
            using (var context = new OreroMenuEntities())
            {
                Console.WriteLine($"Открываем стол: TableID={table.TableID}");
                var dbTable = context.Tables
                    .Include(t => t.Orders)
                    .FirstOrDefault(t => t.TableID == table.TableID);
                if (!dbTable.Orders.Any(o => o.Status == "Открыт"))
                {
                    var activeUser = context.Users.FirstOrDefault(u => u.Activity == 1);
                    if (activeUser == null)
                    {
                        System.Windows.MessageBox.Show("Нет активных официантов.", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }

                    var order = new Orders
                    {
                        TableID = table.TableID,
                        WaiterID = activeUser.UserID,
                        DateCreated = DateTime.Now,
                        Status = "Открыт",
                        TotalSum = 0m
                    };
                    Console.WriteLine($"Создаём заказ: TableID={order.TableID}, WaiterID={order.WaiterID}");
                    context.Orders.Add(order);
                }
                dbTable.StatusTable = "Открыт";
                context.SaveChanges();
                table.StatusTable = "Открыт";
            }
            NavigateToOrdersCommand.Execute(table);
        }
        private void NavigateToOrders(Tables table)
        {
            var orderPage = new OrderPage(_mainFrame, table, SessionData.Instance.UserID);
            _mainFrame.Navigate(orderPage);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class WaiterInfo
    {
        public string ShowWaiter { get; set; }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
        
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
            

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;
            public void Execute(object parameter) => _execute((T)parameter);
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
