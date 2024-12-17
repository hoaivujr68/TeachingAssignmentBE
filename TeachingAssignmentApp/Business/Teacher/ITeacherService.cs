using Microsoft.AspNetCore.Mvc;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Teacher
{
    public interface ITeacherService
    {
        Task<Pagination<TeacherModel>> GetAllAsync(QueryModel queryModel, string? role = "lanhdao");
        Task<TeacherModel> GetByIdAsync(Guid id);
        Task<Data.Teacher> GetByNameAsync(string name);
        Task AddAsync(TeacherModel teacher);
        Task UpdateAsync(TeacherModel teacher);
        Task DeleteAsync(Guid id);
        Task<bool> ImportTeachersAsync(IFormFile file);
        FileContentResult DownloadTeacherTemplate();
        Task<bool> ImportTeachersAfterAsync(IFormFile file);
    }
}
