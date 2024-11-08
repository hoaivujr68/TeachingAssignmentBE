using TeachingAssignmentApp.Data;

namespace TeachingAssignmentApp.Repositories
{
    public interface ITeacherProfessionalGroupRepository
    {
        Task<IEnumerable<TeacherProfessionalGroup>> GetAllAsync();
        Task<IEnumerable<TeacherProfessionalGroup>> GetByTeacherIdAsync(Guid teacherId);
        Task<IEnumerable<TeacherProfessionalGroup>> GetByProfessionalGroupIdAsync(Guid professionalGroupId);
        Task AddAsync(TeacherProfessionalGroup teacherProfessionalGroup);
        Task AddRangeAsync(IEnumerable<TeacherProfessionalGroup> teacherProfessionalGroups);
        Task DeleteAsync(Guid teacherId, Guid professionalGroupId);
    }
}
