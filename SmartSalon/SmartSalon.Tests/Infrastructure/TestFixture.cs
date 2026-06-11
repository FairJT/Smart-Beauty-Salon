using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartSalon.Data;
using SmartSalon.DTOs;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SmartSalon.Tests;

public class TestFixture : WebApplicationFactory<Program>, IDisposable
{
    private const string TestJwtKey = "TestSecretKey_12345678901234567890!ForTesting";
    private readonly SqliteConnection _connection;

    public TestFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = OFF;";
        cmd.ExecuteNonQuery();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("JwtSettings:Key", TestJwtKey);
        builder.UseSetting("JwtSettings:Issuer", "SmartSalonAPI");
        builder.UseSetting("JwtSettings:Audience", "SmartSalonApp");
        builder.UseSetting("JwtSettings:ExpiryMinutes", "60");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=.\\SQLEXPRESS;Database=SmartSalonTestDb;Trusted_Connection=True");
        builder.UseSetting("MeliPayamak:ApiKey", "");
        builder.UseSetting("MeliPayamak:Sender", "");

        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(DbContext) ||
                d.ImplementationType == typeof(ApplicationDbContext) ||
                d.ServiceType.FullName?.Contains("SqlServer") == true ||
                d.ImplementationType?.FullName?.Contains("SqlServer") == true ||
                d.ServiceType.FullName?.Contains("EntityFramework") == true ||
                d.ServiceType.FullName?.Contains("DbContextOptions") == true ||
                d.ServiceType.FullName?.Contains("InMemory") == true ||
                d.ImplementationType?.FullName?.Contains("InMemory") == true).ToList();
            foreach (var d in descriptorsToRemove) services.Remove(d);

            var optionsConfigDescriptors = services.Where(d =>
                d.ServiceType.FullName?.Contains("DbContextOptionsConfiguration") == true ||
                d.ServiceType.FullName?.Contains("DbContextOptionsBuilder") == true).ToList();
            foreach (var d in optionsConfigDescriptors) services.Remove(d);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = "SmartSalonAPI",
                    ValidateAudience = true,
                    ValidAudience = "SmartSalonApp",
                    ValidateLifetime = true
                };
            });

            var hostedServiceDescriptors = services.Where(d =>
                d.ServiceType.FullName?.Contains("HostedService") == true ||
                d.ServiceType.FullName?.Contains("IHostedService") == true).ToList();
            foreach (var d in hostedServiceDescriptors) services.Remove(d);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
        return host;
    }

    public HttpClient CreateClientWithToken(string userId = "test-user-1", string userType = "Client")
    {
        var client = CreateClient();
        var token = TestTokenHelper.GenerateTestToken(userId, userType);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public new void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}

public static class JsonHelper
{
    public static async Task<int> GetIdAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException(
                $"Response body is empty. Status: {response.StatusCode}");
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        return json.GetProperty("id").GetInt32();
    }

    public static async Task<T?> ReadAsAsync<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>();
    }
}

public static class TestTokenHelper
{
    private const string JwtKey = "TestSecretKey_12345678901234567890!ForTesting";

    public static string GenerateTestToken(string userId, string userType)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, userType),
            new Claim("tenantId", "test-tenant")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "SmartSalonAPI",
            audience: "SmartSalonApp",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class TestDataHelper
{
    public static async Task<(int salonId, int artistId, int serviceId, string managerToken)>
        SetupSalonWithArtistAndServiceAsync(TestFixture fixture, string managerId = "manager-test")
    {
        var managerClient = fixture.CreateClientWithToken(managerId, "SalonManager");

        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Test Salon",
            Slug = $"test-salon-{Guid.NewGuid().ToString("N")[..8]}",
            ManagerId = managerId
        });
        salonResponse.EnsureSuccessStatusCode();
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var userClient = fixture.CreateClient();
        var registerResponse = await userClient.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = $"0912{Random.Shared.Next(1000000, 9999999)}",
            Password = "Test1234",
            FirstName = "Artist",
            LastName = "Test",
            NationalCode = Random.Shared.Next(1000000000, 2000000000).ToString()
        });
        registerResponse.EnsureSuccessStatusCode();
        var userResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        string artistUserId = userResult!.User.Id;

        var artistResponse = await managerClient.PostAsJsonAsync("/api/artists", new
        {
            UserId = artistUserId,
            SalonId = salonId,
            BioShort = "Expert stylist",
            ContractType = 1
        });
        artistResponse.EnsureSuccessStatusCode();
        int artistId = await JsonHelper.GetIdAsync(artistResponse);

        var serviceResponse = await managerClient.PostAsJsonAsync("/api/services", new
        {
            Name = "Haircut",
            Category = "Hair",
            DurationMinutes = 30,
            Price = 150000,
            SalonId = salonId
        });
        serviceResponse.EnsureSuccessStatusCode();
        int serviceId = await JsonHelper.GetIdAsync(serviceResponse);

        return (salonId, artistId, serviceId, managerClient.DefaultRequestHeaders
            .Authorization?.ToString()?.Replace("Bearer ", "") ?? "");
    }

    public static async Task<string> RegisterAndGetTokenAsync(HttpClient client, string mobile = "09121234567")
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = mobile,
            Password = "Test1234",
            FirstName = "Test",
            LastName = "User",
            NationalCode = "1234567890"
        });
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return result!.Token;
    }
}
