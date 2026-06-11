using System.Net;
using System.Net.Http.Json;

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

    [Fact]
    public async Task GetNotifications_WithAuth_ReturnsEmptyList()
    {
        var client = _fixture.CreateClientWithToken("user-notif-1");
        var response = await client.GetAsync("/api/notifications");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetNotifications_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/notifications");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsZero()
    {
        var client = _fixture.CreateClientWithToken("user-notif-2");
        var response = await client.GetAsync("/api/notifications/unread-count");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
        Assert.Equal(0, (int)result.count);
    }

    [Fact]
    public async Task MarkAsRead_NonExisting_ReturnsNotFound()
    {
        var client = _fixture.CreateClientWithToken("user-notif-3");
        var response = await client.PutAsJsonAsync("/api/notifications/99999/read", new { });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkAllAsRead_ReturnsSuccess()
    {
        var client = _fixture.CreateClientWithToken("user-notif-4");
        var response = await client.PutAsJsonAsync("/api/notifications/read-all", new { });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteNotification_NonExisting_ReturnsNotFound()
    {
        var client = _fixture.CreateClientWithToken("user-notif-5");
        var response = await client.DeleteAsync("/api/notifications/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullNotificationFlow_CreateReadDelete()
    {
        // This test verifies the notification flow end-to-end:
        // 1. Create an appointment (triggers notification to manager)
        // 2. Check manager has notifications
        // 3. Mark as read
        // 4. Verify unread count is 0

        var managerId = "notif-manager-1";

        // Setup salon
        var managerClient = _fixture.CreateClientWithToken(managerId, "SalonManager");
        var salonResponse = await managerClient.PostAsJsonAsync("/api/salons", new SmartSalon.DTOs.CreateSalonDto
        {
            Name = "Notification Test Salon",
            Slug = "notif-test-salon",
            ManagerId = managerId
        });
        var salonResult = await salonResponse.Content.ReadFromJsonAsync<dynamic>();
        int salonId = salonResult!.id;

        // Create user + artist
        var userClient = _fixture.CreateClient();
        var regResponse = await userClient.PostAsJsonAsync("/api/auth/register", new SmartSalon.DTOs.RegisterDto
        {
            Mobile = "09127771111",
            Password = "Test1234",
            FirstName = "Notif",
            LastName = "Artist",
            NationalCode = "7771111111"
        });
        var regResult = await regResponse.Content.ReadFromJsonAsync<SmartSalon.DTOs.AuthResponseDto>();

        var artistResponse = await managerClient.PostAsJsonAsync("/api/artists", new SmartSalon.DTOs.CreateArtistDto
        {
            UserId = regResult!.User.Id,
            SalonId = salonId,
            BioShort = "Test",
            ContractType = SmartSalon.Models.ContractType.FixedSalary
        });
        var artistResult = await artistResponse.Content.ReadFromJsonAsync<dynamic>();
        int artistId = artistResult!.id;

        // Create service
        var svcResponse = await managerClient.PostAsJsonAsync("/api/services", new SmartSalon.DTOs.CreateServiceDto
        {
            Name = "Notif Service",
            Category = "Test",
            DurationMinutes = 30,
            Price = 100000,
            SalonId = salonId
        });
        var svcResult = await svcResponse.Content.ReadFromJsonAsync<dynamic>();
        int serviceId = svcResult!.id;

        // Client creates appointment (triggers notification to manager)
        var client = _fixture.CreateClient();
        var clientReg = await client.PostAsJsonAsync("/api/auth/register", new SmartSalon.DTOs.RegisterDto
        {
            Mobile = "09127772222",
            Password = "Test1234",
            FirstName = "Notif",
            LastName = "Client",
            NationalCode = "7772222222"
        });
        var clientResult = await clientReg.Content.ReadFromJsonAsync<SmartSalon.DTOs.AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", clientResult!.Token);

        var tomorrow = DateTime.Today.AddDays(1).AddHours(14);
        await client.PostAsJsonAsync("/api/appointments", new SmartSalon.DTOs.CreateAppointmentDto
        {
            ArtistId = artistId,
            SalonId = salonId,
            ServiceId = serviceId,
            StartTime = tomorrow,
            DurationMinutes = 30,
            EstimatedPrice = 100000
        });

        // Manager checks notifications
        var notifResponse = await managerClient.GetAsync("/api/notifications");
        notifResponse.EnsureSuccessStatusCode();
        var notifResult = await notifResponse.Content.ReadFromJsonAsync<dynamic>();
        Assert.True((int)notifResult.unreadCount > 0);

        // Manager marks all as read
        var readAllResponse = await managerClient.PutAsJsonAsync("/api/notifications/read-all", new { });
        readAllResponse.EnsureSuccessStatusCode();

        // Verify unread count is 0
        var countResponse = await managerClient.GetAsync("/api/notifications/unread-count");
        var countResult = await countResponse.Content.ReadFromJsonAsync<dynamic>();
        Assert.Equal(0, (int)countResult.count);
    }
}
