using ClosedXML.Excel;
using System;
using System.IO;
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
using System.Data.Entity;

namespace Kvalif
{
    /// <summary>
    /// Логика взаимодействия для ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : Page
    {
        private Frame _mainFrame;
        public ReportsPage(Frame mainFrame)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
        }

        private void RevenueReport_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new OreroMenuEntities())
            {
                var reportData = context.Orders
                    .Where(o => o.Status == "Закрыт")
                    .GroupBy(o => DbFunctions.TruncateTime(o.DateCreated))  // <-- Вот здесь
                    .Select(g => new
                    {
                        Date = g.Key,  // DateTime? (nullable)
                        TotalRevenue = g.Sum(o => o.TotalSum)
                    })
                    .OrderBy(r => r.Date)
                    .ToList();

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Выручка по дням");

                worksheet.Cell(1, 1).Value = "Дата";
                worksheet.Cell(1, 2).Value = "Сумма";

                int row = 2;
                foreach (var item in reportData)
                {
                    string dateStr = item.Date.HasValue ? item.Date.Value.ToShortDateString() : "";

                    worksheet.Cell(row, 1).Value = dateStr;
                    worksheet.Cell(row, 2).Value = item.TotalRevenue;
                    row++;
                }

                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Выручка.xlsx");
                workbook.SaveAs(filePath);

                MessageBox.Show("Отчет сохранен на рабочем столе!");
            }
        }

        private void WaiterStatsReport_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new OreroMenuEntities())
            {
                var stats = context.Orders
                    .Where(o => o.Status == "Закрыт")
                    .GroupBy(o => o.Users.Username)
                    .Select(g => new
                    {
                        Waiter = g.Key,
                        OrdersCount = g.Count(),
                        TotalSum = g.Sum(o => o.TotalSum)
                    })
                    .ToList();

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Статистика по официантам");

                worksheet.Cell(1, 1).Value = "Официант";
                worksheet.Cell(1, 2).Value = "Кол-во заказов";
                worksheet.Cell(1, 3).Value = "Сумма";

                int row = 2;
                foreach (var item in stats)
                {
                    worksheet.Cell(row, 1).Value = item.Waiter;
                    worksheet.Cell(row, 2).Value = item.OrdersCount;
                    worksheet.Cell(row, 3).Value = item.TotalSum;
                    row++;
                }

                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Официанты.xlsx");
                workbook.SaveAs(filePath);

                MessageBox.Show("Отчет сохранен на рабочем столе!");
            }
        }
    }
}