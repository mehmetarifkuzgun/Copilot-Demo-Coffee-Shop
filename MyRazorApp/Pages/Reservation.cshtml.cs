using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using System.Text.Json;
using System.Web;

namespace MyRazorApp.Pages
{
    public class Branch
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public double Rating { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class _Branch
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class ReservationModel : PageModel
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReservationModel> _logger;

        public ReservationModel(IMemoryCache cache, IHttpClientFactory httpClientFactory, ILogger<ReservationModel> logger, IConfiguration configuration)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public List<Branch> Branches { get; set; }
        public Branch SelectedBranch { get; set; }

        [BindProperty]
        public int Branch { get; set; }

        [BindProperty]
        public string Location { get; set; }

        public async Task OnGet() 
        {
            // LoadBranches();
            Branches = await FetchBranchesAsync();
        }

        private async Task<List<Branch>> FetchBranchesAsync()
        {
            if (!_cache.TryGetValue("Branches", out List<Branch> branches))
            {
                branches = new List<Branch>();

                var client = _httpClientFactory.CreateClient();
                var apiGatewayUrl = _configuration["ApiGatewayUrl"] + "/getDataFromTableWithFilter";

                var filterData = new
                {
                    TableName = "restaurant.branch",
                    Filter = "1=1" // Tüm veriyi çekmek için basit bir filtre
                };

                try
                {
                    var response = await client.PostAsJsonAsync(apiGatewayUrl, filterData);
                    response.EnsureSuccessStatusCode();

                    var jsonString = await response.Content.ReadAsStringAsync();
                    jsonString = jsonString.Replace("\\r\\n", "").Replace("\\u0022", "\"");
                    var _branches = JsonSerializer.Deserialize<List<_Branch>>(jsonString);

                    foreach (var b in _branches)
                    {
                        branches.Add(new Branch
                        {
                            RestaurantId = b.RestaurantId,
                            Name = b.Name,
                            City = b.City,
                            District = b.District,
                            Latitude = b.Latitude,
                            Longitude = b.Longitude
                        });
                    }

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(30));

                    _cache.Set("Branches", branches, cacheEntryOptions);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching branches from API Gateway");
                }
            }

            return branches;
        }

        public async Task<IActionResult> OnPostMakeReservation()
        {
            if (Branches == null || Branches.Count == 0)
                Branches = await FetchBranchesAsync();

            if (Branch == 0)
            {
                ModelState.AddModelError(string.Empty, "Lütfen bir restoran şubesi seçin.");
                return Page();
            }

            SelectedBranch = Branches.Find(b => b.RestaurantId == Branch);
            if (SelectedBranch == null)
            {
                ModelState.AddModelError(string.Empty, "Seçilen restoran şubesi bulunamadı.");
                return Page();
            }

            var selectedBranch = Branch;

            TempData["BranchName"] = SelectedBranch.Name;

            return RedirectToPage("/order");
        }
        /*
        public void LoadBranches()
        {
            if (!_cache.TryGetValue("Branches", out List<Branch> branches))
            {
                branches = new List<Branch>();

                string connectionString = "Server=c-cosmos4postgre.623r4wjhuwcfz4.postgres.cosmos.azure.com;Database=citus;Port=5432;User Id=citus;Password=Deneme!12345;Ssl Mode=Require;";
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand("SELECT restaurant_id, name, city, district, latitude, longitude FROM restaurant.branch", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                branches.Add(new Branch
                                {
                                    RestaurantId = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    City = reader.GetString(2),
                                    District = reader.GetString(3),
                                    Latitude = reader.GetDouble(4),
                                    Longitude = reader.GetDouble(5)
                                });
                            }
                        }
                    }
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));

                _cache.Set("Branches", branches, cacheEntryOptions);
            }

            Branches = branches;
        }
*/
    }
}