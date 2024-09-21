using Npgsql;
using System;
using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RecommendationService
{
    public class FilterData
    {
        public string TableName { get; set; }
        public string Filter { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Add services to the container.
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.MapPost("/api/GetDataFromTableWithFilter", async (HttpRequest request) =>
            {
                using var reader = new StreamReader(request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var filterData = JsonSerializer.Deserialize<FilterData>(requestBody);

                // Tablo adı ve filtre bilgilerini kullanarak verileri çek
                var data = GetDataFromTable(filterData.TableName, filterData.Filter);

                // Dönen veriyi monitör etmek için konsola yazdır
                Console.WriteLine("Dönen Veri:");
                Console.WriteLine(data);

                var responseData = JsonSerializer.Serialize(data);
                return Results.Content(responseData, "application/json");
            });

            app.Run();
        }
        record FilterData(string TableName, string Filter);
        public static string GetDataFromTable(string tableName, string filter)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("PostgreSqlConnection");

            DataTable dataTable;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT restaurant_id, name, city, district, latitude, longitude FROM restaurant.branch", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        dataTable = new DataTable();
                        dataTable.Columns.Add("RestaurantId", typeof(int));
                        dataTable.Columns.Add("Name", typeof(string));
                        dataTable.Columns.Add("City", typeof(string));
                        dataTable.Columns.Add("District", typeof(string));
                        dataTable.Columns.Add("Latitude", typeof(double));
                        dataTable.Columns.Add("Longitude", typeof(double));
                        
                        while (reader.Read())
                        {
                            var restaurantId = reader.GetInt32(0);
                            var name = reader.GetString(1);
                            var city = reader.GetString(2);
                            var district = reader.GetString(3);
                            var latitude = reader.GetDouble(4);
                            var longitude = reader.GetDouble(5);

                            // Dönen veriyi DataTable'a ekle
                            dataTable.Rows.Add(restaurantId, name, city, district, latitude, longitude);
                        }
                    }
                }
            }

            if (dataTable == null)
                return null;

            return DataTableToJson(dataTable);
        }

        public static string DataTableToJson(DataTable dataTable)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dataTable.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }

            return JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}