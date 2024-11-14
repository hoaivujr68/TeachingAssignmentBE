using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Course
{
    public interface ICourseService
    {
        Task<Pagination<Data.Course>> GetAllAsync(CourseQueryModel queryModel);
        Task<CourseModel> GetByIdAsync(Guid id);
        Task AddAsync(CourseModel course);
        Task UpdateAsync(CourseModel course);
        Task DeleteAsync(Guid id);
    }
}
