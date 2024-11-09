using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Repositories
{
    public interface ITeacherRepository
    {
        Task<Pagination<TeacherModel>> GetAllAsync(TeacherQueryModel queryModel);
        Task<Teacher> GetByIdAsync(Guid id);
        Task<Teacher> GetByNameAsync(string name);
        Task AddAsync(Teacher teacher);
        Task UpdateAsync(Teacher teacher);
        Task DeleteAsync(Guid id);
        Task AddRangeAsync(IEnumerable<Teacher> teachers);
    }
}
