using System.ComponentModel.DataAnnotations;

namespace TeachingAssignmentApp.Model
{
    public class CourseQueryModel
    {
        public string Name { get; set; }
        [Range(1, int.MaxValue)]
        public int? CurrentPage { get; set; } = 1;


        [Range(1, int.MaxValue)]
        public int? PageSize { get; set; } = 20;
    }
}
