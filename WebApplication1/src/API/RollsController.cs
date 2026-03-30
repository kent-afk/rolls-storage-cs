using Microsoft.AspNetCore.Mvc;
using WebApplication1.Entity;
using WebApplication1.Entity.Filters;
using WebApplication1.Entity.Rolls;
using WebApplication1.Entity.Statistics;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.API;

/// <summary>
/// Controller for managing metal rolls in the warehouse.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class RollsController : ControllerBase
{
    private readonly IRollService _rollService;
    private readonly ILogger<RollsController> _logger;

    public RollsController(IRollService rollService, ILogger<RollsController> logger)
    {
        _rollService = rollService;
        _logger = logger;
    }

    /// <summary>
    /// Adds a new roll to the warehouse.
    /// </summary>
    /// <param name="request">Roll creation request containing length and weight.</param>
    /// <returns>Created roll with assigned identifier.</returns>
    /// <response code="201">Roll successfully created.</response>
    /// <response code="400">Invalid input data.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Roll>> AddRoll([FromBody] CreateRollRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var roll = await _rollService.AddRollAsync(request);
            _logger.LogInformation("Roll added with ID: {RollId}", roll.Id);

            return CreatedAtAction(nameof(GetRoll), new { id = roll.Id }, roll);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input when adding roll");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding roll");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while adding the roll" });
        }
    }

    /// <summary>
    /// Removes a roll from the warehouse by its identifier.
    /// </summary>
    /// <param name="id">Roll identifier (GUID).</param>
    /// <returns>Removed roll.</returns>
    /// <response code="200">Roll successfully removed.</response>
    /// <response code="404">Roll not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Roll>> DeleteRoll(Guid id)
    {
        try
        {
            var roll = await _rollService.DeleteRollAsync(id);
            
            if (roll == null)
            {
                return NotFound(new { error = $"Roll with id '{id}' not found" });
            }

            _logger.LogInformation("Roll deleted with ID: {RollId}", id);
            return Ok(roll);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting roll with ID: {RollId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while deleting the roll" });
        }
    }

    /// <summary>
    /// Gets a roll by its identifier.
    /// </summary>
    /// <param name="id">Roll identifier (GUID).</param>
    /// <returns>Found roll.</returns>
    /// <response code="200">Roll found.</response>
    /// <response code="404">Roll not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Roll>> GetRoll(Guid id)
    {
        try
        {
            var roll = await _rollService.GetRollAsync(id);
            
            if (roll == null)
            {
                return NotFound(new { error = $"Roll with id '{id}' not found" });
            }

            return Ok(roll);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roll with ID: {RollId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while getting the roll" });
        }
    }

    /// <summary>
    /// Gets list of rolls with optional filtering.
    /// Supports filtering by: id, weight range, length range, addition date range, removal date range.
    /// Multiple filters can be combined.
    /// </summary>
    /// <param name="filter">Filter parameters.</param>
    /// <returns>List of filtered rolls.</returns>
    /// <response code="200">Returns list of rolls.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<Roll>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<Roll>>> GetRolls([FromQuery] FilterRolls filter)
    {
        try
        {
            var rolls = await _rollService.GetRollsAsync(filter);
            return Ok(rolls.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rolls");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while getting rolls" });
        }
    }

    /// <summary>
    /// Gets statistics for rolls during specified period.
    /// </summary>
    /// <param name="request">Statistics request with date range.</param>
    /// <returns>Statistics for the period.</returns>
    /// <response code="200">Returns statistics.</response>
    /// <response code="400">Invalid date range.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("statistics")]
    [ProducesResponseType(typeof(StatisticResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StatisticResponse>> GetStatistics([FromBody] StatisticRequest request)
    {
        try
        {
            if (request.From > request.To)
            {
                return BadRequest(new { error = "'From' date cannot be greater than 'To' date" });
            }

            var stats = await _rollService.GetStatisticsAsync(request);
            _logger.LogInformation(
                "Statistics calculated for period: {From} to {To}",
                request.From,
                request.To);

            return Ok(stats);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid statistics request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while calculating statistics" });
        }
    }
}
