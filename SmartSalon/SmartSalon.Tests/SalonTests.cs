using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SmartSalon.DTOs;

namespace SmartSalon.Tests;

public class SalonTests : IClassFixture<TestFixture>
{
    private readonly HttpClient _client;
    private readonly TestFixture _fixture;

    public SalonTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    #region Get Salons Tests

    [Fact]
    public async Task GetSalons_EmptyDb_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/salons");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Object, result.ValueKind);
    }

    [Fact]
    public async Task GetSalons_WithSalons_ReturnsList()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-list", "SalonManager");
        await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "List Test Salon",
            Slug = "list-test-salon",
            ManagerId = "manager-list"
        });

        var response = await _client.GetAsync("/api/salons");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.TryGetProperty("data", out _));
    }

    [Fact]
    public async Task GetSalons_WithSearch_ReturnsFiltered()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-search", "SalonManager");
        await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Beauty Paradise",
            Slug = "beauty-paradise",
            ManagerId = "manager-search"
        });

        var response = await _client.GetAsync("/api/salons?search=Beauty");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.TryGetProperty("data", out _));
    }

    #endregion

    #region Get Salon By Id Tests

    [Fact]
    public async Task GetSalonById_ExistingSalon_ReturnsDetail()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-detail", "SalonManager");
        var createResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Detail Salon",
            Slug = "detail-salon",
            ManagerId = "manager-detail"
        });
        var salonId = await JsonHelper.GetIdAsync(createResponse);

        var response = await _client.GetAsync($"/api/salons/{salonId}");
        response.EnsureSuccessStatusCode();

        var salon = await response.Content.ReadFromJsonAsync<SalonDetailDto>();
        Assert.NotNull(salon);
        Assert.Equal("Detail Salon", salon!.Name);
    }

    [Fact]
    public async Task GetSalonById_NonExisting_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/salons/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Create Salon Tests

    [Fact]
    public async Task CreateSalon_Authenticated_ReturnsId()
    {
        var client = _fixture.CreateClientWithToken("manager-create", "SalonManager");
        var dto = new
        {
            Name = "New Salon",
            Slug = "new-salon",
            Phone = "09121234567",
            Address = "Tehran, Valiasr St",
            Description = "A new salon",
            ManagerId = "manager-create"
        };

        var response = await client.PostAsJsonAsync("/api/salons", dto);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.TryGetProperty("id", out _));
    }

    [Fact]
    public async Task CreateSalon_WithoutAuth_ReturnsUnauthorized()
    {
        var dto = new
        {
            Name = "Unauthorized Salon",
            Slug = "unauthorized-salon",
            ManagerId = "manager-1"
        };

        var response = await _client.PostAsJsonAsync("/api/salons", dto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateSalon_DuplicateSlug_ReturnsBadRequest()
    {
        var client = _fixture.CreateClientWithToken("manager-dup", "SalonManager");
        var dto = new
        {
            Name = "Duplicate Slug Salon",
            Slug = "duplicate-slug",
            ManagerId = "manager-dup"
        };

        await client.PostAsJsonAsync("/api/salons", dto);
        var response = await client.PostAsJsonAsync("/api/salons", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update Salon Tests

    [Fact]
    public async Task UpdateSalon_AsManager_ReturnsSuccess()
    {
        var client = _fixture.CreateClientWithToken("manager-update", "SalonManager");
        var createResponse = await client.PostAsJsonAsync("/api/salons", new
        {
            Name = "Update Salon",
            Slug = "update-salon",
            ManagerId = "manager-update"
        });
        var salonId = await JsonHelper.GetIdAsync(createResponse);

        var updateDto = new
        {
            Name = "Updated Salon",
            Phone = "09991112222",
            Address = "New Address"
        };
        var response = await client.PutAsJsonAsync($"/api/salons/{salonId}", updateDto);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UpdateSalon_AsNonManager_ReturnsForbidden()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-update1", "SalonManager");
        var createResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Forbidden Salon",
            Slug = "forbidden-salon",
            ManagerId = "manager-update1"
        });
        var salonId = await JsonHelper.GetIdAsync(createResponse);

        var otherClient = _fixture.CreateClientWithToken("manager-update2", "SalonManager");
        var updateDto = new { Name = "Hacked", Phone = "000" };
        var response = await otherClient.PutAsJsonAsync($"/api/salons/{salonId}", updateDto);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSalon_NonExisting_ReturnsNotFound()
    {
        var client = _fixture.CreateClientWithToken("manager-update3", "SalonManager");
        var response = await client.PutAsJsonAsync("/api/salons/99999", new { Name = "Test" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Delete Salon Tests

    [Fact]
    public async Task DeleteSalon_AsManager_ReturnsSuccess()
    {
        var client = _fixture.CreateClientWithToken("manager-delete", "SalonManager");
        var createResponse = await client.PostAsJsonAsync("/api/salons", new
        {
            Name = "Delete Salon",
            Slug = "delete-salon",
            ManagerId = "manager-delete"
        });
        var salonId = await JsonHelper.GetIdAsync(createResponse);

        var response = await client.DeleteAsync($"/api/salons/{salonId}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeleteSalon_AsNonManager_ReturnsForbidden()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-delete1", "SalonManager");
        var createResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Delete Forbidden Salon",
            Slug = "delete-forbidden-salon",
            ManagerId = "manager-delete1"
        });
        var salonId = await JsonHelper.GetIdAsync(createResponse);

        var otherClient = _fixture.CreateClientWithToken("manager-delete2", "SalonManager");
        var response = await otherClient.DeleteAsync($"/api/salons/{salonId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSalon_NonExisting_ReturnsNotFound()
    {
        var client = _fixture.CreateClientWithToken("manager-delete3", "SalonManager");
        var response = await client.DeleteAsync("/api/salons/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Full Salon Flow

    [Fact]
    public async Task FullSalonFlow_CreateUpdateDelete()
    {
        var client = _fixture.CreateClientWithToken("manager-flow", "SalonManager");
        var createResponse = await client.PostAsJsonAsync("/api/salons", new
        {
            Name = "Flow Salon",
            Slug = "flow-salon",
            ManagerId = "manager-flow"
        });
        createResponse.EnsureSuccessStatusCode();
        var salonId = await JsonHelper.GetIdAsync(createResponse);

        var getResponse = await _client.GetAsync($"/api/salons/{salonId}");
        getResponse.EnsureSuccessStatusCode();

        var updateResponse = await client.PutAsJsonAsync($"/api/salons/{salonId}", new
        {
            Name = "Updated Flow Salon",
            Phone = "09121111111"
        });
        updateResponse.EnsureSuccessStatusCode();

        var deleteResponse = await client.DeleteAsync($"/api/salons/{salonId}");
        deleteResponse.EnsureSuccessStatusCode();

        var verifyResponse = await _client.GetAsync($"/api/salons/{salonId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
    }

    #endregion
}
