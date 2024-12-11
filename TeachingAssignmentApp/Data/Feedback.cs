namespace TeachingAssignmentApp.Data
{
    public class Feedback
    {
        public Guid? Id { get; set; }
        public string? Code { get; set; }
        public string? TeacherCode { get; set; }
        public string? TeacherName { get; set; }
        public string? Content { get; set; }
        public int? StatusCode { get; set; }
        public string? StatusName { get; set; }
    }
}
