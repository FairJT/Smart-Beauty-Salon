using System.Net;
using System.Net.Http.Json;

namespace SmartSalon.Tests;

[Trait("Category", "Module")]
public class InventoryTests : IClassFixture<TestFixture>
{
    private readonly HttpClient _client;
    private readonly TestFixture _fixture;

    public InventoryTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task GetInventoryItems_WithAuth_ReturnsList()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task GetInventoryItems_WithoutAuth_ReturnsUnauthorized()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task CreateInventoryItem_AsManager_ReturnsSuccess()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task CreateInventoryItem_AsClient_ReturnsForbidden()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task CreateInventoryItem_InvalidData_ReturnsBadRequest()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task UpdateInventoryItem_AsManager_ReturnsSuccess()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task UpdateInventoryItem_AsNonManager_ReturnsForbidden()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task DeleteInventoryItem_AsManager_ReturnsSuccess()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task DeleteInventoryItem_AsNonManager_ReturnsForbidden()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task AddStockMovement_AsManager_ReturnsSuccess()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task GetStockMovements_ReturnsList()
    {
    }

    [Fact(Skip = "Module endpoint not in SmartSalon")]
    public async Task FullInventoryFlow_CreateUpdateDelete()
    {
    }
}
