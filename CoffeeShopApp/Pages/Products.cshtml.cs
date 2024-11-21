using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

public class ProductsModel : PageModel
{
    public List<Product> Products { get; set; }

    public void OnGet()
    {
        LoadProducts();
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
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Price = reader.GetDouble(2)
                    });
                }
            }
        }
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
}
