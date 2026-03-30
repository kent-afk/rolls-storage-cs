using WebApplication1.Entity;
using WebApplication1.Entity.Filters;
using WebApplication1.Entity.Statistics;

namespace WebApplication1.Services.Interfaces;

/// <summary>
/// Repository interface for roll storage operations.
/// </summary>
public interface IRollRepository
{
    /// <summary>
    /// Adds a new roll to the warehouse.
    /// </summary>
    /// <param name="roll">Roll to add.</param>
    /// <returns>Added roll with assigned identifier.</returns>
    Task<Roll> AddAsync(Roll roll);

    /// <summary>
    /// Gets a roll by its unique identifier.
    /// </summary>
    /// <param name="id">Roll identifier.</param>
    /// <returns>Found roll or null if not found.</returns>
    Task<Roll?> GetByIdAsync(Guid id);

    /// <summary>
    /// Removes a roll (marks as deleted).
    /// </summary>
    /// <param name="id">Roll identifier.</param>
    /// <returns>Removed roll or null if not found.</returns>
    Task<Roll?> DeleteAsync(Guid id);

    /// <summary>
    /// Gets list of rolls with filtering.
    /// </summary>
    /// <param name="filter">Filter parameters.</param>
    /// <returns>Filtered list of rolls.</returns>
    Task<IEnumerable<Roll>> GetByFilterAsync(FilterRolls filter);

    /// <summary>
    /// Gets statistics for rolls during specified period.
    /// </summary>
    /// <param name="request">Statistics request parameters.</param>
    /// <returns>Statistics response.</returns>
    Task<StatisticResponse> GetStatisticsAsync(StatisticRequest request);
}
