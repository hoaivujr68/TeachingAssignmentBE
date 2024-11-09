using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Services
{
    public interface ITeacherService
    {
        Task<Pagination<TeacherModel>> GetAllAsync(TeacherQueryModel queryModel);
        Task<TeacherModel> GetByIdAsync(Guid id);
        Task<Teacher> GetByNameAsync(string name);
        Task AddAsync(TeacherModel teacher);
        Task UpdateAsync(TeacherModel teacher);
        Task DeleteAsync(Guid id);
        Task<bool> ImportTeachersAsync(IFormFile file);
    }
}
