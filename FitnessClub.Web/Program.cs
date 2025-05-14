using FitnessClub.BLL.AutoMapping;
using FitnessClub.BLL.Interfaces;
using FitnessClub.BLL.Services;
using FitnessClub.Core.Abstractions;
using FitnessClub.DAL;
using FitnessClub.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using FitnessClub.BLL;
using FitnessClub.Entities;
using Autofac;
using Autofac.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FitnessClubContext>(options =>
    options.UseSqlite(connectionString));

try
{
    builder.Services.AddAutoMapper(typeof(MappingProfile));
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка реєстрації AutoMapper: {ex.Message}");
    throw;
}

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<ClubService>().As<IClubService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<TrainerService>().As<ITrainerService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MembershipService>().As<IMembershipService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<ClassScheduleService>().As<IClassScheduleService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<BookingService>().As<IBookingService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MembershipBookingStrategy>().As<IBookingStrategy>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<GuestBookingStrategy>().As<IBookingStrategy>().InstancePerLifetimeScope();
});

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
    try
    {
        await DbInitializer.InitializeAsync(services, configuration);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Критична помилка під час ініціалізації бази даних: {ex.Message}");
        throw;
    }
}

try
{
    AppSettings.Initialize(app.Configuration);
}
catch(Exception ex)
{
    Console.WriteLine($"Критична помилка під час ініціалізації AppSettings: {ex.Message}");
    throw;
}

app.Run();
