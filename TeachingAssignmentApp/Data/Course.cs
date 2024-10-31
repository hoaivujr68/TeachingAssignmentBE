using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssignmentApp.Data
{
    [Table("Course")]
    public class Course
    {
        [Key]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid? ProfessionalGroupId { get; set; }
    }
}
