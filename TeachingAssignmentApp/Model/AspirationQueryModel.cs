using System.ComponentModel.DataAnnotations;

namespace TeachingAssignmentApp.Model
{
    public class AspirationQueryModel
    {
        [Range(1, int.MaxValue)]
        public int? CurrentPage { get; set; } = 1;


        [Range(1, int.MaxValue)]
        public int? PageSize { get; set; } = 20;
    }
}
