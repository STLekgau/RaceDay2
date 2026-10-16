using System.Security.Claims;

namespace RaceDay2.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // Reads the logged-in user's ID from the JWT (NameIdentifier claim).
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}