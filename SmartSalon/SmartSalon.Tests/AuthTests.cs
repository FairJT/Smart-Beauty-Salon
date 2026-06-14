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

    #region Register Tests

    [Fact]
    public async Task Register_ValidData_ReturnsTokenAndUser()
    {
        var dto = new
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
        Assert.False(string.IsNullOrEmpty(result!.Token));
        Assert.Equal("Ali", result.User.FirstName);
    }

    [Fact]
    public async Task Register_DuplicateMobile_ReturnsBadRequest()
    {
        var dto = new
        {
            Mobile = "09129999999",
            Password = "Test1234",
            FirstName = "Test",
            LastName = "User",
            NationalCode = "1234567890"
        };

        var first = await _client.PostAsJsonAsync("/api/auth/register", dto);
        first.EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidMobile_ReturnsBadRequest()
    {
        var dto = new
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
    public async Task Register_ShortPassword_ReturnsBadRequest()
    {
        var dto = new
        {
            Mobile = "09121111111",
            Password = "123",
            FirstName = "Test",
            LastName = "User",
            NationalCode = "1234567890"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_EmptyFirstName_ReturnsBadRequest()
    {
        var dto = new
        {
            Mobile = "09121111111",
            Password = "Test1234",
            FirstName = "",
            LastName = "User",
            NationalCode = "1234567890"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var registerDto = new
        {
            Mobile = "09121111111",
            Password = "Test1234",
            FirstName = "Login",
            LastName = "Test",
            NationalCode = "1234567890"
        };
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        regResponse.EnsureSuccessStatusCode();

        var loginDto = new { Mobile = "09121111111", Password = "Test1234" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Token);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var registerDto = new
        {
            Mobile = "09122222222",
            Password = "Test1234",
            FirstName = "Test",
            LastName = "User",
            NationalCode = "1234567890"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new { Mobile = "09122222222", Password = "WrongPass1" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_NonExistingUser_ReturnsUnauthorized()
    {
        var loginDto = new { Mobile = "09999999999", Password = "Test1234" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Profile Tests

    [Fact]
    public async Task GetProfile_WithValidToken_ReturnsProfile()
    {
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Mobile = "09126666666",
            Password = "Test1234",
            FirstName = "Profile",
            LastName = "Test",
            NationalCode = "6666666666"
        });
        regResponse.EnsureSuccessStatusCode();
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var response = await client.GetAsync("/api/auth/profile");
        response.EnsureSuccessStatusCode();

        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal("Profile", profile!.FirstName);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithInvalidToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token-12345");
        var response = await _client.GetAsync("/api/auth/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Change Password Tests

    [Fact]
    public async Task ChangePassword_ValidData_ReturnsSuccess()
    {
        var registerDto = new
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

        var changeDto = new
        {
            CurrentPassword = "OldPass1234",
            NewPassword = "NewPass1234"
        };
        var response = await client.PostAsJsonAsync("/api/auth/change-password", changeDto);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsBadRequest()
    {
        var registerDto = new
        {
            Mobile = "09124444444",
            Password = "CorrectPass1",
            FirstName = "Test",
            LastName = "User",
            NationalCode = "1234567890"
        };
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regResult!.Token);

        var changeDto = new
        {
            CurrentPassword = "WrongPass1",
            NewPassword = "NewPass1234"
        };
        var response = await client.PostAsJsonAsync("/api/auth/change-password", changeDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_ReturnsSuccess()
    {
        var client = _fixture.CreateClientWithToken();
        var response = await client.PostAsync("/api/auth/logout", null);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Logout_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Full Authentication Flow

    [Fact]
    public async Task FullAuthFlow_Register_Login_Profile_ChangePassword()
    {
        var registerDto = new
        {
            Mobile = "09125555555",
            Password = "InitialPass1",
            FirstName = "Flow",
            LastName = "Test",
            NationalCode = "5555555555"
        };
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        regResponse.EnsureSuccessStatusCode();
        var regResult = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotEmpty(regResult!.Token);

        var loginDto = new { Mobile = "09125555555", Password = "InitialPass1" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        var token = loginResult!.Token;

        var profileClient = _fixture.CreateClient();
        profileClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var profileResponse = await profileClient.GetAsync("/api/auth/profile");
        profileResponse.EnsureSuccessStatusCode();

        var changeDto = new
        {
            CurrentPassword = "InitialPass1",
            NewPassword = "NewPass1234"
        };
        var changeResponse = await profileClient.PostAsJsonAsync("/api/auth/change-password", changeDto);
        changeResponse.EnsureSuccessStatusCode();

        var login2Dto = new { Mobile = "09125555555", Password = "NewPass1234" };
        var login2Response = await _client.PostAsJsonAsync("/api/auth/login", login2Dto);
        login2Response.EnsureSuccessStatusCode();
    }

    #endregion
}
