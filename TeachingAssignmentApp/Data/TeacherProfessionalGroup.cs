using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssignmentApp.Data
{
    [Table("TeacherProfessionalGroup")]
    public class TeacherProfessionalGroup
    {
        public Guid Id { get; set; }
        public Guid TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public Guid ProfessionalGroupId { get; set; }
        public ProfessionalGroup? ProfessionalGroup { get; set; }
    }
}
