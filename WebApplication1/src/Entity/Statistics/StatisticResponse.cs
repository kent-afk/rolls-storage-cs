namespace WebApplication1.Entity.Statistics;

/// <summary>
/// Response containing warehouse statistics for a specified period.
/// </summary>
public class StatisticResponse
{
    /// <summary>
    /// Number of rolls added during the period.
    /// </summary>
    public int TotalAdded { get; set; }

    /// <summary>
    /// Number of rolls removed during the period.
    /// </summary>
    public int TotalRemoved { get; set; }

    /// <summary>
    /// Average length of rolls in warehouse during the period.
    /// </summary>
    public double AverageLength { get; set; }

    /// <summary>
    /// Average weight of rolls in warehouse during the period.
    /// </summary>
    public double AverageWeight { get; set; }

    /// <summary>
    /// Maximum length among rolls in warehouse during the period.
    /// </summary>
    public double MaxLength { get; set; }

    /// <summary>
    /// Minimum length among rolls in warehouse during the period.
    /// </summary>
    public double MinLength { get; set; }

    /// <summary>
    /// Maximum weight among rolls in warehouse during the period.
    /// </summary>
    public double MaxWeight { get; set; }

    /// <summary>
    /// Minimum weight among rolls in warehouse during the period.
    /// </summary>
    public double MinWeight { get; set; }

    /// <summary>
    /// Total weight of all rolls in warehouse during the period.
    /// </summary>
    public double TotalWeight { get; set; }

    /// <summary>
    /// Maximum time a roll spent in the warehouse.
    /// </summary>
    public TimeSpan MaxTimeInStock { get; set; }

    /// <summary>
    /// Minimum time a roll spent in the warehouse.
    /// </summary>
    public TimeSpan MinTimeInStock { get; set; }

    // Bonus: Day with minimum/maximum roll count
    /// <summary>
    /// Day when the warehouse had minimum number of rolls.
    /// </summary>
    public DateTime? DayWithMinRollCount { get; set; }

    /// <summary>
    /// Day when the warehouse had maximum number of rolls.
    /// </summary>
    public DateTime? DayWithMaxRollCount { get; set; }

    // Bonus: Day with minimum/maximum total weight
    /// <summary>
    /// Day when total weight of rolls was minimum.
    /// </summary>
    public DateTime? DayWithMinWeight { get; set; }

    /// <summary>
    /// Day when total weight of rolls was maximum.
    /// </summary>
    public DateTime? DayWithMaxWeight { get; set; }
}
