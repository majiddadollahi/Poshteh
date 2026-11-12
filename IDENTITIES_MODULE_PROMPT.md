# تسک: طراحی و توسعه ماژول اختیاری Identities برای فریم‌ورک Poshteh

## 1. هدف

یک ماژول مستقل، قابل استفاده مجدد و اختیاری به نام **Poshteh Identities** بساز که **قابلیت مدیریت سامانه** را از طریق دو پکیج NuGet ارائه کند و با پارامتر `--with-identities` به پروژه تولیدشده از Template اضافه شود، تا این قابلیت در هیچ اپلیکیشن دیگری دوباره پیاده‌سازی نشود.

**مدل دامنه ماژول:** User، Role، Feature و Access و ارتباطات میان آن‌ها.

**خروجی نهایی:** با اضافه کردن ماژول به هر سرویس، تمام APIهای مدیریت این مدل‌ها زیر یک Prefix واحد در همان سرویس در دسترس باشد.

**این ماژول Authentication و Authorization برنامه را انجام نمی‌دهد.** صدور و اعتبارسنجی Token، Login، Refresh Token و Session مسئولیت برنامه میزبان است.

پوشه `Identities/` فقط **مرجع قابلیت‌ها، قوانین کسب‌وکار و رفتار** است. هدف، کپی کردن آن یا انتقال معماری‌اش نیست.

بدون پارامتر، هیچ کد، پکیج، تنظیم، سرویس یا Endpoint مربوط به Identities نباید در خروجی باشد. با پارامتر، ماژول بدون انتقال سورس به پروژه میزبان کار می‌کند.

## 2. نحوه اجرا و Gateها

کار در چهار مرحله انجام می‌شود. **در پایان Gate 1 و Gate 2 متوقف شو و منتظر تأیید صریح بمان.**

| Gate | خروجی | پس از آن |
|---|---|---|
| Gate 1: تحلیل | `Identities-Architecture-Analysis.md` با ماتریس قابلیت‌ها و فهرست تصمیم‌ها | توقف. هیچ کد تولیدی نوشته نشود |
| Gate 2: طراحی | سند تصمیم‌های معماری (ADR)، شامل مرز ماژول با Auth میزبان و سطح عمومی Contracts | توقف |
| Gate 3: پیاده‌سازی | کد، Migration، تست‌ها و دو پکیج محلی | ادامه به Gate 4 |
| Gate 4: یکپارچه‌سازی | اتصال Template، اجرای ماتریس تست، مستندات | گزارش نهایی |

تست‌ها همراه هر بخش نوشته و اجرا شوند، نه فقط در پایان.

نتیجه هر تست باید واقعی و حاصل اجرا باشد. اگر تستی اجرا نشد یا شکست خورد، همان را با خروجی و دلیل گزارش کن.

## 3. واقعیت‌های تأییدشده ریپو

این موارد از سورس استخراج شده‌اند. در ابتدای کار صحت آن‌ها را دوباره بررسی کن و هر مغایرت را گزارش بده.

### 3.1. Poshteh

- کلاس پایه Controller در پشته `BaseController` است (`Poshteh.Endpoint.Api`) و `IResult` برمی‌گرداند. `BaseApiController` متعلق به پروژه مرجع است.
- در Framework و Template هیچ `AddAuthentication`، `AddAuthorization`، `UseAuthentication`، `UseAuthorization` یا سرویس کاربر جاری (Current User) وجود ندارد.
- Serilog و Scalar در پشته وجود ندارند. Template فقط از Swashbuckle استفاده می‌کند. Logging از `ILogger` است.
- `Poshteh.Core.ModuleKernel` پروژه‌ای بدون کد است و قرارداد ماژولی در پشته وجود ندارد.
- `AddApplicationService(params Assembly[])` ثبت‌هایش را با `AddScoped` انجام می‌دهد و Idempotent نیست. فراخوانی دوباره آن Pipeline Behaviorها را تکراری می‌کند.
- `IValidator<T>` به‌صورت خودکار Scan نمی‌شود.
- `AddSqlCommandPersistence<TContext>` یک `BaseCommandDbContext` و یک `IUnitOfWork` سراسری ثبت می‌کند. بنابراین فراخوانی آن برای Context دوم، Context برنامه میزبان را جایگزین می‌کند.
- `ICommand` و `IQuery` در `Poshteh.Core.ApplicationService` هستند، نه در `Poshteh.Core.Contract`.
- پروژه‌های `IsPackable` روی هر Build پک می‌شوند. Workflow انتشار، همه `*.nupkg`ها را به nuget.org می‌فرستد.
- نسخه همه پکیج‌ها یکی است (`PoshtehVersion`) و CI یکسان بودن آن را با Template چک می‌کند.
- پوشه‌های `tests/ArchitectureTests`، `ContractTests` و `IntegrationTests` فعلاً فقط README دارند.
- Template از SQLite، `ConnectionStrings:Default` و `EnsureCreated` استفاده می‌کند و Post Action ندارد.
- Composition Root بسته به `--ModularMonolith` در یکی از دو فایل `Endpoint.Api/Program.cs` یا `Host.Web/Program.cs` است.

### 3.2. پروژه مرجع

- Referenceهای `BuildingBlocks.Data`، `BuildingBlocks.Web`، `PM.Vendor.*`، `PM.Procurement.*` و `PM.Platform.*` و Host اصلی (`PM.Host.RestEndpoint`) در ریپو نیستند و **ارائه هم نخواهند شد**. مرجع اینجا Build نمی‌شود و ماژول نباید به آن‌ها وابسته باشد.
- `PMIdentityDbContext` از `IdentityDbContext<User, Role, long, ...>` ارث می‌برد، نه `BaseCommandDbContext`، و `UserManager` خودش `SaveChanges` را انجام می‌دهد.
- Provider عملاً SQL Server است و فیلترها و Check Constraintها SQL مختص آن دارند.
- جدول تاریخچه Migration بین ماژول‌ها مشترک است (`SharedMigrationsHistory`).
- موجودیت `User` و بخش بزرگی از کد به Vendor و Buyer گره خورده است.
- Routeها `identity/...` و `vendor/Onboarding` هستند. حدود ۶۵ Action وجود دارد که ۱۱ مورد در کنترلرهای اختصاصی Vendor است.
- لایه‌بندی معکوس است (ApplicationService به Infra.DAL و Contract به Domain وابسته است).
- Multi-Tenancy وجود ندارد و فقط یک ثابت Claim به نام `TenantIds` دیده می‌شود.
- مقادیر خاص پروژه و شخص در کد هست: نام کاربری و نام واقعی پیش‌فرض در `BootstrapAdminOptions`، و نام‌های `PM` و `Mahta`.
- طول رمز عبور پیش‌فرض ۶ کاراکتر است.
- Cacheهای دسترسی Singleton و درون‌حافظه‌ای هستند.
- پوشه‌های `bin`، `obj` و `.tmp-build` داخل مرجع، جزو تحلیل نیستند.

### 3.3. سند قبلی

`MANAGEMENT_MODULE_FEASIBILITY.md` فقط ورودی تحلیلی است. هرجا با این پرامپت تعارض دارد (نام Management، چهار پکیج، Minimal API)، این پرامپت ملاک است.

## 4. قوانین الزامی

1. از زیرساخت موجود پشته استفاده کن و پیاده‌سازی موازی نساز.
2. Business Logic در Controller نباشد و DbContext مستقیماً به Controller تزریق نشود.
3. برای قابلیت‌های جدید از Command، Query و Handler پشته استفاده کن. استفاده از `UserManager` و `RoleManager` داخل Handler مجاز است.
4. هر کلاس یا Record در فایل هم‌نام خودش باشد.
5. تغییر هسته پشته باید حداقلی، افزایشی و Backward Compatible باشد و در گزارش با دلیل ثبت شود.
6. قابلیتی از مرجع بدون ثبت وضعیت و دلیل حذف نشود.
7. وابستگی غیرضروری اضافه نکن. پکیج قدیمی `Microsoft.AspNetCore.Identity` 2.x را به کار نبر و از Shared Framework استفاده کن.
8. نام‌ها، Secretها، رمزها و داده‌های پروژه مرجع وارد کد نهایی نشوند. جست‌وجوی `PM.`، `Mahta`، `dadollahi` و `Vendor` در خروجی باید نتیجه‌ای نداشته باشد یا هر مورد توجیه شود.
9. پوشه `Identities/` مرجع و تغییرات غیرمرتبط موجود در Working Tree را تغییر نده و Stage نکن.
10. Commit فقط با درخواست صریح انجام شود.

**بدون مجوز صریح انجام نده:** انتشار پکیج، اجرای Workflow انتشار، بازنویسی گسترده هسته، تغییر تخریبی دیتابیس، یا حذف قابلیت موجود پشته.

## 5. فاز تحلیل (Gate 1)

### 5.1. پروژه مرجع

این موارد را استخراج کن: Entityها، کاربران، نقش‌ها، Featureها، Accessها و ارتباطات آن‌ها، Commandها و Queryها، سرویس‌ها، Validatorها، Repositoryها، Configurationها، Migrationها، Seed و Bootstrap، Cacheها، Localization، رویدادها و همه وابستگی‌های بیرونی.

فهرست کامل Endpointها را با Method، Route فعلی، Route جدید، ورودی، خروجی، الزام دسترسی و وضعیت تهیه کن.

Login، JWT، Refresh Token، Session و جریان‌های وابسته به آن‌ها (مانند `AuthController` و `TokenService`) فقط با وضعیت «خارج از دامنه» در ماتریس ثبت شوند.

برای هر وابستگی به `BuildingBlocks`، Host یا Vendor/Procurement/Platform یکی را مشخص کن: حذف، بازطراحی با امکانات Poshteh، یا Port در Contracts. سورس این بخش‌ها در دسترس نیست و درخواست نشود؛ هر رفتاری که فقط از روی آن‌ها قابل تشخیص است، با طراحی مستقل و ثبت در گزارش حل شود.

### 5.2. Poshteh

معماری واقعی پشته را بررسی کن: Template، پروژه‌ها، Dispatcher و Pipeline، Persistence، Outbox، Exception Handling، Localization، OpenAPI، Packaging و CI.

### 5.3. خروجی

گزارش باید شامل این موارد باشد: معماری هر دو سیستم، ماتریس قابلیت‌ها، وابستگی‌ها، ریسک‌ها، ساختار نهایی و فهرست تصمیم‌های بخش 6 با پیشنهاد تو.

وضعیت هر قابلیت یکی از این‌هاست: **منتقل می‌شود**، **بازطراحی می‌شود**، **به Extension Point تبدیل می‌شود**، **خارج از دامنه با دلیل**.

## 6. تصمیم‌ها

### تصمیم‌های قطعی مالک محصول

| موضوع | تصمیم |
|---|---|
| دامنه | فقط User، Role، Feature و Access |
| Authentication و Authorization برنامه | خارج از ماژول و بر عهده میزبان |
| Vendor و Buyer | خارج از ماژول. فیلدها، Endpointها و Workflowهای آن‌ها منتقل نمی‌شوند |
| `BuildingBlocks` و Host مرجع | وابستگی و درخواست سورس ممنوع |
| Provider | در نسخه اول فقط SQL Server. افزودن Provider دیگر کار آینده است |

### پیش‌فرض‌ها (تأیید در Gate 1)

این پیش‌فرض‌ها از شواهد ریپو آمده‌اند. اگر تحلیل خلاف آن‌ها را نشان داد، با دلیل پیشنهاد جایگزین بده.

| موضوع | پیش‌فرض نسخه اول |
|---|---|
| دیتابیس در Template | Connection جدا با نام `ConnectionStrings:Identities`. برنامه نمونه روی SQLite می‌ماند |
| Schema | ثابت `identity`. پیکربندی Schema در نسخه اول ارائه نمی‌شود، چون Migration کامپایل‌شده Schema را در خود دارد |
| Tenancy | تک‌مستأجر، کاربران سراسری. محدودیت مستند شود |
| نوع UserId | `long`، مطابق مرجع؛ این نوع در Contracts تثبیت می‌شود |
| نسخه‌بندی | هم‌گام با `PoshtehVersion` |
| Routeها | Prefix پیش‌فرض `identity` و قابل تغییر در هر پروژه (بخش 10). جدول نگاشت Routeهای مرجع در مستندات می‌آید |
| داده موجود PM | مهاجرت آن خارج از دامنه است. پکیج با یک `InitialCreate` تمیز شروع می‌شود |
| سورس | `Framwrok/src/06.Modules/` و تست‌ها زیر `Framwrok/tests/` |
| مستندات | `docs/identities/` |

## 7. معماری هدف و پکیج‌ها

Modular Monolith، اجرا در همان فرایند میزبان، و بدون Microservice یا سیستم Plugin جدید.

### 7.1. `Poshteh.Identities.Contracts`

شامل Interfaceهای عمومی، DTOها، Enumها و رویدادهای عمومی. نام‌ها طبق استاندارد پشته باشد (نمونه: `IIdentityUserReader`، `IIdentityPermissionService`).

- فقط به `Poshteh.Core.Contract` یا پایین‌تر وابسته باشد.
- به EF Core، ASP.NET Identity، JWT و `Poshteh.Core.ApplicationService` وابسته نباشد.
- Commandها و Queryها به‌صورت پیش‌فرض داخل Implementation می‌مانند، چون `ICommand` در ApplicationService است. اگر نیاز واقعی به عمومی شدن وجود داشت، در Gate 2 مطرح کن.

### 7.2. `Poshteh.Identities`

پیاده‌سازی: Domain، Handlerها، Validatorها، Repositoryها، DbContext، Migrationها، Controllerها، Registration و Resourceهای Localization (با اطمینان از قرار گرفتن در پکیج).

### 7.3. ساختار داخلی

پیش‌فرض، **دو پروژه سورس** با تفکیک Domain، Application، Infrastructure و Api در پوشه و Namespace است.

مرزها با تست معماری کنترل شوند. وابستگی Entityها به ASP.NET Identity استثنای مستند این ماژول است.

وابستگی‌های پکیج و اجرا باید با نصب واقعی `.nupkg` در پروژه‌ای خارج از Solution تأیید شوند.

## 8. Persistence و Migration

1. `IdentitiesDbContext` از `IdentityDbContext<...>` ارث ببرد.
2. **`AddSqlCommandPersistence` برای آن فراخوانی نشود** و `IUnitOfWork` یا `BaseCommandDbContext` میزبان تغییر نکند.
3. رفتار `TransactionBehavior` سراسری روی Commandهای Identities بررسی شود. Handlerهایی که چند عملیات اتمیک دارند، تراکنش صریح روی `IdentitiesDbContext` بگیرند.
4. ConnectionString، و `DbContextOptions` در حد لازم، از Composition Root قابل تنظیم باشد. نبود ConnectionString خطای روشن در Startup بدهد.
5. Migrationها در اسمبلی `Poshteh.Identities` باشند، `MigrationsAssembly` صریح تنظیم شود و جدول تاریخچه اختصاصی در Schema `identity` داشته باشد.
6. Foreign Key از سایر ماژول‌ها به جدول‌های Identities الزامی نباشد.
7. Application و Domain سایر ماژول‌ها از Contracts استفاده کنند. دسترسی مستقیم به DbContext جزو API پایدار نیست و در مستندات به‌عنوان استثنای Infrastructure با هشدار داده‌های حساس می‌آید.
8. در Production، Migration هنگام Startup اجرا نشود. تولید Script و Bundle از پروژه میزبانی که فقط NuGet را دارد، واقعاً تست شود (از جمله نیاز به `Microsoft.EntityFrameworkCore.Design`).
9. `EnsureCreated` برای Identities استفاده نشود.
10. اگر کسی دیتابیس Identities را با دیتابیس برنامه نمونه یکی کند، `EnsureCreated` برنامه ممکن است به‌دلیل وجود جدول‌ها جدول‌های خودش را نسازد. این حالت تست یا مستند شود.

## 9. Seed و راه‌اندازی اولیه

- مرحله Seed از Migration جدا و قابل اجرا در Deployment باشد و روی دیتابیس Migrate‌نشده خطای روشن بدهد.
- ایجاد اولین مدیر پیش‌فرض خاموش باشد، نام یا رمز پیش‌فرض نداشته باشد و Idempotent باشد.
- همگام‌سازی کاتالوگ Access و Feature به‌گونه‌ای طراحی شود که **Endpointهای برنامه میزبان** هم بتوانند Permission تعریف کنند.

## 10. API

1. Controllerها از `BaseController` ارث ببرند. قالب پاسخ همان Result و ProblemDetails پشته باشد و `ApiResponse` مرجع منتقل نشود.
2. Prefix پیش‌فرض `identity` است و هر پروژه می‌تواند آن را تغییر دهد (مثلاً `admin/identity`). Controllerها فقط Route نسبی دارند و Prefix با یک Convention محدود به Controllerهای همین ماژول اعمال می‌شود. مقدار از این ترتیب خوانده شود: تنظیم در کد، سپس بخش `Identities:RoutePrefix` در Configuration، سپس پیش‌فرض. مقدار خالی یا نامعتبر در Startup خطای روشن بدهد و Swagger باید Prefix نهایی را نشان دهد.
3. **ماژول فقط با فراخوانی `AddPoshtehIdentities` فعال شود.** SDK وب ممکن است Controllerهای یک پکیج مرجع را خودکار کشف کند؛ حالت «پکیج نصب شده ولی ثبت نشده» باید تست شود و Endpointی ظاهر نشود.
4. چون `BaseController` مقدار `IResult` برمی‌گرداند، قرارداد پاسخ‌ها با `ProducesResponseType` یا معادل آن صریح شود تا Swagger Schema کامل داشته باشد.
5. برخورد Route با میزبان باید خطای قابل تشخیص یا تست داشته باشد.
6. متد جداگانه‌ای مانند `MapPoshtehIdentities` وجود نداشته باشد؛ `MapControllers` موجود در میزبان کافی است.

## 11. Dependency Injection

ماژول مسئول تمام ثبت‌های خودش است و میزبان **فقط یک Extension** را در `Program.cs` صدا می‌زند:

```csharp
builder.Services.AddPoshtehIdentities(builder.Configuration);

// اختیاری: تغییر تنظیمات در کد
builder.Services.AddPoshtehIdentities(builder.Configuration, options =>
{
    options.RoutePrefix = "admin/identity";
});
```

این متد به‌تنهایی همه موارد را ثبت می‌کند: DbContext، `AddIdentityCore`، Handlerها، Validatorها، Repositoryها، سرویس‌ها، Interfaceهای Contracts، Options، Controllerها (Application Part)، Convention مربوط به Prefix، Policy و Localization. میزبان نباید هیچ سرویس، Assembly یا Controller ماژول را جداگانه ثبت کند.

تنها موارد خارج از این متد، `MapControllers` و Middlewareهای Auth هستند (`UseAuthentication` و `UseAuthorization`)، چون بخشی از Pipeline و Auth میزبان‌اند و Template آن‌ها را تولید می‌کند.

### شیوه بهینه ثبت

1. متد عمومی فقط یکی باشد و داخل آن متدهای `internal` جدا برای هر بخش (Persistence، Application، Api) صدا زده شوند.
2. سرویس‌های ماژول **صریح** ثبت شوند. Reflection فقط برای Handlerها و Validatorهای **Assembly همین ماژول** و با مکانیزم پشته انجام شود؛ Scan کل AppDomain یا Assemblyهای میزبان ممنوع است.
3. فراخوانی کور `AddApplicationService` از داخل ماژول ممنوع است. یک راه‌حل **افزایشی** در هسته انتخاب کن که هم زیرساخت مشترک را با `TryAdd` ثبت کند و هم Handlerهای یک Assembly را Scan کند، مثلاً متد عمومی جدید یا Idempotent کردن ثبت Behaviorها.
4. رفتار مستقل از ترتیب فراخوانی نسبت به `AddApplicationService` و `AddEndpointApi` میزبان باشد و ماژول حتی بدون فراخوانی قبلی آن‌ها زیرساخت لازم را داشته باشد.
5. فراخوانی دوباره متد، ثبت تکراری ایجاد نکند (Marker داخلی).
6. درون متد `BuildServiceProvider` ممنوع است. مقادیر وابسته به تنظیمات با Options Pattern و `IConfigureOptions` خوانده شوند.
7. Options با `ValidateOnStart` اعتبارسنجی شوند تا خطاها هنگام Startup دیده شوند، نه در اولین درخواست.
8. Lifetime پیش‌فرض Scoped باشد و Singleton فقط برای Cache و سرویس‌های بدون وابستگی Scoped.
9. فقط سرویس‌های Extension Point با `TryAdd` ثبت شوند تا میزبان بتواند جایگزینشان کند. سرویس‌های داخلی `internal` بمانند.
10. Validatorها طبق الگوی موجود پشته ثبت شوند.
11. Localization با `AddPoshtehMessageResources` و Prefix اختصاصی ثبت شود.
12. تعداد هر Behavior و Handler در تست شمرده شود.
- کل گراف با `ValidateOnBuild` و `ValidateScopes` تست شود.
- اگر چند Instance اجرا می‌شود، Cache دسترسی باید Invalidation معتبر داشته باشد یا محدودیت آن مستند شود.

## 12. مرز ماژول با Authentication و Authorization میزبان

ماژول Login، Token، Refresh Token، Session و Authentication Scheme ندارد و هیچ تنظیم Auth میزبان را تغییر نمی‌دهد. وابستگی به JWT نباید وارد پکیج‌ها شود.

ارتباط با میزبان از دو مسیر است:

- **مصرف داده:** Auth میزبان، اطلاعات کاربر، نقش، Feature و Access را از Interfaceهای Contracts می‌خواند.
- **حفاظت APIها:** میزبان نام یک Authorization Policy را در تنظیمات ماژول مشخص می‌کند و همه Controllerها به آن Policy نیاز دارند.

الزامات:

1. APIهای مدیریتی **Fail-Closed** باشند: اگر Policy تنظیم نشده یا میزبان Authentication ندارد، هیچ داده‌ای برنگردد و برنامه در Startup هشدار روشن ثبت کند. نصب ماژول هرگز نباید این APIها را عمومی کند.
2. هیچ Action بدون الزام Policy یا با `AllowAnonymous` وجود نداشته باشد و تستی این را کنترل کند.
3. Template در حالت `--with-identities` حداقل ثبت و Middleware لازم را تولید کند و ترتیب آن قبل از `MapControllers` باشد. برنامه تولیدشده باید بالا بیاید، حتی وقتی APIها تا اتصال Auth میزبان بسته‌اند.
4. Endpointهای مبتنی بر کاربر جاری (مانند دسترسی‌های من) شناسه کاربر را از `HttpContext.User` با نام Claim قابل تنظیم بخوانند.
5. از `AddIdentityCore`، `UserManager` و Password Hasher استاندارد برای مدیریت کاربر استفاده شود. فقط AddIdentity یا Cookie/Scheme ثبت نشود.
6. Password Policy قابل تنظیم باشد و پیش‌فرض آن از ۶ کاراکتر مرجع قوی‌تر باشد.
7. جلوگیری از Privilege Escalation و حذف یا غیرفعال‌سازی آخرین مدیر.
8. Audit عملیات حساس از طریق Port عمومی انجام شود، چون سرویس Audit مرجع در `PM.Platform` است.
9. PasswordHash و داده حساس در هیچ DTO یا Contract نباشد.

## 13. Extension Pointها و رویدادها

فقط موارد دارای نیاز واقعی: رویدادهای ایجاد و غیرفعال‌سازی کاربر، تغییر نقش و مجوز، Password Policy، و Port بخش 12.

`IdentitiesDbContext` جدول Outbox پشته را ندارد. تضمین تحویل رویدادها را مشخص کن: یا تحویل In-Process پس از Commit با اعلام محدودیت، یا طراحی Outbox اختصاصی در ADR.

## 14. Template

- Symbol از نوع bool با نام `WithIdentities` و پیش‌فرض `false`. نام خط فرمان `--with-identities` با `.template.config/dotnetcli.host.json` تعریف شود، چون خط‌تیره در عبارت‌های شرطی قابل استفاده نیست.
- شرط‌ها روی `Directory.Packages.props`، `.csproj`، `appsettings.json` و **هر دو** `Program.cs` اعمال شوند.
- `Poshteh.Identities` فقط به پروژه Composition Root اضافه شود. Contracts فقط به پروژه‌ای که واقعاً مصرفش می‌کند.
- نسخه پکیج‌ها از `$(PoshtehFrameworkVersion)` بیاید و Floating نباشد.
- حداقل نسخه Framework که Identities دارد مستند شود.
- سورس Identities در پروژه تولیدشده کپی نشود.
- در `Program.cs` برای ثبت ماژول فقط یک خط `AddPoshtehIdentities` تولید شود.
- `appsettings.json` بخش `Identities` را با `RoutePrefix` برابر `identity` داشته باشد تا محل تغییر آن مشخص باشد.
- Default مقادیر عددی جدید `template.json` با چک نسخه CI تداخل نداشته باشد.

## 15. Packaging و انتشار

- `Directory.Build.props/targets` و Central Package Management پشته استفاده شود و متادیتا و `PACKAGE.md` کامل باشد.
- پیش از افزودن پروژه‌ها به `Poshteh.slnx`، گزارش کن که اجرای بعدی Workflow این پکیج‌ها را به nuget.org می‌فرستد.
- تست‌ها فقط با Feed محلی و `--PoshtehFeed` انجام شوند.

## 16. تست‌ها

Template در این ماتریس تولید، Build و اجرا شود:

| | بدون Identities | با Identities |
|---|---|---|
| حالت پیش‌فرض | تست شود | تست شود |
| `--ModularMonolith` | تست شود | تست شود |

حداقل یک اجرا با نام دارای خط‌تیره باشد (مثل `my-app`).

| سناریو | بررسی‌ها |
|---|---|
| A. بدون Identities | Build و اجرا، نبود PackageReference، تنظیمات، سرویس و Endpoint، و خروجی معادل Template قبلی |
| B. با Identities | نصب از Feed محلی، بارگذاری بدون سورس، کشف Controllerها، Prefix، Swagger با Schema، DI، اعمال Migration روی SQL Server واقعی، و CRUD کامل User، Role، Feature و Access با یک Authentication Scheme آزمایشی |
| C. Contracts | ماژول نمونه فقط با Contracts کار کند و گراف پکیجش EF یا ASP.NET Identity نداشته باشد |
| D. امنیت | پاسخ 401 و 403 با Policy میزبان، بسته بودن APIها وقتی Policy یا Auth تنظیم نشده، نبود Action بدون Policy، Escalation، و تغییر نکردن Schemeها و تنظیمات Auth میزبان |
| E. ارتقا | دو نسخه آزمایشی با دو Migration؛ ارتقای پکیج و Schema بدون از دست رفتن داده |
| F. پوشش مرجع | ماتریس قابلیت با وضعیت: پیاده‌سازی و تست‌شده، نیازمند تست، عمداً تغییر یافته، خارج از دامنه |
| G. هم‌زیستی پشته | Command برنامه نمونه همچنان Commit می‌شود، Behaviorها تکراری نیستند، و پکیج نصب‌شده بدون ثبت غیرفعال است |
| H. ثبت و Prefix | Prefix پیش‌فرض، تغییر از `appsettings`، تغییر از کد، رد مقدار نامعتبر، نمایش Prefix نهایی در Swagger؛ کار کردن فقط با همان یک Extension؛ فراخوانی قبل و بعد از `AddApplicationService`؛ فراخوانی دوباره بدون ثبت تکراری؛ و اعتبارسنجی کامل گراف DI |

## 17. مستندات

در `docs/identities/`: `Identities-Architecture-Analysis.md`، `Identities-Implementation.md`، `Identities-Integration.md`، `Identities-Migrations.md`، `Identities-Testing.md` (با نتایج واقعی) و `Identities-Release.md`.

## 18. معیار پذیرش

- همه قابلیت‌های مرجع وضعیت ثبت‌شده دارند.
- دو NuGet تولید شده و در پروژه خارج از Solution کار می‌کنند.
- با یک فراخوانی `AddPoshtehIdentities`، همه APIهای مدیریت User، Role، Feature و Access با Prefix پیش‌فرض یا پیکربندی‌شده در سرویس میزبان در دسترس و محافظت‌شده‌اند.
- ماژول هیچ Login، Token یا Authentication Scheme ندارد و تنظیمات Auth میزبان را تغییر نمی‌دهد.
- هر چهار حالت ماتریس Template و سناریوهای A تا H اجرا و نتیجه‌شان ثبت شده است.
- تغییرات هسته افزایشی‌اند و تست‌های موجود پشته سبز هستند.
- هیچ داده یا نام خاص پروژه مرجع در خروجی نیست.
- پکیجی منتشر نشده و هیچ تغییر تخریبی یا مشکل بازی بدون گزارش نمانده است.

**هدف نهایی، ماژولی پایدار و قابل استفاده در پروژه‌های متعدد پشته است؛ نه صرفاً انتقال پروژه فعلی به یک NuGet.**
