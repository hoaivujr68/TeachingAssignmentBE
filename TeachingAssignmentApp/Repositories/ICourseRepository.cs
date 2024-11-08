using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Repositories
{
    public interface ICourseRepository
    {
        Task<Pagination<Course>> GetAllAsync(CourseQueryModel queryModel);
        Task<Course> GetByIdAsync(Guid id);
        Task AddAsync(Course course);
        Task UpdateAsync(Course course);
        Task DeleteAsync(Guid id);
        Task AddRangeAsync(IEnumerable<Course> courses);
    }
}
