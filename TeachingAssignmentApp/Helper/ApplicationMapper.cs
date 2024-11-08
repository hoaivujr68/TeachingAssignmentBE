using AutoMapper;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Helper
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper() {
            CreateMap<Teacher, TeacherModel>().ReverseMap();
            CreateMap<Course, CourseModel>().ReverseMap();
            CreateMap<ProfessionalGroup, ProfessionalGroupModel>().ReverseMap();
        }
    }
}
