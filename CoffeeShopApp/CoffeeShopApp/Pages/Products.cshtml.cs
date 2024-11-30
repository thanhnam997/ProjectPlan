using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using CoffeeShopApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoffeeShopApp.Pages.Products
{
    public class ProductsModel : PageModel
    {
        public List<Product> Products { get; set; }

        [BindProperty]
        public Product NewProduct { get; set; }

        public void OnGet()
        {
            LoadProducts();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadProducts();
                return Page();
            }

            using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Products (Name, Price) VALUES (@Name, @Price)";
                command.Parameters.AddWithValue("@Name", NewProduct.Name);
                command.Parameters.AddWithValue("@Price", NewProduct.Price);
                await command.ExecuteNonQueryAsync();
            }

            return RedirectToPage();
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
