using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Course
{
    public interface ICourseRepository
    {
        Task<Pagination<Data.Course>> GetAllAsync(CourseQueryModel queryModel);
        Task<Data.Course> GetByIdAsync(Guid id);
        Task AddAsync(Data.Course course);
        Task UpdateAsync(Data.Course course);
        Task DeleteAsync(Guid id);
        Task AddRangeAsync(IEnumerable<Data.Course> courses);
    }
}
