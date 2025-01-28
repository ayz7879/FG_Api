using FG_RO_PLANT.DTOs;
using FG_RO_PLANT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FG_RO_PLANT.Controllers
{
    [Route("api/entry")]
    [ApiController]
    [Authorize]
    public class DailyEntryController(DailyEntryService dailyEntryService) : ControllerBase
    {
        private readonly DailyEntryService _dailyEntryService = dailyEntryService;

        // Add Daily Entry
        [HttpPost("add")]
        public async Task<IActionResult> AddEntry(DailyEntryDTO dailyEntryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                return Ok(new { message = await _dailyEntryService.AddEntryAsync(dailyEntryDto) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get Entry by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntryById(int id)
        {
            if (id <= 0)
                return NotFound(new { message = "Entry not found." });

            try
            {
                return Ok(new { message = "Entry fetched successfully", data = await _dailyEntryService.GetEntryByIdAsync(id) });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Get All Entries for a Customer (Paginated)
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetCustomerEntries(int customerId, [FromQuery] DailyEntriesQuery query)
        {
            if (customerId <= 0)
                return NotFound(new { message = "Entry not found." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var entry = await _dailyEntryService.GetCustomerEntriesAsync(
                    customerId,
                    query.PageSize,
                    query.LastFetchId,
                    query.StartDate,
                    query.EndDate
                );
                var lastId = entry.LastOrDefault()?.Id ?? 0;
                return Ok(new { message = "Entries fetched", data = entry, lastFetchId = lastId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get Customer's Daily Entry Summary
        [HttpGet("summary/{customerId}")]
        public async Task<IActionResult> GetCustomerSummary(int customerId, [FromQuery] DailyEntriesQuery query)
        {
            if (customerId <= 0)
                return NotFound(new { message = "Customer not found." });

            try
            {
                return Ok(new { message = "Summary fetched", data = await _dailyEntryService.GetCustomerSummaryAsync(customerId, query.StartDate, query.EndDate) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Edit Daily Entry
        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditEntry(int id, DailyEntryDTO dailyEntryDto)
        {
            if (id <= 0)
                return NotFound(new { message = "Entry not found." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                return Ok(new { message = await _dailyEntryService.EditEntryAsync(id, dailyEntryDto) });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Delete Daily Entry
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            if (id <= 0)
                return NotFound(new { message = "Entry not found." });

            try
            {
                return Ok(new { message = await _dailyEntryService.DeleteEntryAsync(id) });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
