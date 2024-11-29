namespace TeachingAssignmentApp.Business.Assignment.Model
{
    public class TeacherInputModel
    {
        public string? Code { get; set; }
        public List<CourseInputModel>? ListCourse { get; set; }
        public double? GdTeaching { get; set; }
        public double? GdInstruct { get; set; }
        public List<TeacherScheduleModel> Schedule { get; set; } = new List<TeacherScheduleModel>();
    }
}
