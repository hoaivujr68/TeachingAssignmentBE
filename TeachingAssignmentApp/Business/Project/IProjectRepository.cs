using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Project
{
    public interface IProjectRepository
    {
        Task<Pagination<ProjectModel>> GetAllAsync(QueryModel queryModel);
        Task<Data.Project> GetByIdAsync(Guid id);
        Task<Data.Project> GetByNameAsync(string name);
        Task<Data.Project> GetByCourseNameAsync(string courseName);
        Task<Data.Project> GetByStudentIdAsync(string studentId);
        Task AddAsync(Data.Project project);
        Task UpdateAsync(Data.Project project);
        Task DeleteAsync(Guid id);
        Task AddRangeAsync(IEnumerable<Data.Project> projects);
    }
}
