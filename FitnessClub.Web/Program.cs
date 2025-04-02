using FitnessClub.BLL.AutoMapping;
using FitnessClub.BLL.Interfaces;
using FitnessClub.BLL.Services;
using FitnessClub.Core.Abstractions;
using FitnessClub.DAL;
using FitnessClub.DAL.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
}
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

app.Run();
