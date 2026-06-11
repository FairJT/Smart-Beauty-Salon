using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SmartSalon.DTOs;

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

    #region Get Slots Tests

    [Fact]
    public async Task GetSlots_ValidData_ReturnsAvailableSlots()
    {
        var (salonId, artistId, serviceId, _) = await TestDataHelper.SetupSalonWithArtistAndServiceAsync(_fixture);

        var date = DateTime.Today.AddDays(1);
        var response = await _client.GetAsync(
            $"/api/appointments/slots?artistId={artistId}&date={date:yyyy-MM-dd}&duration=30");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SlotsResponseDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Slots);
    }

    [Fact]
    public async Task GetSlots_NonExistingArtist_ReturnsSlots()
    {
        var date = DateTime.Today.AddDays(1);
        var response = await _client.GetAsync(
            $"/api/appointments/slots?artistId=99999&date={date:yyyy-MM-dd}&duration=30");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SlotsResponseDto>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetSlots_PastDate_ReturnsBadRequest()
    {
        var pastDate = DateTime.Today.AddDays(-1);
        var response = await _client.GetAsync(
            $"/api/appointments/slots?artistId=1&date={pastDate:yyyy-MM-dd}&duration=30");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Create Appointment Tests

    [Fact]
    public async Task CreateAppointment_ValidData_ReturnsSuccess()
    {
        var (salonId, artistId, serviceId, _) = await TestDataHelper.SetupSalonWithArtistAndServiceAsync(_fixture);

        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
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
        var dto = new
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
        Assert.True(result!.Id > 0);
    }

    [Fact]
    public async Task CreateAppointment_WithoutAuth_ReturnsUnauthorized()
    {
        var dto = new
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
    public async Task CreateAppointment_PastTime_ReturnsBadRequest()
    {
        var (salonId, artistId, serviceId, _) = await TestDataHelper.SetupSalonWithArtistAndServiceAsync(_fixture);

        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09125551112",
            Password = "Test1234",
            FirstName = "Client",
            LastName = "Past",
            NationalCode = "1111111112"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var pastTime = DateTime.Today.AddDays(-1).AddHours(10);
        var dto = new
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = pastTime,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        };

        var response = await client.PostAsJsonAsync("/api/appointments", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Get My Appointments Tests

    [Fact]
    public async Task GetMine_ReturnsClientAppointments()
    {
        var (salonId, artistId, serviceId, _) = await TestDataHelper.SetupSalonWithArtistAndServiceAsync(_fixture);

        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
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
        await client.PostAsJsonAsync("/api/appointments", new
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = tomorrow,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        });

        var response = await client.GetAsync("/api/appointments/mine");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<AppointmentListItemDto>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetMine_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/appointments/mine");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Confirm Appointment Tests

    [Fact]
    public async Task ConfirmAppointment_AsManager_ReturnsSuccess()
    {
        var (salonId, artistId, serviceId, managerToken) = await TestDataHelper.SetupSalonWithArtistAndServiceAsync(_fixture);

        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
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
        var createResponse = await client.PostAsJsonAsync("/api/appointments", new
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = tomorrow,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        int appointmentId = createResult.GetProperty("id").GetInt32();

        var managerClient = _fixture.CreateClient();
        managerClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", managerToken);
        var response = await managerClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/confirm", new { });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ConfirmAppointment_AsNonManager_ReturnsForbidden()
    {
        var (salonId, artistId, serviceId, _) = await TestDataHelper.SetupSalonWithArtistAndServiceAsync(_fixture);

        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09125553334",
            Password = "Test1234",
            FirstName = "Client",
            LastName = "Four",
            NationalCode = "3333333334"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var tomorrow = DateTime.Today.AddDays(1).AddHours(13);
        var createResponse = await client.PostAsJsonAsync("/api/appointments", new
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = tomorrow,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        int appointmentId = createResult.GetProperty("id").GetInt32();

        var otherClient = _fixture.CreateClientWithToken("other-apt-confirm", "Client");
        var response = await otherClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/confirm", new { });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Complete Appointment Tests

    [Fact]
    public async Task CompleteAppointment_AsManager_ReturnsSuccess()
    {
        var (salonId, artistId, serviceId, managerToken) = await TestDataHelper.SetupSalonWithArtistAndServiceAsync(_fixture);

        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09125557777",
            Password = "Test1234",
            FirstName = "Client",
            LastName = "Complete",
            NationalCode = "7777777777"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var tomorrow = DateTime.Today.AddDays(1).AddHours(14);
        var createResponse = await client.PostAsJsonAsync("/api/appointments", new
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = tomorrow,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        int appointmentId = createResult.GetProperty("id").GetInt32();

        var managerClient = _fixture.CreateClient();
        managerClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", managerToken);
        var confirmResponse = await managerClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/confirm", new { });
        confirmResponse.EnsureSuccessStatusCode();

        var completeResponse = await managerClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/complete", new { });
        completeResponse.EnsureSuccessStatusCode();
    }

    #endregion

    #region Cancel Appointment Tests

    [Fact]
    public async Task CancelAppointment_AsClient_ReturnsSuccess()
    {
        var (salonId, artistId, serviceId, _) = await TestDataHelper.SetupSalonWithArtistAndServiceAsync(_fixture);

        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
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
        var createResponse = await client.PostAsJsonAsync("/api/appointments", new
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = futureDate,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        int appointmentId = createResult.GetProperty("id").GetInt32();

        var response = await client.PutAsJsonAsync($"/api/appointments/{appointmentId}/cancel", new { });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CancelAppointment_CompletedAppointment_ReturnsBadRequest()
    {
        var (salonId, artistId, serviceId, managerToken) = await TestDataHelper.SetupSalonWithArtistAndServiceAsync(_fixture);

        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09125554445",
            Password = "Test1234",
            FirstName = "Client",
            LastName = "Five",
            NationalCode = "4444444445"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var futureDate = DateTime.Today.AddDays(5).AddHours(10);
        var createResponse = await client.PostAsJsonAsync("/api/appointments", new
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = futureDate,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        int appointmentId = createResult.GetProperty("id").GetInt32();

        var managerClient = _fixture.CreateClient();
        managerClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", managerToken);
        await managerClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/confirm", new { });
        await managerClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/complete", new { });

        var response = await client.PutAsJsonAsync($"/api/appointments/{appointmentId}/cancel", new { });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Rate Appointment Tests

    [Fact]
    public async Task RateAppointment_CompletedAppointment_ReturnsSuccess()
    {
        var (salonId, artistId, serviceId, managerToken) = await TestDataHelper.SetupSalonWithArtistAndServiceAsync(_fixture);

        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09125555555",
            Password = "Test1234",
            FirstName = "Client",
            LastName = "Rate",
            NationalCode = "5555555555"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var futureDate = DateTime.Today.AddDays(5).AddHours(10);
        var createResponse = await client.PostAsJsonAsync("/api/appointments", new
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = futureDate,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        int appointmentId = createResult.GetProperty("id").GetInt32();

        var managerClient = _fixture.CreateClient();
        managerClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", managerToken);
        await managerClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/confirm", new { });
        await managerClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/complete", new { });

        var response = await client.PostAsJsonAsync($"/api/appointments/{appointmentId}/rate", new
        {
            Rating = 5,
            Comment = "Great service!"
        });
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task RateAppointment_NotCompleted_ReturnsBadRequest()
    {
        var (salonId, artistId, serviceId, _) = await TestDataHelper.SetupSalonWithArtistAndServiceAsync(_fixture);

        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09125555556",
            Password = "Test1234",
            FirstName = "Client",
            LastName = "RateFail",
            NationalCode = "5555555556"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var futureDate = DateTime.Today.AddDays(5).AddHours(10);
        var createResponse = await client.PostAsJsonAsync("/api/appointments", new
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = futureDate,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        });
        var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        int appointmentId = createResult.GetProperty("id").GetInt32();

        var response = await client.PostAsJsonAsync($"/api/appointments/{appointmentId}/rate", new
        {
            Rating = 5,
            Comment = "Too early"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Full Appointment Flow

    [Fact]
    public async Task FullAppointmentFlow_CreateConfirmCompleteRateCancel()
    {
        var (salonId, artistId, serviceId, managerToken) = await TestDataHelper.SetupSalonWithArtistAndServiceAsync(_fixture);

        var client = _fixture.CreateClient();
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09125556666",
            Password = "Test1234",
            FirstName = "Flow",
            LastName = "Client",
            NationalCode = "6666666666"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var futureDate = DateTime.Today.AddDays(5).AddHours(10);
        var createResponse = await client.PostAsJsonAsync("/api/appointments", new
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = futureDate,
            DurationMinutes = 30,
            EstimatedPrice = 150000
        });
        createResponse.EnsureSuccessStatusCode();
        var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        int appointmentId = createResult.GetProperty("id").GetInt32();

        var getMineResponse = await client.GetAsync("/api/appointments/mine");
        getMineResponse.EnsureSuccessStatusCode();

        var managerClient = _fixture.CreateClient();
        managerClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", managerToken);
        var confirmResponse = await managerClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/confirm", new { });
        confirmResponse.EnsureSuccessStatusCode();

        var completeResponse = await managerClient.PutAsJsonAsync($"/api/appointments/{appointmentId}/complete", new { });
        completeResponse.EnsureSuccessStatusCode();

        var rateResponse = await client.PostAsJsonAsync($"/api/appointments/{appointmentId}/rate", new
        {
            Rating = 5,
            Comment = "Excellent!"
        });
        rateResponse.EnsureSuccessStatusCode();
    }

    #endregion
}
