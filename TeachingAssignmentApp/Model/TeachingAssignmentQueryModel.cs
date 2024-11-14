using System.ComponentModel.DataAnnotations;

namespace TeachingAssignmentApp.Model
{
    public class TeachingAssignmentQueryModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        [Range(1, int.MaxValue)]
        public int? CurrentPage { get; set; } = 1;


        [Range(1, int.MaxValue)]
        public int? PageSize { get; set; } = 20;
    }
}
