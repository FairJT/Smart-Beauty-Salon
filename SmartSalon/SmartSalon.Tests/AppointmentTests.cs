using System.Net;
using System.Net.Http.Json;
using SmartSalon.DTOs;
using SmartSalon.Models;

namespace SmartSalon.Tests;

public class AppointmentTests : IClassFixture<TestFixture>
{
    private readonly HttpClient _client;
    private readonly TestFixture _fixture;

    public AppointmentTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    private async Task<(int salonId, int artistId, int serviceId, string managerToken)>
        SetupTestDataAsync()
    {
        // Register manager
        var managerClient = _fixture.CreateClientWithToken("manager-apt", "SalonManager");

        // Create salon
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new CreateSalonDto
        {
            Name = "Appointment Test Salon",
            Slug = "apt-test-salon",
            ManagerId = "manager-apt"
        });
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = salonResult!.id;

        // Create a user for the artist
        var userClient = _fixture.CreateClient();
        var registerResponse = await userClient.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Mobile = "09129876543",
            Password = "Test1234",
            FirstName = "Artist",
            LastName = "Test",
            NationalCode = "9876543210"
        });
        var userResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        string artistUserId = userResult!.User.Id;

        // Create artist
        var artistResponse = await managerClient.PostAsJsonAsync("/api/artists", new CreateArtistDto
        {
            UserId = artistUserId,
            SalonId = salonId,
            BioShort = "Expert stylist",
            ContractType = ContractType.FixedSalary
        });
        var artistResult = await artistResponse.Content.ReadFromJsonAsync<dynamic>();
        int artistId = artistResult!.id;

        // Create service
        var serviceResponse = await managerClient.PostAsJsonAsync("/api/services", new CreateServiceDto
        {
            Name = "Haircut",
            Category = "Hair",
            DurationMinutes = 30,
            Price = 150000,
            SalonId = salonId
        });
        var serviceResult = await serviceResponse.Content.ReadFromJsonAsync<dynamic>();
        int serviceId = serviceResult!.id;

        return (salonId, artistId, serviceId, managerClient.DefaultRequestHeaders
            .Authorization?.ToString()?.Replace("Bearer ", "") ?? "");
    }

    [Fact]
    public async Task GetSlots_ReturnsAvailableSlots()
    {
        var (_, artistId, _, _) = await SetupTestDataAsync();

        var date = DateTime.Today.AddDays(1);
        var response = await _client.GetAsync(
            $"/api/appointments/slots?artistId={artistId}&date={date:yyyy-MM-dd}&duration=30");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SlotsResponseDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Slots);
        Assert.Equal(artistId, result.ArtistId);
    }

    [Fact]
    public async Task GetSlots_NonExistingArtist_ReturnsNotFound()
    {
        var date = DateTime.Today.AddDays(1);
        var response = await _client.GetAsync(
            $"/api/appointments/slots?artistId=99999&date={date:yyyy-MM-dd}&duration=30");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateAppointment_ValidData_ReturnsSuccess()
    {
        var (salonId, artistId, serviceId, _) = await SetupTestDataAsync();

        // Register a client
        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Mobile = "09125551111",
            Password = "Test1234",
            FirstName = "Client",
            LastName = "One",
            NationalCode = "1111111111"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var tomorrow = DateTime.Today.AddDays(1).AddHours(10);
        var dto = new CreateAppointmentDto
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = tomorrow,
            DurationMinutes = 30,
            EstimatedPrice = 150000,
            Notes = "First visit"
        };

        var response = await client.PostAsJsonAsync("/api/appointments", dto);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateAppointmentResponseDto>();
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(150000 * 0.3m, result.Deposit);
    }

    [Fact]
    public async Task CreateAppointment_WithoutAuth_ReturnsUnauthorized()
    {
        var dto = new CreateAppointmentDto
        {
            ArtistId = 1,
            SalonId = 1,
            ServiceId = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 30,
            EstimatedPrice = 100000
        };

        var response = await _client.PostAsJsonAsync("/api/appointments", dto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMine_ReturnsClientAppointments()
    {
        var (salonId, artistId, serviceId, _) = await SetupTestDataAsync();

        // Register client and create appointment
        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Mobile = "09125552222",
            Password = "Test1234",
            FirstName = "Client",
            LastName = "Two",
            NationalCode = "2222222222"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var tomorrow = DateTime.Today.AddDays(1).AddHours(11);
        await client.PostAsJsonAsync("/api/appointments", new CreateAppointmentDto
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = tomorrow,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        });

        // Get mine
        var response = await client.GetAsync("/api/appointments/mine");
        response.EnsureSuccessStatusCode();

        var list = await response.Content.ReadFromJsonAsync<List<AppointmentListItemDto>>();
        Assert.NotNull(list);
        Assert.Single(list);
    }

    [Fact]
    public async Task ConfirmAppointment_AsManager_ReturnsSuccess()
    {
        var (salonId, artistId, serviceId, _) = await SetupTestDataAsync();

        // Create appointment as client
        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Mobile = "09125553333",
            Password = "Test1234",
            FirstName = "Client",
            LastName = "Three",
            NationalCode = "3333333333"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var tomorrow = DateTime.Today.AddDays(1).AddHours(12);
        var createResponse = await client.PostAsJsonAsync("/api/appointments", new CreateAppointmentDto
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = tomorrow,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateAppointmentResponseDto>();

        // Confirm as manager
        var managerClient = _fixture.CreateClientWithToken("manager-apt", "SalonManager");
        var response = await managerClient.PutAsJsonAsync($"/api/appointments/{createResult!.Id}/confirm", new { });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CancelAppointment_AsClient_ReturnsSuccess()
    {
        var (salonId, artistId, serviceId, _) = await SetupTestDataAsync();

        // Create appointment
        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Mobile = "09125554444",
            Password = "Test1234",
            FirstName = "Client",
            LastName = "Four",
            NationalCode = "4444444444"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var futureDate = DateTime.Today.AddDays(5).AddHours(10);
        var createResponse = await client.PostAsJsonAsync("/api/appointments", new CreateAppointmentDto
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = futureDate,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateAppointmentResponseDto>();

        // Cancel
        var response = await client.PutAsJsonAsync($"/api/appointments/{createResult!.Id}/cancel", new { });
        response.EnsureSuccessStatusCode();
    }
}
