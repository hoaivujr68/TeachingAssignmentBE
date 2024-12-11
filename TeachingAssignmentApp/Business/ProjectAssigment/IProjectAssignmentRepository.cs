using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.ProjectAssigment
{
    public interface IProjectAssignmentRepository
    {
        Task<Pagination<Data.ProjectAssigment>> GetAllAsync(QueryModel queryModel, string role);
        Task AddAsync(Data.ProjectAssigment projectAssignment);
        Task AddRangeAsync(IEnumerable<Data.ProjectAssigment> projectAssignments);
        Task<Data.ProjectAssigment> UpdateAsync(Guid id, Data.ProjectAssigment updatedProjectAssigment);
        Task<double> GetTotalGdTeachingByTeacherCode(string teacherCode);
        Task<IEnumerable<TeacherModel>> GetAvailableTeachersForStudentId(string studentId);
        Task DeleteAsync(Guid id);
        Task<Pagination<AspirationModel>> GetProjectNotAssignmentAsync(QueryModel queryModel);
        Task<Pagination<TeacherModel>> GetTeacherNotAssignmentAsync(QueryModel queryModel);
        Task<byte[]> ExportProjectAssignment(string role);
        Task<byte[]> ExportAspirationAssignment(string role);
        Task<double?> GetRangeGdInstruct();
    }
}
