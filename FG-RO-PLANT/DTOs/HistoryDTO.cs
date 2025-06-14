using System.ComponentModel.DataAnnotations;
using FG_RO_PLANT.Models;

namespace FG_RO_PLANT.DTOs
{
    public class HistoryDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "PageSize must be greater than 0.")]
        public int PageSize { get; set; } = 10;

        [Range(0, int.MaxValue, ErrorMessage = "LastFetchId must be greater than or equal to 0.")]
        public int LastFetchId { get; set; } = 0;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }

    public class HistoryWithDetails
    {
        public int HistoryId { get; set; }
        public DateOnly? DateField { get; set; }
        public int DailyEntryId { get; set; }
        public DailyEntry? DailyEntryData { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerPhone { get; set; }
    }
}
