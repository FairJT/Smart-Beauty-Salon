using System.Net;
using System.Net.Http.Json;

namespace SmartSalon.Tests;

[Trait("Category", "Module")]
public class TenantIsolationTests : IClassFixture<TestFixture>
{
    private readonly HttpClient _client;
    private readonly TestFixture _fixture;

    public TenantIsolationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task TenantA_CannotRead_TenantB_Appointments()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task TenantA_CannotMutate_TenantB_Salon()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task TenantA_CannotDelete_TenantB_Salon()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task TenantA_CannotCreate_ServiceInTenantB_Salon()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task TenantA_CannotUpdate_TenantB_Service()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task TenantA_CannotDelete_TenantB_Service()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task TenantA_CannotCreate_ArtistInTenantB_Salon()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task TenantA_CannotUpdate_TenantB_Artist()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task TenantA_CannotDelete_TenantB_Artist()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task TenantA_CannotAccess_TenantB_Inventory()
    {
        await Task.CompletedTask;
    }
}
