using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

    #region Get Services Tests

    [Fact]
    public async Task GetServices_BySalonId_ReturnsList()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Service Test Salon",
            Slug = "svc-test-salon",
            ManagerId = "manager-svc"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        await managerClient.PostAsJsonAsync("/api/services", new
        {
            Name = "Manicure",
            Category = "Nails",
            DurationMinutes = 45,
            Price = 80000,
            SalonId = salonId
        });

        var response = await _client.GetAsync($"/api/services?salonId={salonId}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<ServiceListItemDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!);
    }

    [Fact]
    public async Task GetServices_NonExistingSalon_ReturnsEmpty()
    {
        var response = await _client.GetAsync("/api/services?salonId=99999");
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region Get Service By Id Tests

    [Fact]
    public async Task GetServiceById_ExistingService_ReturnsDetail()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc-detail", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Service Detail Salon",
            Slug = "svc-detail-salon",
            ManagerId = "manager-svc-detail"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var serviceResponse = await managerClient.PostAsJsonAsync("/api/services", new
        {
            Name = "Pedicure",
            Category = "Nails",
            DurationMinutes = 60,
            Price = 120000,
            SalonId = salonId
        });
        int serviceId = await JsonHelper.GetIdAsync(serviceResponse);

        var response = await _client.GetAsync($"/api/services/{serviceId}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ServiceListItemDto>();
        Assert.NotNull(result);
        Assert.Equal("Pedicure", result!.Name);
    }

    [Fact]
    public async Task GetServiceById_NonExisting_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/services/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Create Service Tests

    [Fact]
    public async Task CreateService_AsManager_ReturnsId()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc-create", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Create Service Salon",
            Slug = "create-svc-salon",
            ManagerId = "manager-svc-create"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var response = await managerClient.PostAsJsonAsync("/api/services", new
        {
            Name = "Facial",
            Category = "Skin",
            DurationMinutes = 60,
            Price = 200000,
            SalonId = salonId
        });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.TryGetProperty("id", out _));
    }

    [Fact]
    public async Task CreateService_AsNonManager_ReturnsForbidden()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc-forbidden", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Forbidden Service Salon",
            Slug = "forbidden-svc-salon",
            ManagerId = "manager-svc-forbidden"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var otherClient = _fixture.CreateClientWithToken("other-svc-user", "SalonManager");
        var response = await otherClient.PostAsJsonAsync("/api/services", new
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
    public async Task CreateService_InvalidData_ReturnsBadRequest()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc-invalid", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Invalid Service Salon",
            Slug = "invalid-svc-salon",
            ManagerId = "manager-svc-invalid"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var response = await managerClient.PostAsJsonAsync("/api/services", new
        {
            Name = "",
            Category = "Test",
            DurationMinutes = 30,
            Price = 50000,
            SalonId = salonId
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update Service Tests

    [Fact]
    public async Task UpdateService_AsManager_ReturnsSuccess()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc-update", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Update Service Salon",
            Slug = "update-svc-salon",
            ManagerId = "manager-svc-update"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var svcResponse = await managerClient.PostAsJsonAsync("/api/services", new
        {
            Name = "Old Name",
            Category = "Test",
            DurationMinutes = 30,
            Price = 50000,
            SalonId = salonId
        });
        int serviceId = await JsonHelper.GetIdAsync(svcResponse);

        var response = await managerClient.PutAsJsonAsync($"/api/services/{serviceId}", new
        {
            Name = "New Name",
            Category = "Updated",
            DurationMinutes = 45,
            Price = 75000
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UpdateService_AsNonManager_ReturnsForbidden()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc-update1", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Update Forbidden Salon",
            Slug = "update-forbidden-svc-salon",
            ManagerId = "manager-svc-update1"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var svcResponse = await managerClient.PostAsJsonAsync("/api/services", new
        {
            Name = "Protected Service",
            Category = "Test",
            DurationMinutes = 30,
            Price = 50000,
            SalonId = salonId
        });
        int serviceId = await JsonHelper.GetIdAsync(svcResponse);

        var otherClient = _fixture.CreateClientWithToken("other-svc-update", "SalonManager");
        var response = await otherClient.PutAsJsonAsync($"/api/services/{serviceId}", new
        {
            Name = "Hacked",
            Category = "Bad",
            DurationMinutes = 30,
            Price = 0
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Delete Service Tests

    [Fact]
    public async Task DeleteService_AsManager_ReturnsSuccess()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc-delete", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Delete Service Salon",
            Slug = "delete-svc-salon",
            ManagerId = "manager-svc-delete"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var svcResponse = await managerClient.PostAsJsonAsync("/api/services", new
        {
            Name = "To Delete",
            Category = "Test",
            DurationMinutes = 30,
            Price = 50000,
            SalonId = salonId
        });
        int serviceId = await JsonHelper.GetIdAsync(svcResponse);

        var response = await managerClient.DeleteAsync($"/api/services/{serviceId}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeleteService_AsNonManager_ReturnsForbidden()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc-delete1", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Delete Forbidden Salon",
            Slug = "delete-forbidden-svc-salon",
            ManagerId = "manager-svc-delete1"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var svcResponse = await managerClient.PostAsJsonAsync("/api/services", new
        {
            Name = "Protected Delete Service",
            Category = "Test",
            DurationMinutes = 30,
            Price = 50000,
            SalonId = salonId
        });
        int serviceId = await JsonHelper.GetIdAsync(svcResponse);

        var otherClient = _fixture.CreateClientWithToken("other-svc-delete", "SalonManager");
        var response = await otherClient.DeleteAsync($"/api/services/{serviceId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Full Service Flow

    [Fact]
    public async Task FullServiceFlow_CreateUpdateDelete()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-svc-flow", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Service Flow Salon",
            Slug = "svc-flow-salon",
            ManagerId = "manager-svc-flow"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var createResponse = await managerClient.PostAsJsonAsync("/api/services", new
        {
            Name = "Flow Service",
            Category = "Test",
            DurationMinutes = 30,
            Price = 100000,
            SalonId = salonId
        });
        createResponse.EnsureSuccessStatusCode();
        int serviceId = await JsonHelper.GetIdAsync(createResponse);

        var getResponse = await _client.GetAsync($"/api/services/{serviceId}");
        getResponse.EnsureSuccessStatusCode();

        var updateResponse = await managerClient.PutAsJsonAsync($"/api/services/{serviceId}", new
        {
            Name = "Updated Flow Service",
            Category = "Test",
            DurationMinutes = 30,
            Price = 150000
        });
        updateResponse.EnsureSuccessStatusCode();

        var deleteResponse = await managerClient.DeleteAsync($"/api/services/{serviceId}");
        deleteResponse.EnsureSuccessStatusCode();

        var verifyResponse = await _client.GetAsync($"/api/services/{serviceId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
    }

    #endregion
}
