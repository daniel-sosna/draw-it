using System.Collections.Generic;
using Draw.it.Server.Enums;

namespace Draw.it.Server.Hubs.DTO
{
    public record StrokeDto(List<Point> Points, Color Color, int Size, bool Eraser);
}