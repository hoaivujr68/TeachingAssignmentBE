using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Teacher
{
    public interface ITeacherRepository
    {
        Task<Pagination<TeacherModel>> GetAllAsync(QueryModel queryModel);
        Task<Data.Teacher> GetByIdAsync(Guid id);
        Task<Data.Teacher> GetByCodeAsync(string code);
        Task<Data.Teacher> GetByNameAsync(string name);
        Task AddAsync(Data.Teacher teacher);
        Task UpdateAsync(Data.Teacher teacher);
        Task DeleteAsync(Guid id);
        Task AddRangeAsync(IEnumerable<Data.Teacher> teachers);
    }
}
