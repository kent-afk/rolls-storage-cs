using Microsoft.EntityFrameworkCore;
using WebApplication1.Core.Data;
using WebApplication1.Entity;
using WebApplication1.Entity.Filters;
using WebApplication1.Entity.Statistics;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementation;

/// <summary>
/// Repository implementation for roll storage using Entity Framework Core.
/// </summary>
public sealed class RollRepository : IRollRepository
{
    private readonly RollsDbContext _context;

    public RollRepository(RollsDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Roll> AddAsync(Roll roll)
    {
        roll.DateAdd = DateTime.UtcNow;
        _context.Rolls.Add(roll);
        await _context.SaveChangesAsync();
        return roll;
    }

    /// <inheritdoc/>
    public async Task<Roll?> GetByIdAsync(Guid id)
    {
        return await _context.Rolls.FindAsync(id);
    }

    /// <inheritdoc/>
    public async Task<Roll?> DeleteAsync(Guid id)
    {
        var roll = await _context.Rolls.FindAsync(id);
        if (roll == null)
            return null;

        roll.DateRemove = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return roll;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Roll>> GetByFilterAsync(FilterRolls filter)
    {
        var query = _context.Rolls.AsQueryable();

        // Filter by ID
        if (filter.Id.HasValue)
        {
            query = query.Where(e => e.Id == filter.Id.Value);
        }

        // Filter by weight range
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

        // Filter by length range
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

        // Filter by addition date range
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

        // Filter by removal date range
        if (filter.RemoveTime != null)
        {
            if (filter.RemoveTime.From.HasValue)
            {
                query = query.Where(e => e.DateRemove >= filter.RemoveTime.From.Value);
            }

            if (filter.RemoveTime.To.HasValue)
            {
                query = query.Where(e => e.DateRemove <= filter.RemoveTime.To.Value);
            }
        }

        return await query.OrderBy(e => e.DateAdd).ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<StatisticResponse> GetStatisticsAsync(StatisticRequest request)
    {
        // Get all rolls that were in warehouse during the period
        var allRollsInPeriod = await _context.Rolls
            .Where(x => x.DateAdd <= request.To && (x.DateRemove == null || x.DateRemove >= request.From))
            .ToListAsync();

        // Rolls added during the period
        var addedRolls = allRollsInPeriod
            .Where(x => x.DateAdd >= request.From && x.DateAdd <= request.To)
            .ToList();

        // Rolls removed during the period
        var removedRolls = allRollsInPeriod
            .Where(x => x.DateRemove.HasValue && x.DateRemove.Value >= request.From && x.DateRemove.Value <= request.To)
            .ToList();

        var statistics = new StatisticResponse
        {
            TotalAdded = addedRolls.Count,
            TotalRemoved = removedRolls.Count,
            AverageLength = allRollsInPeriod.Any() ? allRollsInPeriod.Average(x => x.Length) : 0,
            AverageWeight = allRollsInPeriod.Any() ? allRollsInPeriod.Average(x => x.Weight) : 0,
            MaxLength = allRollsInPeriod.Any() ? allRollsInPeriod.Max(x => x.Length) : 0,
            MinLength = allRollsInPeriod.Any() ? allRollsInPeriod.Min(x => x.Length) : 0,
            MaxWeight = allRollsInPeriod.Any() ? allRollsInPeriod.Max(x => x.Weight) : 0,
            MinWeight = allRollsInPeriod.Any() ? allRollsInPeriod.Min(x => x.Weight) : 0,
            TotalWeight = allRollsInPeriod.Sum(x => x.Weight)
        };

        // Time in stock calculations
        var rollsWithRemoval = allRollsInPeriod.Where(x => x.DateRemove.HasValue).ToList();
        if (rollsWithRemoval.Any())
        {
            var timeInStock = rollsWithRemoval
                .Select(x => x.DateRemove!.Value - x.DateAdd)
                .ToList();

            statistics.MaxTimeInStock = timeInStock.Max();
            statistics.MinTimeInStock = timeInStock.Min();
        }

        // Bonus: Calculate daily statistics for min/max roll count and weight
        CalculateDailyStatistics(statistics, allRollsInPeriod, request.From, request.To);

        return statistics;
    }

    /// <summary>
    /// Calculates daily statistics for bonus points.
    /// </summary>
    private void CalculateDailyStatistics(
        StatisticResponse stats,
        List<Roll> rollsInPeriod,
        DateTime from,
        DateTime to)
    {
        var dailyData = new Dictionary<DateTime, (int Count, double Weight)>();

        // Iterate through each day in the period
        for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
        {
            // Count rolls that were in warehouse on this day
            var rollsOnDay = rollsInPeriod.Where(r =>
                r.DateAdd <= date && (r.DateRemove == null || r.DateRemove > date)).ToList();

            dailyData[date] = (rollsOnDay.Count, rollsOnDay.Sum(r => r.Weight));
        }

        if (dailyData.Any())
        {
            var minCountDay = dailyData.MinBy(x => x.Value.Count);
            var maxCountDay = dailyData.MaxBy(x => x.Value.Count);
            var minWeightDay = dailyData.MinBy(x => x.Value.Weight);
            var maxWeightDay = dailyData.MaxBy(x => x.Value.Weight);

            stats.DayWithMinRollCount = minCountDay.Key;
            stats.DayWithMaxRollCount = maxCountDay.Key;
            stats.DayWithMinWeight = minWeightDay.Key;
            stats.DayWithMaxWeight = maxWeightDay.Key;
        }
    }
}
