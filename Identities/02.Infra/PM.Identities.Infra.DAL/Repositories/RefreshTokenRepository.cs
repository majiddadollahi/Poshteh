using PM.Identities.Core.Domain.Users;
using PM.Identities.Infra.DAL.DbContext;
using Microsoft.EntityFrameworkCore;

namespace PM.Identities.Infra.DAL.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly PMIdentityDbContext _context;

    public RefreshTokenRepository(PMIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<List<RefreshToken>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > now)
            .ToListAsync(cancellationToken);
    }

    public async Task<RefreshToken> AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        return refreshToken;
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }

    public async Task RevokeAllUserTokensAsync(long userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = now;
            token.IsRevoked = true;
        }

        _context.RefreshTokens.UpdateRange(tokens);
    }
}
