using TeachingAssignmentApp.Data;

namespace TeachingAssignmentApp.Model
{
    public class ProfessionalGroupModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public List<CourseModel>? ListCourse { get; set; }
    }
}
