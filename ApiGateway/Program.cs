using System.Text;

namespace ApiGateway // Namespace değişikliği
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddHttpClient(); // HttpClient servisini ekliyoruz

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.MapPost("/getDataFromTableWithFilter", async (HttpRequest request, IHttpClientFactory httpClientFactory) =>
            {
                using var reader = new StreamReader(request.Body);
                var requestBody = await reader.ReadToEndAsync();
                var client = httpClientFactory.CreateClient();
                var backendUrl = Environment.GetEnvironmentVariable("BACKEND_URL") ?? "http://localhost:5190/api/getDataFromTableWithFilter";

                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(backendUrl, content);
                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<string>();
                return Results.Content(data, "application/json");
            });

            app.Run();
        }
    }
}
