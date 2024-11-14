using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Project
{
    public interface IProjectService
    {
        Task<Pagination<ProjectModel>> GetAllAsync(ProjectQueryModel queryModel);
        Task<ProjectModel> GetByIdAsync(Guid id);
        Task<Data.Project> GetByNameAsync(string name);
        Task AddAsync(ProjectModel project);
        Task UpdateAsync(ProjectModel project);
        Task<double> GetTotalGdInstruct();
        Task DeleteAsync(Guid id);
        Task<bool> ImportProjectsAsync(IFormFile file);
    }
}
