using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using System.Collections.Generic;

namespace MyRazorApp.Pages
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Availability { get; set; }
        public int MenuId { get; set; }
        public string Img { get; set; } // Base64 string
        public string Name { get; set; }
    }

    public class Order
    {
        public int MenuItemId { get; set; }
        public List<MenuItem> Items { get; set; }
    }

    public class OrderModel : PageModel
    {
        public List<MenuItem> MenuItems { get; set; }
        public Order CurrentOrder { get; set; }
        public string BranchName { get; set; }
        public IActionResult OnGet()
        {
			// eğer sayfa ilk kez yükleniyorsa ve BranchName TempData'de yoksa Reservation sayfasına yönlendir
			// eğer BranchName TempData'de varsa sayfayı yükle

			LoadMenuItems();

			if (TempData.ContainsKey("BranchName"))
            {
                BranchName = TempData["BranchName"].ToString();
				return Page();
			}
            else
            {
				return RedirectToPage("/Reservation");
			}
        }

        private void LoadOrder()
        {
            // Sipariş veritabanından çekilir
            // Örneğin, kullanıcıya göre sipariş çekme işlemi yapılabilir
            // Bu örnekte sabit bir kullanıcı ID kullanıyoruz
            int userId = 1; // Örnek kullanıcı ID

            string connectionString = "Server=c-cosmos4postgre.623r4wjhuwcfz4.postgres.cosmos.azure.com;Database=citus;Port=5432;User Id=citus;Password=Deneme!12345;Ssl Mode=Require;";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT menu_item_id FROM restaurant.order_menu_item WHERE order_id = @userId", connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            CurrentOrder = new Order
                            {
                                MenuItemId = reader.GetInt32(0),
                                Items = new List<MenuItem>()
                            };
                        }
                    }
                }

                if (CurrentOrder != null)
                {
                    using (var command = new NpgsqlCommand("SELECT * FROM restaurant.order_menu_item oi JOIN restaurant.menu_item mi ON oi.menu_item_id = mi.menu_item_id WHERE oi.order_id = @menuItemId", connection))
                    {
                        command.Parameters.AddWithValue("@menuItemId", CurrentOrder.MenuItemId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CurrentOrder.Items.Add(new MenuItem
                                {
                                    MenuItemId = reader.GetInt32(0),
                                    Description = reader.GetString(1),
                                    Price = reader.GetDouble(2),
                                    Availability = reader.GetInt32(3),
                                    MenuId = reader.GetInt32(4),
                                    Img = reader.GetString(5),
                                    Name = reader.GetString(6)
                                });
                            }
                        }
                    }
                }
            }
        }

        private void LoadMenuItems()
        {
            MenuItems = new List<MenuItem>();

            string connectionString = "Server=c-cosmos4postgre.623r4wjhuwcfz4.postgres.cosmos.azure.com;Database=citus;Port=5432;User Id=citus;Password=Deneme!12345;Ssl Mode=Require;";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM restaurant.menu_item where img is not null", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MenuItems.Add(new MenuItem
                            {
                                MenuItemId = reader.GetInt32(0),
                                Description = reader.GetString(1),
                                Price = reader.GetDouble(2),
                                Availability = reader.GetInt32(3),
                                MenuId = reader.GetInt32(4),
                                Img = reader.GetString(5),
                                Name = reader.GetString(6)
                            });
                        }
                    }
                }
            }
        }

        private string GetBranchNameFromID(int branchId)
        {
            string connectionString = "";

            return "test";
        }
    }
}