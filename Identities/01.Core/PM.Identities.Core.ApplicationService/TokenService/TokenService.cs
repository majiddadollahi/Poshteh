using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using PM.Identities.Core.Contract.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PM.Identities.Core.Domain.Users;

namespace PM.Identities.Core.ApplicationService.TokenService;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    /// <summary>Access Token lifetime must never be shorter than this, whatever the configuration says.</summary>
    public const int MinimumAccessTokenLifetimeMinutes = 60;

    public TimeSpan AccessTokenLifetime
    {
        get
        {
            var configured = _configuration.GetSection("JwtSettings").GetValue<int?>("ExpirationInMinutes")
                ?? MinimumAccessTokenLifetimeMinutes;
            return TimeSpan.FromMinutes(Math.Max(configured, MinimumAccessTokenLifetimeMinutes));
        }
    }

    public string GenerateAccessToken(User user, List<Claim>? additionalClaims = null)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = JwtSigningKey.GetRequiredSecret(_configuration);
        var issuer = jwtSettings["Issuer"] ?? "MahtaAPI";
        var audience = jwtSettings["Audience"] ?? "MahtaAPIUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var issuedAt = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // TASK-0223: exp - iat lets the client derive the lifetime independently of its own clock.
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(issuedAt).ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(MahtaJwtClaimTypes.UserId, user.Id.ToString()),
            new(MahtaJwtClaimTypes.SecurityStamp, user.SecurityStamp ?? string.Empty),
            new("UserName", user.UserName ?? string.Empty),
            new("FullName", user.DisplayName),
            new(MahtaJwtClaimTypes.DisplayName, user.DisplayName),
            new(MahtaJwtClaimTypes.AccountType, user.AccountType.ToString())
        };

        if (additionalClaims != null)
        {
            claims.AddRange(additionalClaims);
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: issuedAt.Add(AccessTokenLifetime),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(long userId)
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        
        var token = Convert.ToBase64String(randomNumber);
        
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var refreshTokenExpirationDays = int.Parse(jwtSettings["RefreshTokenExpirationInDays"] ?? "7");

        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
            CreatedDate = DateTime.UtcNow
        };
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var secretKey = JwtSigningKey.GetRequiredSecret(_configuration);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false // مهم: برای expired token ها
        };

        try
        {
            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
