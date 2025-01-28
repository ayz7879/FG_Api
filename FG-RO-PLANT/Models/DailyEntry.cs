using System.Text.Json.Serialization;

namespace FG_RO_PLANT.Models
{
    public class DailyEntry
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int? JarGiven { get; set; }
        public int? JarTaken { get; set; }
        public int? CapsuleGiven { get; set; }
        public int? CapsuleTaken { get; set; }
        public decimal? CustomerPay { get; set; }
        public DateOnly? DateField { get; set; }

        [JsonIgnore]
        public Customer? Customer { get; set; }
    }

}
