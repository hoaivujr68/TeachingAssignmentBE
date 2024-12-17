using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.CuckooAssignment
{
    public interface ICuckooTeachingRepository
    {
        Task<Pagination<Data.CuckooTeachingAssignment>> GetAllAsync(QueryModel queryModel, string role);
        Task AddAsync(Data.CuckooTeachingAssignment teachingAssignment);
        Task AddRangeAsync(IEnumerable<Data.CuckooTeachingAssignment> teachingAssignments);
        Task<Data.CuckooTeachingAssignment> UpdateAsync(Guid id, Data.CuckooTeachingAssignment updatedTeachingAssignment);
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
