using System.ComponentModel.DataAnnotations;

namespace TeachingAssignmentApp.Model
{
    public class TimeTableModel
    {
        [Key]
        public Guid Id { get; set; }
        public string Day { get; set; }
        public string Seasion { get; set; }
        public string ClassPeriod { get; set; }
        public string Room { get; set; }
        public string Week { get; set; }
    }
}
