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

                services.AddDbContext<RollsDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task AddRoll_ShouldReturnCreatedResult_WithValidData()
    {
        
        var createRollDto = new CreateRollRequest
        {
            Length = 100.5,
            Weight = 50.25
        };

        var jsonContent = JsonConvert.SerializeObject(createRollDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        
        var response = await _client.PostAsync("/api/rolls", content);

        
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdRoll = JsonConvert.DeserializeObject<Roll>(responseContent);
        
        Assert.NotNull(createdRoll);
        Assert.Equal(100.5, createdRoll.Length);
        Assert.Equal(50.25, createdRoll.Weight);
        Assert.True(createdRoll.DateAdd <= DateTime.UtcNow);
    }

    [Fact]
    public async Task AddRoll_ShouldReturnBadRequest_WithInvalidData()
    {
        // Arrange
        var invalidRollDto = new CreateRollRequest
        {
            Length = -10,
            Weight = 50.25
        };

        var jsonContent = JsonConvert.SerializeObject(invalidRollDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        
        var response = await _client.PostAsync("/api/rolls", content);

        
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRoll_ShouldReturnRoll_WhenRollExists()
    {
        var createRollDto = new CreateRollRequest
        {
            Length = 100.5,
            Weight = 50.25
        };

        var jsonContent = JsonConvert.SerializeObject(createRollDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var createResponse = await _client.PostAsync("/api/rolls", content);
        var createResult = JsonConvert.DeserializeObject<Roll>(await createResponse.Content.ReadAsStringAsync());
        
        var response = await _client.GetAsync($"/api/rolls/{createResult!.Id}");

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var roll = JsonConvert.DeserializeObject<Roll>(responseContent);
        
        Assert.NotNull(roll);
        Assert.Equal(createResult.Id, roll.Id);
        Assert.Equal(100.5, roll.Length);
        Assert.Equal(50.25, roll.Weight);
    }

    [Fact]
    public async Task GetRoll_ShouldReturnNotFound_WhenRollNotExists()
    {
        
        var response = await _client.GetAsync("/api/rolls/999");

        
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveRoll_ShouldReturnRemovedRoll_WhenRollExists()
    {
        var createRollDto = new CreateRollRequest
        {
            Length = 100.5,
            Weight = 50.25
        };

        var jsonContent = JsonConvert.SerializeObject(createRollDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var createResponse = await _client.PostAsync("/api/rolls", content);
        var createResult = JsonConvert.DeserializeObject<Roll>(await createResponse.Content.ReadAsStringAsync());

        var response = await _client.DeleteAsync($"/api/rolls/{createResult!.Id}");

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var removedRoll = JsonConvert.DeserializeObject<Roll>(responseContent);
        
        Assert.NotNull(removedRoll);
        Assert.Equal(createResult.Id, removedRoll.Id);
        Assert.NotNull(removedRoll.DateRemove);
    }

    [Fact]
    public async Task GetRolls_ShouldReturnFilteredList()
    {
        var roll1 = new CreateRollRequest { Length = 100, Weight = 50 };
        var roll2 = new CreateRollRequest { Length = 200, Weight = 100 };
        var roll3 = new CreateRollRequest { Length = 150, Weight = 75 };

        await CreateRoll(roll1);
        await CreateRoll(roll2);
        await CreateRoll(roll3);

        
        var response = await _client.GetAsync("/api/rolls?weight.min=60&weight.max=80");

        
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var rolls = JsonConvert.DeserializeObject<List<Roll>>(responseContent);
        
        Assert.NotNull(rolls);
        Assert.Single(rolls);
        Assert.Equal(150, rolls[0].Length);
        Assert.Equal(75, rolls[0].Weight);
    }

    [Fact]
    public async Task GetStatistics_ShouldReturnCorrectStatistics()
    {
        var now = DateTime.UtcNow;
        var startDate = now.AddDays(-10);
        var endDate = now;

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RollsDbContext>();
        
        context.Rolls.RemoveRange(context.Rolls);
        await context.SaveChangesAsync();

        var roll1 = new Roll { Length = 100, Weight = 50, DateAdd = startDate.AddDays(2) };
        var roll2 = new Roll { Length = 200, Weight = 100, DateAdd = startDate.AddDays(5) };
        var roll3 = new Roll { Length = 300, Weight = 150, DateAdd = startDate.AddDays(2), DateRemove = startDate.AddDays(8) };

        await context.Rolls.AddRangeAsync(roll1, roll2, roll3);
        await context.SaveChangesAsync();

        var request = new StatisticRequest(startDate, endDate);
        var jsonContent = JsonConvert.SerializeObject(request);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        
        var response = await _client.PostAsync("/api/rolls/statistics", content);

        
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var statistics = JsonConvert.DeserializeObject<StatisticResponse>(responseContent);
        
        Assert.NotNull(statistics);
        Assert.Equal(3, statistics.TotalAdd);
        Assert.Equal(1, statistics.TotalRemoved);
        Assert.Equal(200, statistics.AverageLength); // (100 + 200 + 300) / 3
        Assert.Equal(100, statistics.AverageWeight); // (50 + 100 + 150) / 3
    }

    private async Task<Roll> CreateRoll(CreateRollRequest rollDto)
    {
        var jsonContent = JsonConvert.SerializeObject(rollDto);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/rolls", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<Roll>(responseContent)!;
    }
}