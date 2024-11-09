using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Repositories
{
    public interface IProfessionalGroupRepository
    {
        Task<Pagination<ProfessionalGroupModel>> GetAllAsync(ProfessionalGroupQueryModel queryModel);
        Task<ProfessionalGroup> GetByIdAsync(Guid id);
        Task AddAsync(ProfessionalGroup professionalGroup);
        Task UpdateAsync(ProfessionalGroup professionalGroup);
        Task DeleteAsync(Guid id);
        Task AddRangeAsync(IEnumerable<ProfessionalGroup> professionalGroups);
    }
}
