namespace WebApplication1.Entity.Filters;

/// <summary>
/// Filter parameters for retrieving rolls.
/// Supports multiple filters combined (AND logic).
/// </summary>
public class FilterRolls
{
    /// <summary>
    /// Filter by roll identifier (GUID).
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Filter by weight range.
    /// </summary>
    public FilterRangeRolls? Weight { get; set; }

    /// <summary>
    /// Filter by length range.
    /// </summary>
    public FilterRangeRolls? Length { get; set; }

    /// <summary>
    /// Filter by addition date range.
    /// </summary>
    public FilterTimeRolls? AddTime { get; set; }

    /// <summary>
    /// Filter by removal date range.
    /// </summary>
    public FilterTimeRolls? RemoveTime { get; set; }
}
