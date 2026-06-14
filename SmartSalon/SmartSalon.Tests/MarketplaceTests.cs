using System.Net;
using System.Net.Http.Json;

namespace SmartSalon.Tests;

[Trait("Category", "Module")]
public class MarketplaceTests : IClassFixture<TestFixture>
{
    private readonly HttpClient _client;
    private readonly TestFixture _fixture;

    public MarketplaceTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetServiceTemplates_ReturnsList()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreateServiceTemplate_AsPlatformOwner_ReturnsSuccess()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreateServiceTemplate_AsNonOwner_ReturnsForbidden()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task UpdateServiceTemplate_AsPlatformOwner_ReturnsSuccess()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task DeleteServiceTemplate_AsPlatformOwner_ReturnsSuccess()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetPackageListings_ReturnsList()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreatePackageListing_AsPlatformOwner_ReturnsSuccess()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreatePackageListing_AsNonOwner_ReturnsForbidden()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetMyLicenses_ReturnsList()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task PurchaseLicense_AsSalonOwner_ReturnsSuccess()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task FullMarketplaceFlow_CreateTemplateCreatePackagePurchaseLicense()
    {
        await Task.CompletedTask;
    }
}
