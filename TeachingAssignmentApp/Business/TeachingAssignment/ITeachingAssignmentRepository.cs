using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.TeachingAssignment
{
    public interface ITeachingAssignmentRepository
    {
        Task<Pagination<Data.TeachingAssignment>> GetAllAsync(QueryModel queryModel, string role);
        Task AddAsync(Data.TeachingAssignment teachingAssignment);
        Task AddRangeAsync(IEnumerable<Data.TeachingAssignment> teachingAssignments);
        Task<Data.TeachingAssignment> UpdateAsync(Guid id, Data.TeachingAssignment updatedTeachingAssignment);
        Task<IEnumerable<TeacherModel>> GetAvailableTeachersForClass(string classId);
        Task<double> GetTotalGdTeachingByTeacherCode(string teacherCode);
        Task DeleteAsync(Guid id);
        Task<Pagination<ClassModel>> GetClassNotAssignmentAsync(QueryModel queryModel);
        Task<Pagination<TeacherModel>> GetTeacherNotAssignmentAsync(QueryModel queryModel);
        Task<byte[]> ExportTeachingAssignment(string role);
        Task<byte[]> ExportClassAssignment(string role);
        Task<double?> GetRangeGdTeaching();
    }
}
