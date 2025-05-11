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
    /// Логика взаимодействия для WaiterPage.xaml
    /// </summary>
    public partial class WaiterPage : Page
    {
        public class WaiterInfo
        {
            public string ShowWaiter { get; set; }
        }
        private Frame _mainFrame;
        public WaiterPage(Frame mainFrame)
        {
            InitializeComponent();
            _mainFrame = mainFrame;

            GenerateEmptyTableCells();

            using (var context = new OreroMenuEntities())
            {
                var activeWaiters = context.Users
                    .Where(u => u.Activity == 1)
                    .Select(u => new WaiterInfo
                    {
                        ShowWaiter = u.Username
                    })
                    .ToList();

                WLV.ItemsSource = activeWaiters;
            }

        }

        private void GenerateEmptyTableCells()
        {
            TableGrid.Children.Clear();

            for(int i = 0; i < 9; i++)
            {
                var btn = new Button
                {
                    Content = "+",
                    FontSize = 50,
                    Tag = i
                };

                btn.Click += (s, e) =>
                {
                    TablesWindow tablesWindow = new TablesWindow(_mainFrame);
                    tablesWindow.ShowDialog();
                };

                TableGrid.Children.Add(btn);

                Grid.SetRow(btn, i / 3);
                Grid.SetColumn(btn, i % 3);

                
            }
        }
       
        
    }
}
