# Unit Tests

Implemented framework unit tests are currently in
`Poshteh.Core.ApplicationService.Tests/` and cover:

- resx localization and exception safety (`Localization/`, `Behaviors/`, `ExceptionHandling/`)
- JSON localization: catalog loading, culture fallback, `Get(Error)` argument handling (`Localization/Json/`)
- `ValidationException` and `Error.WithArgs` (`Validations/`)
- `BaseApiController`, `ApiResponse` JSON shape, status-code mapping, the validation filter and the
  `Accept-Language` middleware (`Endpoint/`)
- Jalali formatting (`Calendar/`)

Other framework projects do not yet have dedicated unit-test projects.

See `../../../docs/CURRENT-STATE.md`.
