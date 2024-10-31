using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssignmentApp.Data
{
    [Table("ProfessionalGroup")]
    public class ProfessionalGroup
    {
        [Key]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public List<Course>? ListCourse { get; set; }
        public Guid? TeacherId { get; set; }
    }
}
