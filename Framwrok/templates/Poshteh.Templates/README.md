# Poshteh.Templates

قالب‌های `dotnet new` برای ساخت اپلیکیشن بر پایه‌ی چارچوب **پوشته (Poshteh)**.

| قالب | Short Name | Identity |
|---|---|---|
| Poshteh Application | `poshteh` | `Poshteh.Templates.Application` |

```bash
dotnet new poshteh -n Mahtab
```

دستور بالا یک Web API مستقل و قابل اجرا به نام `Mahtab` می‌سازد:
- با Controllerها و Swagger
- با نمونه‌ی کامل CRUD سفارش، شامل فیلتر، مرتب‌سازی و صفحه‌بندی
- با سمت نوشتن و خواندن جدا (CQRS) و تست‌های واحد
- از طریق NuGet به پکیج‌های `Poshteh.*` ارجاع می‌دهد

با `--ModularMonolith` راه‌اندازی برنامه به یک پروژه‌ی Host.Web جدا منتقل می‌شود.

---

## پیش‌نیازها

| مورد | نسخه / توضیح |
|---|---|
| .NET SDK | `10.0.302` یا Patch بالاتر (`rollForward: latestPatch`) |
| پکیج‌های چارچوب | `Poshteh.Core.Domain`، `Poshteh.Core.Contract`، `Poshteh.Core.ApplicationService`، `Poshteh.Infra.Data.Sql.Command`، `Poshteh.Infra.Data.Sql.Query`، `Poshteh.Endpoint.Api` نسخه‌ی `10.0.1` |
| Feed | پکیج‌های `Poshteh.*` روی nuget.org نیستند. Feed داخلی (پوشه یا URL) باید در دسترس باشد. |
| nuget.org | برای EF Core، Swashbuckle، xUnit و وابستگی‌های گذرا |

---

## نصب قالب

### از پوشه‌ی سورس (توسعه‌ی محلی)

```powershell
dotnet new install "<repo>\Framwrok\templates\Poshteh.Templates\content\PoshtehStarter"
```

### از پکیج NuGet

```powershell
# از فایل nupkg محلی
dotnet new install "<path>\Poshteh.Templates.10.0.1.nupkg"

# از Feed (پس از انتشار)
dotnet new install Poshteh.Templates
dotnet new install Poshteh.Templates::10.0.1 --add-source <feed>
```

بررسی نصب:

```powershell
dotnet new list poshteh
dotnet new poshteh --help
```

---

## ساخت اپلیکیشن جدید

```powershell
# پیش‌فرض: بدون Host جدا؛ Endpoint.Api خودش اجرا می‌شود
dotnet new poshteh -n Mahtab --PoshtehFeed "D:\NuGets"
cd Mahtab
dotnet test
dotnet run --project src/03.Endpoints/Mahtab.Endpoint.Api   # → http://localhost:5080/swagger

# Modular Monolith: راه‌اندازی در یک Host جدا؛ Endpoint.Api یک Library است
dotnet new poshteh -n Shop --ModularMonolith --PoshtehFeed "D:\NuGets"
cd Shop
dotnet run --project src/04.Host/Shop.Host.Web             # → http://localhost:5080/swagger
```

### پارامترها

| پارامتر | پیش‌فرض | توضیح |
|---|---|---|
| `-n`, `--name` | نام پوشه‌ی جاری | نام اپلیکیشن؛ جایگزین `PoshtehStarter` در نام Solution، پوشه‌ها، پروژه‌ها، Namespaceها، ProjectReferenceها و فایل‌های تنظیمات می‌شود. |
| `-o`, `--output` | `./<name>` | مسیر خروجی |
| `--PoshtehFeed` | خالی | Feed پکیج‌های `Poshteh.*` (URL یا مسیر پوشه). اگر مقدار داشته باشد، در `NuGet.config` خروجی کنار nuget.org اضافه می‌شود. |
| `-M`, `--ModularMonolith` | `false` | جزئیات در جدول زیر. |
| `--FrameworkVersion` | `10.0.1` | نسخه‌ی پکیج‌های `Poshteh.*` (مقدار `PoshtehFrameworkVersion` در `Directory.Packages.props`) |

تفاوت خروجی با و بدون `--ModularMonolith`:

| | بدون `--ModularMonolith` (پیش‌فرض) | با `--ModularMonolith` |
|---|---|---|
| پروژه‌ی اجرایی | `Endpoint.Api` | `Host.Web` |
| نوع `Endpoint.Api` | Web App (`Microsoft.NET.Sdk.Web`) با `Program.cs`، `appsettings.json`، `launchSettings.json` و فایل `.http` | Library (`Microsoft.NET.Sdk`) |
| پوشه‌ی `src/04.Host` | ساخته نمی‌شود | ساخته می‌شود |

### قواعد نام‌گذاری

- فقط نام‌های متعلق به اپلیکیشن تغییر می‌کنند. نام پکیج‌ها و Namespaceهای چارچوب (`Poshteh.Core.Domain`، `Poshteh.Endpoint.Api` و …) دست نمی‌خورند.
- نام به شکل امن برای Namespace (`safe_namespace`) تبدیل می‌شود. همین شکل هم در نام فایل‌ها و هم در محتوا استفاده می‌شود تا مسیر ProjectReferenceها همیشه درست باشد:

| `-n` | نام پروژه‌ها و Namespace | پوشه‌ی خروجی |
|---|---|---|
| `Mahtab` | `Mahtab.Core.Domain` | `Mahtab/` |
| `Mah.Tab` | `Mah.Tab.Core.Domain` | `Mah.Tab/` |
| `my-app` | `my_app.Core.Domain` | `my-app/` |
| `1App` | `_1App.Core.Domain` | `1App/` |

- نام `Poshteh` مجاز نیست، چون پروژه‌های اپلیکیشن با Package IDهای چارچوب هم‌نام می‌شوند. در این حالت `Directory.Build.targets` در زمان Restore یا Build خطای صریح می‌دهد.
- کلمات کلیدی C# (مثل `class`) را به‌عنوان نام استفاده نکنید.
- اپلیکیشن تولیدشده با نام‌های PascalCase (مثل `Mahtab`) بدون هیچ هشداری Build می‌شود. نام‌هایی که به `_` تبدیل می‌شوند (مثل `my-app`) هشدار CA1707 در Namespace و نام Assembly می‌گیرند.

---

## ساختار اپلیکیشن تولیدشده

```text
Mahtab/
├── Mahtab.slnx
├── Directory.Build.props        # net10.0، Nullable، Analyzers
├── Directory.Build.targets      # جلوگیری از هم‌نامی با پکیج‌های چارچوب
├── Directory.Packages.props     # Central Package Management + PoshtehFrameworkVersion
├── NuGet.config                 # nuget.org (+ Feed پوشته در صورت تعیین)
├── global.json                  # SDK 10.0.302
├── src/
│   ├── 01.Core/
│   │   ├── Mahtab.Core.Domain/                 Aggregate Order          → Poshteh.Core.Domain
│   │   ├── Mahtab.Core.Contract/               Repository Portها، DTOها، PagedResult → Poshteh.Core.Contract
│   │   └── Mahtab.Core.ApplicationService/     Command/Query Handlerها  → Poshteh.Core.ApplicationService
│   ├── 02.Infra/
│   │   ├── Mahtab.Infra.Data.Sql.Command/      سمت نوشتن (EF)           → Poshteh.Infra.Data.Sql.Command
│   │   └── Mahtab.Infra.Data.Sql.Query/        سمت خواندن، فیلتر و صفحه‌بندی → Poshteh.Infra.Data.Sql.Query
│   ├── 03.Endpoints/
│   │   └── Mahtab.Endpoint.Api/                OrdersController (+ Program.cs و Swagger در حالت پیش‌فرض) → Poshteh.Endpoint.Api
│   └── 04.Host/                                ← فقط با --ModularMonolith
│       └── Mahtab.Host.Web/                    Program.cs، Swagger، SQLite
└── tests/
    └── UnitTests/
        └── Mahtab.Core.Domain.Tests/           xUnit
```

جهت وابستگی‌ها همان جهت چارچوب است:
- `Endpoint.Api` (یا `Host.Web`) → `Core.ApplicationService` → `Core.Contract` → `Core.Domain`
- `Infra.Data.Sql.Command` و `Infra.Data.Sql.Query` → `Core.Contract`

### API نمونه

`OrdersController` از `BaseController` چارچوب مشتق شده است:

| متد | مسیر | Use Case |
|---|---|---|
| `GET` | `/api/orders?customerName=&productName=&createdFrom=&createdTo=&sortBy=&descending=&page=&pageSize=` | `GetOrdersQuery` → `PagedResult<OrderSummaryDto>` |
| `GET` | `/api/orders/{id}` | `GetOrderByIdQuery` |
| `POST` | `/api/orders` | `CreateOrderCommand` |
| `PUT` | `/api/orders/{id}` | `UpdateOrderCommand` |
| `DELETE` | `/api/orders/{id}` | `DeleteOrderCommand` |

- **Swagger:** با Swashbuckle در محیط `Development` فعال است. `launchSettings.json` مرورگر را مستقیم روی `/swagger` باز می‌کند.
- **کدهای خطا:** مقدار `pageSize` باید بین 1 و 100 باشد. مقدار نامعتبر کد 422 برمی‌گرداند. سفارش ناموجود 404 برمی‌گرداند.

---

## به‌روزرسانی و حذف قالب

```powershell
# به‌روزرسانی قالب‌های نصب‌شده از Feed
dotnet new update

# نصب نسخه‌ی مشخص
dotnet new install Poshteh.Templates::10.0.2

# حذف (با PackageId یا مسیر پوشه‌ای که نصب شده است)
dotnet new uninstall Poshteh.Templates
dotnet new uninstall "<repo>\Framwrok\templates\Poshteh.Templates\content\PoshtehStarter"
```

---

## تولید و انتشار پکیج NuGet

```powershell
cd Framwrok/templates/Poshteh.Templates
dotnet pack -c Release -o ./artifacts
```

- `PackageId`: `Poshteh.Templates`، `PackageType`: `Template`
- نسخه و مسیر خروجی پیش‌فرض از `Framwrok/Directory.Build.props` خوانده می‌شود (`PoshtehVersion`، `PoshtehPackageOutputPath`). در نتیجه نسخه‌ی قالب همیشه با نسخه‌ی پکیج‌های چارچوب یکی است.
  - تغییر نسخه: `dotnet pack -p:PoshtehVersion=10.0.2`
  - بدون `-o` و در صورت وجود درایو `D:`، خروجی در `D:\NuGets` قرار می‌گیرد.
- فایل‌های `bin/`، `obj/`، `.vs/`، `*.user` و `*.db` داخل `content/` در پکیج قرار نمی‌گیرند.

انتشار (فقط با مجوز صریح):

```powershell
dotnet nuget push ./artifacts/Poshteh.Templates.10.0.1.nupkg --source <feed> --api-key <key>
```

> پیش از انتشار قالب، پکیج‌های `Poshteh.*` هم‌نسخه باید در همان Feed منتشر شده باشند. در غیر این صورت Restore اپلیکیشن تولیدشده شکست می‌خورد.

---

## توسعه و نگهداری قالب

```text
Poshteh.Templates/
├── Poshteh.Templates.csproj     # پروژه‌ی بسته‌بندی (PackageType=Template)
├── README.md                    # همین فایل (README پکیج)
└── content/
    └── PoshtehStarter/          # محتوای قالب (منبع تولید؛ مستقیم Build نمی‌شود)
        ├── .template.config/template.json
        ├── NuGet.config         # حالت پیش‌فرض (بدون Feed)
        ├── NuGet.feed.config    # در صورت تعیین --PoshtehFeed جایگزین NuGet.config می‌شود
        └── ...
```

- **حالت `ModularMonolith`:**
  - در `template.json` (بخش `sources.modifiers`):
    - بدون این پارامتر، پوشه‌ی `src/04.Host/` حذف می‌شود.
    - با این پارامتر، `Program.cs`، `appsettings.json`، `Properties/` و فایل `.http` پروژه‌ی `Endpoint.Api` حذف می‌شوند.
  - بلوک‌های `<!--#if (ModularMonolith) -->` … `<!--#else -->` … `<!--#endif -->` در این فایل‌ها پردازش می‌شوند:
    - `.slnx`
    - `README.md`
    - `Endpoint.Api.csproj`، که بسته به حالت `Sdk.Web` یا `Sdk` می‌گیرد
  - تغییر در `Program.cs` را در هر دو نسخه اعمال کنید: `Endpoint.Api/Program.cs` و `Host.Web/Program.cs`.
- **Build محتوای قالب:** فایل `Endpoint.Api.csproj` در سورس هر دو حالت را دارد، پس `content/PoshtehStarter` مستقیم Build نمی‌شود. برای تست، قالب را از همین پوشه نصب کنید و هر دو حالت را تولید و Build کنید (چک‌لیست پایین).

- **نام پایه:** `PoshtehStarter`، که در `sourceName` و نماد مشتق `AppName` تعریف شده است. هر رشته‌ی `PoshtehStarter` در نام فایل یا محتوا جایگزین می‌شود. رشته‌ی `Poshteh` به‌تنهایی جایگزین **نمی‌شود**.
- `PoshtehStarter` را داخل نام کلاس‌ها به کار نبرید (مثلاً `PoshtehStarterDbContext`)، چون برای نام‌هایی مثل `Mah.Tab` شناسه‌ی نامعتبر C# می‌سازد. فقط در نام پروژه، Namespace، مسیر و تنظیمات استفاده شود.
- **ارتقای نسخه‌ی چارچوب:** مقدار پیش‌فرض `FrameworkVersion` در `template.json` و `PoshtehFrameworkVersion` در `Directory.Packages.props` را با هم تغییر دهید. سپس `PoshtehVersion` را در `Framwrok/Directory.Build.props` به‌روز کنید.
- **ارتقای سایر پکیج‌ها:** نسخه‌های EF Core و xUnit (و Swashbuckle، که فقط در قالب استفاده می‌شود) در `content/PoshtehStarter/Directory.Packages.props` باید با `Framwrok/Directory.Packages.props` هماهنگ بمانند.
- **وابستگی گذرای آسیب‌پذیر:** `System.Security.Cryptography.Pkcs` نسخه‌ی 9.0.11 از مسیر `Poshteh.Infra.Data.Sql.Command → EF Core SqlServer → Microsoft.Data.SqlClient` وارد می‌شود. این پکیج در قالب با `CentralPackageTransitivePinningEnabled` روی `10.0.13` ثابت شده است. اگر این وابستگی در خود چارچوب رفع شود، این Pin را می‌توان حذف کرد.
- **هشدارهای Analyzer:** در `.editorconfig` قالب، CA1707 فقط برای پوشه‌ی `tests/` خاموش است و پسوند `EventHandler` برای CA1711 مجاز شده است. لاگ‌ها با `[LoggerMessage]` نوشته می‌شوند (CA1848/CA1873).
- **چک‌لیست تست پیش از انتشار:**

```powershell
dotnet new uninstall Poshteh.Templates
dotnet new install ./artifacts/Poshteh.Templates.<version>.nupkg
dotnet new poshteh -n Mahtab --PoshtehFeed <feed>
dotnet new poshteh -n TestApplication --ModularMonolith --PoshtehFeed <feed>
# در هر پوشه:
dotnet build; dotnet test
dotnet run --project src/03.Endpoints/Mahtab.Endpoint.Api   # یا src/04.Host/TestApplication.Host.Web؛ سپس /swagger و CRUD را امتحان کنید
# هیچ PoshtehStarter باقی نماند:
Get-ChildItem .\Mahtab -Recurse -File | Select-String PoshtehStarter   # یا: grep -r PoshtehStarter Mahtab
```
