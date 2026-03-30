using Microsoft.Extensions.Logging;
using WebApplication1.Entity;
using WebApplication1.Entity.Filters;
using WebApplication1.Entity.Rolls;
using WebApplication1.Entity.Statistics;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementation;

/// <summary>
/// Service implementation for roll warehouse management.
/// Contains business logic and orchestrates repository calls.
/// </summary>
public sealed class RollService : IRollService
{
    private readonly IRollRepository _repository;
    private readonly ILogger<RollService> _logger;

    public RollService(IRollRepository repository, ILogger<RollService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Roll> AddRollAsync(CreateRollRequest request)
    {
        // Validate input
        if (request.Length <= 0)
        {
            throw new ArgumentException("Length must be greater than zero", nameof(request));
        }

        if (request.Weight <= 0)
        {
            throw new ArgumentException("Weight must be greater than zero", nameof(request));
        }

        var roll = new Roll
        {
            Id = Guid.NewGuid(), // Use GUID for unique IDs
            Length = request.Length,
            Weight = request.Weight,
            DateAdd = DateTime.UtcNow
        };

        var addedRoll = await _repository.AddAsync(roll);
        _logger.LogInformation("Roll added with ID: {RollId}", addedRoll.Id);

        return addedRoll;
    }

    /// <inheritdoc/>
    public async Task<Roll?> DeleteRollAsync(Guid id)
    {
        var roll = await _repository.DeleteAsync(id);
        
        if (roll == null)
        {
            _logger.LogWarning("Attempted to delete non-existent roll with ID: {RollId}", id);
            return null;
        }

        _logger.LogInformation("Roll deleted with ID: {RollId}", id);
        return roll;
    }

    /// <inheritdoc/>
    public async Task<Roll?> GetRollAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Roll>> GetRollsAsync(FilterRolls filter)
    {
        return await _repository.GetByFilterAsync(filter);
    }

    /// <inheritdoc/>
    public async Task<StatisticResponse> GetStatisticsAsync(StatisticRequest request)
    {
        if (request.From > request.To)
        {
            throw new ArgumentException("'From' date cannot be greater than 'To' date");
        }

        _logger.LogInformation(
            "Calculating statistics for period: {From} to {To}",
            request.From,
            request.To);

        return await _repository.GetStatisticsAsync(request);
    }
}
