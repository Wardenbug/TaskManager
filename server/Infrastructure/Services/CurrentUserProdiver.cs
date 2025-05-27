using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Services;

public sealed class CurrentUserProdiver : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserProdiver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public string GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId;
    }
}
