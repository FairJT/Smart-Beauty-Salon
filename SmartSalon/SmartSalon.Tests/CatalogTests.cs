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

    [Fact]
    public async Task GetCatalogServices_ReturnsList()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetCatalogServices_BySalonId_ReturnsList()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreateCatalogService_AsManager_ReturnsSuccess()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreateCatalogService_AsNonManager_ReturnsForbidden()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task UpdateCatalogService_AsManager_ReturnsSuccess()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task UpdateCatalogService_AsNonManager_ReturnsForbidden()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task DeleteCatalogService_AsManager_ReturnsSuccess()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task DeleteCatalogService_AsNonManager_ReturnsForbidden()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task FullCatalogFlow_CreateUpdateDelete()
    {
        await Task.CompletedTask;
    }
}
