using BusinessObjects.Models;
using Repositories;

namespace Services;

public class ApartmentService : IApartmentService
{
    private readonly IApartmentRepository _apartmentRepository;
    private readonly IResidentRepository _residentRepository;
    private readonly IApartmentResidentRepository _apartmentResidentRepository;

    public ApartmentService(
        IApartmentRepository apartmentRepository,
        IResidentRepository residentRepository,
        IApartmentResidentRepository apartmentResidentRepository)
    {
        _apartmentRepository = apartmentRepository;
        _residentRepository = residentRepository;
        _apartmentResidentRepository = apartmentResidentRepository;
    }

    public async Task<IReadOnlyList<Apartment>> GetApartmentsAsync()
    {
        return await _apartmentRepository.GetApartmentsAsync();
    }

    public Task<Apartment?> GetApartmentAsync(int apartmentId)
    {
        return _apartmentRepository.GetApartmentByIdAsync(apartmentId);
    }

    public Task<Apartment?> GetApartmentDetailAsync(int apartmentId)
    {
        return _apartmentRepository.GetApartmentDetailAsync(apartmentId);
    }

    public async Task<IReadOnlyList<Building>> GetBuildingsAsync()
    {
        return await _apartmentRepository.GetBuildingsAsync();
    }

    public async Task<IReadOnlyList<Resident>> GetResidentsAsync()
    {
        return await _residentRepository.GetResidentsAsync();
    }

    public async Task<OperationResult> CreateApartmentAsync(ApartmentFormRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ApartmentCode))
        {
            return OperationResult.Failure("Apartment code is required.");
        }

        if (request.Floor < 0)
        {
            return OperationResult.Failure("Floor cannot be negative.");
        }

        if (request.Area.HasValue && request.Area.Value < 0)
        {
            return OperationResult.Failure("Area must be positive.");
        }

        if (request.NumberOfRooms <= 0)
        {
            return OperationResult.Failure("Number of rooms must be at least 1.");
        }

        var building = await _apartmentRepository.GetBuildingByIdAsync(request.BuildingId);
        if (building is null)
        {
            return OperationResult.Failure("Building not found.");
        }

        var existing = await _apartmentRepository.GetApartmentByCodeAsync(request.ApartmentCode);
        if (existing is not null)
        {
            return OperationResult.Failure("Apartment code already exists.");
        }

        var apartment = new Apartment
        {
            ApartmentCode = request.ApartmentCode.Trim(),
            BuildingId = request.BuildingId,
            Floor = request.Floor,
            Area = request.Area,
            NumberOfRooms = request.NumberOfRooms,
            ApartmentType = string.IsNullOrWhiteSpace(request.ApartmentType) ? null : request.ApartmentType.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Available" : request.Status.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _apartmentRepository.CreateApartmentAsync(apartment);
        return OperationResult.Success();
    }

    public async Task<OperationResult> UpdateApartmentAsync(int apartmentId, ApartmentFormRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ApartmentCode))
        {
            return OperationResult.Failure("Apartment code is required.");
        }

        if (request.Floor < 0)
        {
            return OperationResult.Failure("Floor cannot be negative.");
        }

        if (request.Area.HasValue && request.Area.Value < 0)
        {
            return OperationResult.Failure("Area must be positive.");
        }

        if (request.NumberOfRooms <= 0)
        {
            return OperationResult.Failure("Number of rooms must be at least 1.");
        }

        var apartment = await _apartmentRepository.GetApartmentByIdAsync(apartmentId);
        if (apartment is null)
        {
            return OperationResult.Failure("Apartment not found.");
        }

        var building = await _apartmentRepository.GetBuildingByIdAsync(request.BuildingId);
        if (building is null)
        {
            return OperationResult.Failure("Building not found.");
        }

        var existing = await _apartmentRepository.GetApartmentByCodeAsync(request.ApartmentCode);
        if (existing is not null && existing.Id != apartmentId)
        {
            return OperationResult.Failure("Apartment code already exists.");
        }

        apartment.ApartmentCode = request.ApartmentCode.Trim();
        apartment.BuildingId = request.BuildingId;
        apartment.Floor = request.Floor;
        apartment.Area = request.Area;
        apartment.NumberOfRooms = request.NumberOfRooms;
        apartment.ApartmentType = string.IsNullOrWhiteSpace(request.ApartmentType) ? null : request.ApartmentType.Trim();
        apartment.Status = string.IsNullOrWhiteSpace(request.Status) ? "Available" : request.Status.Trim();
        apartment.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        apartment.UpdatedAt = DateTime.UtcNow;

        await _apartmentRepository.UpdateApartmentAsync(apartment);
        return OperationResult.Success();
    }

    public async Task<OperationResult> DeleteApartmentAsync(int apartmentId)
    {
        var apartment = await _apartmentRepository.GetApartmentDetailAsync(apartmentId);
        if (apartment is null)
        {
            return OperationResult.Failure("Apartment not found.");
        }

        if (apartment.ApartmentResidents.Count > 0)
        {
            return OperationResult.Failure("Cannot delete apartment with resident history.");
        }

        await _apartmentRepository.DeleteApartmentAsync(apartment);
        return OperationResult.Success();
    }

    public async Task<OperationResult> AssignResidentAsync(AssignResidentRequest request)
    {
        if (request.MoveOutDate.HasValue && request.MoveOutDate.Value < request.MoveInDate)
        {
            return OperationResult.Failure("Move-out date must be after move-in date.");
        }

        var apartment = await _apartmentRepository.GetApartmentByIdAsync(request.ApartmentId);
        if (apartment is null)
        {
            return OperationResult.Failure("Apartment not found.");
        }

        var resident = await _residentRepository.GetResidentByIdAsync(request.ResidentId);
        if (resident is null)
        {
            return OperationResult.Failure("Resident not found.");
        }

        var existingSameApartment = await _apartmentResidentRepository.GetActiveStayAsync(
            request.ApartmentId,
            request.ResidentId);
        if (existingSameApartment is not null)
        {
            return OperationResult.Failure("Resident is already active in this apartment.");
        }

        var existingActiveStay = await _apartmentResidentRepository.GetActiveStayAsync(request.ResidentId);
        if (existingActiveStay is not null)
        {
            return OperationResult.Failure("Resident is already active in another apartment.");
        }

        var stay = new ApartmentResident
        {
            ApartmentId = request.ApartmentId,
            ResidentId = request.ResidentId,
            IsOwner = request.IsOwner,
            MoveInDate = request.MoveInDate,
            MoveOutDate = request.MoveOutDate,
            IsActive = true,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _apartmentResidentRepository.CreateAsync(stay);
        return OperationResult.Success();
    }

    public async Task<OperationResult> EndResidentStayAsync(int apartmentResidentId, DateOnly? moveOutDate)
    {
        var stay = await _apartmentResidentRepository.GetByIdAsync(apartmentResidentId);
        if (stay is null)
        {
            return OperationResult.Failure("Resident stay not found.");
        }

        if (!stay.IsActive)
        {
            return OperationResult.Failure("Resident stay is already closed.");
        }

        var effectiveMoveOut = moveOutDate ?? DateOnly.FromDateTime(DateTime.Today);
        if (effectiveMoveOut < stay.MoveInDate)
        {
            return OperationResult.Failure("Move-out date must be after move-in date.");
        }

        stay.MoveOutDate = effectiveMoveOut;
        stay.IsActive = false;

        await _apartmentResidentRepository.UpdateAsync(stay);
        return OperationResult.Success();
    }
}
