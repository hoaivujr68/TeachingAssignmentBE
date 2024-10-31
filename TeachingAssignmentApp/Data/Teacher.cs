using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssignmentApp.Data
{
    [Table("Teacher")]
    public class Teacher
    {
        [Key]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public List<ProfessionalGroup>? ListProfessional { get; set; }
        public double? GdTeaching { get; set; }
        public double? GdInstruct { get; set; }
    }
}
