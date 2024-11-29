using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeachingAssignmentApp.Model
{
    public class TimeTableModel
    {
        [Key]
        public Guid Id { get; set; }
        public string Day { get; set; }
        public string Seasion { get; set; }
        public string ClassPeriod { get; set; }
        public string Room { get; set; }
        public string Week { get; set; }
        [NotMapped]
        public int[] Period
        {
            get => string.IsNullOrEmpty(PeriodJson)
                ? Array.Empty<int>()
                : JsonConvert.DeserializeObject<int[]>(PeriodJson);
            set => PeriodJson = JsonConvert.SerializeObject(value);
        }
        public string PeriodJson { get; set; }
    }
}
