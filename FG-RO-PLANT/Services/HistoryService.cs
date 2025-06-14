using FG_RO_PLANT.Data;
using FG_RO_PLANT.DTOs;
using FG_RO_PLANT.Models;
using Microsoft.EntityFrameworkCore;

namespace FG_RO_PLANT.Services
{
    public class HistoryService(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;

        // Add History 
        public async Task AddHistoryAsync(int dailyEntryId, DateOnly? dateField)
        {
            if (dailyEntryId <= 0)
                throw new ArgumentException("Invalid Entry Id.");

            try
            {
                var history = new History
                {
                    DailyEntryId = dailyEntryId,
                    DateField = dateField
                };

                await _context.Histories.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // Update History
        public async Task UpdateHistoryAsync(int dailyEntryId, DateOnly newDate)
        {
            if (dailyEntryId <= 0)
                throw new ArgumentException("Invalid Entry Id.");

            try
            {
                var history = await _context.Histories
                .FirstOrDefaultAsync(h => h.DailyEntryId == dailyEntryId);

                if (history != null)
                {
                    history.DateField = newDate;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // Get History by Date Range
        public async Task<List<HistoryWithDetails>> GetHistoryAsync(DateOnly? startDate = null, DateOnly? endDate = null, int pageSize = 10, int lastFetchId = 0)
        {
            var query = from h in _context.Histories.AsNoTracking()
                        join de in _context.DailyEntries.AsNoTracking() on h.DailyEntryId equals de.Id
                        join c in _context.Customers.AsNoTracking() on de.CustomerId equals c.Id
                        select new HistoryWithDetails
                        {
                            HistoryId = h.Id,
                            DateField = h.DateField,
                            DailyEntryId = de.Id,
                            DailyEntryData = de,
                            CustomerName = c.Name,
                            CustomerAddress = c.Address,
                            CustomerPhone = c.Phone
                        };

            if (startDate.HasValue)
                query = query.Where(h => h.DateField >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(h => h.DateField <= endDate.Value);

            if (!startDate.HasValue && !endDate.HasValue)
            {
                lastFetchId = lastFetchId > 0 ? lastFetchId : int.MaxValue;
                query = query.Where(h => h.HistoryId < lastFetchId)
                             .OrderByDescending(h => h.HistoryId)
                             .Take(pageSize);
            }
            else
            {
                query = query.OrderByDescending(h => h.DateField);
            }

            return await query.ToListAsync();
        }

        // Get History Summary
        public async Task<object> GetHistorySummaryAsync(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var query = _context.Histories.AsNoTracking()
                .Join(_context.DailyEntries.AsNoTracking(), h => h.DailyEntryId, de => de.Id, (h, de) => new { h, de });

            if (startDate.HasValue)
                query = query.Where(x => x.h.DateField >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.h.DateField <= endDate.Value);

            var totalEntries = await query.AsNoTracking().CountAsync();

            var summary = await query
                .GroupBy(x => 1) 
                .Select(g => new
                {
                    TotalJarGiven = g.Sum(x => x.de.JarGiven),
                    TotalJarTaken = g.Sum(x => x.de.JarTaken),
                    TotalCapsuleGiven = g.Sum(x => x.de.CapsuleGiven),
                    TotalCapsuleTaken = g.Sum(x => x.de.CapsuleTaken),
                    TotalCustomerPay = g.Sum(x => x.de.CustomerPay)
                })
                .FirstOrDefaultAsync();

            return new
            {
                TotalEntries = totalEntries,
                TotalJarGiven = summary?.TotalJarGiven ?? 0,
                TotalJarTaken = summary?.TotalJarTaken ?? 0,
                TotalCapsuleGiven = summary?.TotalCapsuleGiven ?? 0,
                TotalCapsuleTaken = summary?.TotalCapsuleTaken ?? 0,
                TotalCustomerPay = summary?.TotalCustomerPay ?? 0
            };
        }
    }
}
