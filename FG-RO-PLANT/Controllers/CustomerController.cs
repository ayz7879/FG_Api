using FG_RO_PLANT.DTOs;
using FG_RO_PLANT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FG_RO_PLANT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController(CustomerService customerService) : ControllerBase
    {
        private readonly CustomerService _customerService = customerService;

        // Add Customer
        [HttpPost("add")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> AddCustomer(CustomerDTO customer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                return Ok(new { message = await _customerService.AddCustomerAsync(customer) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get Customer by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "User,Admin,Customer")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            if (id <= 0)
                return NotFound(new { message = "Customer not found." });

            try
            {
                return Ok(new { message = "Customer found", data = await _customerService.GetCustomerByIdAsync(id) });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Get All Customers with Pagination
        [HttpGet("all")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetAllCustomers([FromQuery] int pageSize = 10, [FromQuery] int lastFetchId = 0, [FromQuery] int customerType = 0)
        {
            if (customerType < 0 || customerType > 2)
            {
                return BadRequest(new { message = "Invalid customerType." });
            }
            try
            {
                var customers = await _customerService.GetAllCustomersAsync(pageSize, lastFetchId, customerType);
                var lastId = customers.LastOrDefault()?.Id ?? 0;
                return Ok(new { message = "Customers fetched", data = customers, lastFetchId = lastId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get Total Customer Count
        [HttpGet("count")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetTotalCustomerCountAsync()
        {
            try
            {
                var counts = await _customerService.GetTotalCustomerCountAsync();
                return Ok(new
                {
                    All = counts[0],
                    Regular = counts[1],
                    Events = counts[2]
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Search Customers
        [HttpGet("search")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> SearchCustomers([FromQuery] string searchTerm = "", [FromQuery] int pageSize = 10, [FromQuery] int lastFetchId = 0)
        {
            try
            {
                var customers = await _customerService.SearchCustomersAsync(searchTerm, pageSize, lastFetchId);
                var lastId = customers.LastOrDefault()?.Id ?? 0;
                return Ok(new { message = "Customers fetched", data = customers, lastFetchId = lastId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Update Customer
        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCustomer(int id, CustomerDTO customer)
        {
            if (id <= 0)
                return NotFound(new { message = "Customer not found." });

            if (!ModelState.IsValid)
                return BadRequest(new { message = ModelState });

            try
            {
                return Ok(new { message = await _customerService.UpdateCustomerAsync(id, customer) });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Delete Customer
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            if (id <= 0)
                return NotFound(new { message = "Customer not found." });

            try
            {
                return Ok(new { message = await _customerService.DeleteCustomerAsync(id) });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Get due customers
        [HttpGet("due-customers")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetDueCustomers(int page = 1, int pageSize = 10, string? search = null)
        {
            try
            {
                var result = await _customerService.GetDueCustomersAsync(page, pageSize, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Login Customer
        [HttpPost("login")]
        public async Task<IActionResult> CustomerLogin([FromBody] CustomerLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ModelState });

            try
            {
                var (message, token) = await _customerService.CustomerLoginAsync(dto.PhoneNumber);
                return Ok(new { message, token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
