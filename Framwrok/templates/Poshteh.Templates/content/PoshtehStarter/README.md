# PoshtehStarter

این اپلیکیشن با قالب `dotnet new poshteh` ساخته شده و بر پایه‌ی چارچوب **پوشته (Poshteh)** است: DDD، CQRS (جدا بودن سمت نوشتن و خواندن)، Unit of Work با Transactional Outbox و نگاشت یکسان `Result` به HTTP.

## پیش‌نیازها

- .NET SDK `10.0.302` یا Patch بالاتر (طبق `global.json`)
- دسترسی به Feedی که پکیج‌های `Poshteh.*` نسخه‌ی `10.0.1` را دارد. این پکیج‌ها روی nuget.org منتشر نشده‌اند. Feed را در [NuGet.config](NuGet.config) یا در NuGet.config سطح کاربر اضافه کنید.

## ساخت، تست و اجرا

<!--#if (ModularMonolith) -->
```bash
dotnet build
dotnet test
dotnet run --project src/04.Host/PoshtehStarter.Host.Web
```

<!--#else -->
```bash
dotnet build
dotnet test
dotnet run --project src/03.Endpoints/PoshtehStarter.Endpoint.Api
```

<!--#endif -->
- برنامه روی `http://localhost:5080` اجرا می‌شود و مرورگر مستقیم روی **Swagger UI** (`/swagger`) باز می‌شود.
- Swagger فقط در محیط `Development` فعال است. این محیط در `launchSettings.json` تنظیم شده است.
- پایگاه‌داده‌ی SQLite (`PoshtehStarter.db`) در اولین اجرا ساخته می‌شود.
<!--#if (ModularMonolith) -->
- نمونه‌ی همه‌ی درخواست‌ها در [PoshtehStarter.Host.Web.http](src/04.Host/PoshtehStarter.Host.Web/PoshtehStarter.Host.Web.http) آمده است.
<!--#else -->
- نمونه‌ی همه‌ی درخواست‌ها در [PoshtehStarter.Endpoint.Api.http](src/03.Endpoints/PoshtehStarter.Endpoint.Api/PoshtehStarter.Endpoint.Api.http) آمده است.
<!--#endif -->

## API نمونه (CRUD سفارش)

| متد | مسیر | توضیح |
|---|---|---|
| `GET` | `/api/orders` | فهرست با فیلتر، مرتب‌سازی و صفحه‌بندی |
| `GET` | `/api/orders/{id}` | یک سفارش با اقلام آن (404 اگر نباشد) |
| `POST` | `/api/orders` | ایجاد؛ شناسه‌ی سفارش را برمی‌گرداند |
| `PUT` | `/api/orders/{id}` | جایگزینی نام مشتری و اقلام |
| `DELETE` | `/api/orders/{id}` | حذف سفارش و اقلام آن |

پارامترهای Query String در `GET /api/orders` (همه اختیاری):

| پارامتر | توضیح |
|---|---|
| `customerName` | بخشی از نام مشتری |
| `productName` | بخشی از نام یکی از کالاهای سفارش |
| `createdFrom` / `createdTo` | بازه‌ی تاریخ ایجاد (UTC) |
| `sortBy` | `Id` (پیش‌فرض)، `CustomerName` یا `CreatedDate` |
| `descending` | `true` برای مرتب‌سازی نزولی |
| `page` | شماره‌ی صفحه، از 1 (پیش‌فرض 1) |
| `pageSize` | 1 تا 100 (پیش‌فرض 10) |

پاسخ فهرست به این شکل است:

```json
{ "items": [ ... ], "page": 1, "pageSize": 10, "totalCount": 42, "totalPages": 5, "hasPreviousPage": false, "hasNextPage": true }
```

**خطاها** با ProblemDetails برگردانده می‌شوند: «NotFound» → 404، «Validation» → 422، سایر خطاهای دامنه → 400.

## ساختار

| پروژه | لایه | محتوا |
|---|---|---|
| `src/01.Core/PoshtehStarter.Core.Domain` | دامنه | Aggregate `Order`، Invariantها، Domain Eventها |
| `src/01.Core/PoshtehStarter.Core.Contract` | قراردادها | `IOrderCommandRepository`، `IOrderQueryRepository`، DTOها، `PagedResult<T>` |
| `src/01.Core/PoshtehStarter.Core.ApplicationService` | Use Caseها | Command/Queryها و Handlerها، `OrderErrors` |
| `src/02.Infra/PoshtehStarter.Infra.Data.Sql.Command` | سمت نوشتن | `OrderingDbContext`، پیکربندی EF، `OrderCommandRepository` |
| `src/02.Infra/PoshtehStarter.Infra.Data.Sql.Query` | سمت خواندن | `OrderingQueryDbContext` (No-Tracking)، Read Modelها، `OrderQueryRepository` (فیلتر و صفحه‌بندی) |
<!--#if (ModularMonolith) -->
| `src/03.Endpoints/PoshtehStarter.Endpoint.Api` | API | `OrdersController` (Library) |
| `src/04.Host/PoshtehStarter.Host.Web` | Host | `Program.cs`، Swagger، تنظیمات، SQLite |
<!--#else -->
| `src/03.Endpoints/PoshtehStarter.Endpoint.Api` | API و Host | `OrdersController`، `Program.cs`، Swagger، تنظیمات، SQLite |
<!--#endif -->
| `tests/UnitTests/PoshtehStarter.Core.Domain.Tests` | تست | تست‌های واحد دامنه (xUnit) |

نسخه‌ی پکیج‌ها به‌صورت مرکزی در [Directory.Packages.props](Directory.Packages.props) مدیریت می‌شود. برای ارتقای چارچوب کافی است `PoshtehFrameworkVersion` را تغییر دهید.

## افزودن یک موجودیت جدید

1. **Domain:** Aggregate و Invariantهایش را بنویسید (ارث‌بری از `AggregateRoot`، بازگرداندن `Result`).
2. **Contract:** Repositoryهای نوشتن و خواندن، DTOها و معیار جست‌وجو را تعریف کنید.
3. **ApplicationService:** Commandها، Queryها و Handlerها را اضافه کنید. Handlerها خودکار ثبت می‌شوند.
   - Commit را `TransactionBehavior` انجام می‌دهد. فقط Create که شناسه‌ی تولیدشده را لازم دارد، خودش `CommitAsync` را صدا می‌زند.
4. **Infra:**
   - سمت نوشتن: پیکربندی EF و Repository مشتق از `CommandRepository<T>`.
   - سمت خواندن: Read Model، نگاشت در `OrderingQueryDbContext` و Repository خواندن.
5. **Endpoint:** یک Controller مشتق از `BaseController` بسازید و Repositoryها را در `Program.cs` ثبت کنید.

برای SQL Server یا PostgreSQL، در `Program.cs` به‌جای `AddSqlCommandPersistence` و `AddSqlQueryPersistence` از نسخه‌های `AddSqlServer…` یا `AddPostgres…` استفاده کنید. در محیط واقعی هم به‌جای `EnsureCreated` از EF Migrations استفاده کنید.
