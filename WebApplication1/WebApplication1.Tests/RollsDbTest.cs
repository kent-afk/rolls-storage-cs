using Microsoft.EntityFrameworkCore;
using WebApplication1.Core.Data;
using WebApplication1.Core.DataBase;
using WebApplication1.Entity;
using Xunit;

namespace WebApplication1.test;

public class RollsDbTest
{
    private readonly RollsDbContext _context;
    private readonly RollsDb _rollsDb;
    
    public RollsDbTest()
    {
        var options = new DbContextOptionsBuilder<RollsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new RollsDbContext(options);
        _rollsDb = new RollsDb(_context);
    }
    
    [Fact]
    public async Task AddAsync_ShouldAddRoll()
    {
        
        var roll = new Roll { Length = 100.5, Weight = 50.25 };

        
        var result = await _rollsDb.AddAsync(roll);

        
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(100.5, result.Length);
        Assert.Equal(50.25, result.Weight);
        Assert.True(result.DateAdd <= DateTime.UtcNow);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnRoll()
    {
        
        var roll = new Roll { Id = 1, Length = 100.5, Weight = 50.25 };
        await _context.Rolls.AddAsync(roll);
        await _context.SaveChangesAsync();

        
        var result = await _rollsDb.GetAsync(1);

        
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(100.5, result.Length);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenRollNotExists()
    {
        
        var result = await _rollsDb.GetAsync(999);

        
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveRoll()
    {
        
        var roll = new Roll { Id = 1, Length = 100.5, Weight = 50.25 };
        await _context.Rolls.AddAsync(roll);
        await _context.SaveChangesAsync();

        
        var result = await _rollsDb.DeleteAsync(1);

        
        Assert.NotNull(result);
        Assert.NotNull(result.DateRemove);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNull_WhenRollNotExists()
    {
        
        var result = await _rollsDb.DeleteAsync(999);

        
        Assert.Null(result);
    }
}
