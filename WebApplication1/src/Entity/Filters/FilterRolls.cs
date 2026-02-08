namespace WebApplication1.Entity.Filters;

public class FilterRolls
{
    public int? Id { get; set; }

    public FilterRangeRolls? Weight { get; set; }
    public FilterRangeRolls? Length { get; set; }

    public FilterTimeRolls? AddTime { get; set; }
    public FilterTimeRolls? RemoveTime { get; set; }
}