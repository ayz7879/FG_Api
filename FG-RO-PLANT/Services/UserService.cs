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

        // Get Users with Pagination
        public async Task<List<User>> GetAllUsersAsync(int pageSize, int lastFetchId)
        {
            return await _context.Users.AsNoTracking().Where(u => u.Id > lastFetchId).OrderBy(u => u.Id)
                .Take(pageSize)
                .Select(u => new User
                {
                    Id = u.Id,
                    Name = u.Name,
                    Phone = u.Phone,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive
                })
                .ToListAsync();
        }

        // Total count user
        public async Task<int> GetTotalUserCountAsync()
        {
            return await _context.Users.AsNoTracking().CountAsync();
        }

        // Get User by search
        public async Task<List<User>> SearchUsersAsync(string searchTerm, int pageSize, int lastFetchId)
        {
            var query = _context.Users.AsNoTracking().Where(u => u.Id > lastFetchId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => u.Name.ToLower().Contains(searchTerm) ||
                                         u.Phone.Contains(searchTerm) ||
                                         u.Email.ToLower().Contains(searchTerm));
            }

            return await query.OrderBy(u => u.Id)
                               .Take(pageSize)
                               .Select(u => new User
                               {
                                   Id = u.Id,
                                   Name = u.Name,
                                   Phone = u.Phone,
                                   Email = u.Email,
                                   Role = u.Role,
                                   IsActive = u.IsActive
                               })
                               .ToListAsync();
        }

        // Get User by ID
        public async Task<User> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id) ?? throw new Exception("User not found");
            return user;
        }

        // Update Profile
        public async Task<string> UpdateProfileAsync(int id, RegisterDto registerDto)
        {
            var existingUser = await _context.Users.FindAsync(id) ?? throw new Exception("User not found");
            
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email && u.Id != id))
            {
                throw new Exception("Email already exists.");
            }

            existingUser.Name = registerDto.Name;
            existingUser.Email = registerDto.Email;
            existingUser.Phone = registerDto.Phone;
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            _context.Entry(existingUser).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return "User updated successfully";
        }

        // Add User (Admin only)
        public async Task<string> AddUserAsync(RegisterDto registerDto)
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

        // Update User (Admin only)
        public async Task<string> UpdateUserAsync(int id, RegisterDto registerDto)
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
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

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
