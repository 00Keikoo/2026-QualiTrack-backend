using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QualiTrack.Constants;
using QualiTrack.Data;
using System.Security.Claims;

namespace QualiTrack.Filters;

public class ValidateUserFilter(AppDbContext db, IMemoryCache cache) : IAsyncActionFilter
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public static string CacheKey(Guid userId) => $"user_status_{userId}";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (IsAnonymousAllowed(context) || !TryGetUserId(context.HttpContext.User, out var userId))
        {
            await next();
            return;
        }

        if (!await IsActiveUserAsync(userId))
        {
            cache.Remove(CacheKey(userId));
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Akun tidak aktif atau tidak ditemukan"
            });
            return;
        }

        await next();
    }

    private static bool IsAnonymousAllowed(ActionExecutingContext context) =>
        context.ActionDescriptor.EndpointMetadata.Any(m => m is AllowAnonymousAttribute);

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out userId);
    }

    private async Task<bool> IsActiveUserAsync(Guid userId) =>
        await cache.GetOrCreateAsync(CacheKey(userId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await db.Users.AnyAsync(u =>
                u.Id == userId &&
                u.Status == UserStatus.Active &&
                u.EmailVerified);
        });
}