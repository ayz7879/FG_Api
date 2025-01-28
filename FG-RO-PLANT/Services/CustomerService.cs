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
        public async Task<int> GetTotalCustomerCountAsync(int customerType)
        {
            if (customerType > 0)
            {
                return await _context.Customers
                    .AsNoTracking()
                    .Where(c => (int)c.CustomerType == customerType)
                    .CountAsync();
            }
            else
            {
                return await _context.Customers.AsNoTracking().CountAsync();
            }
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
    }
}
