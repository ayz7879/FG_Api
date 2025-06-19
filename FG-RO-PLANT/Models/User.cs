namespace FG_RO_PLANT.Models
{
    public enum UserRole
    {
        User = 1,
        Admin = 2,
        Customer = 3,
    }
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public bool IsActive { get; set; } = false;
    }

}
