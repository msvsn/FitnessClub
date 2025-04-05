using FitnessClub.BLL.AutoMapping;
using FitnessClub.BLL.Interfaces;
using FitnessClub.BLL.Services;
using FitnessClub.Core.Abstractions;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using FitnessClub.DAL.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using FitnessClub.BLL;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FitnessClubContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

try
{
    builder.Services.AddAutoMapper(typeof(MappingProfile));
}
catch (Exception ex)
{
    Console.WriteLine($"Error registering AutoMapper: {ex.Message}");
    throw;
}

builder.Services.AddScoped<IClubService, ClubService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITrainerService, TrainerService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IClassScheduleService, ClassScheduleService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IBookingStrategy, MembershipBookingStrategy>();
builder.Services.AddScoped<IBookingStrategy, GuestBookingStrategy>();
builder.Services.AddLogging();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); 
        options.SlidingExpiration = true; 
        options.LoginPath = "/Account/Login"; 
        options.AccessDeniedPath = "/Account/AccessDenied"; 
        options.Cookie.HttpOnly = true; 
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/hello", () => "Hello World!");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var configuration = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<FitnessClubContext>>();
    try
    {
        await DbInitializer.InitializeAsync(services, configuration, logger);
    }
    catch (Exception ex)
    {
        var initLogger = services.GetRequiredService<ILogger<Program>>();
        initLogger.LogError(ex, "An error occurred during database initialization.");
    }
}

try
{
    AppSettings.Initialize(app.Configuration);
    app.Logger.LogInformation("AppSettings initialized successfully.");
}
catch(Exception ex)
{
    app.Logger.LogCritical(ex, "Failed to initialize AppSettings.");
}

app.Run();
