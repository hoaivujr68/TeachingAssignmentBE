using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Data
{
    [Table("CuckooTeachingAssignment")]
    public class CuckooTeachingAssignment
    {
        [Key]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Type { get; set; }
        public string? CourseName { get; set; }
        public string? GroupName { get; set; }
        public int? MaxEnrol { get; set; }
        public string? TimeTable { get; set; }
        public double? GdTeaching { get; set; }
        public List<TimeTableModel>? TimeTableDetail { get; set; }
        public string? TeacherCode { get; set; }
        public string? TeachingName { get; set; }
    }
}
