using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.ProfessionalGroup
{
    public interface IProfessionalGroupService
    {
        Task<Pagination<ProfessionalGroupModel>> GetAllAsync(QueryModel queryModel);
        Task<ProfessionalGroupModel> GetByIdAsync(Guid id);
        Task AddAsync(ProfessionalGroupModel professionalGroup);
        Task UpdateAsync(ProfessionalGroupModel professionalGroup);
        Task DeleteAsync(Guid id);
        Task<bool> ImportProfessionalGroupsAsync(IFormFile file);
    }
}
