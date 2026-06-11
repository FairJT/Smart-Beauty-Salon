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

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task GetServiceTemplates_ReturnsList()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task CreateServiceTemplate_AsPlatformOwner_ReturnsSuccess()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task CreateServiceTemplate_AsNonOwner_ReturnsForbidden()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task UpdateServiceTemplate_AsPlatformOwner_ReturnsSuccess()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task DeleteServiceTemplate_AsPlatformOwner_ReturnsSuccess()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task GetPackageListings_ReturnsList()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task CreatePackageListing_AsPlatformOwner_ReturnsSuccess()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task CreatePackageListing_AsNonOwner_ReturnsForbidden()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task GetMyLicenses_ReturnsList()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task PurchaseLicense_AsSalonOwner_ReturnsSuccess()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task FullMarketplaceFlow_CreateTemplateCreatePackagePurchaseLicense()
    {
    }
}
