using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.ProfessionalGroup
{
    public interface IProfessionalGroupRepository
    {
        Task<Pagination<ProfessionalGroupModel>> GetAllAsync(QueryModel queryModel);
        Task<Data.ProfessionalGroup> GetByIdAsync(Guid id);
        Task<Data.ProfessionalGroup> GetByNameAsync(string professionalGroupName);
        Task AddAsync(Data.ProfessionalGroup professionalGroup);
        Task UpdateAsync(Data.ProfessionalGroup professionalGroup);
        Task DeleteAsync(Guid id);
        Task AddRangeAsync(IEnumerable<Data.ProfessionalGroup> professionalGroups);
    }
}
