using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;

namespace MyRazorApp.Pages
{
    public class Menu_Item
    {
        public int MenuItemId { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Availability { get; set; }
        public int MenuId { get; set; }
        public string Img { get; set; } // Base64 string
        public string Name { get; set; }
    }

    public class Item
    {
        public int MenuItemId { get; set; }
        public List<Menu_Item> Items { get; set; }
    }

    public class MenuModel : PageModel
    {
        public List<Menu_Item> MenuItems { get; set; }
        public Order CurrentOrder { get; set; }
        public string BranchName { get; set; }

        public void OnGet(string branchName)
        {
            BranchName = branchName;
            LoadMenuItems();
        }

        private void LoadMenuItems()
        {
            MenuItems = new List<Menu_Item>();

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
                            MenuItems.Add(new Menu_Item
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
}
