namespace WebApplication1.Entity;

/// <summary>
/// Represents a metal roll in the warehouse.
/// </summary>
public class Roll
{
    /// <summary>
    /// Unique identifier for the roll (GUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Length of the roll in meters.
    /// </summary>
    public double Length { get; set; }

    /// <summary>
    /// Weight of the roll in kilograms.
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    /// Date and time when the roll was added to the warehouse.
    /// </summary>
    public DateTime DateAdd { get; set; }

    /// <summary>
    /// Date and time when the roll was removed from the warehouse (null if still in warehouse).
    /// </summary>
    public DateTime? DateRemove { get; set; }
}
