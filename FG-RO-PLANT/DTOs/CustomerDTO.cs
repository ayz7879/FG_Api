using FG_RO_PLANT.Models;
using System.ComponentModel.DataAnnotations;

namespace FG_RO_PLANT.DTOs
{
    public class CustomerDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Max 100 characters allowed.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300, ErrorMessage = "Max 300 characters allowed.")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone Number must be 10 digits.")]
        public string? Phone { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Advance Pay must be a positive number.")]
        public decimal? AdvancePay { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Deposit Jars must be a positive number.")]
        public int? InitialDepositJar { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Deposit Capsules must be a positive number.")]
        public int? InitialDepositCapsule { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price per Jar must be a positive number.")]
        public decimal? PricePerJar { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price per Capsule must be a positive number.")]
        public decimal? PricePerCapsule { get; set; }

        [Required(ErrorMessage = "Customer Type is required.")]
        [Range(1, 2, ErrorMessage = "Customer Type must be either 1 (Regular) or 2 (Event).")]
        public CustomerType CustomerType { get; set; } = CustomerType.Regular;
        public bool IsActive { get; set; } = false;

        [Range(1, 31, ErrorMessage = "BillDay must be between 1 and 31.")]
        public int BillDay { get; set; } = 1;
    }

    public class CustomerLoginDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class DueCustomerDto
    {
        public Customer Customer { get; set; } = default!;
        public int DueAmount { get; set; }
    }

    public class TodayDueCustomerResponse
    {
        public List<DueCustomerDto> Data { get; set; } = [];
        public int TotalDueCustomer { get; set; }
        public int TotalDueAmount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

}
