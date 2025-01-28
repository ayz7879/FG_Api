namespace FG_RO_PLANT.Models
{
    public class History
    {
        public int Id { get; set; }
        public int DailyEntryId { get; set; }
        public DateOnly? DateField { get; set; }

        public DailyEntry? DailyEntry { get; set; }
    }

}
