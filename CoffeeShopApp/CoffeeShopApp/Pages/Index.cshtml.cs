using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeShopApp.Pages
{
    public class IndexModel : PageModel
    {
        public List<OrderStatistic> OrderStatistics { get; set; }

        public void OnGet()
        {
            LoadOrderStatistics(); // Load order statistics when the page is loaded
        }

        // Method to load order statistics from the database
        private void LoadOrderStatistics()
        {
            OrderStatistics = new List<OrderStatistic>();
            using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT OrderDate, COUNT(*) as OrderCount
                    FROM Orders
                    GROUP BY OrderDate
                    ORDER BY OrderDate
                ";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        OrderStatistics.Add(new OrderStatistic
                        {
                            OrderDate = reader.GetString(0),
                            OrderCount = reader.GetInt32(1)
                        });
                    }
                }
            }
        }
    }

    // Class to represent order statistics
    public class OrderStatistic
    {
        public string OrderDate { get; set; }
        public int OrderCount { get; set; }
    }
}
