using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
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

// ---------- ASP.NET Core Identity ----------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
    })
    .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// ---------- Dependency injection ----------
builder.Services.AddScoped<IUnitOfWork, StudentHousing.Repositories.Implementations.UnitOfWork>();

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
    await db.Database.MigrateAsync();

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await DbSeeder.SeedAsync(db, userManager, roleManager);
}

// ---------- HTTP pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

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
