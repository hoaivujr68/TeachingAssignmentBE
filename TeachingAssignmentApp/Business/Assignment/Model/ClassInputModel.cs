using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Assignment.Model
{
    public class ClassInputModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string CourseName { get; set; }
        public string GroupName { get; set; }
        public int MaxEnrol { get; set; }
        public double GdTeaching { get; set; }
        public List<TimeTableModel> TimeTableDetail { get; set; }
    }
}
