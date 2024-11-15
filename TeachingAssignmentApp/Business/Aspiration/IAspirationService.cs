using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Aspiration
{
    public interface IAspirationService
    {
        Task<Pagination<AspirationModel>> GetAllAsync(QueryModel queryModel);
        Task<AspirationModel> GetByIdAsync(Guid id);
        Task AddAsync(AspirationModel aspiration);
        Task UpdateAsync(AspirationModel aspiration);
        Task DeleteAsync(Guid id);
        Task<bool> ImportAspirationsAsync(IFormFile file);
    }
}
