using System.ComponentModel.DataAnnotations;

namespace FG_RO_PLANT.DTOs
{
    public class DailyEntryDTO
    {
        [Required(ErrorMessage = "Customer ID is required.")]
        public int CustomerId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Jar Given must be a non-negative value.")]
        public int? JarGiven { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Jar Taken must be a non-negative value.")]
        public int? JarTaken { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Capsule Given must be a non-negative value.")]
        public int? CapsuleGiven { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Capsule Taken must be a non-negative value.")]
        public int? CapsuleTaken { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Customer Pay must be a non-negative value.")]
        public decimal? CustomerPay { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateOnly? DateField { get; set; }
    }

    public class DailyEntriesQuery
    {
        [Range(1, int.MaxValue, ErrorMessage = "PageSize must be greater than 0.")]
        public int PageSize { get; set; } = 10;

        [Range(0, int.MaxValue, ErrorMessage = "LastFetchId must be greater than or equal to 0.")]
        public int LastFetchId { get; set; } = 0;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }

}
