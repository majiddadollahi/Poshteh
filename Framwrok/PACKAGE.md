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
- **Consistent HTTP layer**: `Result` to `ProblemDetails` mapping and a global exception handler.

## Packages

| Package | Purpose |
|---|---|
| `Poshteh.Shared.Kernel` | Dependency-free shared primitives (clock, correlation) |
| `Poshteh.Core.Domain` | Domain model kernel |
| `Poshteh.Core.Contract` | Ports between Core and Infra |
| `Poshteh.Core.ApplicationService` | Dispatchers, pipeline behaviors, domain event bus |
| `Poshteh.Core.ModuleKernel` | Module registration contract |
| `Poshteh.Infra.Data.Sql.Command` | EF Core write side, Unit of Work, Outbox |
| `Poshteh.Infra.Data.Sql.Query` | EF Core read side |
| `Poshteh.Endpoint.Api` | HTTP adapter helpers |
| `Poshteh.Testing.Kit` | Contract-test bases and in-memory ports |

## Getting started

The quickest start is the project template, which generates a runnable Web API with a full CRUD sample:

```shell
dotnet new install Poshteh.Templates
dotnet new poshteh -n MyApp
```

Source, documentation and issues: https://github.com/majiddadollahi/Poshteh
