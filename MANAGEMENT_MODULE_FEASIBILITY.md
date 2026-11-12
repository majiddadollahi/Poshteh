# گزارش امکان‌سنجی افزودن ماژول مدیریت اختیاری به Poshteh Template

تاریخ بررسی: ۱۴۰۵/۰۸/۱۷ (۸ نوامبر ۲۰۲۶)

## ۱. نتیجه اجرایی

این قابلیت **شدنی است** و با معماری فعلی Poshteh تعارض بنیادی ندارد. پیشنهاد مناسب این است که بخش مدیریت کاربران، نقش‌ها، Featureها و Permissionها به‌صورت یک یا چند پکیج NuGet مستقل ارائه شود و تمپلیت با یک پارامتر Boolean مانند `--Management` فقط در صورت درخواست مصرف‌کننده:

1. `PackageReference` مربوط به ماژول مدیریت را اضافه کند؛
2. ثبت سرویس‌ها و `DbContext` ماژول را در Composition Root قرار دهد؛
3. Endpointهای ماژول را Map کند؛
4. تنظیمات و Connection String لازم را اضافه کند.

در حالت پیش‌فرض پارامتر باید `false` باشد؛ بنابراین اگر کاربر آن را وارد نکند، نه پکیج مدیریت به خروجی وابسته می‌شود، نه جدول‌ها و سرویس‌های آن ثبت می‌شوند و نه Endpointهای آن در دسترس خواهند بود.

تصمیم پیشنهادی: **Go، مشروط به طراحی روشن مالکیت داده، Migration و امنیت.** بزرگ‌ترین ریسک فنی خود NuGet یا تمپلیت نیست؛ مهم‌ترین ریسک‌ها عبارت‌اند از شیوه دسترسی سایر ماژول‌ها به داده مدیریت، هماهنگی Migrationها، انتخاب Provider دیتابیس و جلوگیری از بازشدن ناخواسته Endpointهای حساس.

## ۲. شواهد موجود در Repository

ساختار فعلی چند قابلیت لازم را از قبل دارد:

- تمپلیت در `Framwrok/templates/Poshteh.Templates/content/PoshtehStarter/.template.config/template.json` پارامتر Boolean، شرط و `sources.modifiers` دارد. پارامتر فعلی `ModularMonolith` نمونه عملی اضافه/حذف شرطی فایل‌ها و تغییر شکل پروژه است. بنابراین افزودن یک پارامتر اختیاری جدید از نظر Template Engine مسئله تازه‌ای نیست.
- پروژه‌های تولیدشده همین حالا فریم‌ورک Poshteh را با `PackageReference` مصرف می‌کنند و نسخه همه پکیج‌های Poshteh در `Directory.Packages.props` متمرکز شده است. ماژول مدیریت نیز می‌تواند دقیقاً با همین روش توزیع شود.
- در Persistence فعلی، متدهای `AddSqlCommandPersistence<TContext>` و `AddSqlQueryPersistence<TContext>`، Context مشخص برنامه را با `AddDbContext` ثبت می‌کنند. پس الگوی ثبت Context از طریق Extension Method از قبل در معماری وجود دارد.
- در API فعلی، `AddEndpointApi()` سرویس‌های MVC را ثبت می‌کند و Host، `app.MapControllers()` را صدا می‌زند. بنابراین Controllerهای یک اسمبلی NuGet را می‌توان با Application Part صریح کشف کرد؛ یا می‌توان ماژول مدیریت را با Minimal API و متد Map اختصاصی ارائه داد.
- Host فعلی احراز هویت و مجوزدهی را پیکربندی نمی‌کند و `AddAuthentication`، `AddAuthorization`، `UseAuthentication` و `UseAuthorization` در آن دیده نمی‌شود. برای ماژولی که User/Role/Permission را مدیریت می‌کند، این یک پیش‌نیاز قطعی است، نه یک قابلیت اختیاری جانبی.
- تمپلیت نمونه در زمان اجرا از `Database.EnsureCreated()` استفاده می‌کند. این روش برای نمونه ساده مناسب است، اما برای انتشار مستقل نسخه‌های ماژول مدیریت و ارتقای Schema در محیط واقعی کافی نیست و باید Migration Strategy جایگزین آن شود.

این بررسی معماری است؛ در Repository فعلی پیاده‌سازی مشخصی از ماژول User/Role/Feature/Permission وجود ندارد که کیفیت یا سازگاری داخلی آن ارزیابی شود.

## ۳. معماری پیشنهادی

### ۳.۱. مرز پکیج‌ها

حداقل جداسازی پیشنهادی:

| پکیج | مسئولیت | سطح دسترسی مصرف‌کننده |
|---|---|---|
| `Poshteh.Management.Contracts` | DTOها، قرارداد Query/Command، شناسه Permissionها و Eventهای عمومی | عمومی و پایدار |
| `Poshteh.Management.EntityFrameworkCore` | `ManagementDbContext`، Mappingها، Repositoryها و Migrationها | عمدتاً داخلی؛ فقط Seamهای لازم عمومی |
| `Poshteh.Management.AspNetCore` | ثبت DI، Authentication/Authorization integration، Endpointها و Policyها | Extension Methodهای عمومی |
| `Poshteh.Management` | Meta-package برای نصب ساده وابستگی‌های بالا | پکیجی که تمپلیت مصرف می‌کند |

می‌توان در نسخه اول تعداد پکیج‌ها را کمتر کرد، ولی حداقل باید Contracts از Implementation جدا باشد. این جداسازی اجازه می‌دهد سایر ماژول‌ها برای خواندن اطلاعات کاربر و مجوزها به قرارداد پایدار وابسته باشند، نه به EF Core و جدول‌های داخلی.

### ۳.۲. قرارداد Composition Root

ماژول بهتر است دو نقطه ورود صریح داشته باشد:

- یک Extension Method برای ثبت سرویس‌ها، Context، Authentication integration و تنظیمات؛
- یک Extension Method برای Map کردن Endpointها زیر یک Route Prefix مشخص، برای مثال `/api/management`.

این تفکیک باعث می‌شود صرف نصب NuGet به‌تنهایی Endpointی را فعال نکند. فعال‌سازی فقط زمانی رخ می‌دهد که تمپلیت، در حالت `--Management true`، هر دو فراخوانی را در Host تولید کند.

برای Endpointهای داخل پکیج دو راه وجود دارد:

1. **Minimal API با Map اختصاصی — پیشنهاد اصلی:** کشف Endpoint کاملاً صریح است، Route Prefix و Policy مرکزی کنترل می‌شوند و وابستگی به Controller discovery کمتر می‌شود.
2. **Controller داخل NuGet:** شدنی است، ولی باید اسمبلی ماژول به‌صورت صریح با MVC Application Parts ثبت شود تا کشف Controllerها تصادفی یا وابسته به جزئیات dependency discovery نباشد. مستندات ASP.NET Core نیز Application Parts را راه اشتراک Controller و سایر قابلیت‌های MVC میان اسمبلی‌ها معرفی می‌کند.

### ۳.۳. رفتار پارامتر تمپلیت

رفتار مطلوب پارامتر:

| فرمان تولید | نتیجه |
|---|---|
| `dotnet new poshteh -n Sample` | بدون PackageReference، ثبت DI، تنظیمات، دیتابیس و Endpoint مدیریت |
| `dotnet new poshteh -n Sample --Management` | افزودن NuGet، Composition Root، تنظیمات، Migration strategy و Endpointهای مدیریت |

پارامتر Template یک **تصمیم زمان تولید پروژه** است، نه Feature Flag زمان اجرا. بعد از تولید پروژه، روشن/خاموش‌کردن ماژول باید با افزودن یا حذف وابستگی و Composition انجام شود؛ تغییر یک مقدار در `appsettings` نباید یک ماژول نصب‌نشده را ایجاد کند. اگر خاموش/روشن‌کردن در Runtime نیز لازم باشد، آن موضوع باید جداگانه با Feature Flag طراحی شود، ولی پکیج همچنان در خروجی Deploy وجود خواهد داشت.

## ۴. دسترسی سایر بخش‌ها به داده مدیریت

خواسته دسترسی به داده‌های `DbContext` از بخش‌های دیگر از نظر فنی قابل انجام است، اما نحوه انجام آن مهم‌ترین تصمیم معماری این طرح است.

### گزینه A ـ تزریق مستقیم `ManagementDbContext`

Context و Entityهای لازم در NuGet عمومی می‌شوند و Context با Lifetime از نوع Scoped در DI ثبت می‌شود. سرویس‌های دیگر می‌توانند آن را Constructor Inject کنند.

مزیت: ساده، مستقیم و دارای تمام قابلیت‌های LINQ/EF.

عیب: مصرف‌کننده به Schema و EF ماژول مدیریت قفل می‌شود، می‌تواند قواعد دامنه را دور بزند، داده حساس را تغییر دهد و ارتقای نسخه‌های NuGet دشوار می‌شود. همچنین `DbContext` thread-safe نیست و باید در Scope کوتاه و به‌عنوان Unit of Work استفاده شود.

نتیجه: فقط به‌عنوان **Advanced escape hatch** و با مستندسازی محدودیت‌ها مناسب است، نه مسیر پیش‌فرض.

### گزینه B ـ اینترفیس Context محدود

یک قرارداد مانند Context خواندنی، فقط Queryableهای موردنیاز را نمایش می‌دهد و `SaveChanges` یا مجموعه‌های حساس را عمومی نمی‌کند.

مزیت: دسترسی Query ممکن می‌ماند و سطح دسترسی کمتر از Context کامل است.

عیب: همچنان قرارداد به EF Core، Entityها و شکل Schema وابسته است و تغییر Entityها به Breaking Change تبدیل می‌شود.

نتیجه: برای یک اکوسیستم کاملاً EF محور قابل قبول است، ولی باید فقط مدل‌های خواندنی را نمایش دهد.

### گزینه C ـ Query Service/Reader Contract ـ پیشنهاد اصلی

پکیج Contracts سرویس‌هایی مانند خواندن مشخصات کاربر، نقش‌ها، Featureها و بررسی Permission را با DTOهای پایدار ارائه می‌دهد. سایر بخش‌ها از این قرارداد استفاده می‌کنند و Implementation داخل NuGet باقی می‌ماند.

مزیت: کمترین Coupling، امکان Cache، کنترل داده حساس، حفظ قواعد دامنه و آزادی تغییر Schema داخلی.

عیب: Queryهای کاملاً آزاد و Join دلخواه را نمی‌دهد و برای نیازهای جدید باید Contract جدید تعریف شود.

نتیجه: مسیر پیش‌فرض باید این گزینه باشد. اگر دسترسی تحلیلی گسترده لازم است، Read Model یا View اختصاصی بهتر از بازکردن Context نوشتن است.

### گزینه D ـ Integration Event و داده محلی

برای اطلاعات پرتکرار مثل نام نمایشی کاربر یا وضعیت Feature، ماژول مدیریت Event منتشر می‌کند و هر ماژول Snapshot موردنیاز خود را نگه می‌دارد.

مزیت: استقلال بالا و مناسب معماری Modular Monolith یا حرکت آینده به Microservice.

عیب: سازگاری نهایی، Outbox/Inbox، Retry و پیچیدگی عملیاتی.

نتیجه: برای ارتباطات بین Bounded Contextها و بار خواندن بالا مناسب است، نه نیازهای ساده اولیه.

### جمع‌بندی دسترسی داده

ترتیب پیشنهادی مصرف:

1. Permission check و اطلاعات معمول کاربر از طریق Contract؛
2. Queryهای گزارشی از طریق Read Model عمومی؛
3. Event برای تکثیر داده موردنیاز سایر ماژول‌ها؛
4. دسترسی مستقیم به Context فقط برای موارد استثنایی و ترجیحاً Read-only.

نباید `DbContext` Scoped را Singleton کرد یا بین Threadها به اشتراک گذاشت. مستندات EF Core نیز Context را Unit of Work کوتاه‌عمر و غیر thread-safe معرفی می‌کند.

## ۵. استراتژی دیتابیس و Migration

### حالت پیشنهادی

- ماژول مدیریت مالک Context و جدول‌های خودش باشد.
- می‌تواند از همان Database فیزیکی برنامه استفاده کند، اما در SQL Server/PostgreSQL Schema مستقل مانند `management` داشته باشد.
- برای Providerهایی مانند SQLite که Schema واقعی ندارند، Prefix مشخص برای نام جدول‌ها در نظر گرفته شود.
- Migrationهای ماژول و جدول تاریخچه Migration آن از Migrationهای برنامه جدا باشند.
- Migrationها در اسمبلی/پکیج مستقل قرار بگیرند و در زمان Registration، `MigrationsAssembly` صریح تنظیم شود.

### تصمیم‌های الزامی پیش از توسعه

1. آیا Providerهای پشتیبانی‌شده SQL Server، PostgreSQL و SQLite هستند یا فقط یکی؟ Migrationهای کامپایل‌شده Provider-specific هستند و نگهداری هم‌زمان چند Provider هزینه تست و انتشار را زیاد می‌کند.
2. Migration در Startup اعمال می‌شود، با Bundle/CLI در Deployment اجرا می‌شود یا SQL Script تحویل تیم عملیات می‌شود؟ پیشنهاد Production، اجرای کنترل‌شده در Pipeline/Deployment است، نه Migration خودکار هم‌زمان توسط چند Instance برنامه.
3. آیا Context مدیریت Database جدا دارد یا Connection String اصلی را مصرف می‌کند؟ هر دو شدنی‌اند، ولی API تنظیمات باید از ابتدا این انتخاب را پشتیبانی کند.
4. آیا ارتباط فیزیکی Foreign Key بین جدول‌های دامنه و User مجاز است؟ پیشنهاد معماری، نگهداری `UserId` به‌عنوان شناسه و اعتبارسنجی از طریق Contract است؛ Foreign Key بین Contextها استقلال Migration و جداسازی آینده را سخت می‌کند.

استفاده فعلی نمونه از `EnsureCreated()` باید برای حالت Management کنار گذاشته شود؛ `EnsureCreated` مسیر ارتقای نسخه‌ای Schema را مدیریت نمی‌کند. EF Core از Migrations assembly جدا پشتیبانی رسمی دارد و این مدل با NuGet ماژولار سازگار است.

## ۶. امنیت و Authorization

تمام Endpointهای مدیریت باید **پیش‌فرض بسته** باشند. «در دسترس بودن Endpointها» باید به معنی Route شدن آنها باشد، نه Anonymous بودن آنها.

موارد الزامی:

- Host باید Scheme احراز هویت را تعیین کند؛ NuGet نباید بدون اطلاع مصرف‌کننده یک Scheme سراسری را جایگزین کند.
- تمام Routeهای مدیریت یک Fallback/Group Policy سخت‌گیرانه داشته باشند.
- Permissionهای مدیریت نقش و Permission از Permissionهای عادی جدا باشند تا Privilege Escalation رخ ندهد.
- جریان امن Bootstrap برای اولین مدیر تعریف شود؛ Secret پیش‌فرض یا کاربر/رمز ثابت داخل پکیج قابل قبول نیست.
- جلوگیری از حذف یا غیرفعال‌کردن آخرین Super Admin در نظر گرفته شود.
- عملیات حساس Audit شوند: ایجاد/حذف کاربر، تغییر نقش، تغییر Permission، Reset credential، قفل/بازکردن کاربر و Impersonation.
- Concurrency token برای ویرایش Role/Permission و سیاست Cache invalidation تعریف شود.
- Rate limiting و محافظت ویژه برای Login، Reset و عملیات مدیریتی اعمال شود.
- اگر Multi-tenancy در آینده محتمل است، Tenant boundary باید قبل از تثبیت Schema و کلیدهای یکتا تصمیم‌گیری شود.

ثبت سرویس‌های Authorization توسط پکیج ممکن است، اما ترتیب Middleware در Host همچنان بخشی از قرارداد Integration است. تمپلیت باید ترتیب صحیح Authentication، Authorization و Map endpoint را تولید و تست کند.

## ۷. مزایا

- سورس ماژول مدیریت در Repository پروژه‌های مصرف‌کننده کپی نمی‌شود.
- پروژه‌های ساده‌ای که مدیریت لازم ندارند، بدون وابستگی و سطح حمله اضافی تولید می‌شوند.
- رفع باگ امنیتی و افزودن قابلیت با ارتقای نسخه NuGet بین چند محصول قابل توزیع است.
- یک مدل یکسان برای User/Role/Feature/Permission در تمام محصولات حفظ می‌شود.
- Composition Root صریح، فعال یا غیرفعال بودن ماژول را قابل مشاهده و قابل تست می‌کند.
- امکان استفاده از Feed خصوصی و کنترل دسترسی به پکیج وجود دارد.
- جداسازی Contracts از Implementation، وابستگی سایر ماژول‌ها به دیتابیس مدیریت را کاهش می‌دهد.
- با الگوی نسخه‌گذاری فعلی Poshteh و Central Package Management هم‌راستا است.

## ۸. معایب

- اشکال‌زدایی نسبت به سورس داخل Solution سخت‌تر است و به Symbol/Source Link یا Package محلی نیاز دارد.
- هر تغییر Public API یا Schema نیازمند Versioning، Migration و Compatibility policy است.
- Consumer نمی‌تواند به‌سادگی رفتار داخلی را سفارشی کند؛ نقاط Extension باید از ابتدا طراحی شوند.
- Providerهای متعدد دیتابیس، ماتریس تست و تعداد Migration artifactها را افزایش می‌دهند.
- دسترسی مستقیم سایر بخش‌ها به Context، استقلال ماژول را عملاً از بین می‌برد.
- Controller discovery، Middleware order، Policyها و Route collision باید در تست Integration پوشش داده شوند.
- ارتقای پکیج ممکن است هم‌زمان تغییر باینری و تغییر Schema بخواهد؛ Rollback دشوارتر از یک Library بدون دیتابیس است.
- NuGet به‌تنهایی حفاظت مالکیت فکری نیست و اسمبلی .NET قابل Decompile است.

## ۹. چالش‌ها و ریسک‌ها

| ریسک | شدت | کنترل پیشنهادی |
|---|---:|---|
| دسترسی بیش از حد به `DbContext` و دورزدن قواعد | زیاد | Contract/Reader پیش‌فرض؛ Context مستقیم فقط escape hatch |
| بازشدن Endpoint حساس بدون Policy | بحرانی | Policy اجباری روی کل Route Group و تست منفی Anonymous |
| Bootstrap اولین مدیر و Privilege Escalation | بحرانی | فرآیند یک‌بارمصرف امن، Audit و محافظت از آخرین مدیر |
| ناسازگاری Migration و نسخه پکیج | زیاد | Migration assembly مستقل، SemVer و تست ارتقا از نسخه‌های پشتیبانی‌شده |
| چند Provider دیتابیس | زیاد | اعلام ماتریس رسمی Provider/Version و انتشار Migration مختص هر Provider |
| استفاده هم‌زمان از `EnsureCreated` و Migrations | زیاد | حذف `EnsureCreated` در خروجی Management و یک Strategy واحد |
| Transaction بین Context دامنه و Context مدیریت | متوسط تا زیاد | پرهیز از تراکنش مشترک؛ Integration Event/Outbox؛ در صورت الزام، طراحی صریح transaction sharing |
| Cache شدن Permission قدیمی | زیاد | Version stamp/invalidation و TTL کوتاه برای تصمیم‌های حساس |
| شکستن Consumer با تغییر DTO/Entity | متوسط | Contracts پایدار، عدم انتشار Entity داخلی و تست سازگاری |
| Route یا Service registration collision | متوسط | Prefix ثابت، نام‌گذاری Options و متدهای Map/Add اختصاصی |
| مخفی‌نبودن واقعی کد | متوسط | شفاف‌سازی هدف؛ Feed خصوصی؛ عدم انتشار Symbol عمومی در صورت محرمانگی؛ پذیرش اینکه Obfuscation تضمین نیست |
| وابستگی نسخه‌ای به .NET/EF/Poshteh | متوسط | ماتریس Compatibility و هم‌ترازی نسخه‌ها با Central Package Management |

## ۱۰. NuGet و مسئله «اصل کد در پروژه نباشد»

اگر منظور این است که فایل‌های `.cs` ماژول مدیریت داخل Solution مصرف‌کننده ایجاد نشوند، NuGet دقیقاً این هدف را تأمین می‌کند. مصرف‌کننده فقط Reference باینری و Public API را می‌بیند.

اگر منظور مخفی‌کردن مالکیت فکری است، NuGet کافی نیست:

- اسمبلی‌های managed معمولاً قابل Decompile هستند؛
- تنظیم فعلی `Framwrok/Directory.Build.props` تولید Symbol package و Source Link را فعال کرده است؛
- Metadata فعلی پروژه نیز License از نوع MIT و Repository عمومی را اعلام می‌کند.

بنابراین باید پیش از انتشار مشخص شود این ماژول Open Source است یا Proprietary. برای حالت Proprietary می‌توان Feed خصوصی، دسترسی محدود و سیاست جداگانه Symbol/Source Link داشت، ولی حتی Obfuscation نیز محرمانگی کامل ایجاد نمی‌کند.

## ۱۱. سیاست Versioning پیشنهادی

- نسخه Public API پکیج، Schema دیتابیس و Permission catalog باید با هم Release شوند.
- تغییر Contract یا حذف Endpoint، Major؛ افزودن سازگار، Minor؛ رفع باگ بدون تغییر Contract، Patch تلقی شود.
- هر نسخه باید حداقل مسیر Upgrade از نسخه قبلی پشتیبانی‌شده و رفتار Rollback را مستند کند.
- Package Version ماژول می‌تواند با نسخه فریم‌ورک Poshteh هم‌تراز باشد تا مدیریت Compatibility ساده شود؛ اگر چرخه انتشار مستقل لازم است، `ManagementVersion` جدا در Central Package Management تعریف شود.
- تمپلیت فقط نسخه‌ای را پیش‌فرض کند که پکیج آن واقعاً در Feed هدف منتشر شده است.

## ۱۲. معیارهای پذیرش برای فاز پیاده‌سازی آینده

پیاده‌سازی زمانی قابل قبول است که حداقل این سناریوها خودکار تست شوند:

1. تولید تمپلیت بدون `--Management`: هیچ Reference، Route، Service یا جدول مدیریت وجود نداشته باشد و Build/Test موفق باشد.
2. تولید تمپلیت با `--Management` در هر دو حالت عادی و `--ModularMonolith`: Restore، Build و Startup موفق باشند.
3. تمام Endpointهای مورد انتظار در OpenAPI ظاهر شوند، ولی درخواست Anonymous پاسخ مجاز نگیرد.
4. کاربر مجاز بتواند CRUD کاربر/نقش/Feature/Permission را طبق قواعد انجام دهد.
5. سایر ماژول‌ها بتوانند Contract خواندن داده مدیریت را از DI دریافت کنند.
6. دسترسی مستقیم اختیاری به Context، اگر ارائه شد، Scoped و قابل Resolve باشد و تست concurrent misuse نداشته باشد.
7. ایجاد دیتابیس خالی، ارتقای نسخه قبلی و اجرای مجدد Migration بدون خرابی تست شود.
8. Providerهای اعلام‌شده هر کدام Integration Test واقعی داشته باشند.
9. تغییر Permission، Cache را به‌موقع بی‌اعتبار کند.
10. Audit عملیات حساس و محافظت از آخرین مدیر تست شود.

## ۱۳. پیشنهاد فازبندی

### فاز ۱ ـ تثبیت قرارداد

- تعیین مدل User/Role/Feature/Permission و مرزهای عمومی؛
- تعیین Providerها، Multi-tenancy و Authentication schemeهای قابل پشتیبانی؛
- انتخاب Query Service به‌عنوان مسیر اصلی دسترسی داده؛
- مشخص‌کردن Migration/deployment policy.

### فاز ۲ ـ بسته‌بندی ماژول

- جداسازی Contracts، EF و ASP.NET integration؛
- ساخت Registration/Mapping API صریح؛
- آماده‌سازی Migration artifact و تست امنیت/ارتقا؛
- انتشار آزمایشی در Feed خصوصی.

### فاز ۳ ـ اتصال به تمپلیت

- افزودن پارامتر Boolean با مقدار پیش‌فرض `false`؛
- تولید شرطی PackageReference، تنظیمات و Composition Root؛
- تست ماتریسی چهار خروجی: Management روشن/خاموش × ModularMonolith روشن/خاموش.

## ۱۴. تصمیم‌های باز پیش از شروع کدنویسی

پیش از توسعه باید پاسخ این موارد قطعی شود:

1. Authentication مبتنی بر Cookie است، JWT/OIDC است یا ماژول باید هر دو را پشتیبانی کند؟
2. User store داخلی است یا باید با Identity Provider خارجی نیز کار کند؟
3. Providerهای رسمی دیتابیس کدام‌اند؟
4. Multi-tenancy اکنون یا در آینده لازم است؟
5. دسترسی «به دیتای Context» دقیقاً شامل Query خواندنی است یا تغییر مستقیم داده نیز لازم است؟
6. Migration را Application اجرا می‌کند یا Deployment Pipeline؟
7. پکیج عمومی/Open Source است یا خصوصی/تجاری؟
8. آیا خاموش/روشن‌شدن فقط هنگام `dotnet new` کافی است یا Runtime Feature Flag نیز لازم است؟

## ۱۵. جمع‌بندی نهایی

راهکار از نظر فنی و با توجه به قابلیت‌های فعلی Template و NuGet **کاملاً امکان‌پذیر** است. پیشنهاد نهایی این است که ماژول مدیریت یک Bounded Context مستقل و مالک داده خود باشد، از طریق NuGet وارد شود، با `--Management` فقط در زمان ساخت پروژه Compose شود و Endpointهایش با Map صریح و Policy اجباری فعال شوند.

برای دسترسی سایر بخش‌ها، ثبت Context در DI ممکن است، اما API اصلی نباید Context کامل را منتشر کند. Contract خواندن/بررسی مجوز باید مسیر استاندارد باشد و دسترسی مستقیم Context فقط گزینه پیشرفته و محدود باقی بماند. پیش از هر توسعه، تصمیم Provider/Migration، Authentication، Bootstrap مدیر و Multi-tenancy باید بسته شود؛ در غیر این صورت بیشترین هزینه آینده از تغییر Schema و امنیت خواهد آمد، نه از خود تمپلیت.

## منابع فنی

- [مستندات رسمی ساخت Template و پارامترهای `template.json`](https://learn.microsoft.com/en-us/dotnet/core/tools/custom-templates)
- [مستندات رسمی ASP.NET Core Application Parts برای اشتراک Controller بین Assemblyها](https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-10.0)
- [مستندات رسمی EF Core درباره Lifetime و ثبت `DbContext`](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- [مستندات رسمی EF Core درباره Migrations Project جدا](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/projects)
- [مرور رسمی EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations)
