# راهنمای استقرار NaderEcommerce

این راهنما برای تحویل نسخه ASP.NET Core 8 Web API، Blazor Web App و SQL Server نوشته شده است.

## پیش‌نیازها

- .NET SDK/Runtime 8 روی سرور build و runtime.
- SQL Server در دسترس API.
- reverse proxy مثل Nginx یا IIS با TLS فعال.
- مقداردهی secretها با environment variable یا secret store؛ secret واقعی داخل repository ذخیره نشود.

## تنظیمات Production

برای API این کلیدها باید در محیط Production override شوند:

```powershell
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=...;Database=NaderEcommerceDb;User Id=...;Password=...;TrustServerCertificate=False;MultipleActiveResultSets=True
Jwt__Secret=<حداقل 32 کاراکتر تصادفی و محرمانه>
Jwt__Issuer=NaderEcommerce
Jwt__Audience=NaderEcommerce.Client
Site__BaseUrl=https://www.nafashshop786.com
SeedData__Admin__Email=admin@example.com
SeedData__Admin__Password=<رمز قوی اولیه>
SeedData__Admin__FullName=مدیر سیستم
AllowedHosts=example.com
```

برای Blazor:

```powershell
ASPNETCORE_ENVIRONMENT=Production
Api__BaseUrl=https://api.example.com/
AllowedHosts=www.example.com
```

`Database:ApplyMigrationsOnStartup` در Production به‌صورت پیش‌فرض `false` است. migration را در فرایند release اجرا کنید، نه با هر startup برنامه.

## آماده‌سازی دیتابیس

از ریشه repository:

```powershell
dotnet restore
dotnet build -c Release
dotnet ef database update --project src/NaderEcommerce.Infrastructure/NaderEcommerce.Infrastructure.csproj --startup-project src/NaderEcommerce.WebApi/NaderEcommerce.WebApi.csproj --configuration Release
```

Seed اولیه هنگام startup API اجرا می‌شود و شامل نقش‌های `Customer` و `Admin`، کاربر ادمین تنظیم‌شده، دسته‌بندی‌ها، محصولات نمونه، کوپن‌ها و محتوای پایه CMS است.

## انتشار برنامه‌ها

```powershell
dotnet publish src/NaderEcommerce.WebApi/NaderEcommerce.WebApi.csproj -c Release -o .\publish\api
dotnet publish src/NaderEcommerce.BlazorWeb/NaderEcommerce.BlazorWeb.csproj -c Release -o .\publish\web
```

روی سرور، API و Blazor را به‌عنوان دو سرویس جدا اجرا کنید و reverse proxy را روی پورت‌های داخلی آن‌ها تنظیم کنید. API باید پشت HTTPS باشد؛ برنامه از `ForwardedHeaders`، HSTS، security headers، response compression، rate limiting و output cache پشتیبانی می‌کند.

## استقرار با Docker روی Ubuntu VPS

فایل‌های آماده Docker در repository قرار دارند:

- `deploy/docker-compose.yml`
- `deploy/.env.example`
- `deploy/nginx-nafashshop786.conf.example`
- `src/NaderEcommerce.WebApi/Dockerfile`
- `src/NaderEcommerce.BlazorWeb/NaderEcommerce.BlazorWeb/Dockerfile`

نیازی نیست SQL Server روی خود VPS نصب شده باشد. `docker-compose.yml` یک container جدا برای SQL Server بالا می‌آورد و داده‌های دیتابیس را داخل volume پایدار `sqlserver-data` نگه می‌دارد. فقط Docker باید روی VPS فعال باشد و سرور بهتر است حداقل ۲ گیگابایت RAM داشته باشد.

روی VPS، ابتدا DNS دامنه‌های `nafashshop786.com` و `www.nafashshop786.com` را به IP سرور وصل کنید. سپس:

```bash
sudo apt update
sudo apt install -y git nginx certbot python3-certbot-nginx
git clone https://github.com/vahidmohamadii/NamelenamEcommerce.git
cd NamelenamEcommerce
cp deploy/.env.example deploy/.env
nano deploy/.env
```

داخل `deploy/.env` همه رمزها را تغییر دهید، مخصوصاً:

- `SQL_PASSWORD`
- `JWT_SECRET`
- `ADMIN_PASSWORD`

برای اجرای اولیه:

```bash
docker compose -f deploy/docker-compose.yml --env-file deploy/.env up -d --build
docker compose -f deploy/docker-compose.yml --env-file deploy/.env ps
docker compose -f deploy/docker-compose.yml --env-file deploy/.env logs -f api web
```

در اجرای اول، مقدار `APPLY_MIGRATIONS_ON_STARTUP=true` دیتابیس را می‌سازد و migrationها را اجرا می‌کند. بعد از اینکه سایت بالا آمد، این مقدار را در `deploy/.env` به `false` تغییر دهید و سرویس‌ها را دوباره بالا بیاورید:

```bash
nano deploy/.env
docker compose -f deploy/docker-compose.yml --env-file deploy/.env up -d
```

برای اتصال دامنه به Blazor، فایل نمونه Nginx را فعال کنید:

```bash
sudo cp deploy/nginx-nafashshop786.conf.example /etc/nginx/sites-available/nafashshop786.com
sudo ln -s /etc/nginx/sites-available/nafashshop786.com /etc/nginx/sites-enabled/nafashshop786.com
sudo nginx -t
sudo systemctl reload nginx
```

بعد SSL رایگان Let's Encrypt را بگیرید:

```bash
sudo certbot --nginx -d nafashshop786.com -d www.nafashshop786.com
```

بعد از deploy این URLها را چک کنید:

```bash
curl -I https://www.nafashshop786.com
curl https://www.nafashshop786.com/robots.txt
curl https://www.nafashshop786.com/sitemap.xml
curl http://127.0.0.1:5111/health
```

برای releaseهای بعدی:

```bash
git pull
docker compose -f deploy/docker-compose.yml --env-file deploy/.env up -d --build
docker image prune -f
```

## کنترل‌های قبل از تحویل

- `dotnet test` باید بدون خطا پاس شود.
- `/health` در API باید پاسخ موفق بدهد.
- Swagger در محیط Development قابل بررسی است؛ در Production پیش‌فرض فعال نیست.
- مسیر ثبت‌نام، ورود، مشاهده محصول، سبد خرید، checkout، پرداخت آزمایشی و مشاهده سفارش بررسی شود.
- پنل ادمین با نقش `Admin` بررسی شود: مدیریت محصول، دسته‌بندی، سفارش، کوپن و CMS.
- UI در عرض‌های موبایل و دسکتاپ برای صفحات خانه، محصولات، جزئیات محصول، سبد خرید، checkout و پنل ادمین مرور شود.

## چک‌لیست rollback

- قبل از migration از دیتابیس backup بگیرید.
- artifactهای publish هر release را نگه دارید.
- در صورت خطای بحرانی، سرویس را به artifact قبلی برگردانید و در صورت نیاز backup دیتابیس را restore کنید.
