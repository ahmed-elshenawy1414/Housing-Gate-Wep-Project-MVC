using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using StudentHousing.Data;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ---------- MVC + DataAnnotations localization ----------
builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(SharedResource));
    });

// Localize built-in validation-attribute messages (server + client) by stamping
// every attribute without an explicit message with a convention resource key.
builder.Services.Configure<MvcOptions>(options =>
{
    options.ModelMetadataDetailsProviders.Add(new LocalizedValidationMetadataProvider());
});

// ---------- Localization (Arabic default, English secondary) ----------
// The resource classes live in StudentHousing.Resources.* so no ResourcesPath
// prefix is needed: Resources/SharedResource.resx -> StudentHousing.Resources.SharedResource
builder.Services.AddLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("ar-EG"),
        new CultureInfo("en-US")
    };

    options.DefaultRequestCulture = new RequestCulture("ar-EG");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;

    // Cookie first, then Accept-Language header.
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
    options.RequestCultureProviders.Insert(1, new AcceptLanguageHeaderRequestCultureProvider());
});

// ---------- Data access ----------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- ASP.NET Core Identity — hardened ----------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Keep registration flow functional (no email sender configured) but harden password.
        // Documented: RequireConfirmedAccount=false to avoid breaking current Register->SignIn.
        // When email sender is added, flip to true and require confirmation.
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredUniqueChars = 1;

        // Lockout hardening
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    // In Development allow HTTP, in Production require HTTPS
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = ".HousingGate.Auth";
});

// Rate limiting for auth endpoints (login) — fixed window, does not block NAT aggressively
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests. Try again later.", token);
    };
});

// ---------- Dependency injection ----------
builder.Services.AddMemoryCache();
builder.Services.AddScoped<StudentHousing.Services.Interfaces.ISiteSettingService, StudentHousing.Services.Implementations.SiteSettingService>();
builder.Services.AddScoped<StudentHousing.Services.Interfaces.IHousingRequestService, StudentHousing.Services.Implementations.HousingRequestService>();
builder.Services.AddScoped<IUnitOfWork, StudentHousing.Repositories.Implementations.UnitOfWork>();

builder.Services.AddScoped<StudentHousing.Services.Interfaces.IDocumentStorageService, StudentHousing.Services.Implementations.DocumentStorageService>();

builder.Services.AddScoped<IAccountService, StudentHousing.Services.Implementations.AccountService>();
builder.Services.AddScoped<IPropertyService, StudentHousing.Services.Implementations.PropertyService>();
builder.Services.AddScoped<IStudentService, StudentHousing.Services.Implementations.StudentService>();
builder.Services.AddScoped<IMatchingService, StudentHousing.Services.Implementations.MatchingService>();
builder.Services.AddScoped<IReviewService, StudentHousing.Services.Implementations.ReviewService>();
builder.Services.AddScoped<IComplaintService, StudentHousing.Services.Implementations.ComplaintService>();
builder.Services.AddScoped<IAdminService, StudentHousing.Services.Implementations.AdminService>();
builder.Services.AddScoped<INotificationService, StudentHousing.Services.Implementations.NotificationService>();
builder.Services.AddScoped<IAuditLogService, StudentHousing.Services.Implementations.AuditLogService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IOwnerService, StudentHousing.Services.Implementations.OwnerService>();

builder.Services.Configure<StudentHousing.Settings.PropertyReviewSettings>(
    builder.Configuration.GetSection("PropertyReviewSettings"));

var app = builder.Build();

// ---------- Apply pending migrations + seed demo data ----------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    // Skip Migrate in Testing (WebApplicationFactory with InMemory)
    if (!app.Environment.IsEnvironment("Testing"))
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await DbSeeder.SeedAsync(db, userManager, roleManager);

    // Safe idempotent migration of legacy verification docs from wwwroot/uploads to private storage
    try
    {
        var env = services.GetRequiredService<IWebHostEnvironment>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        var docService = services.GetRequiredService<StudentHousing.Services.Interfaces.IDocumentStorageService>();

        var legacyStudents = await db.StudentProfiles
            .Where(s => (s.NationalIdDocumentUrl != null && s.NationalIdDocumentUrl.StartsWith("/uploads/"))
                     || (s.UniversityIdDocumentUrl != null && s.UniversityIdDocumentUrl.StartsWith("/uploads/")))
            .ToListAsync();
        foreach (var s in legacyStudents)
        {
            if (s.NationalIdDocumentUrl != null && s.NationalIdDocumentUrl.StartsWith("/uploads/"))
            {
                var newPath = await docService.MigrateLegacyIfNeededAsync(s.NationalIdDocumentUrl, s.UserId, "student-verify");
                if (newPath != null && newPath != s.NationalIdDocumentUrl)
                {
                    s.NationalIdDocumentUrl = newPath;
                    logger.LogInformation("Migrated NationalId for user {UserId}", s.UserId);
                }
            }
            if (s.UniversityIdDocumentUrl != null && s.UniversityIdDocumentUrl.StartsWith("/uploads/"))
            {
                var newPath = await docService.MigrateLegacyIfNeededAsync(s.UniversityIdDocumentUrl, s.UserId, "student-verify");
                if (newPath != null && newPath != s.UniversityIdDocumentUrl)
                {
                    s.UniversityIdDocumentUrl = newPath;
                    logger.LogInformation("Migrated UniversityId for user {UserId}", s.UserId);
                }
            }
        }

        var legacyOwners = await db.OwnerProfiles
            .Where(o => o.VerificationDocumentUrl != null && o.VerificationDocumentUrl.StartsWith("/uploads/"))
            .ToListAsync();
        foreach (var o in legacyOwners)
        {
            var newPath = await docService.MigrateLegacyIfNeededAsync(o.VerificationDocumentUrl, o.UserId, "owner-verify");
            if (newPath != null && newPath != o.VerificationDocumentUrl)
            {
                o.VerificationDocumentUrl = newPath;
                logger.LogInformation("Migrated Owner doc for user {UserId}", o.UserId);
            }
        }

        if (legacyStudents.Count > 0 || legacyOwners.Count > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Legacy document migration completed: {Students} students, {Owners} owners", legacyStudents.Count, legacyOwners.Count);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Legacy document migration failed — continuing startup");
    }
}

// ---------- HTTP pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // In dev, still hide stack via exception handler for privacy, but show detailed errors via logging only
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();

// Security headers (minimal, no large framework)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-XSS-Protection"] = "0";
    await next();
});

// Block direct anonymous access to legacy verification documents via /uploads
// Public property images remain served; sensitive docs must go via DocumentsController
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/uploads", StringComparison.OrdinalIgnoreCase))
    {
        var fileName = Path.GetFileName(context.Request.Path.Value ?? string.Empty);
        if (!string.IsNullOrEmpty(fileName))
        {
            var legacyPath = $"/uploads/{fileName}";
            // Check if this path is still referenced as a verification document (legacy not yet migrated or backup failed)
            try
            {
                using var scope = context.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                bool isSensitive = await db.StudentProfiles.AnyAsync(s => s.NationalIdDocumentUrl == legacyPath || s.UniversityIdDocumentUrl == legacyPath)
                    || await db.OwnerProfiles.AnyAsync(o => o.VerificationDocumentUrl == legacyPath);
                if (isSensitive)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Not found");
                    return;
                }
            }
            catch
            {
                // On DB error, fail closed for sensitive-looking paths? Allow static to proceed to avoid blocking property images
            }
        }
    }
    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.UseRateLimiter();

app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

// ---------- Routing (areas first, then default) ----------
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
