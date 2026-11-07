using Microsoft.EntityFrameworkCore;
using Poshteh.Core.ApplicationService.DependencyInjection;
using Poshteh.Endpoint.Api.DependencyInjection;
using Poshteh.Infra.Data.Sql.Command.DependencyInjection;
using Poshteh.Infra.Data.Sql.Query.DependencyInjection;
using PoshtehStarter.Core.ApplicationService.Ordering.Commands;
using PoshtehStarter.Core.Contract.Ordering;
using PoshtehStarter.Infra.Data.Sql.Command.DBContexts;
using PoshtehStarter.Infra.Data.Sql.Command.Repositories;
using PoshtehStarter.Infra.Data.Sql.Query.DBContexts;
using PoshtehStarter.Infra.Data.Sql.Query.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");

// Write side (aggregates + Unit of Work + Outbox) and read side (no-tracking read models) share one database.
builder.Services.AddSqlCommandPersistence<OrderingDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddSqlQueryPersistence<OrderingQueryDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddScoped<IOrderCommandRepository, OrderCommandRepository>();
builder.Services.AddScoped<IOrderQueryRepository, OrderQueryRepository>();

builder.Services.AddApplicationService(typeof(CreateOrderCommandHandler).Assembly);
builder.Services.AddEndpointApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    dbContext.Database.EnsureCreated();
}

app.MapControllers();

app.Run();
