using FG_RO_PLANT.DTOs;
using FG_RO_PLANT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FG_RO_PLANT.Controllers
{
    [Route("api/history")]
    [ApiController]
    [Authorize]
    public class HistoryController(HistoryService historyService) : ControllerBase
    {
        private readonly HistoryService _historyService = historyService;

        // Get History by Date Range
        [HttpGet("all")]
        public async Task<IActionResult> GetHistory([FromQuery] HistoryDTO historyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var history = await _historyService.GetHistoryAsync(historyDto.StartDate, historyDto.EndDate, historyDto.PageSize, historyDto.LastFetchId);
                var lastId = history.LastOrDefault()?.HistoryId ?? 0;
                return Ok(new { message = "History fetched", data = history, lastFetchId = lastId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get History Summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetHistorySummary([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate)
        {
            try
            {
                return Ok(new { message = "History summary fetched", data = await _historyService.GetHistorySummaryAsync(startDate, endDate) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
