using BusinessObjects.Models;

namespace Services;

public interface IApartmentService
{
    Task<IReadOnlyList<Apartment>> GetApartmentsAsync();
    Task<Apartment?> GetApartmentAsync(int apartmentId);
    Task<Apartment?> GetApartmentDetailAsync(int apartmentId);
    Task<IReadOnlyList<Building>> GetBuildingsAsync();
    Task<IReadOnlyList<Resident>> GetResidentsAsync();
    Task<OperationResult> CreateApartmentAsync(ApartmentFormRequest request);
    Task<OperationResult> UpdateApartmentAsync(int apartmentId, ApartmentFormRequest request);
    Task<OperationResult> DeleteApartmentAsync(int apartmentId);
    Task<OperationResult> AssignResidentAsync(AssignResidentRequest request);
    Task<OperationResult> EndResidentStayAsync(int apartmentResidentId, DateOnly? moveOutDate);
}
