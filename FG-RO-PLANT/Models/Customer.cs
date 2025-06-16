namespace FG_RO_PLANT.Models
{
    public enum CustomerType
    {
        Regular = 1,
        Event = 2
    }
    public class Customer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public decimal? AdvancePay { get; set; }
        public int? InitialDepositJar { get; set; }
        public int? InitialDepositCapsule { get; set; }
        public decimal? PricePerJar { get; set; }
        public decimal? PricePerCapsule { get; set; }
        public CustomerType CustomerType { get; set; } = CustomerType.Regular;
        public bool IsActive { get; set; } = false;
        public string? Token { get; set; }
    }

    public class CustomerPayment
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public decimal? PricePerJar { get; set; }
        public decimal? PricePerCapsule { get; set; }
        public int? DueAmount { get; set; }
    }

    public class DueCustomerResponse
    {
        public List<CustomerPayment> Data { get; set; } = [];
        public int TotalDueAmount { get; set; }
        public int TotalDueCustomer { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

}
