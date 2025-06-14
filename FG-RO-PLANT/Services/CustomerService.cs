using FG_RO_PLANT.Data;
using FG_RO_PLANT.DTOs;
using FG_RO_PLANT.Models;
using Microsoft.EntityFrameworkCore;

namespace FG_RO_PLANT.Services
{
    public class CustomerService(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;

        // Add Customer
        public async Task<string> AddCustomerAsync(CustomerDTO customerDto)
        {
            if (await _context.Customers.AnyAsync(c => c.Phone == customerDto.Phone))
                throw new Exception("Customer already exists");

            var customer = new Customer
            {
                Name = customerDto.Name,
                Address = customerDto.Address,
                Phone = customerDto.Phone,
                AdvancePay = customerDto.AdvancePay,
                InitialDepositJar = customerDto.InitialDepositJar,
                InitialDepositCapsule = customerDto.InitialDepositCapsule,
                PricePerJar = customerDto.PricePerJar,
                PricePerCapsule = customerDto.PricePerCapsule,
                CustomerType = customerDto.CustomerType
            };

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            return "Customer added successfully";
        }

        // Get Customer by ID
        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id) ?? throw new Exception("Customer not found");
            return customer;
        }

        // Get All Customers with Pagination
        public async Task<List<Customer>> GetAllCustomersAsync(int pageSize, int lastFetchId, int customerType)
        {
            var query = _context.Customers.AsNoTracking().Where(c => c.Id > lastFetchId);

            if (customerType > 0)
            {
                query = query.Where(c => (int)c.CustomerType == customerType);
            }

            return await query
                .OrderBy(c => c.Id)
                .Take(pageSize)
                .ToListAsync();
        }

        // Total count customer
        public async Task<Dictionary<int, int>> GetTotalCustomerCountAsync()
        {
            var result = new Dictionary<int, int>
            {
                { 0, await _context.Customers.AsNoTracking().CountAsync() },
                { 1, await _context.Customers.AsNoTracking().Where(c => (int)c.CustomerType == 1).CountAsync() },
                { 2, await _context.Customers.AsNoTracking().Where(c => (int)c.CustomerType == 2).CountAsync() }
            };

            return result;
        }

        // Search Customers
        public async Task<List<Customer>> SearchCustomersAsync(string searchTerm, int pageSize, int lastFetchId)
        {
            var query = _context.Customers.AsNoTracking().Where(c => c.Id > lastFetchId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(searchTerm) ||
                                         c.Address.ToLower().Contains(searchTerm) ||
                                         c.Phone.Contains(searchTerm));
            }

            return await query.OrderBy(c => c.Id)
                               .Take(pageSize)
                               .ToListAsync();
        }

        // Update Customer
        public async Task<string> UpdateCustomerAsync(int id, CustomerDTO updatedCustomer)
        {
            var existingCustomer = await _context.Customers.FindAsync(id) ?? throw new Exception("Customer not found");

            // Check if phone number is being changed
            if (existingCustomer.Phone != updatedCustomer.Phone)
            {
                bool phoneExists = await _context.Customers
                    .AsNoTracking().AnyAsync(c => c.Phone == updatedCustomer.Phone && c.Id != id);

                if (phoneExists)
                {
                    throw new Exception("Phone Number already exists !");
                }
            }

            existingCustomer.Name = updatedCustomer.Name;
            existingCustomer.Address = updatedCustomer.Address;
            existingCustomer.Phone = updatedCustomer.Phone;
            existingCustomer.AdvancePay = updatedCustomer.AdvancePay;
            existingCustomer.InitialDepositJar = updatedCustomer.InitialDepositJar;
            existingCustomer.InitialDepositCapsule = updatedCustomer.InitialDepositCapsule;
            existingCustomer.PricePerJar = updatedCustomer.PricePerJar;
            existingCustomer.PricePerCapsule = updatedCustomer.PricePerCapsule;
            existingCustomer.CustomerType = updatedCustomer.CustomerType;

            _context.Entry(existingCustomer).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return "Customer updated successfully";
        }

        // Delete Customer
        public async Task<string> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id) ?? throw new Exception("Customer not found");
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return "Customer deleted successfully";
        }

        // Get all due amount customer
        public async Task<DueCustomerResponse> GetDueCustomersAsync(int page, int pageSize, string? search)
        {
            var loweredSearch = search?.ToLower();
            // First get aggregated data using raw SQL or simpler LINQ
            var customerDueData = await (
                from c in _context.Customers.AsNoTracking()
                join e in _context.DailyEntries.AsNoTracking() on c.Id equals e.CustomerId
                where string.IsNullOrEmpty(loweredSearch) ||
                      c.Name.ToLower().Contains(loweredSearch) ||
                      c.Phone.ToLower().Contains(loweredSearch) ||
                      c.Address.ToLower().Contains(loweredSearch)
                group new { e.JarGiven, e.CapsuleGiven, e.CustomerPay, c.PricePerJar, c.PricePerCapsule }
                by new { c.Id, c.Name, c.Address, c.Phone, c.PricePerJar, c.PricePerCapsule } into g
                select new
                {
                    Customer = g.Key,
                    TotalJar = g.Sum(x => x.JarGiven),
                    TotalCapsule = g.Sum(x => x.CapsuleGiven),
                    TotalPaid = g.Sum(x => x.CustomerPay)
                }).ToListAsync();

            // Calculate due amounts in memory (small dataset after grouping)
            var dueCustomers = customerDueData
                .Select(x => new CustomerPayment
                {
                    Id = x.Customer.Id,
                    Name = x.Customer.Name,
                    Address = x.Customer.Address,
                    Phone = x.Customer.Phone,
                    PricePerJar = x.Customer.PricePerJar,
                    PricePerCapsule = x.Customer.PricePerCapsule,
                    DueAmount = (int)((x.TotalJar * x.Customer.PricePerJar) + (x.TotalCapsule * x.Customer.PricePerCapsule) - x.TotalPaid)
                })
                .Where(x => x.DueAmount > 0)
                .OrderByDescending(x => x.DueAmount)
                .ToList();

            // Apply pagination
            var pagedData = dueCustomers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new DueCustomerResponse
            {
                Data = pagedData,
                TotalDueCustomer = dueCustomers.Count,
                TotalDueAmount = dueCustomers.Sum(c => c.DueAmount ?? 0),
                CurrentPage = page,
                PageSize = pageSize
            };
        }

    }
}
