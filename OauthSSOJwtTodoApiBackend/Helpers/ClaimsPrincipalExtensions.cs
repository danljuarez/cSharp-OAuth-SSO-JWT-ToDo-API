using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OauthSSOJwtTodoApiBackend.Helpers;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extracts the user ID (GUID) from JWT claims.
    /// Looks for "sub" (standard JWT subject) first, then falls back to NameIdentifier.
    /// Throws UnauthorizedAccessException if not found or invalid.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userId = TryGetUserId(user);

        if (userId == null)
            throw new UnauthorizedAccessException("User ID claim is missing or invalid.");

        return userId.Value;
    }

    /// <summary>
    /// Tries to extract the user ID as a GUID from the current claims principal.
    /// Returns null if missing or malformed.
    /// </summary>
    public static Guid? TryGetUserId(this ClaimsPrincipal user)
    {
        // Prioritize "sub" as it's the JWT standard
        var idClaim = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(idClaim, out var parsed))
            return parsed;

        return null;
    }
}
