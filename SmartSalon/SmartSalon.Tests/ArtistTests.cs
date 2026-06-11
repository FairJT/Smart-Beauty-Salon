using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SmartSalon.DTOs;

namespace SmartSalon.Tests;

public class ArtistTests : IClassFixture<TestFixture>
{
    private readonly HttpClient _client;
    private readonly TestFixture _fixture;

    public ArtistTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    #region Get Artists Tests

    [Fact]
    public async Task GetArtists_BySalonId_ReturnsList()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Artist Test Salon",
            Slug = "artist-test-salon",
            ManagerId = "manager-art"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09128881111",
            Password = "Test1234",
            FirstName = "Artist",
            LastName = "One",
            NationalCode = "8881111111"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        await managerClient.PostAsJsonAsync("/api/artists", new
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "Hair expert",
            ContractType = 2
        });

        var response = await _client.GetAsync($"/api/artists?salonId={salonId}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<ArtistListItemDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!);
    }

    [Fact]
    public async Task GetArtists_NonExistingSalon_ReturnsEmpty()
    {
        var response = await _client.GetAsync("/api/artists?salonId=99999");
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region Get Artist By Id Tests

    [Fact]
    public async Task GetArtistById_ExistingArtist_ReturnsDetail()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art-detail", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Artist Detail Salon",
            Slug = "artist-detail-salon",
            ManagerId = "manager-art-detail"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09128882222",
            Password = "Test1234",
            FirstName = "Detail",
            LastName = "Artist",
            NationalCode = "8882222222"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var artistResponse = await managerClient.PostAsJsonAsync("/api/artists", new
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "Expert stylist",
            ContractType = 1
        });
        int artistId = await JsonHelper.GetIdAsync(artistResponse);

        var response = await _client.GetAsync($"/api/artists/{artistId}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ArtistListItemDto>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetArtistById_NonExisting_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/artists/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Create Artist Tests

    [Fact]
    public async Task CreateArtist_AsManager_ReturnsId()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art-create", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Create Artist Salon",
            Slug = "create-artist-salon",
            ManagerId = "manager-art-create"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09128883333",
            Password = "Test1234",
            FirstName = "New",
            LastName = "Artist",
            NationalCode = "8883333333"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var response = await managerClient.PostAsJsonAsync("/api/artists", new
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "Nail specialist",
            ContractType = 3
        });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.TryGetProperty("id", out _));
    }

    [Fact]
    public async Task CreateArtist_AsNonManager_ReturnsForbidden()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art-forbidden", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Forbidden Artist Salon",
            Slug = "forbidden-artist-salon",
            ManagerId = "manager-art-forbidden"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var otherClient = _fixture.CreateClientWithToken("hacker-art", "SalonManager");
        var response = await otherClient.PostAsJsonAsync("/api/artists", new
        {
            UserId = "fake-user",
            SalonId = salonId,
            BioShort = "Hacker",
            ContractType = 1
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Update Artist Tests

    [Fact]
    public async Task UpdateArtist_AsManager_ReturnsSuccess()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art-update", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Update Artist Salon",
            Slug = "update-artist-salon",
            ManagerId = "manager-art-update"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09128884444",
            Password = "Test1234",
            FirstName = "Update",
            LastName = "Me",
            NationalCode = "8884444444"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var artistResponse = await managerClient.PostAsJsonAsync("/api/artists", new
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "Old bio",
            ContractType = 1
        });
        int artistId = await JsonHelper.GetIdAsync(artistResponse);

        var response = await managerClient.PutAsJsonAsync($"/api/artists/{artistId}", new
        {
            BioShort = "Updated bio",
            BioLong = "Long bio here",
            ContractType = 2
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UpdateArtist_AsNonManager_ReturnsForbidden()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art-update1", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Update Forbidden Salon",
            Slug = "update-forbidden-artist-salon",
            ManagerId = "manager-art-update1"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09128885555",
            Password = "Test1234",
            FirstName = "Protected",
            LastName = "Artist",
            NationalCode = "8885555555"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var artistResponse = await managerClient.PostAsJsonAsync("/api/artists", new
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "Protected bio",
            ContractType = 1
        });
        int artistId = await JsonHelper.GetIdAsync(artistResponse);

        var otherClient = _fixture.CreateClientWithToken("other-art-update", "SalonManager");
        var response = await otherClient.PutAsJsonAsync($"/api/artists/{artistId}", new
        {
            BioShort = "Hacked"
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Delete Artist Tests

    [Fact]
    public async Task DeleteArtist_AsManager_ReturnsSuccess()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art-delete", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Delete Artist Salon",
            Slug = "delete-artist-salon",
            ManagerId = "manager-art-delete"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09128886666",
            Password = "Test1234",
            FirstName = "Delete",
            LastName = "Me",
            NationalCode = "8886666666"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var artistResponse = await managerClient.PostAsJsonAsync("/api/artists", new
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "To delete",
            ContractType = 1
        });
        int artistId = await JsonHelper.GetIdAsync(artistResponse);

        var response = await managerClient.DeleteAsync($"/api/artists/{artistId}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeleteArtist_AsNonManager_ReturnsForbidden()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art-delete1", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Delete Forbidden Salon",
            Slug = "delete-forbidden-artist-salon",
            ManagerId = "manager-art-delete1"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09128887777",
            Password = "Test1234",
            FirstName = "Protected",
            LastName = "Delete",
            NationalCode = "8887777777"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var artistResponse = await managerClient.PostAsJsonAsync("/api/artists", new
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "Protected delete",
            ContractType = 1
        });
        int artistId = await JsonHelper.GetIdAsync(artistResponse);

        var otherClient = _fixture.CreateClientWithToken("other-art-delete", "SalonManager");
        var response = await otherClient.DeleteAsync($"/api/artists/{artistId}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Full Artist Flow

    [Fact]
    public async Task FullArtistFlow_CreateUpdateDelete()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art-flow", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Artist Flow Salon",
            Slug = "artist-flow-salon",
            ManagerId = "manager-art-flow"
        });
        int salonId = await JsonHelper.GetIdAsync(salonResponse);

        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09128888888",
            Password = "Test1234",
            FirstName = "Flow",
            LastName = "Artist",
            NationalCode = "8888888888"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var createResponse = await managerClient.PostAsJsonAsync("/api/artists", new
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "Flow bio",
            ContractType = 1
        });
        createResponse.EnsureSuccessStatusCode();
        int artistId = await JsonHelper.GetIdAsync(createResponse);

        var getResponse = await _client.GetAsync($"/api/artists/{artistId}");
        getResponse.EnsureSuccessStatusCode();

        var updateResponse = await managerClient.PutAsJsonAsync($"/api/artists/{artistId}", new
        {
            BioShort = "Updated flow bio",
            ContractType = 1
        });
        updateResponse.EnsureSuccessStatusCode();

        var deleteResponse = await managerClient.DeleteAsync($"/api/artists/{artistId}");
        deleteResponse.EnsureSuccessStatusCode();

        var verifyResponse = await _client.GetAsync($"/api/artists/{artistId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyResponse.StatusCode);
    }

    #endregion
}
