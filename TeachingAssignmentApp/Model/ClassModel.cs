namespace TeachingAssignmentApp.Model
{
    public class ClassModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string CourseName { get; set; }
        public string GroupName { get; set; }
        public int MaxEnrol { get; set; }
        public double GdTeaching { get; set; }
        public string TimeTable { get; set; }
        public List<TimeTableModel> TimeTableDetail { get; set; }
    }
}
