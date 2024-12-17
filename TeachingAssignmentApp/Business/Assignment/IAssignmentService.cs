using TeachingAssignmentApp.Business.Assignment.Model;
using TeachingAssignmentApp.Data;

namespace TeachingAssignmentApp.Business.Assignment
{
    public interface IAssignmentService
    {
        Task<List<TeacherInputModel>> GetAllTeacherInfo();
        Task<List<ClassInputModel>> GetAllClassInfo();
        Task<List<ProjectAssignmentInput>> GetAllAspirationInfo();
        Task<SolutionModel> TeachingAssignment();
        Task<SolutionProjectModel> ProjectAssignment();
    }
}
