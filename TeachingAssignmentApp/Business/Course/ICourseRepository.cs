using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Course
{
    public interface ICourseRepository
    {
        Task<Pagination<Data.Course>> GetAllAsync(QueryModel queryModel);
        Task<Data.Course> GetByIdAsync(Guid id);
        Task<Data.Course> GetByNameAsync(string name);
        Task<IEnumerable<Data.Course>> GetByTeacherIdAsync(Guid teacherId);
        Task AddAsync(Data.Course course);
        Task UpdateAsync(Data.Course course);
        Task DeleteAsync(Guid id);
        Task AddRangeAsync(IEnumerable<Data.Course> courses);
    }
}
