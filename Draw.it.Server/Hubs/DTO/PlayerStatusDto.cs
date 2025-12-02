namespace Draw.it.Server.Hubs.DTO;

public record PlayerStatusDto(
    string Name,
    int Score,
    bool IsDrawer,
    bool HasGuessed
);