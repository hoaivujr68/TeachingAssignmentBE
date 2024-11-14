using TeachingAssignmentApp.Data;

namespace TeachingAssignmentApp.Business.TeacherProfessionalGroup
{
    public interface ITeacherProfessionalGroupRepository
    {
        Task<IEnumerable<Data.TeacherProfessionalGroup>> GetAllAsync();
        Task<IEnumerable<Data.TeacherProfessionalGroup>> GetByTeacherIdAsync(Guid teacherId);
        Task<IEnumerable<Data.TeacherProfessionalGroup>> GetByProfessionalGroupIdAsync(Guid professionalGroupId);
        Task AddAsync(Data.TeacherProfessionalGroup teacherProfessionalGroup);
        Task AddRangeAsync(IEnumerable<Data.TeacherProfessionalGroup> teacherProfessionalGroups);
        Task DeleteAsync(Guid teacherId, Guid professionalGroupId);
    }
}
