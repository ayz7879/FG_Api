using FG_RO_PLANT.DTOs;
using FG_RO_PLANT.Services;
using Microsoft.AspNetCore.Mvc;

namespace FG_RO_PLANT.Controllers
{
    [Route("api/public-customer")]
    [ApiController]
    public class PublicController(CustomerService customerService, DailyEntryService dailyEntryService) : ControllerBase
    {
        private readonly CustomerService _customerService = customerService;
        private readonly DailyEntryService _dailyEntryService = dailyEntryService;

        // Get Customer by Token
        [HttpGet("{token}")]
        public async Task<IActionResult> GetCustomerByToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized(new { message = "Access denied." });

            try
            {
                return Ok(new { message = "Customer found", data = await _customerService.GetCustomerByTokenAsync(token) });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Access denied." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // Get All Entries for a Public Customer (Paginated)
        [HttpGet("entries/{customerId}")]
        public async Task<IActionResult> GetPublicCustomerEntries(int customerId, [FromQuery] DailyEntriesQuery query)
        {
            if (customerId <= 0)
                return NotFound(new { message = "Entry not found." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var entry = await _dailyEntryService.GetPublicCustomerEntriesAsync(
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
                return Ok(new { message = "Summary fetched", data = await _dailyEntryService.GetPublicCustomerSummaryAsync(customerId, query.StartDate, query.EndDate) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
