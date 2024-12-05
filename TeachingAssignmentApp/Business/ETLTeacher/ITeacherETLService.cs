using TeachingAssignmentApp.Business.ETLGeneral.Model;
using TeachingAssignmentApp.Business.ETLTeacher.Model;

namespace TeachingAssignmentApp.Business.ETLTeacher
{
    public interface ITeacherETLService
    {
        Task<IEnumerable<ETLGenteralResponse>> GetAllAync(ETLTeacherQueryModel eTLTeacherQueryModel);
        Task<IEnumerable<Data.ETLTeacher>> RefreshAsync(string role);
        Task CreateAll();
    }
}
