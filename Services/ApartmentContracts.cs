namespace Services;

public sealed record ApartmentFormRequest(
    string ApartmentCode,
    int BuildingId,
    int Floor,
    decimal? Area,
    int NumberOfRooms,
    string? ApartmentType,
    string? Status,
    string? Description);

public sealed record AssignResidentRequest(
    int ApartmentId,
    int ResidentId,
    bool IsOwner,
    DateOnly MoveInDate,
    DateOnly? MoveOutDate,
    string? Notes);
