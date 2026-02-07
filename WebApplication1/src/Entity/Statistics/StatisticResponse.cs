namespace WebApplication1.Entity.Statistics;

public class StatisticResponse
{
    public int TotalAdd { get; set; }
    public int TotalRemoved { get; set; }
    
    public double AverageLength { get; set; }
    public double AverageWeight { get; set; }
    
    public double MaxLength { get; set; }
    public double MaxWeight { get; set; }
    public double MinLength { get; set; }
    public double MinWeight { get; set; }
    
    public TimeSpan MaxTimeInStock { get; set; }
    public TimeSpan MinTimeInStock { get; set; }
}