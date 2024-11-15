using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Teacher
{
    public interface ITeacherService
    {
        Task<Pagination<TeacherModel>> GetAllAsync(QueryModel queryModel);
        Task<TeacherModel> GetByIdAsync(Guid id);
        Task<Data.Teacher> GetByNameAsync(string name);
        Task AddAsync(TeacherModel teacher);
        Task UpdateAsync(TeacherModel teacher);
        Task DeleteAsync(Guid id);
        Task<bool> ImportTeachersAsync(IFormFile file);
    }
}
