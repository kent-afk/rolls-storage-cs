using Microsoft.AspNetCore.Mvc;
using WebApplication1.Core.DataBase;
using WebApplication1.Entity;
using WebApplication1.Entity.Filters;
using WebApplication1.Entity.Rolls;
using WebApplication1.Entity.Statistics;

namespace WebApplication1.API;

[ApiController]
[Route("api/[controller]")]
public sealed class RollsController : Controller
{
    private readonly IDataBase _dataBase;

    private readonly ILogger<RollsController> _logger;

    private static int _nextId = 1;

    public RollsController(IDataBase dataBase, ILogger<RollsController> logger)
    {
        _dataBase = dataBase;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Roll>> AddRoll([FromBody] CreateRollRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var roll = new Roll
            {
                Id = _nextId++,
                Length = request.Length,
                Weight = request.Weight,
                DateAdd = DateTime.Now,
            };

            var addedRoll = await _dataBase.AddAsync(roll);
            _logger.LogInformation($"Added roll with id {addedRoll.Id}");

            return CreatedAtAction(nameof(AddRoll), new { id = addedRoll.Id }, addedRoll);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, "Error adding roll");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Roll>> DeleteRoll(int id)
    {
        try
        {
            var roll = await _dataBase.DeleteAsync(id);
            if (roll == null)
            {
                return NotFound($"Roll with id {id} not found");
            }

            _logger.LogInformation($"Roll with id {id} deleted");
            return Ok(roll);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting roll with id: {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Roll>> GetRoll(int id)
    {
        try
        {
            var roll = await _dataBase.GetAsync(id);
            if (roll == null)
                return NotFound($"Roll with id {id} not found");

            return Ok(roll);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting roll with id: {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status400BadRequest)]

    public async Task<ActionResult<List<Roll>>> GetRolls([FromQuery] FilterRolls filter)
    {
        try
        {
            var roll = await _dataBase.GetByFilterAsync(filter);
            return Ok(roll);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting rolls");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("statistics")]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Roll), StatusCodes.Status400BadRequest)]

    public async Task<ActionResult<StatisticResponse>> GetStatistics([FromBody] StatisticRequest request)
    {
        try
        {
            if (request.From > request.To)
            {
                return BadRequest("Date From cannot be greater than To");
            }

            var stats = await _dataBase.GetStatisticAsync(request);
            return Ok(stats);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting stats");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}