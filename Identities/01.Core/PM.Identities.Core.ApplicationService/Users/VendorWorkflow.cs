using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PM.Identities.Core.Contract.Users.DTOs;
using PM.Identities.Core.Domain.Users;
using PM.Identities.Infra.DAL.DbContext;
using PM.Vendor.Core.Contract.Integration.Companies;
namespace PM.Identities.Core.ApplicationService.Users;

public sealed class VendorWorkflow(PMIdentityDbContext db, IVendorOnboardingStore profiles)
{
    public Task<User?> UserAsync(long id, CancellationToken ct) => db.Users
        .SingleOrDefaultAsync(u => u.Id == id && u.IsActive && AccountTypes.Vendor.Contains(u.AccountType), ct);
    public async Task<IDbContextTransaction?> LockAsync(long id, CancellationToken ct)
    {
        if (!db.Database.IsRelational()) return null;
        var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        try { await db.Database.ExecuteSqlInterpolatedAsync($@"SELECT [Id] FROM [Identity].[AspNetUsers] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}", ct); return tx; }
        catch { await tx.DisposeAsync(); throw; }
    }
    public async Task<VendorOnboardingDto> DtoAsync(User u, CancellationToken ct)
    {
        var p = await profiles.GetAsync(u.Id, ct);
        var missing = VendorProfileRules.Missing(u, p);
        return new(u.Id, p.Company is null ? "individual" : "legal", p.Company?.Name ?? u.DisplayName,
            u.VendorReviewStatus, u.VendorRejectionReason, u.VendorCorrectionReason, u.FirstName, u.LastName, u.NationalId,
            u.PhoneNumber, p.Company?.Address ?? p.IndividualAddress, p.Company, u.RegisteredAt,
            u.VendorProfileSubmittedAt, VendorProfileRules.CanEdit(u.VendorReviewStatus),
            VendorProfileRules.CanEdit(u.VendorReviewStatus) && missing.Count == 0, missing);
    }
}
