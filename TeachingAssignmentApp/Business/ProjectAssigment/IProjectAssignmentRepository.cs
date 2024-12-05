using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.ProjectAssigment
{
    public interface IProjectAssignmentRepository
    {
        Task<Pagination<Data.ProjectAssigment>> GetAllAsync(QueryModel queryModel, string role);
        Task AddAsync(Data.ProjectAssigment projectAssignment);
        Task AddRangeAsync(IEnumerable<Data.ProjectAssigment> projectAssignments);
        Task UpdateAsync(Data.ProjectAssigment projectAssignment);
        Task DeleteAsync(Guid id);
        Task<Pagination<AspirationModel>> GetProjectNotAssignmentAsync(QueryModel queryModel);
        Task<Pagination<TeacherModel>> GetTeacherNotAssignmentAsync(QueryModel queryModel);
        Task<byte[]> ExportProjectAssignment(string role);
    }
}
