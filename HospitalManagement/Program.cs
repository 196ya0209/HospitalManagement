using HospitalManagement.Data;
using HospitalManagement.Repository;
using HospitalManagement.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HospitalManagement.Utility;
using Microsoft.Extensions.Logging;
using Serilog;
using HospitalManagement.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
//Connection to Database
builder.Services.AddDbContext<ApplicationDbContext>(option => {
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddRoles<IdentityRole>().
    AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
//Services Registered
builder.Services.AddScoped<IDoctorRepository,DoctorRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(HospitalRoles.Admin, policy => policy.RequireRole(HospitalRoles.Admin));
    options.AddPolicy(HospitalRoles.Doctor, policy => policy.RequireRole(HospitalRoles.Doctor));
    options.AddPolicy(HospitalRoles.Patient, policy => policy.RequireRole(HospitalRoles.Patient));
});
//Logging
Log.Logger = new LoggerConfiguration().
    MinimumLevel.Debug().
    WriteTo.Console().WriteTo.
    File("Logs\\log.txt", rollingInterval: RollingInterval.Day).CreateLogger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    //Custom Middleware for Custom Exception
    app.UseMyExceptionMiddleware();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
