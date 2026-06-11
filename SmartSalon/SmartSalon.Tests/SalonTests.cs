using System.Net;
using System.Net.Http.Json;
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

    [Fact]
    public async Task GetSalons_EmptyDb_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/salons");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<SalonListItemDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task CreateSalon_Authenticated_ReturnsId()
    {
        var client = _fixture.CreateClientWithToken("manager-1", "SalonManager");
        var dto = new CreateSalonDto
        {
            Name = "Test Salon",
            Slug = "test-salon",
            Phone = "09121234567",
            Address = "Tehran, Valiasr St",
            Description = "A test salon",
            ManagerId = "manager-1"
        };

        var response = await client.PostAsJsonAsync("/api/salons", dto);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateSalon_WithoutAuth_ReturnsUnauthorized()
    {
        var dto = new CreateSalonDto
        {
            Name = "Test Salon",
            Slug = "test-salon-unauth",
            ManagerId = "manager-1"
        };

        var response = await _client.PostAsJsonAsync("/api/salons", dto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSalonById_ExistingSalon_ReturnsDetail()
    {
        // Create salon
        var client = _fixture.CreateClientWithToken("manager-1", "SalonManager");
        var createDto = new CreateSalonDto
        {
            Name = "Detail Salon",
            Slug = "detail-salon",
            ManagerId = "manager-1"
        };
        var createResponse = await client.PostAsJsonAsync("/api/salons", createDto);
        var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = createResult!.id;

        // Get detail
        var response = await _client.GetAsync($"/api/salons/{salonId}");
        response.EnsureSuccessStatusCode();

        var salon = await response.Content.ReadFromJsonAsync<SalonDetailDto>();
        Assert.NotNull(salon);
        Assert.Equal("Detail Salon", salon.Name);
        Assert.Equal("detail-salon", salon.Slug);
    }

    [Fact]
    public async Task GetSalonById_NonExisting_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/salons/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSalon_AsManager_ReturnsSuccess()
    {
        // Create
        var client = _fixture.CreateClientWithToken("manager-1", "SalonManager");
        var createDto = new CreateSalonDto
        {
            Name = "Update Salon",
            Slug = "update-salon",
            ManagerId = "manager-1"
        };
        var createResponse = await client.PostAsJsonAsync("/api/salons", createDto);
        var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = createResult!.id;

        // Update
        var updateDto = new UpdateSalonDto
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
        // Create with manager-1
        var managerClient = _fixture.CreateClientWithToken("manager-1", "SalonManager");
        var createDto = new CreateSalonDto
        {
            Name = "Forbidden Salon",
            Slug = "forbidden-salon",
            ManagerId = "manager-1"
        };
        var createResponse = await managerClient.PostAsJsonAsync("/api/salons", createDto);
        var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = createResult!.id;

        // Try update with different user
        var otherClient = _fixture.CreateClientWithToken("manager-2", "SalonManager");
        var updateDto = new UpdateSalonDto { Name = "Hacked", Phone = "000" };
        var response = await otherClient.PutAsJsonAsync($"/api/salons/{salonId}", updateDto);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SearchSalons_ByQuery_ReturnsFiltered()
    {
        // Create salon
        var client = _fixture.CreateClientWithToken("manager-1", "SalonManager");
        await client.PostAsJsonAsync("/api/salons", new CreateSalonDto
        {
            Name = "Beauty Paradise",
            Slug = "beauty-paradise",
            ManagerId = "manager-1"
        });

        // Search
        var response = await _client.GetAsync("/api/salons?search=Beauty");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<SalonListItemDto>>();
        Assert.NotNull(result);
        Assert.Contains(result.Data, s => s.Name == "Beauty Paradise");
    }
}
