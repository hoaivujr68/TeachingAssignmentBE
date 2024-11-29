namespace TeachingAssignmentApp.Business.Assignment.Model
{
    public class TeacherScheduleModel
    {
        public string ClassCode { get; set; } // Mã lớp
        public string ClassName { get; set; } // Tên lớp
        public string Subject { get; set; } // Môn học
        public List<int> Periods { get; set; } // Các tiết dạy
        public string Day { get; set; } // Ngày dạy
        public string Seasion { get; set; } // Buổi (Sáng/Chiều)
        public string Room { get; set; } // Phòng học
    }
}
