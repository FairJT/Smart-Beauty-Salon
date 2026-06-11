using System.Net;
using System.Net.Http.Json;
using SmartSalon.DTOs;
using SmartSalon.Models;

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

    [Fact]
    public async Task GetArtists_BySalonId_ReturnsList()
    {
        // Setup
        var managerClient = _fixture.CreateClientWithToken("manager-art", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new CreateSalonDto
        {
            Name = "Artist Test Salon",
            Slug = "artist-test-salon",
            ManagerId = "manager-art"
        });
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = salonResult!.id;

        // Register a user for artist
        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Mobile = "09128881111",
            Password = "Test1234",
            FirstName = "Artist",
            LastName = "One",
            NationalCode = "8881111111"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        await managerClient.PostAsJsonAsync("/api/artists", new CreateArtistDto
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "Hair expert",
            ContractType = ContractType.LineRent
        });

        // Act
        var response = await _client.GetAsync($"/api/artists?salonId={salonId}");
        response.EnsureSuccessStatusCode();

        var list = await response.Content.ReadFromJsonAsync<List<ArtistListItemDto>>();
        Assert.NotNull(list);
        Assert.Single(list);
        Assert.Equal("Artist", list[0].FirstName);
    }

    [Fact]
    public async Task CreateArtist_AsManager_ReturnsId()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art2", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new CreateSalonDto
        {
            Name = "Create Artist Salon",
            Slug = "create-artist-salon",
            ManagerId = "manager-art2"
        });
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = salonResult!.id;

        // Register user
        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Mobile = "09128882222",
            Password = "Test1234",
            FirstName = "New",
            LastName = "Artist",
            NationalCode = "8882222222"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var response = await managerClient.PostAsJsonAsync("/api/artists", new CreateArtistDto
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "Nail specialist",
            ContractType = ContractType.RoomRent
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateArtist_AsNonManager_ReturnsForbidden()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art3", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new CreateSalonDto
        {
            Name = "Forbidden Artist Salon",
            Slug = "forbidden-artist-salon",
            ManagerId = "manager-art3"
        });
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = salonResult!.id;

        var otherClient = _fixture.CreateClientWithToken("hacker", "SalonManager");
        var response = await otherClient.PostAsJsonAsync("/api/artists", new CreateArtistDto
        {
            UserId = "fake-user",
            SalonId = salonId,
            BioShort = "Hacker",
            ContractType = ContractType.FixedSalary
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateArtist_AsManager_ReturnsSuccess()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art4", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new CreateSalonDto
        {
            Name = "Update Artist Salon",
            Slug = "update-artist-salon",
            ManagerId = "manager-art4"
        });
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = salonResult!.id;

        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Mobile = "09128883333",
            Password = "Test1234",
            FirstName = "Update",
            LastName = "Me",
            NationalCode = "8883333333"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var artistResponse = await managerClient.PostAsJsonAsync("/api/artists", new CreateArtistDto
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "Old bio",
            ContractType = ContractType.FixedSalary
        });
        var artistResult = await artistResponse.Content.ReadFromJsonAsync<dynamic>();
        int artistId = artistResult!.id;

        var response = await managerClient.PutAsJsonAsync($"/api/artists/{artistId}", new UpdateArtistDto
        {
            BioShort = "Updated bio",
            BioLong = "Long bio here",
            ContractType = ContractType.LineRent
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeleteArtist_AsManager_ReturnsSuccess()
    {
        var managerClient = _fixture.CreateClientWithToken("manager-art5", "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new CreateSalonDto
        {
            Name = "Delete Artist Salon",
            Slug = "delete-artist-salon",
            ManagerId = "manager-art5"
        });
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = salonResult!.id;

        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Mobile = "09128884444",
            Password = "Test1234",
            FirstName = "Delete",
            LastName = "Me",
            NationalCode = "8884444444"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var artistResponse = await managerClient.PostAsJsonAsync("/api/artists", new CreateArtistDto
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "To delete",
            ContractType = ContractType.FixedSalary
        });
        var artistResult = await artistResponse.Content.ReadFromJsonAsync<dynamic>();
        int artistId = artistResult!.id;

        var response = await managerClient.DeleteAsync($"/api/artists/{artistId}");
        response.EnsureSuccessStatusCode();
    }
}
