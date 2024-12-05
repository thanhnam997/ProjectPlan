using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace CoffeeShopApp.Pages.Reports
{
    public class InventoryReportModel : PageModel
    {
        public List<InventoryReport> InventoryReports { get; set; }

        public void OnGet()
        {
            LoadInventoryReport();
        }

        private void LoadInventoryReport()
        {
            InventoryReports = new List<InventoryReport>();
            using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT p.Name, COALESCE(SUM(oi.Quantity), 0) AS TotalSold
                    FROM Products p
                    LEFT JOIN OrderItems oi ON p.ProductId = oi.ProductId
                    GROUP BY p.Name
                ";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        InventoryReports.Add(new InventoryReport
                        {
                            ProductName = reader.GetString(0),
                            TotalSold = reader.GetInt32(1)
                        });
                    }
                }
            }
        }
    }

    public class InventoryReport
    {
        public string ProductName { get; set; }
        public int TotalSold { get; set; }
    }
}
