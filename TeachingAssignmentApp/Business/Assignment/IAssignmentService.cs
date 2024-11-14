using TeachingAssignmentApp.Business.Assignment.Model;

namespace TeachingAssignmentApp.Business.Assignment
{
    public interface IAssignmentService
    {
        Task<List<TeacherInputModel>> GetAllTeacherInfo();
        Task<List<ClassInputModel>> GetAllClassInfo();
        Task<SolutionModel> TeachingAssignment();
    }
}
