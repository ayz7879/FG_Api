using FG_RO_PLANT.Data;
using FG_RO_PLANT.DTOs;
using FG_RO_PLANT.Helpers;
using FG_RO_PLANT.Models;
using Microsoft.EntityFrameworkCore;

namespace FG_RO_PLANT.Services
{
    public class CustomerService(ApplicationDbContext context, JwtHelper jwtService)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly JwtHelper _jwtService = jwtService;

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
                CustomerType = customerDto.CustomerType,
                IsActive = customerDto.IsActive,
                Token = Guid.NewGuid().ToString(),
                BillDay = customerDto.BillDay,
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
        
        // Get Customer by token
        public async Task<Customer> GetCustomerByTokenAsync(string token)
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Token == token && c.IsActive) ?? throw new UnauthorizedAccessException("Access denied.");
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
            existingCustomer.IsActive = updatedCustomer.IsActive;
            existingCustomer.BillDay = updatedCustomer.BillDay;

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

            // Create base query with all calculations done at DB level
            var baseQuery = from c in _context.Customers.AsNoTracking()
                            join e in _context.DailyEntries.AsNoTracking() on c.Id equals e.CustomerId into entries
                            where string.IsNullOrEmpty(loweredSearch) ||
                                  c.Name.ToLower().Contains(loweredSearch) ||
                                  c.Phone.ToLower().Contains(loweredSearch) ||
                                  c.Address.ToLower().Contains(loweredSearch)
                            let totalJar = entries.Sum(x => x.JarGiven)
                            let totalCapsule = entries.Sum(x => x.CapsuleGiven)
                            let totalPaid = entries.Sum(x => x.CustomerPay)
                            let dueAmount = (totalJar * c.PricePerJar) + (totalCapsule * c.PricePerCapsule) - totalPaid
                            where dueAmount > 0
                            select new CustomerPayment
                            {
                                Id = c.Id,
                                Name = c.Name,
                                Address = c.Address,
                                Phone = c.Phone,
                                PricePerJar = c.PricePerJar,
                                PricePerCapsule = c.PricePerCapsule,
                                DueAmount = (int)dueAmount,
                                BillDay = c.BillDay
                            };

            // Get total count for pagination info (before applying skip/take)
            var totalCount = await baseQuery.CountAsync();

            // Get total due amount for all customers (before pagination)
            var totalDueAmount = await baseQuery.SumAsync(x => x.DueAmount ?? 0);

            // Apply sorting and pagination at DB level
            var pagedData = await baseQuery
                .OrderByDescending(x => x.DueAmount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new DueCustomerResponse
            {
                Data = pagedData,
                TotalDueCustomer = totalCount,
                TotalDueAmount = totalDueAmount,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        // Login Customer
        public async Task<(string, string)> CustomerLoginAsync(string phoneNumber)
        {
            var existingCustomer = await _context.Customers.FirstOrDefaultAsync(u => u.Phone == phoneNumber) ?? throw new UnauthorizedAccessException("Customer not found");

            if (!existingCustomer.IsActive)
                throw new UnauthorizedAccessException("Account inactive");

                var user = new User
                {
                    Id = existingCustomer.Id,
                    Name = existingCustomer.Name,
                    Role = UserRole.Customer,
                };

            var token = _jwtService.GenerateToken(user);
            return ("Login successful", token);
        }

        // Get Today due customer
        public async Task<TodayDueCustomerResponse> GetTodayDueCustomersAsync(int page, int pageSize, string? search)
        {
            var indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            int today = indiaTime.Day;
            int currentMonth = indiaTime.Month;
            var loweredSearch = search?.ToLower();

            // 🔁 Step 1: Auto reset IsBillDone = false if BillDoneDate is old + amount pending
            var resetList = await (
                from c in _context.Customers
                join e in _context.DailyEntries on c.Id equals e.CustomerId
                where c.BillDay == today &&
                      c.IsBillDone &&
                      (
                          !c.BillDoneDate.HasValue ||
                          c.BillDoneDate.Value.Month != currentMonth
                      )
                group new { e, c } by new { c.Id, c.PricePerJar, c.PricePerCapsule } into g
                select new
                {
                    CustomerId = g.Key.Id,
                    DueAmount = (int)(
                        g.Sum(x => x.e.JarGiven) * g.Key.PricePerJar +
                        g.Sum(x => x.e.CapsuleGiven) * g.Key.PricePerCapsule -
                        g.Sum(x => x.e.CustomerPay)
                    )
                }
            ).ToListAsync();

            var resetCustomerIds = resetList
                .Where(x => x.DueAmount > 0)
                .Select(x => x.CustomerId)
                .ToList();

            if (resetCustomerIds.Count != 0)
            {
                await _context.Customers
                    .Where(c => resetCustomerIds.Contains(c.Id))
                    .ExecuteUpdateAsync(set => set
                        .SetProperty(c => c.IsBillDone, false)
                        .SetProperty(c => c.BillDoneDate, (DateOnly?)null));
            }

            // ✅ Step 2: Create base query for due customers (DB LEVEL FIXED VERSION)
            var baseQuery = from grouped in (
                                from c in _context.Customers.AsNoTracking()
                                join e in _context.DailyEntries.AsNoTracking() on c.Id equals e.CustomerId
                                where !c.IsBillDone &&
                                      (string.IsNullOrEmpty(loweredSearch) ||
                                       EF.Functions.Like(c.Name.ToLower(), $"%{loweredSearch}%") ||
                                       EF.Functions.Like(c.Phone.ToLower(), $"%{loweredSearch}%") ||
                                       EF.Functions.Like(c.Address.ToLower(), $"%{loweredSearch}%"))
                                group e by new { c.Id, c.Name, c.Phone, c.Address, c.PricePerJar, c.PricePerCapsule, c.BillDay } into g
                                select new
                                {
                                    CustomerId = g.Key.Id,
                                    CustomerName = g.Key.Name,
                                    CustomerPhone = g.Key.Phone,
                                    CustomerAddress = g.Key.Address,
                                    PricePerJar = g.Key.PricePerJar,
                                    PricePerCapsule = g.Key.PricePerCapsule,
                                    BillDay = g.Key.BillDay,
                                    DueAmount = (int)(g.Sum(x => x.JarGiven) * g.Key.PricePerJar +
                                                     g.Sum(x => x.CapsuleGiven) * g.Key.PricePerCapsule -
                                                     g.Sum(x => x.CustomerPay))
                                }
                            )
                            where grouped.DueAmount > 0
                            select new DueCustomerDto
                            {
                                Customer = new Customer
                                {
                                    Id = grouped.CustomerId,
                                    Name = grouped.CustomerName,
                                    Phone = grouped.CustomerPhone,
                                    Address = grouped.CustomerAddress,
                                    PricePerJar = grouped.PricePerJar,
                                    PricePerCapsule = grouped.PricePerCapsule,
                                    BillDay = grouped.BillDay
                                },
                                DueAmount = grouped.DueAmount
                            };

            // Get TODAY's due customers count and amount only
            var todayDueCustomers = await baseQuery
                .Where(x => x.Customer.BillDay == today)
                .Select(x => new { x.DueAmount })
                .ToListAsync();

            var todayDueCustomerCount = todayDueCustomers.Count;
            var todayTotalDueAmount = todayDueCustomers.Sum(x => x.DueAmount);

            // Get paginated data with proper ordering
            var pagedData = await baseQuery
                .OrderByDescending(x => x.Customer.BillDay == today ? int.MaxValue : x.Customer.BillDay)
                .ThenByDescending(x => x.DueAmount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new TodayDueCustomerResponse
            {
                Data = pagedData,
                TotalDueCustomer = todayDueCustomerCount,  // Only today's customers
                TotalDueAmount = todayTotalDueAmount,      // Only today's due amount
                CurrentPage = page,
                PageSize = pageSize
            };
        }
        // Mark as done bill send api 
        public async Task<bool> MarkAsDoneAsync(int customerId)
        {
            var indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            var todayDate = DateOnly.FromDateTime(indiaTime);

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer is null) return false;

            customer.IsBillDone = true;
            customer.BillDoneDate = todayDate;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
