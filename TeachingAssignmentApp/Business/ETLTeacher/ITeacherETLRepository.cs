using TeachingAssignmentApp.Business.ETLGeneral.Model;
using TeachingAssignmentApp.Business.ETLTeacher.Model;

namespace TeachingAssignmentApp.Business.ETLTeacher
{
    public interface ITeacherETLRepository
    {
        Task<IEnumerable<ETLGenteralResponse>> GetAllAync(ETLTeacherQueryModel eTLTeacherQueryModel);
        Task<Data.Teacher> ListAllTeacherAsync(string role);
        Task<IEnumerable<Data.TeachingAssignment>> ListAllGDRealAsync(string role);
        Task<IEnumerable<Data.Project>> ListAllProjectAsync();
        Task<IEnumerable<Data.ProjectAssigment>> ListAllGDProjectRealAsync(string role);
        Task DeleteByTypeAsync(List<string> types, string role);
        Task<IEnumerable<Data.ETLTeacher>> SaveAsync(IEnumerable<Data.ETLTeacher> generals, string role);
    }
}
