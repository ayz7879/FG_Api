using FG_RO_PLANT.Data;
using FG_RO_PLANT.DTOs;
using FG_RO_PLANT.Models;
using Microsoft.EntityFrameworkCore;

namespace FG_RO_PLANT.Services
{
    public class DailyEntryService(ApplicationDbContext context, HistoryService historyService)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly HistoryService _historyService = historyService;

        // Add Entry
        public async Task<string> AddEntryAsync(DailyEntryDTO dailyEntryDto)
        {
            _ = await _context.Customers.FindAsync(dailyEntryDto.CustomerId) ?? throw new Exception("Customer not found");

            var dailyEntry = new DailyEntry
            {
                CustomerId = dailyEntryDto.CustomerId,
                JarGiven = dailyEntryDto.JarGiven,
                JarTaken = dailyEntryDto.JarTaken,
                CapsuleGiven = dailyEntryDto.CapsuleGiven,
                CapsuleTaken = dailyEntryDto.CapsuleTaken,
                CustomerPay = dailyEntryDto.CustomerPay,
                DateField = dailyEntryDto.DateField
            };

            await _context.DailyEntries.AddAsync(dailyEntry);
            await _context.SaveChangesAsync();

            await _historyService.AddHistoryAsync(dailyEntry.Id, dailyEntry.DateField);

            return "Entry added successfully";
        }

        // Get Single Entry by ID
        public async Task<DailyEntry> GetEntryByIdAsync(int id)
        {
            var entry = await _context.DailyEntries.FindAsync(id) ?? throw new Exception("Entry not found");
            return entry;
        }

        // Get All Entries for a Customer (Paginated)
        public async Task<List<DailyEntry>> GetCustomerEntriesAsync(int customerId, int pageSize, int lastFetchId, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            _ = await _context.Customers.FindAsync(customerId) ?? throw new Exception("Customer not found");

            var query = _context.DailyEntries.Where(e => e.CustomerId == customerId && e.Id > lastFetchId);

            if (startDate.HasValue)
                query = query.Where(e => e.DateField >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.DateField <= endDate.Value);

            return await query
                .OrderBy(e => e.Id)
                .Take(pageSize)
                .ToListAsync();
        }

        // Edit Entry
        public async Task<string> EditEntryAsync(int id, DailyEntryDTO dailyEntryDto)
        {
            var existingEntry = await _context.DailyEntries.FindAsync(id) ?? throw new Exception("Entry not found");

            var oldDate = existingEntry.DateField;

            existingEntry.JarGiven = dailyEntryDto.JarGiven;
            existingEntry.JarTaken = dailyEntryDto.JarTaken;
            existingEntry.CapsuleGiven = dailyEntryDto.CapsuleGiven;
            existingEntry.CapsuleTaken = dailyEntryDto.CapsuleTaken;
            existingEntry.CustomerPay = dailyEntryDto.CustomerPay;
            existingEntry.DateField = dailyEntryDto.DateField;

            _context.Entry(existingEntry).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            if (existingEntry.DateField != oldDate && existingEntry.DateField.HasValue)
            {
                await _historyService.UpdateHistoryAsync(existingEntry.Id, existingEntry.DateField.Value);
            }

            return "Entry updated successfully";
        }

        // Delete Entry
        public async Task<string> DeleteEntryAsync(int id)
        {
            var existingEntry = await _context.DailyEntries.FindAsync(id) ?? throw new Exception("Daily entry not found");

            _context.DailyEntries.Remove(existingEntry);
            await _context.SaveChangesAsync();

            return "Daily entry deleted successfully";
        }

        // Calculate specific customer entries total
        public async Task<object> GetCustomerSummaryAsync(int customerId, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            // Step 1: Fetch aggregated totals directly for a specific customer from database
            var query = _context.DailyEntries.AsNoTracking().Where(e => e.CustomerId == customerId);

            if (startDate.HasValue)
                query = query.Where(e => e.DateField >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.DateField <= endDate.Value);

            var entryTotals = await query
                .GroupBy(e => 1)
                .Select(g => new
                {
                    TotalJarGiven = g.Sum(e => e.JarGiven),
                    TotalJarTaken = g.Sum(e => e.JarTaken),
                    TotalCapsuleGiven = g.Sum(e => e.CapsuleGiven),
                    TotalCapsuleTaken = g.Sum(e => e.CapsuleTaken),
                    TotalPaid = g.Sum(e => e.CustomerPay)
                })
                .FirstOrDefaultAsync();

            if (entryTotals == null)
                return new { Message = "No entries found for this customer." };

            // Step 2: Fetch Customer Price
            var customer = await _context.Customers.AsNoTracking()
                .Where(c => c.Id == customerId)
                .Select(c => new { c.PricePerJar, c.PricePerCapsule })
                .FirstOrDefaultAsync();

            if (customer == null)
                return new { Message = "Customer not found." };

            // Step 3: Calculate Payments
            var totalJarPayment = entryTotals.TotalJarGiven * customer.PricePerJar;
            var totalCapsulePayment = entryTotals.TotalCapsuleGiven * customer.PricePerCapsule;
            var totalPayment = totalJarPayment + totalCapsulePayment;
            var pendingPayment = totalPayment - entryTotals.TotalPaid;

            // Step 4: Return Response
            return new
            {
                CustomerId = customerId,
                entryTotals.TotalJarGiven,
                entryTotals.TotalJarTaken,
                PendingJar = entryTotals.TotalJarGiven - entryTotals.TotalJarTaken,
                entryTotals.TotalCapsuleGiven,
                entryTotals.TotalCapsuleTaken,
                PendingCapsule = entryTotals.TotalCapsuleGiven - entryTotals.TotalCapsuleTaken,
                TotalJarPayment = totalJarPayment,
                TotalCapsulePayment = totalCapsulePayment,
                TotalPayment = totalPayment,
                entryTotals.TotalPaid,
                PendingPayment = pendingPayment
            };
        }

    }
}
