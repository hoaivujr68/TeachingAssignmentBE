namespace TeachingAssignmentApp.Model
{
    public class AspirationModel
    {
        public Guid Id { get; set; }
        public string? TeacherCode { get; set; }
        public string? TeacherName { get; set; }
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? Topic { get; set; }
        public string? ClassName { get; set; }
        public string? GroupName { get; set; }
        public string? Status { get; set; }
        public string? DesireAccept { get; set; }
        public string? Aspiration1 { get; set; }
        public string? Aspiration2 { get; set; }
        public string? Aspiration3 { get; set; }
        public int? StatusCode { get; set; }
    }
}
