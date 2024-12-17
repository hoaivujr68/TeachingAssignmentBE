using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssignmentApp.Data
{
    [Table("Project")]
    public class Project
    {
        [Key]
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? CourseName { get; set; }
        public string? StudenId { get; set; }
        public string? StudentName { get; set; }
        public string? GroupName { get; set; }
        public double? GdInstruct { get; set; }
        public string? Topic { get; set; }

    }
}
