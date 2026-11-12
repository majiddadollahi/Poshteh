# Poshteh

Poshteh is a lightweight framework for building .NET services with **DDD**, **CQRS** and a layered / Clean
Architecture, without depending on MediatR.

- **Rich domain model**: `AggregateRoot`, `BaseEntity`, `ValueObject`, `Enumeration`, `StronglyTypedId<T>`,
  `Specification<T>`, `DomainEvent`.
- **Result pattern**: `Result` / `Result<T>` and `Error` with stable codes instead of exceptions for business failures.
- **CQRS**: `ICommandDispatcher` / `IQueryDispatcher` with assembly-scanned handlers and pipeline behaviors
  (logging, validation, exception handling, transactions).
- **Unit of Work + Transactional Outbox**: domain events are stored in the same transaction as the aggregate change.
- **Separate write/read sides**: `BaseCommandDbContext` / `CommandRepository<T>` and no-tracking
  `BaseQueryDbContext` / `ReadRepository<T>` on EF Core.
- **Consistent HTTP layer**: `Result` to `ProblemDetails` mapping and a global exception handler, or the
  `ApiResponse` envelope (`{ success, message, code, errors, data }`) via `BaseApiController`.
- **Localization**: `.resx` resources, or flat JSON files (`Localization/fa.json`) with `Accept-Language`
  request-culture resolution.
- **Jalali calendar**: display-only Persian date formatting in Iran time.

## Packages

| Package | Purpose |
|---|---|
| `Poshteh.Shared.Kernel` | Dependency-free shared primitives (clock, correlation) |
| `Poshteh.Core.Domain` | Domain model kernel |
| `Poshteh.Core.Contract` | Ports between Core and Infra |
| `Poshteh.Core.ApplicationService` | Dispatchers, pipeline behaviors, domain event bus, `ValidationException`, resx/JSON localization |
| `Poshteh.Core.Calendar` | Dependency-free Jalali display formatting (`JalaliCalendarService`, `JalaliDisplayFields`) |
| `Poshteh.Core.ModuleKernel` | Module registration contract |
| `Poshteh.Infra.Data.Sql.Command` | EF Core write side, Unit of Work, Outbox |
| `Poshteh.Infra.Data.Sql.Query` | EF Core read side |
| `Poshteh.Endpoint.Api` | HTTP adapter helpers, `BaseApiController`/`ApiResponse`, request-culture middleware |
| `Poshteh.Testing.Kit` | Contract-test bases and in-memory ports |

## Getting started

The quickest start is the project template, which generates a runnable Web API with a full CRUD sample:

```shell
dotnet new install Poshteh.Templates
dotnet new poshteh -n MyApp
```

## JSON localization and `ApiResponse` (10.0.3)

```csharp
builder.Services.AddPoshtehJsonLocalization(builder.Configuration, builder.Environment); // section "Localization"
builder.Services.AddControllers(o => o.Filters.Add<LocalizationValidationExceptionFilter>());

var app = builder.Build();
app.UsePoshtehRequestCulture();
```

- Files: `{ResourcesPath}/{culture}.json` (default `Localization/fa.json`, `Localization/en.json`), flat
  `"code": "message"` objects; comments and trailing commas allowed, codes case-sensitive. Every other `*.json`
  in the folder is loaded as an extra culture. Loading happens at registration and fails fast on a missing folder,
  a missing supported-culture file, or an unloaded default culture.
- `IJsonMessageLocalizer` (`Get(code)`, `Get(code, args)`, `Get(Error)`, `GetForCulture(culture, code, args)`)
  uses the request culture; a missing translation returns the code (`code|arg1|arg2` with args). The same
  instance replaces `IMessageLocalizer`, so `ToApiResult` and `GlobalExceptionHandler` read the JSON files too.
- Lookup order: request culture, then `DefaultCulture`, then any culture that has the code.
- Workers without HTTP: `AddPoshtehJsonLocalizationCore(configuration, environment)` from
  `Poshteh.Core.ApplicationService`, then `GetForCulture`.
- `BaseApiController.HandleResult` maps error codes containing `NotFound`→404, `Unauthorized`/`InvalidCredentials`→401,
  `Forbidden`/`AccessDenied`→403, `Conflict`/`AlreadyExists`→409, anything else→400.
- Use `Error.WithArgs(code, args)` for an error that has arguments but no explicit message.

Source, documentation and issues: https://github.com/majiddadollahi/Poshteh
