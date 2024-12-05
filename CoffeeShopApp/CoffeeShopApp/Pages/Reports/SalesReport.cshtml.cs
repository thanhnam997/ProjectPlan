using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace CoffeeShopApp.Pages.Reports
{
    public class SalesReportModel : PageModel
    {
        public List<SalesReport> SalesReports { get; set; }

        public void OnGet()
        {
            LoadSalesReport();
        }

        private void LoadSalesReport()
        {
            SalesReports = new List<SalesReport>();
            using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT o.OrderDate, p.Name, SUM(oi.Quantity) AS TotalQuantity, SUM(oi.Quantity * p.Price) AS TotalSales
                    FROM Orders o
                    JOIN OrderItems oi ON o.OrderId = oi.OrderId
                    JOIN Products p ON oi.ProductId = p.ProductId
                    GROUP BY o.OrderDate, p.Name
                    ORDER BY o.OrderDate, p.Name
                ";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesReports.Add(new SalesReport
                        {
                            OrderDate = reader.GetString(0),
                            ProductName = reader.GetString(1),
                            TotalQuantity = reader.GetInt32(2),
                            TotalSales = reader.GetDouble(3)
                        });
                    }
                }
            }
        }
    }

    public class SalesReport
    {
        public string OrderDate { get; set; }
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
        public double TotalSales { get; set; }
    }
}
