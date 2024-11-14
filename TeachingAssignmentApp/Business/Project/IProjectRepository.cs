using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Project
{
    public interface IProjectRepository
    {
        Task<Pagination<ProjectModel>> GetAllAsync(ProjectQueryModel queryModel);
        Task<Data.Project> GetByIdAsync(Guid id);
        Task<Data.Project> GetByNameAsync(string name);
        Task AddAsync(Data.Project project);
        Task UpdateAsync(Data.Project project);
        Task DeleteAsync(Guid id);
        Task AddRangeAsync(IEnumerable<Data.Project> projects);
    }
}
