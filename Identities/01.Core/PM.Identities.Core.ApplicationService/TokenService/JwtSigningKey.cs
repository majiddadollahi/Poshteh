using System.Text;
using Microsoft.Extensions.Configuration;

namespace PM.Identities.Core.ApplicationService.TokenService;

/// <summary>Central validation for JWT signing material used by both token issuance and validation.</summary>
public static class JwtSigningKey
{
    private const int MinimumKeyBytes = 32;
    private static readonly string[] ForbiddenFragments = ["yoursupersecretkey", "changeme", "placeholder"];

    public static string GetRequiredSecret(IConfiguration configuration)
    {
        var secret = configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException(
                "JwtSettings:SecretKey must be supplied by the environment or a secret store.");
        }

        if (Encoding.UTF8.GetByteCount(secret) < MinimumKeyBytes
            || ForbiddenFragments.Any(fragment => secret.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"JwtSettings:SecretKey must contain at least {MinimumKeyBytes} bytes of non-placeholder key material.");
        }

        return secret;
    }
}
