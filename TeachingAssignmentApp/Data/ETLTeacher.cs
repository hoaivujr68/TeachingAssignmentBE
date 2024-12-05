using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssignmentApp.Data
{
    [Table("ETLTeacher")]
    public class ETLTeacher
    {
        [Key]
        public Guid Id { get; set; }
        public string Label { get; set; }
        public double Value { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Role { get; set; }
    }
}
