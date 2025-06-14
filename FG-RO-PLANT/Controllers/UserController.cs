using FG_RO_PLANT.DTOs;
using FG_RO_PLANT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FG_RO_PLANT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(UserService userService) : ControllerBase
    {
        private readonly UserService _userService = userService;

        // Register User
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ModelState });

            try
            {
                var message = await _userService.RegisterAsync(registerDto);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Login User
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ModelState });

            try
            {
                var (message, token) = await _userService.LoginAsync(loginDto);
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

        // Get User by ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            if (id <= 0)
                return NotFound(new { message = "User not found" });

            try
            {
                return Ok(new { message = "User found", data = await _userService.GetUserByIdAsync(id) });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Update Profile
        [HttpPut("updateProfile/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(int id, UpdateUserDto user)
        {
            if (id <= 0)
                return NotFound(new { message = "User not found." });

            if (!ModelState.IsValid)
                return BadRequest(new { message = ModelState });

            try
            {
                return Ok(new { message = await _userService.UpdateProfileAsync(id, user) });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Get Users by saerch (Admin only)
        [HttpGet("search")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers([FromQuery] string searchTerm = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (users, totalCount) = await _userService.GetUsersAsync(searchTerm, page, pageSize);
                return Ok(new
                {
                    message = "Users fetched",
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Users = users
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Add User (Admin only)
        [HttpPost("add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUser(RegisterDto user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if ((int)user.Role < 1 || (int)user.Role > 2)
                return BadRequest(new { message = "Role must be 1 or 2" });

            try
            {
                return Ok(new { message = await _userService.AddUserAsync(user) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Update User (Admin only)
        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto user)
        {
            if (id <= 0)
                return NotFound(new { message = "User not found." });

            if (!ModelState.IsValid)
                return BadRequest(new { message = ModelState });

            if ((int)user.Role < 1 || (int)user.Role > 2)
                return BadRequest(new { message = "Role must be 1 or 2" });

            try
            {
                return Ok(new { message = await _userService.UpdateUserAsync(id, user) });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // Delete User (Admin only)
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (id <= 0)
                return NotFound(new { message = "User not found." });

            try
            {
                return Ok(new { message = await _userService.DeleteUserAsync(id) });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
