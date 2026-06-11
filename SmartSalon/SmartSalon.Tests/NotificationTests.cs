using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SmartSalon.DTOs;

namespace SmartSalon.Tests;

public class NotificationTests : IClassFixture<TestFixture>
{
    private readonly HttpClient _client;
    private readonly TestFixture _fixture;

    public NotificationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    #region Get Notifications Tests

    [Fact]
    public async Task GetNotifications_WithAuth_ReturnsEmptyList()
    {
        var client = _fixture.CreateClientWithToken("user-notif-1");
        var response = await client.GetAsync("/api/notifications");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NotificationsResponseDto>();
        Assert.NotNull(result);
        Assert.Empty(result!.Notifications);
    }

    [Fact]
    public async Task GetNotifications_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/notifications");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Get Unread Count Tests

    [Fact]
    public async Task GetUnreadCount_ReturnsZero()
    {
        var client = _fixture.CreateClientWithToken("user-notif-2");
        var response = await client.GetAsync("/api/notifications/unread-count");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, result.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task GetUnreadCount_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/notifications/unread-count");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Mark As Read Tests

    [Fact]
    public async Task MarkAsRead_NonExisting_ReturnsNotFound()
    {
        var client = _fixture.CreateClientWithToken("user-notif-3");
        var response = await client.PutAsJsonAsync("/api/notifications/99999/read", new { });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.PutAsJsonAsync("/api/notifications/1/read", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Mark All As Read Tests

    [Fact]
    public async Task MarkAllAsRead_ReturnsSuccess()
    {
        var client = _fixture.CreateClientWithToken("user-notif-4");
        var response = await client.PutAsJsonAsync("/api/notifications/read-all", new { });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.TryGetProperty("message", out _));
    }

    [Fact]
    public async Task MarkAllAsRead_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.PutAsJsonAsync("/api/notifications/read-all", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Delete Notification Tests

    [Fact]
    public async Task DeleteNotification_NonExisting_ReturnsNotFound()
    {
        var client = _fixture.CreateClientWithToken("user-notif-5");
        var response = await client.DeleteAsync("/api/notifications/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNotification_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.DeleteAsync("/api/notifications/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Full Notification Flow

    [Fact]
    public async Task FullNotificationFlow_CreateReadDelete()
    {
        var managerId = "notif-manager-1";

        var managerClient = _fixture.CreateClientWithToken(managerId, "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new
        {
            Name = "Notification Test Salon",
            Slug = $"notif-test-salon-{Guid.NewGuid().ToString("N")[..8]}",
            ManagerId = managerId
        });
        salonResponse.EnsureSuccessStatusCode();
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<JsonElement>();
        int salonId = salonResult.GetProperty("id").GetInt32();

        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09127771111",
            Password = "Test1234",
            FirstName = "Notif",
            LastName = "Artist",
            NationalCode = "7771111111"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var artistResponse = await managerClient.PostAsJsonAsync("/api/artists", new
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "Test",
            ContractType = 1
        });
        artistResponse.EnsureSuccessStatusCode();
        var artistResult = await artistResponse.Content.ReadFromJsonAsync<JsonElement>();
        int artistId = artistResult.GetProperty("id").GetInt32();

        var svcResponse = await managerClient.PostAsJsonAsync("/api/services", new
        {
            Name = "Notif Service",
            Category = "Test",
            DurationMinutes = 30,
            Price = 100000,
            SalonId = salonId
        });
        svcResponse.EnsureSuccessStatusCode();
        var svcResult = await svcResponse.Content.ReadFromJsonAsync<JsonElement>();
        int serviceId = svcResult.GetProperty("id").GetInt32();

        var client = _fixture.CreateClient();
        var clientReg = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09127772222",
            Password = "Test1234",
            FirstName = "Notif",
            LastName = "Client",
            NationalCode = "7772222222"
        });
        var clientResult = await clientReg.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", clientResult!.Token);

        var tomorrow = DateTime.Today.AddDays(1).AddHours(14);
        await client.PostAsJsonAsync("/api/appointments", new
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = tomorrow,
            DurationMinutes = 30,
            EstimatedPrice = 100000
        });

        var notifResponse = await managerClient.GetAsync("/api/notifications");
        notifResponse.EnsureSuccessStatusCode();
        var notifResult = await notifResponse.Content.ReadFromJsonAsync<NotificationsResponseDto>();
        Assert.True(notifResult!.UnreadCount > 0);

        var readAllResponse = await managerClient.PutAsJsonAsync("/api/notifications/read-all", new { });
        readAllResponse.EnsureSuccessStatusCode();

        var countResponse = await managerClient.GetAsync("/api/notifications/unread-count");
        var countResult = await countResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, countResult.GetProperty("count").GetInt32());
    }

    #endregion
}
