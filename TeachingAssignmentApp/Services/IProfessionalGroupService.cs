using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Services
{
    public interface IProfessionalGroupService
    {
        Task<Pagination<ProfessionalGroup>> GetAllAsync(ProfessionalGroupQueryModel queryModel);
        Task<ProfessionalGroupModel> GetByIdAsync(Guid id);
        Task AddAsync(ProfessionalGroupModel professionalGroup);
        Task UpdateAsync(ProfessionalGroupModel professionalGroup);
        Task DeleteAsync(Guid id);
        Task<bool> ImportProfessionalGroupsAsync(IFormFile file);
    }
}
