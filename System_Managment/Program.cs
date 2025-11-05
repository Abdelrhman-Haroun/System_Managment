using BL.Services.Service;
using BLL.Repository;
using BLL.Services.IService;
using BLL.Services.Service;
using DAL.Data;
using DAL.IRepository;
using DAL.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddControllersWithViews();


builder.Services.AddTransient<IEmailSender, SmtpEmailSender>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>().GetSection("EmailSettings");
    return new SmtpEmailSender(
        host: config["Host"],
        port: int.Parse(config["Port"]),
        enableSSL: bool.Parse(config["EnableSSL"]),
        userName: config["UserName"],
        password: config["Password"],
        senderName: config["SenderName"]
    );
});


builder.Services.AddSingleton<IFileUploader, FileUploader>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(4))
 .AddDefaultTokenProviders()
 .AddDefaultUI()
 .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Adjust as needed
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 0; 
    options.Password.RequiredUniqueChars = 0;
});

// Configure authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Redirect to login page
    options.LogoutPath = "/Account/Logout"; // Redirect after logout
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ReturnUrlParameter = "returnUrl";
    options.ExpireTimeSpan = TimeSpan.FromHours(24); // Session duration
    options.SlidingExpiration = true;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configure middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//Redirect root to login or home page
app.MapGet("/", async context =>
{
    if (!context.User.Identity.IsAuthenticated)
    {
        context.Response.Redirect("/Account/Login");
return;
    }
    context.Response.Redirect("/Home/Index");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
