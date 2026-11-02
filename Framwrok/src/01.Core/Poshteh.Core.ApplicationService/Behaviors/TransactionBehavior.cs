using Microsoft.Extensions.DependencyInjection;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.Domain;

namespace Poshteh.Core.ApplicationService.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IServiceProvider _serviceProvider;

    public TransactionBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<TResponse>> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        if (request is not ICommand<TResponse>)
        {
            return await next();
        }

        var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
        if (unitOfWork is null)
        {
            return await next();
        }

        var result = await next();
        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result;
    }
}
