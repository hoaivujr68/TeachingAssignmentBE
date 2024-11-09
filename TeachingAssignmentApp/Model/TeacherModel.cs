using TeachingAssignmentApp.Data;

namespace TeachingAssignmentApp.Model
{
    public class TeacherModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public List<ProfessionalGroupModel>? ProfessionalGroup { get; set; }
        ////public List<Course>? ListCourse { get; set; }
        public double? GdTeaching { get; set; }
        public double? GdInstruct { get; set; }
    }
}
