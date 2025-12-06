namespace Draw.it.Server.Hubs.DTO;

public record CanvasSnapshotDto(
    string ImageBytes,
    string MimeType
);