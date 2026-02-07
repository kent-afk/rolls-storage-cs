namespace WebApplication1.Entity.Filters;

public class FilterRolls
{
    public int Id { get; set; }
    
    public required FilterRangeRolls Weight { get; set; }
    public required FilterRangeRolls Length { get; set; }
    
    public FilterTimeRolls? AddTime { get; set; }
    public FilterTimeRolls? RemoveTime { get; set; }
}