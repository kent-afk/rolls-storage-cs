using WebApplication1.Entity;
using WebApplication1.Entity.Filters;
using WebApplication1.Entity.Rolls;
using WebApplication1.Entity.Statistics;

namespace WebApplication1.Services.Interfaces;

/// <summary>
/// Service interface for roll warehouse management.
/// </summary>
public interface IRollService
{
    /// <summary>
    /// Adds a new roll to the warehouse.
    /// </summary>
    /// <param name="request">Roll creation request.</param>
    /// <returns>Added roll.</returns>
    Task<Roll> AddRollAsync(CreateRollRequest request);

    /// <summary>
    /// Removes a roll from the warehouse.
    /// </summary>
    /// <param name="id">Roll identifier.</param>
    /// <returns>Removed roll.</returns>
    Task<Roll?> DeleteRollAsync(Guid id);

    /// <summary>
    /// Gets a roll by identifier.
    /// </summary>
    /// <param name="id">Roll identifier.</param>
    /// <returns>Found roll.</returns>
    Task<Roll?> GetRollAsync(Guid id);

    /// <summary>
    /// Gets list of rolls with filtering.
    /// </summary>
    /// <param name="filter">Filter parameters.</param>
    /// <returns>List of rolls.</returns>
    Task<IEnumerable<Roll>> GetRollsAsync(FilterRolls filter);

    /// <summary>
    /// Gets statistics for rolls during period.
    /// </summary>
    /// <param name="request">Request parameters.</param>
    /// <returns>Statistics.</returns>
    Task<StatisticResponse> GetStatisticsAsync(StatisticRequest request);
}
