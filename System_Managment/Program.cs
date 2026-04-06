using BLL.Mapper;
using BLL.Services.IService;
using BLL.Services.Service;
using DAL.Data;
using DAL.Models;
using DAL.Repositories.IRepository;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Westwind.AspNetCore.LiveReload;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var mvcBuilder = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
    builder.Services.AddLiveReload();
}

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

var dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, ".app-data-protection");
Directory.CreateDirectory(dataProtectionPath);

builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("System_Managment");

builder.Services.AddTransient<IEmailSender, SmtpEmailSender>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>().GetSection("EmailSettings");
    var port = int.TryParse(config["Port"], out var parsedPort) ? parsedPort : 25;
    var enableSsl = bool.TryParse(config["EnableSSL"], out var parsedEnableSsl) && parsedEnableSsl;

    return new SmtpEmailSender(
        host: config["Host"] ?? string.Empty,
        port: port,
        enableSSL: enableSsl,
        userName: config["UserName"] ?? string.Empty,
        password: config["Password"] ?? string.Empty,
        senderName: config["SenderName"] ?? "System"
    );
});

builder.Services.AddScoped<IInternalProductUsageService, InternalProductUsageService>();
builder.Services.AddScoped<ITransactionReportService, TransactionReportService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IFileUploader, FileUploader>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IEmployeeTypeService, EmployeeTypeService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeAttendanceService, EmployeeAttendanceService>();
builder.Services.AddScoped<IUserService, UserService>();

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5, // how many times to retry
                maxRetryDelay: TimeSpan.FromSeconds(30), // wait between retries
                errorNumbersToAdd: null // retry on all transient errors
            );
        }));


builder.Services.AddAutoMapper(typeof(DomainProfile));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(4);
    })
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 0;
    options.Password.RequiredUniqueChars = 0;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ReturnUrlParameter = "returnUrl";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseLiveReload();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", context =>
{
    context.Response.Redirect(
        context.User.Identity?.IsAuthenticated == true ? "/Home/Index" : "/Account/Login");
    return Task.CompletedTask;
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
