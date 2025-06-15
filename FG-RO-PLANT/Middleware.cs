using FG_RO_PLANT.Data;
using FG_RO_PLANT.Services;

namespace FG_RO_PLANT
{
    public class ActiveUserMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            if (path is not null && (path.Contains("/user/login") || path.Contains("/user/register") || path.Contains("/user/health")))
            {
                await next(context);
                return;
            }

            var userIdClaim = context.User.FindFirst("UserId")?.Value;

            if (context.User.Identity?.IsAuthenticated != true || !int.TryParse(userIdClaim, out var userId))
            {
                await next(context); // Not authenticated properly — skip, let auth handle
                return;
            }

            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FindAsync(userId);

            context.Response.StatusCode = 401;
            if (user is null)
                await context.Response.WriteAsync("User is deleted.");
            else if (!user.IsActive)
                await context.Response.WriteAsync("User is inactive.");
            else
                await next(context); // ✅ Valid user → continue
        }
    }
}
