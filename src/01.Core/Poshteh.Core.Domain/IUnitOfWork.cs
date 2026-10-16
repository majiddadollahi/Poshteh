using System.Diagnostics.CodeAnalysis;

namespace Poshteh.Core.Domain;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}