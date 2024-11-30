using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using CoffeeShopApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShopApp.Pages.Orders
{
    public class OrdersModel : PageModel
    {
        public List<Order> Orders { get; set; }
        public List<Product> Products { get; set; }

        [BindProperty]
        public Order NewOrder { get; set; }
        [BindProperty]
        public List<int> SelectedProductIds { get; set; }
        [BindProperty]
        public List<int> ProductQuantities { get; set; }

        public void OnGet()
        {
            LoadOrders();
            LoadProducts();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadOrders();
                LoadProducts();
                return Page();
            }

            using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Orders (CustomerName, OrderDate) VALUES (@CustomerName, @OrderDate)";
                command.Parameters.AddWithValue("@CustomerName", NewOrder.CustomerName);
                command.Parameters.AddWithValue("@OrderDate", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                await command.ExecuteNonQueryAsync();

                command.CommandText = "SELECT last_insert_rowid()";
                var orderId = (long)await command.ExecuteScalarAsync();

                for (int i = 0; i < SelectedProductIds.Count; i++)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = (int)orderId,
                        ProductId = SelectedProductIds[i],
                        Quantity = ProductQuantities[i]
                    };
                    NewOrder.OrderItems.Add(orderItem);

                    command.CommandText = "INSERT INTO OrderItems (OrderId, ProductId, Quantity) VALUES (@OrderId, @ProductId, @Quantity)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@OrderId", orderItem.OrderId);
                    command.Parameters.AddWithValue("@ProductId", orderItem.ProductId);
                    command.Parameters.AddWithValue("@Quantity", orderItem.Quantity);
                    await command.ExecuteNonQueryAsync();
                }
            }

            LoadOrders();
            LoadProducts();

            return Page();
        }

        private void LoadOrders()
        {
            Orders = new List<Order>();
            using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT o.OrderId, o.CustomerName, o.OrderDate, oi.OrderItemId, oi.ProductId, oi.Quantity, p.Name AS ProductName, p.Price
                    FROM Orders o
                    LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
                    LEFT JOIN Products p ON oi.ProductId = p.ProductId
                ";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var orderId = reader.GetInt32(0);
                        var order = Orders.FirstOrDefault(o => o.OrderId == orderId);
                        if (order == null)
                        {
                            order = new Order
                            {
                                OrderId = orderId,
                                CustomerName = reader.GetString(1),
                                OrderDate = System.DateTime.Parse(reader.GetString(2)),
                                OrderItems = new List<OrderItem>()
                            };
                            Orders.Add(order);
                        }

                        if (!reader.IsDBNull(3))
                        {
                            var orderItem = new OrderItem
                            {
                                OrderItemId = reader.GetInt32(3),
                                OrderId = reader.GetInt32(0),
                                ProductId = reader.GetInt32(4),
                                Quantity = reader.GetInt32(5),
                                Product = new Product
                                {
                                    ProductId = reader.GetInt32(4),
                                    Name = reader.GetString(6),
                                    Price = reader.GetDouble(7)
                                }
                            };
                            order.OrderItems.Add(orderItem);
                        }
                    }
                }
            }
        }

        private void LoadProducts()
        {
            Products = new List<Product>();
            using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Products";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Products.Add(new Product
                        {
                            ProductId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Price = reader.GetDouble(2)
                        });
                    }
                }
            }
        }
    }
}
