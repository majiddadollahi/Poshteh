using System.Security.Claims;
using PM.Identities.Core.Domain.Users;

namespace PM.Identities.Core.ApplicationService.TokenService;

/// <summary>
/// Interface برای مدیریت JWT Tokens
/// </summary>
public interface ITokenService
{
    /// <summary>طول عمر Access Token؛ تنها منبع برای <c>exp</c> توکن و <c>ExpiresAt</c> پاسخ ورود/تمدید.</summary>
    TimeSpan AccessTokenLifetime { get; }

    string GenerateAccessToken(User user, List<Claim>? additionalClaims = null);
    RefreshToken GenerateRefreshToken(long userId);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

