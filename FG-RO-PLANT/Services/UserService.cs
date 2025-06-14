using FG_RO_PLANT.Data;
using FG_RO_PLANT.DTOs;
using FG_RO_PLANT.Helpers;
using FG_RO_PLANT.Models;
using Microsoft.EntityFrameworkCore;

namespace FG_RO_PLANT.Services
{
    public class UserService(ApplicationDbContext context, JwtHelper jwtService)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly JwtHelper _jwtService = jwtService;

        // Register User
        public async Task<string> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                throw new Exception("User already exists");

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Phone = registerDto.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = UserRole.User,
                IsActive = false
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return "User registered successfully";
        }

        // Login User
        public async Task<(string, string)> LoginAsync(LoginDto loginDto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email) ?? throw new UnauthorizedAccessException("User not found");

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, existingUser.Password))
                throw new UnauthorizedAccessException("Incorrect password");

            if (!existingUser.IsActive)
                throw new UnauthorizedAccessException("Account inactive");

            var token = _jwtService.GenerateToken(existingUser);
            return ("Login successful", token);
        }

        // Get User by search pagination
        public async Task<(List<User> users, int totalCount)> GetUsersAsync(string searchTerm, int page = 1, int pageSize = 10)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                string term = searchTerm.ToLower();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term) ||
                    u.Phone.ToLower().Contains(term));
            }

            var totalCount = await query.AsNoTracking().CountAsync();

            var users = await query
            .OrderBy(u => u.Name) 
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new User
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role,
                IsActive = u.IsActive
            })
            .ToListAsync();

            return (users, totalCount);
        }

        // Get User by ID
        public async Task<User> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id) ?? throw new Exception("User not found");
            user.Password = null;
            return user;
        }

        // Update Profile
        public async Task<string> UpdateProfileAsync(int id, UpdateUserDto registerDto)
        {
            var existingUser = await _context.Users.FindAsync(id) ?? throw new Exception("User not found");

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email && u.Id != id))
            {
                throw new Exception("Email already exists.");
            }

            existingUser.Name = registerDto.Name;
            existingUser.Email = registerDto.Email;
            existingUser.Phone = registerDto.Phone;
            if (!string.IsNullOrEmpty(registerDto.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            }
            _context.Entry(existingUser).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return "User updated successfully";
        }

        // Add User (Admin only)
        public async Task<string> AddUserAsync(RegisterDto registerDto)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                throw new Exception("User already exists");

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Phone = registerDto.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role,
                IsActive = registerDto.IsActive
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return "User added successfully";

            }
            catch (Exception ex)
            {
                // Log full inner exception if available
                var error = ex.InnerException?.Message ?? ex.Message;
                throw new Exception("AddUser failed: " + error);
            }
        }

        // Update User (Admin only)
        public async Task<string> UpdateUserAsync(int id, UpdateUserDto registerDto)
        {
            var existingUser = await _context.Users.FindAsync(id) ?? throw new Exception("User not found");

            if (existingUser.Role == UserRole.Admin)
            {
                throw new Exception("You cannot edit an admin");
            }
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email && u.Id != id))
            {
                throw new Exception("Email already exists.");
            }

            existingUser.Name = registerDto.Name;
            existingUser.Email = registerDto.Email;
            existingUser.Phone = registerDto.Phone;
            existingUser.Role = registerDto.Role;
            existingUser.IsActive = registerDto.IsActive;
            if (!string.IsNullOrEmpty(registerDto.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            }
            _context.Entry(existingUser).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return "User updated successfully";
        }

        // Delete User (Admin only)
        public async Task<string> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id) ?? throw new Exception("User not found");
            if (user.Role == UserRole.Admin)
            {
                throw new Exception("You cannot delete an admin");
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return "User deleted successfully";
        }
    }
}
