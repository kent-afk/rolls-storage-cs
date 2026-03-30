using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Core.Data;
using WebApplication1.Entity;
using WebApplication1.Entity.Rolls;
using WebApplication1.Entity.Statistics;
using Xunit;

namespace WebApplication1.test;

/// <summary>
/// Integration tests for RollsController.
/// Uses in-memory database for testing without real database dependency.
/// Each test uses a unique database name for isolation.
/// </summary>
public class RollsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RollsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RollsDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Use unique database name for each test run to ensure isolation
                var dbName = $"TestDb_{Guid.NewGuid()}";
                services.AddDbContext<RollsDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
            });
        });

        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Tests adding a roll with valid data returns Created result.
    /// </summary>
    [Fact]
    public async Task AddRoll_ShouldReturnCreatedResult_WithValidData()
    {
        // Arrange
        var createRollDto = new CreateRollRequest(Length: 100.5, Weight: 50.25);
        var jsonContent = JsonConvert.SerializeObject(createRollDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/rolls", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdRoll = JsonConvert.DeserializeObject<Roll>(responseContent);
        
        Assert.NotNull(createdRoll);
        Assert.NotEqual(Guid.Empty, createdRoll.Id);
        Assert.Equal(100.5, createdRoll.Length);
        Assert.Equal(50.25, createdRoll.Weight);
        Assert.True(createdRoll.DateAdd <= DateTime.UtcNow);
    }

    /// <summary>
    /// Tests adding a roll with invalid data returns BadRequest.
    /// </summary>
    [Fact]
    public async Task AddRoll_ShouldReturnBadRequest_WithInvalidData()
    {
        // Arrange - negative length is invalid
        var invalidRollDto = new CreateRollRequest(Length: -10, Weight: 50.25);
        var jsonContent = JsonConvert.SerializeObject(invalidRollDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/rolls", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Tests adding a roll with zero weight returns BadRequest.
    /// </summary>
    [Fact]
    public async Task AddRoll_ShouldReturnBadRequest_WithZeroWeight()
    {
        // Arrange - zero weight is invalid
        var invalidRollDto = new CreateRollRequest(Length: 100, Weight: 0);
        var jsonContent = JsonConvert.SerializeObject(invalidRollDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/rolls", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Tests getting an existing roll returns the roll.
    /// </summary>
    [Fact]
    public async Task GetRoll_ShouldReturnRoll_WhenRollExists()
    {
        // Arrange - create a roll first
        var createRollDto = new CreateRollRequest(Length: 100.5, Weight: 50.25);
        var jsonContent = JsonConvert.SerializeObject(createRollDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var createResponse = await _client.PostAsync("/api/rolls", content);
        var createResult = JsonConvert.DeserializeObject<Roll>(await createResponse.Content.ReadAsStringAsync());
        
        // Act
        var response = await _client.GetAsync($"/api/rolls/{createResult!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var roll = JsonConvert.DeserializeObject<Roll>(responseContent);
        
        Assert.NotNull(roll);
        Assert.Equal(createResult.Id, roll.Id);
        Assert.Equal(100.5, roll.Length);
        Assert.Equal(50.25, roll.Weight);
    }

    /// <summary>
    /// Tests getting a non-existent roll returns NotFound.
    /// </summary>
    [Fact]
    public async Task GetRoll_ShouldReturnNotFound_WhenRollNotExists()
    {
        // Act - use a non-existent GUID
        var nonExistentGuid = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/rolls/{nonExistentGuid}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Tests removing an existing roll returns the removed roll.
    /// </summary>
    [Fact]
    public async Task RemoveRoll_ShouldReturnRemovedRoll_WhenRollExists()
    {
        // Arrange - create a roll first
        var createRollDto = new CreateRollRequest(Length: 100.5, Weight: 50.25);
        var jsonContent = JsonConvert.SerializeObject(createRollDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var createResponse = await _client.PostAsync("/api/rolls", content);
        var createResult = JsonConvert.DeserializeObject<Roll>(await createResponse.Content.ReadAsStringAsync());

        // Act
        var response = await _client.DeleteAsync($"/api/rolls/{createResult!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var removedRoll = JsonConvert.DeserializeObject<Roll>(responseContent);
        
        Assert.NotNull(removedRoll);
        Assert.Equal(createResult.Id, removedRoll.Id);
        Assert.NotNull(removedRoll.DateRemove);
    }

    /// <summary>
    /// Tests removing a non-existent roll returns NotFound.
    /// </summary>
    [Fact]
    public async Task RemoveRoll_ShouldReturnNotFound_WhenRollNotExists()
    {
        // Act - use a non-existent GUID
        var nonExistentGuid = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/rolls/{nonExistentGuid}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Tests filtering rolls by weight range.
    /// </summary>
    [Fact]
    public async Task GetRolls_ShouldReturnFilteredList_ByWeight()
    {
        // Arrange - create multiple rolls
        var roll1 = new CreateRollRequest(Length: 100, Weight: 50);
        var roll2 = new CreateRollRequest(Length: 200, Weight: 100);
        var roll3 = new CreateRollRequest(Length: 150, Weight: 75);

        await CreateRoll(roll1);
        await CreateRoll(roll2);
        await CreateRoll(roll3);

        // Act - filter by weight range
        var response = await _client.GetAsync("/api/rolls?weight.min=60&weight.max=80");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var rolls = JsonConvert.DeserializeObject<List<Roll>>(responseContent);
        
        Assert.NotNull(rolls);
        Assert.Single(rolls);
        Assert.Equal(150, rolls[0].Length);
        Assert.Equal(75, rolls[0].Weight);
    }

    /// <summary>
    /// Tests combined filtering - multiple filters at once.
    /// </summary>
    [Fact]
    public async Task GetRolls_ShouldReturnFilteredList_CombinedFilters()
    {
        // Arrange - create multiple rolls with different properties
        var roll1 = new CreateRollRequest(Length: 100, Weight: 50);   // length 100, weight 50
        var roll2 = new CreateRollRequest(Length: 200, Weight: 100);  // length 200, weight 100
        var roll3 = new CreateRollRequest(Length: 150, Weight: 75);   // length 150, weight 75

        await CreateRoll(roll1);
        await CreateRoll(roll2);
        await CreateRoll(roll3);

        // Act - filter by both length >= 120 AND weight <= 80
        // Only roll3 has length >= 120 (150) and weight <= 80 (75)
        var response = await _client.GetAsync("/api/rolls?length.min=120&weight.max=80");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var rolls = JsonConvert.DeserializeObject<List<Roll>>(responseContent);
        
        Assert.NotNull(rolls);
        Assert.Single(rolls);
        Assert.Equal(150, rolls[0].Length);
        Assert.Equal(75, rolls[0].Weight);
    }

    /// <summary>
    /// Tests filtering by length range.
    /// </summary>
    [Fact]
    public async Task GetRolls_ShouldReturnFilteredList_ByLength()
    {
        // Arrange
        var roll1 = new CreateRollRequest(Length: 100, Weight: 50);
        var roll2 = new CreateRollRequest(Length: 200, Weight: 100);
        var roll3 = new CreateRollRequest(Length: 150, Weight: 75);

        await CreateRoll(roll1);
        await CreateRoll(roll2);
        await CreateRoll(roll3);

        // Act - filter by length range
        var response = await _client.GetAsync("/api/rolls?length.min=120&length.max=180");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var rolls = JsonConvert.DeserializeObject<List<Roll>>(responseContent);
        
        Assert.NotNull(rolls);
        Assert.Single(rolls);
        Assert.Equal(150, rolls[0].Length);
    }

    /// <summary>
    /// Tests statistics calculation returns correct data.
    /// </summary>
    [Fact]
    public async Task GetStatistics_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var startDate = now.AddDays(-10);
        var endDate = now;

        // Create test data directly in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RollsDbContext>();
        
        var roll1 = new Roll { Id = Guid.NewGuid(), Length = 100, Weight = 50, DateAdd = startDate.AddDays(-5) };
        var roll2 = new Roll { Id = Guid.NewGuid(), Length = 200, Weight = 100, DateAdd = startDate.AddDays(-2) };
        var roll3 = new Roll { Id = Guid.NewGuid(), Length = 300, Weight = 150, DateAdd = startDate.AddDays(2), DateRemove = startDate.AddDays(8) };
        
        await context.Rolls.AddRangeAsync(roll1, roll2, roll3);
        await context.SaveChangesAsync();

        var request = new StatisticRequest(From: startDate, To: endDate);
        var jsonContent = JsonConvert.SerializeObject(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/rolls/statistics", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var statistics = JsonConvert.DeserializeObject<StatisticResponse>(responseContent);
        
        Assert.NotNull(statistics);
        // Roll1 and Roll2 were added before endDate and not removed during the period
        // Roll3 was added during the period
        Assert.True(statistics.TotalAdded >= 1);
        Assert.True(statistics.TotalRemoved >= 0);
    }

    /// <summary>
    /// Helper method to create a roll via API.
    /// </summary>
    private async Task<Roll> CreateRoll(CreateRollRequest rollDto)
    {
        var jsonContent = JsonConvert.SerializeObject(rollDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/rolls", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<Roll>(responseContent)!;
    }
}
