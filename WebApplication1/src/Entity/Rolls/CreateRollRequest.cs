using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Entity.Rolls;

public record CreateRollRequest
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Длина должна быть положительным числом")]
    public double Length { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Вес должен быть положительным числом")]
    public double Weight { get; init; }
}
