using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartSalon.Data;

namespace SmartSalon.Tests;

public class TestFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();

            var testSettings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=.\\SQLEXPRESS;Database=SmartSalonTestDb;Trusted_Connection=True",
                ["JwtSettings:Key"] = "TestSecretKey_12345678901234567890!",
                ["JwtSettings:Issuer"] = "SmartSalonAPI",
                ["JwtSettings:Audience"] = "SmartSalonApp",
                ["MeliPayamak:ApiKey"] = "",
                ["MeliPayamak:Sender"] = ""
            };

            config.AddInMemoryCollection(testSettings);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Remove existing SqlServer registrations
            var descriptors = services.Where(d =>
                d.ServiceType.FullName?.Contains("SqlServer") == true).ToList();
            foreach (var d in descriptors) services.Remove(d);

            // Add InMemory database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase($"SmartSalonTest_{Guid.NewGuid()}"));
        });
    }

    public HttpClient CreateClientWithToken(string userId = "test-user-1", string userType = "Client")
    {
        var client = CreateClient();
        var token = TestTokenHelper.GenerateTestToken(userId, userType);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}

public static class TestTokenHelper
{
    private const string JwtKey = "TestSecretKey_12345678901234567890!";

    public static string GenerateTestToken(string userId, string userType)
    {
        var claims