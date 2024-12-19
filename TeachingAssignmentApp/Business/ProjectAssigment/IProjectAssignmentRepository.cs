using TeachingAssignmentApp.Business.TeachingAssignment.Model;
using TeachingAssignmentApp.Data;
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
        Task<Pagination<ProjectAssignmentInput>> GetProjectNotAssignmentAsync(QueryModel queryModel);
        Task<Pagination<TeacherModel>> GetTeacherNotAssignmentAsync(QueryModel queryModel);
        Task<byte[]> ExportProjectAssignment(string role);
        Task<byte[]> ExportAspirationAssignment(string role);
        Task<double?> GetRangeGdInstruct();
        Task<IEnumerable<ResultModel>> GetResultAsync();
        Task AddRangeAsync(IEnumerable<ProjectAssignmentInput> projectAssignments);
        Task<Data.ProjectAssignmentInput> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<TeacherResultError>> GetMaxAsync();
        Task SwapTeacherAssignmentAsync(Guid teacherAssignmentId1, Guid teacherAssignmentId2);
        Task<byte[]> ExportProjectAssignmentByQuota();

    }
}
