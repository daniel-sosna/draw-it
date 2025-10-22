using Draw.it.Server.Enums;

namespace Draw.it.Server.Hubs.DTO;

public record DrawDto(
    Point Point,
    Color Color,
    DrawType Type,
    int Size,
    bool Eraser
);

public struct Point
{
    public double X { get; init; }
    public double Y { get; init; }
}