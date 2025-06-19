using System.Security.Claims;
using FG_RO_PLANT.Data;
using FG_RO_PLANT.Services;

namespace FG_RO_PLANT
{
    public class ActiveUserMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            if (path is not null &&
                (path.Contains("/user/login") ||
                 path.Contains("/customer/login") ||
                 path.Contains("/user/register") ||
                 path.Contains("/user/health") ||
                 path.Contains("/api/public-customer")))
            {
                await next(context);
                return;
            }


            if (context.User.Identity?.IsAuthenticated != true)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var userIdClaim = context.User.FindFirst("UserId")?.Value;
            var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out var userId) || string.IsNullOrEmpty(roleClaim))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid token claims");
                return;
            }

            var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();
            bool? isActive = roleClaim.ToLower() == "customer"
                ? (await db.Customers.FindAsync(userId))?.IsActive
                : (await db.Users.FindAsync(userId))?.IsActive;

            if (isActive != true)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(isActive == null ? "User is deleted." : "User is inactive.");
                return;
            }

            await next(context); // ✅ Valid user → continue
        }
    }
}
