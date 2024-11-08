using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Services
{
    public interface ICourseService
    {
        Task<Pagination<Course>> GetAllAsync(CourseQueryModel queryModel);
        Task<CourseModel> GetByIdAsync(Guid id);
        Task AddAsync(CourseModel course);
        Task UpdateAsync(CourseModel course);
        Task DeleteAsync(Guid id);
    }
}
