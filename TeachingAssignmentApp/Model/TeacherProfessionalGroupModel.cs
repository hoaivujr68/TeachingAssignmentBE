using TeachingAssignmentApp.Data;

namespace TeachingAssignmentApp.Model
{
    public class TeacherProfessionalGroupModel
    {
        public Guid Id { get; set; }
        public ProfessionalGroupModel? ProfessionalGroups { get; set; }
    }
}
