using Microsoft.EntityFrameworkCore;
using Poshteh.Core.ApplicationService.Commands;
using Poshteh.Core.ApplicationService.DependencyInjection;
using Poshteh.Core.ApplicationService.Queries;
using Poshteh.Core.Contract;
using Poshteh.Core.Contract.Localization;
using Poshteh.Endpoint.Api.DependencyInjection;
using Poshteh.Endpoint.Api.Results;
using Poshteh.Infra.Data.Sql.Command.DependencyInjection;
using Poshteh.Sample.Ordering.Application.Commands;
using Poshteh.Sample.Ordering.Application.Queries;
using Poshteh.Sample.Ordering.Domain;
using Poshteh.Sample.Ordering.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlCommandPersistence<OrderingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// CommandRepository<TEntity> is abstract — each aggregate supplies its own concrete repository.
builder.Services.AddScoped<ICommandRepository<Order>, OrderCommandRepository>();

builder.Services.AddApplicationService(typeof(CreateOrderCommandHandler).Assembly);
builder.Services.AddEndpointApi();

var app = builder.Build();

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    dbContext.Database.EnsureCreated();
}

app.MapPost("/orders", async (
    CreateOrderRequest request,
    ICommandDispatcher dispatcher,
    IMessageLocalizer localizer,
    CancellationToken cancellationToken) =>
{
    var command = new CreateOrderCommand(
        request.CustomerName,
        request.Items.Select(item => new CreateOrderItem(item.ProductName, item.UnitPrice, item.Quantity)).ToList());

    var result = await dispatcher.Send(command, cancellationToken);

    // Success shape (201 + Location) is endpoint-specific; failures go through the central Result -> HTTP mapper.
    return result.IsSuccess
        ? Results.Created($"/orders/{result.Value}", new { id = result.Value })
        : result.ToApiResult(localizer);
});

app.MapGet("/orders/{id:long}", async (
    long id,
    IQueryDispatcher dispatcher,
    IMessageLocalizer localizer,
    CancellationToken cancellationToken) =>
{
    var result = await dispatcher.Send(new GetOrderByIdQuery(id), cancellationToken);

    return result.ToApiResult(localizer);
});

app.Run();

public sealed record CreateOrderRequest(string CustomerName, IReadOnlyCollection<CreateOrderItemRequest> Items);

public sealed record CreateOrderItemRequest(string ProductName, decimal UnitPrice, int Quantity);
