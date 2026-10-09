using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using System.Security.Claims;

namespace QualiTrack.Filters;

public class ValidateUserFilter : IAsyncActionFilter
{
    private readonly AppDbContext _db;

    public ValidateUserFilter(AppDbContext db)
    {
        _db = db;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                context.Result = new UnauthorizedObjectResult(new 
                { 
                    message = "Akun tidak ditemukan atau telah dihapus" 
                });
                return;
            }
        }

        await next();
    }
}