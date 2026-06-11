using System.Net;
using System.Net.Http.Json;

namespace SmartSalon.Tests;

[Trait("Category", "Module")]
public class CatalogTests : IClassFixture<TestFixture>
{
    private readonly HttpClient _client;
    private readonly TestFixture _fixture;

    public CatalogTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task GetCatalogServices_ReturnsList()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task GetCatalogServices_BySalonId_ReturnsList()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task CreateCatalogService_AsManager_ReturnsSuccess()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task CreateCatalogService_AsNonManager_ReturnsForbidden()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task UpdateCatalogService_AsManager_ReturnsSuccess()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task UpdateCatalogService_AsNonManager_ReturnsForbidden()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task DeleteCatalogService_AsManager_ReturnsSuccess()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task DeleteCatalogService_AsNonManager_ReturnsForbidden()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task FullCatalogFlow_CreateUpdateDelete()
    {
    }
}
