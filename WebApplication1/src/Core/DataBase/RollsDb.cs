using Microsoft.EntityFrameworkCore;
using WebApplication1.Core.Data;
using WebApplication1.Entity;
using WebApplication1.Entity.Filters;
using WebApplication1.Entity.Statistics;

namespace WebApplication1.Core.DataBase;

public sealed class RollsDb : IDataBase
{
    private readonly RollsDbContext _context;

    public RollsDb(RollsDbContext context) => _context = context;

    public async Task<Roll> AddAsync(Roll entity)
    {
        entity.DateAdd = DateTime.UtcNow;
        _context.Rolls.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Roll?> GetAsync(int id)
    {
        return await _context.Rolls.FindAsync(id);
    }

    public async Task<Roll?> DeleteAsync(int id)
    {
        var roll = await _context.Rolls.FindAsync(id);
        if (roll == null)
            return null;

        roll.DateRemove = DateTime.Now;
        await _context.SaveChangesAsync();
        return roll;
    }

    public async Task<IEnumerable<Roll>> GetByFilterAsync(FilterRolls filter)
    {
        var query = _context.Rolls.AsQueryable();

        if (filter.Id.HasValue)
        {
            query = query.Where(e => e.Id == filter.Id.Value);
        }

        if (filter.Weight != null)
        {
            if (filter.Weight.Min.HasValue)
            {
                query = query.Where(e => e.Weight >= filter.Weight.Min.Value);
            }

            if (filter.Weight.Max.HasValue)
            {
                query = query.Where(e => e.Weight <= filter.Weight.Max.Value);
            }
        }

        if (filter.Length != null)
        {
            if (filter.Length.Min.HasValue)
            {
                query = query.Where(e => e.Length >= filter.Length.Min.Value);
            }

            if (filter.Length.Max.HasValue)
            {
                query = query.Where(e => e.Length <= filter.Length.Max.Value);
            }
        }

        if (filter.AddTime != null)
        {
            if (filter.AddTime.From.HasValue)
            {
                query = query.Where(e => e.DateAdd >= filter.AddTime.From.Value);
            }

            if (filter.AddTime.To.HasValue)
            {
                query = query.Where(e => e.DateAdd <= filter.AddTime.To.Value);
            }
        }

        if (filter.RemoveTime != null)
        {
            if (filter.RemoveTime.From.HasValue)
            {
                query = query.Where(e => e.DateRemove.HasValue && e.DateRemove >= filter.RemoveTime.From.Value);
            }

            if (filter.RemoveTime.To.HasValue)
            {
                query = query.Where(e => e.DateRemove.HasValue && e.DateRemove <= filter.RemoveTime.To.Value);
            }
        }

        return await query.OrderBy(e => e.DateAdd).ToListAsync(); //
    }

    public async Task<StatisticResponse> GetStatisticAsync(StatisticRequest request)
    {
        var rolls = await _context.Rolls.Where(x => x.DateAdd <= request.To && (x.DateRemove == null ||
            x.DateRemove >= request.From)).ToListAsync();

        var addedRolls = rolls.Where(x => x.DateAdd >= request.From && x.DateAdd <= request.To);

        var deletedRolls = rolls.Where(x => x.DateRemove.HasValue && (x.DateRemove >= request.From &&
                                                                      x.DateRemove <= request.To));

        var statistics = new StatisticResponse
        {
            TotalAdd = addedRolls.Count(),
            TotalRemoved = deletedRolls.Count(),
            AverageLength = rolls.Any() ? rolls.Average(x => x.Length) : 0,
            AverageWeight = rolls.Any() ? rolls.Average(x => x.Weight) : 0,
            MaxLength = rolls.Any() ? rolls.Max(x => x.Length) : 0,
            MinLength = rolls.Any() ? rolls.Min(x => x.Length) : 0,
            MaxWeight = rolls.Any() ? rolls.Max(x => x.Weight) : 0,
            MinWeight = rolls.Any() ? rolls.Min(x => x.Weight) : 0,
        };

        var timeInStock = rolls.Where(x => x.DateRemove.HasValue).Select(
            x => x.DateRemove!.Value - x.DateAdd).ToList();

        if (timeInStock.Any())
        {
            statistics.MaxTimeInStock = timeInStock.Max();
            statistics.MinTimeInStock = timeInStock.Min();
        }

        return statistics;
    }
}