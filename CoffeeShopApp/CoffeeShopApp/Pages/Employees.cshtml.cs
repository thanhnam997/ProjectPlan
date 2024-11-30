using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using CoffeeShopApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoffeeShopApp.Pages.Employees
{
    public class EmployeesModel : PageModel
    {
        public List<Employee> Employees { get; set; }

        [BindProperty]
        public Employee NewEmployee { get; set; }

        public void OnGet()
        {
            LoadEmployees();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadEmployees();
                return Page();
            }

            using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Employees (Name, Schedule) VALUES (@Name, @Schedule)";
                command.Parameters.AddWithValue("@Name", NewEmployee.Name);
                command.Parameters.AddWithValue("@Schedule", NewEmployee.Schedule);
                await command.ExecuteNonQueryAsync();
            }

            return RedirectToPage();
        }

        private void LoadEmployees()
        {
            Employees = new List<Employee>();
            using (var connection = new SqliteConnection("Data Source=CoffeeShop.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Employees";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employees.Add(new Employee
                        {
                            EmployeeId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Schedule = reader.GetString(2)
                        });
                    }
                }
            }
        }
    }
}
