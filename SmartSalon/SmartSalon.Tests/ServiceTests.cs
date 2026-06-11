using System.Net;
using System.Net.Http.Json;
using SmartSalon.DTOs;

namespace SmartSalon.Tests;

public class ServiceTests : IClassFixture<TestFixture>
{
    private readonly HttpClient _client;
    private readonly TestFixture _fixture;

    public ServiceTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetServices_BySalonId_ReturnsList()
    {
        // Setup: create salon + service
        var managerClient = _fixture.CreateClientWithToken("manager-svc", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new CreateSalonDto
        {
            Name = "Service Test Salon",
            Slug = "svc-test-salon",
            ManagerId = "manager-svc"
        });
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = salonResult!.id;

        await managerClient.PostAsJsonAsync("/api/services", new CreateServiceDto
        {
            Name = "Manicure",
            Category = "Nails",
            DurationMinutes = 45,
            Price = 80000,
            SalonId = salonId
        });

        // Act
        var response = await _client.GetAsync($"/api/services?salonId={salonId}");
        response.EnsureSuccessStatusCode();

        var list = await response.Content.ReadFromJsonAsync<List<ServiceListItemDto>>();
        Assert.NotNull(list);
        Assert.Single(list);
        Assert.Equal("Manicure", list[0].Name);
    }

    [Fact]
    public async Task CreateService_AsManager_ReturnsId()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc2", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new CreateSalonDto
        {
            Name = "Create Service Salon",
            Slug = "create-svc-salon",
            ManagerId = "manager-svc2"
        });
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = salonResult!.id;

        var response = await managerClient.PostAsJsonAsync("/api/services", new CreateServiceDto
        {
            Name = "Facial",
            Category = "Skin",
            DurationMinutes = 60,
            Price = 200000,
            SalonId = salonId
        });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
        Assert.True((int)result.id > 0);
    }

    [Fact]
    public async Task CreateService_AsNonManager_ReturnsForbidden()
    {
        // Create salon with manager-1
        var managerClient = _fixture.CreateClientWithToken("manager-svc3", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new CreateSalonDto
        {
            Name = "Forbidden Service Salon",
            Slug = "forbidden-svc-salon",
            ManagerId = "manager-svc3"
        });
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = salonResult!.id;

        // Try create service with different user
        var otherClient = _fixture.CreateClientWithToken("other-user", "SalonManager");
        var response = await otherClient.PostAsJsonAsync("/api/services", new CreateServiceDto
        {
            Name = "Hacked Service",
            Category = "Bad",
            DurationMinutes = 30,
            Price = 0,
            SalonId = salonId
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateService_AsManager_ReturnsSuccess()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc4", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new CreateSalonDto
        {
            Name = "Update Service Salon",
            Slug = "update-svc-salon",
            ManagerId = "manager-svc4"
        });
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = salonResult!.id;

        var svcResponse = await managerClient.PostAsJsonAsync("/api/services", new CreateServiceDto
        {
            Name = "Old Name",
            Category = "Test",
            DurationMinutes = 30,
            Price = 50000,
            SalonId = salonId
        });
        var svcResult = await svcResponse.Content.ReadFromJsonAsync<dynamic>();
        int serviceId = svcResult!.id;

        var response = await managerClient.PutAsJsonAsync($"/api/services/{serviceId}", new UpdateServiceDto
        {
            Name = "New Name",
            Category = "Updated",
            DurationMinutes = 45,
            Price = 75000
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeleteService_AsManager_ReturnsSuccess()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc5", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new CreateSalonDto
        {
            Name = "Delete Service Salon",
            Slug = "delete-svc-salon",
            ManagerId = "manager-svc5"
        });
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = salonResult!.id;

        var svcResponse = await managerClient.PostAsJsonAsync("/api/services", new CreateServiceDto
        {
            Name = "To Delete",
            Category = "Test",
            DurationMinutes = 30,
            Price = 50000,
            SalonId = salonId
        });
        var svcResult = await svcResponse.Content.ReadFromJsonAsync<dynamic>();
        int serviceId = svcResult!.id;

        var response = await managerClient.DeleteAsync($"/api/services/{serviceId}");
        response.EnsureSuccessStatusCode();
    }
}
