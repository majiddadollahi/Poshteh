using Poshteh.Core.ApplicationService.DependencyInjection;
using Poshteh.Endpoint.Api.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Modules register their own handlers/persistence here once ModuleKernel/IModuleRegistrar lands (Phase 06);
// for now this composition root only wires the framework-level seams that every module will rely on.
builder.Services.AddApplicationService(typeof(Program).Assembly);
builder.Services.AddEndpointApi();

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
