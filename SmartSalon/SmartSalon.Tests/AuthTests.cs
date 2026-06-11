using System.Net;
using System.Net.Http.Json;
using SmartSalon.DTOs;

namespace SmartSalon.Tests;

public class AuthTests : IClassFixture<TestFixture>
{
    private readonly HttpClient _client;
    private readonly TestFixture _fixture;

    public AuthTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Register_ValidData_ReturnsToken()
    {
        var dto = new RegisterDto
        {
            Mobile = "09121234567",
            Password = "Test1234",
            FirstName = "Ali",
            LastName = "Rezaei",
            NationalCode = "1234567890"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("Ali", result.User.FirstName);
        Assert.Equal("Client", result.User.UserType);
    }

    [Fact]
    public async Task Register_DuplicateMobile_ReturnsBadRequest()
    {
        var dto = new RegisterDto
        {
            Mobile = "09129999999",
            Password = "Test1234",
            FirstName = "Test",
            LastName = "User",
            NationalCode = "1234567890"
        };

        // First registration
        await _client.PostAsJsonAsync("/api/auth/register", dto);

        // Duplicate
        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidMobile_ReturnsBadRequest()
    {
        var dto = new RegisterDto
        {
            Mobile = "12345",
            Password = "Test1234",
            FirstName = "Test",
            LastName = "User",
            NationalCode = "1234567890"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Register first
        var registerDto = new RegisterDto
        {
            Mobile = "09121111111",
            Password = "Test1234",
            FirstName = "Login",
            LastName = "Test",
            NationalCode = "1234567890"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Login
        var loginDto = new LoginDto { Mobile = "09121111111", Password = "Test1234" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var registerDto = new RegisterDto
        {
            Mobile = "09122222222",
            Password = "Test1234",
            FirstName = "Test",
            LastName = "User",
            NationalCode = "1234567890"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new LoginDto { Mobile = "09122222222", Password = "WrongPass1" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithValidToken_ReturnsProfile()
    {
        var client = _fixture.CreateClientWithToken("test-user-1");
        var response = await client.GetAsync("/api/auth/profile");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ValidData_ReturnsSuccess()
    {
        // Register
        var registerDto = new RegisterDto
        {
            Mobile = "09123333333",
            Password = "OldPass1234",
            FirstName = "Test",
            LastName = "User",
            NationalCode = "1234567890"
        };
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var changeDto = new ChangePasswordDto
        {
            CurrentPassword = "OldPass1234",
            NewPassword = "NewPass1234"
        };
        var response = await client.PostAsJsonAsync("/api/auth/change-password", changeDto);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Logout_ReturnsSuccess()
    {
        var client = _fixture.CreateClientWithToken();
        var response = await client.PostAsync("/api/auth/logout", null);
        response.EnsureSuccessStatusCode();
    }
}
