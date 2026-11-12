# تحلیل معماری ماژول Poshteh Identities (Gate 1)

تاریخ: ۲۰۲۶-۱۱-۰۹. وضعیت: **منتظر تأیید مالک محصول.** در این مرحله هیچ کد تولیدی نوشته نشده است.

## 1. خلاصه

ساخت ماژول شدنی است و تعارض بنیادی با پشته ندارد. از ۶۳ Action مرجع، ۴۹ مورد به ماژول می‌آیند (منتقل یا بازطراحی) و ۱۴ مورد خارج از دامنه‌اند.

مهم‌ترین نتایج تحلیل:

1. **پرامپت چند نکته را درست پیش‌بینی کرده، ولی پنج مانع فنی تازه هم پیدا شد** که بدون تصمیم، معیارهای پذیرش برآورده نمی‌شوند (بخش 2.3).
2. **مرجع چند نقص امنیتی واقعی دارد** که نباید منتقل شوند، از جمله این‌که نقش غیرفعال همچنان دسترسی می‌دهد و هیچ محافظتی برای آخرین مدیر و در برابر Escalation وجود ندارد (بخش 5).
3. **یک خلأ در دامنه:** ماژول Login ندارد و `PasswordHash` هم نباید بیرون بیاید. بنابراین میزبان راهی برای بررسی رمز ندارد، مگر این‌که Contracts یک Port بررسی Credential داشته باشد (تصمیم T1).
4. **نه تصمیم در بخش 11 منتظر پاسخ شماست.** پیش‌فرض‌های بخش 6 پرامپت همگی با شواهد سازگارند و تغییری برای آن‌ها پیشنهاد نمی‌شود.

خط مبنای تست پشته پیش از هر تغییر: `dotnet test Framwrok/Poshteh.slnx -c Release` نتیجه ۱۴ موفق، ۰ ناموفق.

## 2. راستی‌آزمایی واقعیت‌های پرامپت

### 2.1. تأییدشده بدون مغایرت

همه موارد بخش 3.1 و 3.2 با سورس تطبیق داده شدند. جز موارد جدول بعد، بقیه درست‌اند.

### 2.2. مغایرت‌ها و دقیق‌سازی‌ها

| ادعای پرامپت | یافته | اثر |
|---|---|---|
| حدود ۶۵ Action | **۶۳ Action و ۶۴ Route** (`Submit` دو Route دارد) | فقط عدد |
| ۱۱ Action در کنترلرهای Vendor | **۱۰ Action و ۱۱ Route** (`Vendors` هفت، `VendorOnboarding` سه) | فقط عدد |
| `AuthController` شامل Login است (بخش 5.1) | این کنترلر فقط `register-buyer` و `register-vendor` را دارد. Actionهای `Auth.Login/Logout/RefreshToken` فقط در کاتالوگ نام برده شده‌اند و احتمالاً در Host غایب هستند. منطق آن‌ها در `UserService` است | هر دو Action موجود به‌دلیل Buyer و Vendor خارج از دامنه‌اند |
| `AddApplicationService` همه را با `AddScoped` ثبت می‌کند | `IClock` با `AddSingleton` ثبت می‌شود، بقیه درست است | ناچیز |
| Schema مرجع | در Migration مرجع `Identity` با حرف بزرگ است؛ هدف `identity` است | بدون مشکل، چون `InitialCreate` تمیز است |

نکته‌های تکمیلی راستی‌آزمایی:

- `BaseEntity.Id` از نوع `long` و Setter آن `protected` است؛ با `long` بودن UserId سازگار است.
- `Directory.Build.props` مقدار `Majid Dadollahi` و نشانی `majiddadollahi` را دارد. این نتیجه‌ی جست‌وجوی قانون 8 از قبل وجود داشته و متادیتای مالک پشته است؛ موجه است و ماژول چیزی به آن اضافه نمی‌کند.
- Build پیش‌فرض روی این ماشین پکیج‌ها را در `D:\NuGets` می‌ریزد. در Gate 3 باید همیشه `PoshtehPackageOutputPath` به مسیر موقت Override شود تا Feed شخصی آلوده نشود.
- نسخه‌ی Working Tree برابر `10.0.3` و هنوز Commit نشده است. آخرین Tag برابر `v10.0.2` است.
- SQL Server واقعی برای تست در دسترس است: کانتینرهای `sql2022` (پورت میزبان ۱۴۳۳۰) و `sql2019`.

### 2.3. موانع فنی تازه

| # | مانع | شاهد | پیشنهاد |
|---|---|---|---|
| M1 | **`AddApplicationService` وابسته به ترتیب است.** اگر ماژول زیرساخت را با `TryAdd` ثبت کند و میزبان بعداً `AddApplicationService` را صدا بزند، Behaviorها دو بار ثبت می‌شوند. پس تغییر فقط سمت ماژول کافی نیست | `Poshteh.Core.ApplicationService/DependencyInjection/ServiceCollectionExtensions.cs:60-63` | در هسته: `TryAddEnumerable` برای Behaviorها و `TryAdd` برای زیرساخت، به‌علاوه متد عمومی جدید برای Scan یک Assembly |
| M2 | **تنظیمات Localization وابسته به ترتیب است.** اگر ماژول زودتر `AddPoshtehLocalization` را صدا بزند، `configure` بعدی میزبان بی‌صدا نادیده گرفته می‌شود | همان فایل `:26-29` (`TryAddSingleton(options)`) | تغییر افزایشی: انباشت `configure`ها به‌جای یک Instance ثابت |
| M3 | **نگاشت خطا 403 و Conflict برای کدهای مرجع ندارد.** کد حاوی `Forbidden` به 400 می‌رسد و `Identity.UserAlreadyExists` هم 400 می‌شود | `ResultEndpointExtensions.cs:47-66` | افزودن `Forbidden` به 403 در هسته (افزایشی) و نام‌گذاری کدهای جدید با `Conflict`، `NotFound` و `Validation` |
| M4 | **حالت «نصب‌شده ولی ثبت‌نشده».** SDK وب برای هر Assembly وابسته به MVC یک `ApplicationPartAttribute` می‌سازد و بدون اجرای کد ماژول، Controllerهای `public` کشف می‌شوند. هیچ منطقی در ماژول فرصت جلوگیری ندارد | رفتار `GenerateMvcApplicationPartsAssemblyAttributes` در Web SDK | Controllerها `internal` باشند (کشف‌نشدنی برای provider پیش‌فرض) و `AddPoshtehIdentities` یک `ControllerFeatureProvider` اختصاصی ثبت کند. در ابتدای Gate 3 با Spike تأیید می‌شود |
| M5 | **بدون Scheme، پاسخ 401 ممکن نیست.** `UseAuthorization` برای Challenge بدون Default Scheme استثنا می‌دهد و نتیجه 500 می‌شود (بسته است ولی 401 نیست). ماژول هم حق ثبت Scheme ندارد | بخش 12 پرامپت و رفتار `AuthenticationService` | Template یک Scheme مسدودکننده‌ی کوچک در **پروژه تولیدشده** بسازد که همیشه `NoResult` می‌دهد (T4) |

## 3. معماری دو سیستم

### 3.1. پشته Poshteh

| بخش | واقعیت |
|---|---|
| لایه‌ها | `Shared.Kernel`، `Core.Domain`، `Core.Contract`، `Core.ApplicationService`، `Infra.Data.Sql.Command/Query`، `Endpoint.Api`، `Host.Web/Worker`، `Testing.Kit`، با `net10.0` و CPM |
| Dispatcher | `CommandDispatcher` و `QueryDispatcher` با Reflection؛ Handlerها و Behaviorها از DI |
| ترتیب Pipeline | Logging، Validation، ExceptionHandling، Transaction، Handler |
| Transaction | پس از موفقیت Command، `IUnitOfWork` میزبان را Commit می‌کند؛ اگر `IUnitOfWork` نباشد رد می‌شود |
| Persistence | `BaseCommandDbContext` با Outbox؛ رویدادها پس از Commit و Best-Effort توزیع می‌شوند |
| پاسخ API | `BaseController` مقدار `IResult` می‌دهد. چون Commandها `Result<Unit>` هستند، موفقیت 200 با بدنه `{}` است نه 204. موفقیت‌ها هم 201 ندارند |
| Exception | `ExceptionHandlingBehavior` و `GlobalExceptionHandler` با ProblemDetails |
| Localization | Provider با مالکیت Prefix؛ تکرار Prefix خطای Startup می‌دهد، پس ثبت دوباره ماژول بدون Marker برنامه را می‌خواباند |
| OpenAPI | فقط Swashbuckle در Template. `IResult` برای ApiExplorer Schema تولید نمی‌کند |
| Packaging و CI | Pack روی هر Build؛ Workflow همه `*.nupkg`ها را Push می‌کند و نسخه‌ها را با `grep` مقایسه می‌کند |
| تست | ۱۴ تست واحد. پوشه‌های Architecture، Contract و Integration خالی‌اند |

### 3.2. مرجع Identities

چهار لایه‌ی `Domain`، `Contract`، `ApplicationService` و `Infra.DAL` به‌علاوه `EndpointApi`، با ۲۲۸ فایل سورس. منطق در سرویس‌های کلاسیک است و CQRS فقط برای Workflowهای Vendor و Buyer استفاده شده. لایه‌بندی معکوس است و قواعد اصلی کسب‌وکار داخل سرویس‌ها با `UserManager` و DbContext ترکیب شده‌اند.

## 4. موجودی مرجع

| دسته | موارد | وضعیت |
|---|---|---|
| Entityهای هسته | `User`، `Role`، `UserRole`، `UserClaim`، `RoleClaim`، `UserLogin`، `UserToken`، `Feature`، `Access`، `RoleFeature`، `FeatureAccess` | بازطراحی می‌شود (فیلدهای Vendor حذف) |
| Tokenها | `RefreshToken`، `UserSession`، `PasswordResetToken`، `EmailVerificationToken` | خارج از دامنه: Auth میزبان |
| Vendor | `VendorReviewEntry`، `VendorAccountKind`، `VendorReviewStatus` | خارج از دامنه: تصمیم قطعی مالک |
| `AccountType` (نه مقدار Panel/Buyer/Vendor) | | خارج از دامنه. کاربر عمومی می‌شود و نوع کسب‌وکاری در ماژول میزبان می‌ماند |
| `AccessPanel`، `AccessRequirements` | مفاهیم Buyer و Vendor | خارج از دامنه |
| `AccessMode` | چهار حالت | بازطراحی می‌شود: `FeatureGated`، `Authenticated`، `Public` |
| Commandها، Queryها، Validatorها و Handlerهای موجود | ۸ Command، ۲ Query و ۷ Validator که همگی Vendor یا Buyer هستند | خارج از دامنه. CQRS جدید برای همه قابلیت‌ها نوشته می‌شود |
| سرویس‌ها | `UserService`، `RoleService`، `FeatureService`، `RoleFeatureService`، `AccessService` | بازطراحی می‌شود به Command، Query و Handler |
| `TokenService`، `JwtSigningKey`، `MahtaJwtClaimTypes` | | خارج از دامنه |
| `AdminAccessEvaluator` | | بازطراحی می‌شود (بدون Tenant و Panel) |
| `PMAuthorizationService` و کاتالوگ قواعد | وابسته به Buyer و Panel | Extension Point؛ بخش Vendor حذف |
| Cacheها | `AdminAccessCache`، `AccessRegistryCache` | بازطراحی می‌شود (بخش 5 و T8) |
| `AccessCatalogSyncService`، `EndpointAccessCatalogBuilder` | | بازطراحی می‌شود به Contributor عمومی |
| `AdminFeatureCatalogSeeder` | کلیدهای `Admin.*` خاص PM | بازطراحی: فقط کلیدهای خود ماژول، بقیه از Contributor میزبان |
| Repositoryها | فقط `RefreshTokenRepository` | خارج از دامنه |
| Configuration و Migration | ۳ Configuration و ۲ Migration با ۱۶ جدول | `InitialCreate` تمیز. قید `[Application]='RestEndpoint'` حذف می‌شود |
| Seed و Bootstrap | `IdentityDataInitializer`، `BootstrapAdminInitializer` | بازطراحی: خاموش پیش‌فرض و بدون نام پیش‌فرض |
| Localization | ۹۸ کلید، فقط یک `resx` فارسی | بازطراحی: کلیدهای غیر Vendor با Prefix اختصاصی |
| رویدادها | **هیچ رویداد دامنه‌ای در مرجع وجود ندارد** (جست‌وجوی `IDomainEvent`: صفر نتیجه) | رویدادهای جدید طبق بخش 13 پرامپت |
| Secret در کد | کلید یا رمز ثابتی یافت نشد. فقط نام و نام کاربری پیش‌فرض در `BootstrapAdminOptions` | منتقل نمی‌شود |

### Contractهای یکپارچه‌سازی

| Contract مرجع | وضعیت | مقصد پیشنهادی |
|---|---|---|
| `IIdentityUserProfileReaderContract`، `IIdentityUserProfilesReaderContract` | بازطراحی (متدهای Vendor حذف) | `IIdentityUserReader` |
| `IIdentityAccountReaderContract` | بازطراحی | `IIdentityUserReader` |
| `IIdentityActiveAccountCheckerContract` | بازطراحی | `IIdentityUserReader` |
| `IIdentityAccountCreatorContract` | منتقل می‌شود | `IIdentityAccountService` |
| `IIdentityAdminWorkReaderContract` | خارج از دامنه: شمارش Vendor | — |
| `IAdminAccessEvaluator` | بازطراحی | `IIdentityPermissionService` |
| `IAdminNotificationRecipientResolver` | Extension Point | متد خواندن کاربران یک نقش در `IIdentityUserReader` |

## 5. نقص‌های مرجع که نباید منتقل شوند

| شدت | نقص | محل | رفتار جدید |
|---|---|---|---|
| بحرانی | **نقش غیرفعال همچنان دسترسی و حتی ادمین کامل می‌دهد**؛ `IsActive` در Query بررسی نمی‌شود و غیرفعال‌سازی Cache را هم پاک نمی‌کند | `AdminAccessEvaluator.cs:64-69` و `:296-301`، `RoleService.cs:205` | فیلتر `IsActive` و Invalidation |
| بحرانی | **کاربر غیرفعال** در Evaluator همچنان دسترسی دارد و غیرفعال‌سازی Cache را پاک نمی‌کند | `UserService.cs:729`، `AdminAccessEvaluator` | بررسی `IsActive` و Invalidation |
| بحرانی | **Escalation:** دارنده‌ی `ManageRoles` می‌تواند نقش `admin` را به خودش بدهد و `ManageAccesses` هر Feature را | `RoleService.cs:216`، `RoleFeatureService.cs:125` | Guard سمت Handler (T3) |
| بحرانی | حذف، غیرفعال‌سازی یا برداشتن نقش **آخرین مدیر** آزاد است | `UserService.cs:163,721`، `RoleService.cs:246` | Guard اتمیک داخل تراکنش |
| زیاد | شرایط رقابتی Cache: `SetKeys` برخلاف Registry، Generation را چک نمی‌کند و داده کهنه پس از Invalidate می‌نشیند | `AdminAccessCache.cs:28-39` | چک Generation |
| زیاد | حذف Feature دارای فرزند با FK از نوع `Restrict` به استثنا می‌خورد | `FeatureService.cs:94` | خطای Conflict |
| زیاد | Sync کاتالوگ با حذف یک Endpoint برنامه را در Startup می‌خواباند | `AccessCatalogSyncService.cs:66-71` | غیرفعال‌سازی نرم و هشدار |
| متوسط | `GetListAsync` و `GetManagementListAsync` سقف `PageSize` ندارند | `UserService.cs:261,363` | سقف ۱۰۰ |
| متوسط | `CreatedDate` همیشه `UtcNow` برمی‌گردد و `RegisteredAt` در ایجاد ست نمی‌شود | `UserService.cs:775` | فیلد واقعی |
| متوسط | ایجاد Access مقدار `Panel` را صفر می‌گذارد و Endpoint برای همه Forbidden می‌شود | `AccessService.cs:33-44` | با حذف Panel برطرف می‌شود |
| متوسط | Reset رمز و باطل‌سازی Token اتمیک نیستند | `UserService.cs:624-632` | تراکنش صریح و رویداد |
| پایین | Query تمام‌جدول `Access` روی هر بار `tree` | `AccessService.cs:141` | حفظ با Cache |

## 6. ماتریس Endpointها

`{p}` همان Prefix نهایی با پیش‌فرض `identity` است. ستون «الزام مرجع» کلید دسترسی مرجع را نشان می‌دهد. در ماژول جدید همه‌ی Actionها افزون بر آن به Policy میزبان نیاز دارند.

### Users

| Method | Route فعلی | Route جدید | ورودی | خروجی | الزام مرجع | وضعیت |
|---|---|---|---|---|---|---|
| GET | `identity/Users` | `{p}/users` | Query: `UserListFilterDto` | `PagedUserListDto` | `Users.View` یا `Roles.ManageUsers` | بازطراحی: سقف صفحه |
| GET | `identity/Users/management` | `{p}/users/management` | `UserManagementListFilterDto` | `PagedUserManagementListDto` | `Users.View` | بازطراحی: حذف سازمان Vendor و Buyer |
| GET | `identity/Users/search` | `{p}/users/search` | Query: `text` | `List<UserSearchResultDto>` | `Users.View` | منتقل می‌شود |
| GET | `identity/Users/{id}` | `{p}/users/{id:long}` | Route | `UserDto` | `Users.View` یا `Users.ManageRoles` | بازطراحی: حذف فیلد Vendor |
| GET | `identity/Users/{id}/roles` | `{p}/users/{id:long}/roles` | Route | `List<RoleDto>` | `Users.ManageRoles` | منتقل می‌شود |
| POST | `identity/Users/{id}/roles/{roleId}` | `{p}/users/{id:long}/roles/{roleId:long}` | Route | بدون بدنه | `Users.ManageRoles` | بازطراحی: Escalation |
| DELETE | `identity/Users/{id}/roles/{roleId}` | `{p}/users/{id:long}/roles/{roleId:long}` | Route | بدون بدنه | `Users.ManageRoles` | بازطراحی: آخرین مدیر |
| POST | `identity/Users` | `{p}/users` | `CreateUserDto` | `UserDto` | `Users.Create` | بازطراحی: حذف `AccountType` |
| PUT | `identity/Users/{id}` | `{p}/users/{id:long}` | `UpdateUserDto` | `UserDto` | `Users.Update` | بازطراحی |
| DELETE | `identity/Users/{id}` | `{p}/users/{id:long}` | Route | بدون بدنه | `Users.Update` | بازطراحی: آخرین مدیر و Audit |
| POST | `identity/Users/{id}/change-password` | حذف | `ChangePasswordDto` | — | `Users.ResetPassword` | خارج از دامنه: مدیر رمز فعلی را ندارد. با `reset-password` و `me/change-password` پوشش داده می‌شود |
| POST | `identity/Users/{id}/reset-password` | `{p}/users/{id:long}/reset-password` | `AdminResetPasswordDto` | بدون بدنه | `Users.ResetPassword` | بازطراحی: تراکنش و Audit |
| POST | `identity/Users/request-password-reset` | حذف | `RequestResetPasswordTokenDto` | — | عمومی | خارج از دامنه: جریان Auth ناشناس؛ `AllowAnonymous` ممنوع است. متن مرجع هم Token را نمی‌فرستد |
| POST | `identity/Users/{id}/activate` | `{p}/users/{id:long}/activate` | Route | بدون بدنه | `Users.Activate` | بازطراحی: Audit |
| POST | `identity/Users/{id}/deactivate` | `{p}/users/{id:long}/deactivate` | Route | بدون بدنه | `Users.Deactivate` | بازطراحی: آخرین مدیر و Cache |

### Roles

| Method | Route فعلی | Route جدید | ورودی | خروجی | الزام مرجع | وضعیت |
|---|---|---|---|---|---|---|
| GET | `identity/Roles` | `{p}/roles` | `RoleListFilterDto` | `List<RoleDto>` | `Roles.View` یا `Users.ManageRoles` | منتقل می‌شود |
| GET | `identity/Roles/{id}` | `{p}/roles/{id:long}` | Route | `RoleDto` | `Roles.View` یا `ManageUsers` یا `ManageAccesses` | منتقل می‌شود |
| POST | `identity/Roles` | `{p}/roles` | `CreateRoleDto` | `RoleDto` | `Roles.Create` | منتقل می‌شود |
| PUT | `identity/Roles/{id}` | `{p}/roles/{id:long}` | `UpdateRoleDto` | `RoleDto` | `Roles.Update` | منتقل می‌شود |
| DELETE | `identity/Roles/{id}` | `{p}/roles/{id:long}` | Route | بدون بدنه | `Roles.Update` | منتقل می‌شود |
| POST | `identity/Roles/{id}/activate` | `{p}/roles/{id:long}/activate` | Route | بدون بدنه | `Roles.Activate` | بازطراحی: Cache |
| POST | `identity/Roles/{id}/deactivate` | `{p}/roles/{id:long}/deactivate` | Route | بدون بدنه | `Roles.Deactivate` | بازطراحی: Cache |
| POST | `identity/Roles/{roleId}/users/{userId}` | `{p}/roles/{roleId:long}/users/{userId:long}` | Route | بدون بدنه | `Roles.ManageUsers` | بازطراحی: Escalation |
| DELETE | `identity/Roles/{roleId}/users/{userId}` | `{p}/roles/{roleId:long}/users/{userId:long}` | Route | بدون بدنه | `Roles.ManageUsers` | بازطراحی: آخرین مدیر |
| GET | `identity/Roles/users/{userId}` | `{p}/roles/users/{userId:long}` | Route | `List<RoleDto>` | `Users.ManageRoles` یا `Roles.ManageUsers` | منتقل می‌شود |
| GET | `identity/Roles/{roleId}/users` | `{p}/roles/{roleId:long}/users` | Route | `List<RoleUserDto>` | `Roles.ManageUsers` | منتقل می‌شود |

### Features

| Method | Route فعلی | Route جدید | ورودی | خروجی | الزام مرجع | وضعیت |
|---|---|---|---|---|---|---|
| GET | `identity/Feature` | `{p}/features` | — | `List<FeatureDto>` | `Permissions.View` | منتقل می‌شود |
| GET | `identity/Feature/tree` | `{p}/features/tree` | — | `List<FeatureTreeNodeDto>` | `Permissions.View` | منتقل می‌شود |
| GET | `identity/Feature/{id}` | `{p}/features/{id:long}` | Route | `FeatureDto` | `Permissions.View` | منتقل می‌شود |
| GET | `identity/Feature/{id}/accesses` | `{p}/features/{id:long}/accesses` | Route | `List<FeatureLinkedAccessDto>` | `Permissions.View` | منتقل می‌شود |
| POST | `identity/Feature` | `{p}/features` | `CreateFeatureDto` | `FeatureDto` | `Permissions.Create` | بازطراحی: `Title` به‌جای `PersianTitle` |
| PUT | `identity/Feature/{id}` | `{p}/features/{id:long}` | `UpdateFeatureDto` | `FeatureDto` | `Permissions.Update` | بازطراحی: `Title` و بررسی چرخه |
| PUT | `identity/Feature/{id}/accesses` | `{p}/features/{id:long}/accesses` | `ReplaceFeatureAccessesDto` | بدون بدنه | `Permissions.Update` | منتقل می‌شود |
| DELETE | `identity/Feature/{id}` | `{p}/features/{id:long}` | Route | بدون بدنه | `Permissions.Delete` | بازطراحی: Conflict برای فرزند |

### Accesses

| Method | Route فعلی | Route جدید | ورودی | خروجی | الزام مرجع | وضعیت |
|---|---|---|---|---|---|---|
| GET | `identity/Access` | `{p}/accesses` | `AccessListQuery` | `AccessPageDto` | `Permissions.View` | منتقل می‌شود |
| GET | `identity/Access/tree` | `{p}/accesses/tree` | — | `List<AccessResourceNodeDto>` | `Permissions.View` | بازطراحی: وابسته به کاتالوگ عمومی |
| GET | `identity/Access/{id}` | `{p}/accesses/{id:long}` | Route | `AccessDto` | `Permissions.View` | منتقل می‌شود |
| POST | `identity/Access` | `{p}/accesses` | `CreateAccessDto` | `AccessDto` | `Permissions.Create` | بازطراحی |
| PUT | `identity/Access/{id}` | `{p}/accesses/{id:long}` | `UpdateAccessDto` | `AccessDto` | `Permissions.Update` | بازطراحی |
| DELETE | `identity/Access/{id}` | `{p}/accesses/{id:long}` | Route | بدون بدنه | `Permissions.Delete` | منتقل می‌شود |

### تخصیص Feature به نقش

| Method | Route فعلی | Route جدید | ورودی | خروجی | الزام مرجع | وضعیت |
|---|---|---|---|---|---|---|
| GET | `identity/role-feature/{roleId}/features` | `{p}/role-feature/{roleId:long}/features` | Route | `List<FeatureDto>` | `Roles.ManageAccesses` | منتقل می‌شود |
| GET | `identity/role-feature/{roleId}/all-features-with-selection` | `{p}/role-feature/{roleId:long}/all-features-with-selection` | Route | `List<RoleFeatureSelectionDto>` | `Roles.ManageAccesses` | منتقل می‌شود |
| POST | `identity/role-feature/{roleId}/features/{featureId}` | `{p}/role-feature/{roleId:long}/features/{featureId:long}` | Route | بدون بدنه | `Roles.ManageAccesses` | بازطراحی: Escalation |
| DELETE | `identity/role-feature/{roleId}/features/{featureId}` | `{p}/role-feature/{roleId:long}/features/{featureId:long}` | Route | بدون بدنه | `Roles.ManageAccesses` | منتقل می‌شود |
| POST | `identity/role-feature/{roleId}/features/{featureId}/tree` | `{p}/role-feature/{roleId:long}/features/{featureId:long}/tree` | Route | بدون بدنه | `Roles.ManageAccesses` | بازطراحی: Escalation |
| DELETE | `identity/role-feature/{roleId}/features/{featureId}/tree` | `{p}/role-feature/{roleId:long}/features/{featureId:long}/tree` | Route | بدون بدنه | `Roles.ManageAccesses` | منتقل می‌شود |

### کاربر جاری

| Method | Route فعلی | Route جدید | ورودی | خروجی | الزام مرجع | وضعیت |
|---|---|---|---|---|---|---|
| GET | `identity/me` | `{p}/me` | Claim | `UserDto` | کاربر واردشده | بازطراحی: Claim قابل تنظیم |
| PUT | `identity/me` | `{p}/me` | `UpdateMyProfileDto` | `UserDto` | کاربر واردشده | بازطراحی: حذف `IranianIdentifiers` |
| POST | `identity/me/change-password` | `{p}/me/change-password` | `ChangePasswordDto` | بدون بدنه | کاربر واردشده | بازطراحی: رویداد برای باطل‌سازی نشست میزبان |
| GET | `identity/me/accesses` | `{p}/me/accesses` | Claim | `MyAdminAccessesDto` | کاربر واردشده | بازطراحی: حذف کدهای سازمان |
| GET | `identity/me/access-version` | `{p}/me/access-version` | Claim | `MyAdminAccessVersionDto` | کاربر واردشده | بازطراحی: T8 |

### خارج از دامنه

| Method | Route | وضعیت و دلیل |
|---|---|---|
| POST | `identity/Auth/register-buyer` | خارج از دامنه: Buyer و وابسته به `CreateBuyerProfileCommand` |
| POST | `identity/Auth/register-vendor` | خارج از دامنه: Vendor |
| GET | `identity/Vendors` | خارج از دامنه: Vendor |
| GET | `identity/Vendors/{id:long}` | خارج از دامنه: Vendor |
| POST | `identity/Vendors/{id:long}/approve` | خارج از دامنه: Vendor |
| POST | `identity/Vendors/{id:long}/reject` | خارج از دامنه: Vendor |
| POST | `identity/Vendors/{id:long}/request-correction` | خارج از دامنه: Vendor |
| POST | `identity/Vendors/{id:long}/deactivate` | خارج از دامنه: Vendor |
| POST | `identity/Vendors/{id:long}/activate` | خارج از دامنه: Vendor |
| GET | `vendor/Onboarding/me` | خارج از دامنه: Vendor |
| PUT | `vendor/Onboarding/me` | خارج از دامنه: Vendor |
| POST | `vendor/Onboarding/submit-for-approval` | خارج از دامنه: Vendor |
| POST | `vendor/Onboarding/resubmit` | خارج از دامنه: Vendor |

Login، Logout و RefreshToken در مرجع Action قابل مشاهده ندارند و همراه `TokenService` با وضعیت «خارج از دامنه» ثبت می‌شوند. نقش‌های Claim روی `RoleService` (افزودن، حذف، خواندن Claim) Endpoint ندارند و فقط در صورت نیاز واقعی می‌آیند؛ پیشنهاد حذف از نسخه اول است.

## 7. وابستگی‌ها

| وابستگی | کاربرد | تصمیم |
|---|---|---|
| `BuildingBlocks.Data.Migrations` (`SharedMigrationsHistory`) | جدول تاریخچه | بازطراحی: جدول اختصاصی در Schema `identity` |
| `BuildingBlocks.Web.Controllers` | `BaseApiController` | بازطراحی: `BaseController` پشته |
| `BuildingBlocks.Web.Abstractions` (`IApiModule`) | ثبت ماژول | حذف: `AddPoshtehIdentities` |
| `BuildingBlocks.Web.Authorization` (`IBuyerAccountChecker`) | Buyer | حذف |
| `PM.Vendor.*` (`ICompanyReaderContract`) | سازمان Vendor | حذف |
| `PM.Procurement.*` (`IBuyerProfileReaderContract`، `CreateBuyerProfileCommand`) | پروفایل Buyer | حذف |
| `PM.Platform` Notifications | اعلان ایجاد کاربر | بازطراحی: رویداد عمومی |
| `PM.Platform` AuditLogs (`IAuditLogWriter`) | Audit | Port در Contracts |
| Host (`PM.Host.RestEndpoint`) | اجرای Seed و Sync، فیلتر Authorization (غایب) | بازطراحی مستقل: فرض درباره فیلتر غایب ساخته نمی‌شود |
| `System.IdentityModel.Tokens.Jwt` | Token | حذف |
| `IranianIdentifiers` | نرمال‌سازی شماره | حذف از هسته؛ قابل جایگزینی توسط `IUserValidator` میزبان |
| Rate Limiter با نام `authentication` | Endpointهای عمومی | حذف: Endpointها خارج از دامنه‌اند |

## 8. ریسک‌ها

| ریسک | شدت | کنترل |
|---|---|---|
| تغییرات M1 و M2 روی رفتار هسته | زیاد | افزایشی، تست ترتیب و شمارش Behavior، و سبز ماندن ۱۴ تست موجود |
| `TransactionBehavior` پس از Command ماژول، `IUnitOfWork` میزبان را Commit می‌کند؛ کد میزبان که در همان Scope تغییر معلق دارد ناخواسته ذخیره می‌شود | متوسط | پیشنهاد Marker افزایشی برای رد کردن Commit (T7) |
| `EnableRetryOnFailure` میزبان با تراکنش صریح ناسازگار است | متوسط | اجرای تراکنش‌ها با `CreateExecutionStrategy` |
| انتشار ناخواسته با افزودن به `Poshteh.slnx` | زیاد | گزارش پیش از افزودن به Solution (بخش 15 پرامپت) و بدون اجرای Workflow |
| Pack به `D:\NuGets` | متوسط | Override مسیر در همه Buildها |
| تست SQL در CI روی Ubuntu | متوسط | پروژه‌های یکپارچه‌سازی با متغیر محیطی فعال شوند و `Skip` شفاف باشد؛ ماتریس Template خارج از `slnx` با اسکریپت `tools/` |
| شرط Template در `.props` و JSON | پایین | تولید و مقایسه واقعی خروجی در سناریو A |
| Cache در چند Instance | زیاد | T8 |
| ناسازگاری نسخه: Template نیازمند Core جدید است | متوسط | حداقل نسخه مستند و نسخه یکسان `PoshtehVersion` |

## 9. ساختار نهایی پیشنهادی

```text
Framwrok/src/06.Modules/
  Poshteh.Identities.Contracts/         # فقط Poshteh.Core.Contract
    Users/ Roles/ Features/ Accesses/ Permissions/ Events/ Extensibility/
  Poshteh.Identities/
    Domain/ Application/ Infrastructure/(Persistence, Migrations, Seed) Api/ Registration/ Localization/
Framwrok/tests/
  UnitTests/Poshteh.Identities.Tests/
  ArchitectureTests/Poshteh.Identities.ArchitectureTests/
  ContractTests/Poshteh.Identities.ContractTests/
  IntegrationTests/Poshteh.Identities.IntegrationTests/
docs/identities/
```

کلاس‌های ماژول `internal` هستند، جز سطح عمومی Registration و Options. وابستگی Entityها به ASP.NET Identity استثنای مستند این ماژول است.

## 10. تغییرات پیشنهادی هسته

همگی افزایشی‌اند و جزئیات در Gate 2 می‌آید.

| فایل | تغییر | دلیل |
|---|---|---|
| `Core.ApplicationService/.../ServiceCollectionExtensions.cs` | Idempotent کردن ثبت زیرساخت و متد عمومی Scan یک Assembly | M1 |
| همان فایل | انباشت تنظیمات Localization | M2 |
| `Endpoint.Api/Results/ResultEndpointExtensions.cs` | نگاشت `Forbidden` به 403 | M3 |
| `TransactionBehavior.cs` | Marker اختیاری برای رد شدن از Commit (در صورت تأیید T7) | ریسک بخش 8 |

تأثیر تغییر نخست: ثبت سفارشی `ICommandDispatcher` پیش از `AddApplicationService` از این پس حفظ می‌شود و دیگر بازنویسی نمی‌شود. این تنها تفاوت رفتاری است و در Release Note می‌آید.

## 11. تصمیم‌هایی که منتظر پاسخ شماست

| # | موضوع | گزینه‌ها | پیشنهاد و دلیل |
|---|---|---|---|
| T1 | بررسی رمز توسط Auth میزبان | الف) Port `IIdentityCredentialValidator` در Contracts که با `UserManager` و Lockout نتیجه را برمی‌گرداند، بدون Token. ب) میزبان مستقیم از `UserManager` داخلی استفاده کند | **الف.** بدون آن میزبان نمی‌تواند Login بسازد و گزینه ب مرز ماژول را می‌شکند |
| T2 | حفاظت Endpointهای `me` | الف) یک Policy اختیاری جدا `SelfServicePolicyName` که پیش‌فرضش همان Policy مدیریتی است. ب) فقط یک Policy | **الف.** با یک Policy مدیریتی، کاربر عادی عملاً `me` را ندارد؛ پیش‌فرض همچنان بسته می‌ماند |
| T3 | سطح Authorization داخل ماژول | الف) افزون بر Policy میزبان، کلید دسترسی هر Action هم با `EnforcePermissions=true` چک شود و قوانین Escalation فعال باشد. ب) فقط Policy میزبان | **الف.** بدون آن همه دارندگان Policy معادل مدیر کامل‌اند و Escalation معنا ندارد. هزینه: پیش از استفاده باید اولین مدیر Seed شود |
| T4 | Template بدون Auth واقعی | الف) Scheme مسدودکننده‌ی کوچک داخل پروژه تولیدشده که پاسخ 401 می‌دهد. ب) فقط `AddAuthentication()` و پذیرش 500 تا اتصال Auth | **الف.** رفتار صحیح 401 و بدون نقض مرز ماژول |
| T5 | `AccountType` | الف) حذف کامل. ب) فیلد متنی آزاد `UserType` | **الف.** طبق تصمیم قطعی Vendor و Buyer؛ نوع کاربر در ماژول میزبان با `UserId` می‌ماند |
| T6 | حداقل رمز عبور | الف) ۱۲ کاراکتر با حروف کوچک، بزرگ و عدد. ب) ۸ کاراکتر | **الف**، قابل تنظیم |
| T7 | Commit میزبان پس از Command ماژول | الف) Marker افزایشی در هسته. ب) بدون تغییر و مستندسازی | **الف.** جلوگیری از ذخیره ناخواسته‌ی داده میزبان |
| T8 | Cache در چند Instance | الف) جدول Stamp پایدار که داخل تراکنش افزایش می‌یابد و هر چند ثانیه چک می‌شود. ب) فقط TTL و اعلام محدودیت | **الف.** تأخیر ابطال محدود، و `access-version` پایدار بین Instanceها |
| T9 | تحویل رویدادها | الف) In-Process پس از Commit با اعلام محدودیت. ب) Outbox اختصاصی | **الف** برای نسخه اول. نیازی به تضمین تحویل در مرجع دیده نشد؛ Outbox کار آینده است |

### پیش‌فرض‌های بخش 6 پرامپت

تأیید می‌شوند، بدون پیشنهاد جایگزین: Connection با نام `Identities`، Schema ثابت `identity`، تک‌مستأجر، `long`، `PoshtehVersion`، Prefix با پیش‌فرض `identity`، خارج بودن مهاجرت داده PM، `06.Modules` و `docs/identities/`.

## 12. وضعیت قابلیت‌ها

| قابلیت | وضعیت |
|---|---|
| CRUD کاربر، فعال و غیرفعال‌سازی، Reset مدیریتی | بازطراحی می‌شود |
| CRUD نقش و تخصیص کاربر | منتقل یا بازطراحی می‌شود |
| CRUD و درخت Feature | منتقل می‌شود |
| CRUD و درخت Access | بازطراحی می‌شود |
| کاتالوگ قابل گسترش توسط میزبان | به Extension Point تبدیل می‌شود |
| ارزیابی دسترسی و خواندن از Contracts | بازطراحی می‌شود |
| Audit و رویدادها | به Extension Point تبدیل می‌شود |
| Seed و اولین مدیر | بازطراحی می‌شود |
| Login، JWT، Refresh Token، Session | خارج از دامنه با دلیل: مسئولیت میزبان |
| بازیابی رمز ناشناس و تأیید ایمیل | خارج از دامنه با دلیل: جریان Auth و ناقص در مرجع |
| Vendor و Buyer و Tenant | خارج از دامنه با دلیل: تصمیم قطعی مالک |
