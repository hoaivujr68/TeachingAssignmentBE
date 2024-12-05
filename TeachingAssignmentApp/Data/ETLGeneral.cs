using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssignmentApp.Data
{
    [Table("ETLGeneral")]
    public class ETLGeneral
    {
        [Key]
        public Guid Id { get; set; }
        public string Label { get; set; }
        public int Value { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
    }
}
