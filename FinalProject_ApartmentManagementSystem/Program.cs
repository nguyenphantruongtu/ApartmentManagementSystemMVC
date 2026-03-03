using BusinessObjects.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services;

namespace FinalProject_ApartmentManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            builder.Services.AddDbContext<AMSDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString")));

            var cookieSection = configuration.GetSection("Auth:Cookie");
            var expireHours = cookieSection.GetValue<int?>("ExpireHours") ?? 8;

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
            {
                options.LoginPath = cookieSection.GetValue<string>("LoginPath") ?? "/Account/Login";
                options.LogoutPath = cookieSection.GetValue<string>("LogoutPath") ?? "/Account/Logout";
                options.AccessDeniedPath = cookieSection.GetValue<string>("AccessDeniedPath") ?? "/Account/AccessDenied";
                options.Cookie.Name = cookieSection.GetValue<string>("CookieName") ?? "AMS.Auth";
                options.Cookie.HttpOnly = cookieSection.GetValue<bool?>("HttpOnly") ?? true;
                options.Cookie.SecurePolicy = Enum.TryParse<CookieSecurePolicy>(
                    cookieSection.GetValue<string>("SecurePolicy"),
                    ignoreCase: true,
                    out var securePolicy)
                    ? securePolicy
                    : CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromHours(Math.Max(1, expireHours));
                options.SlidingExpiration = cookieSection.GetValue<bool?>("SlidingExpiration") ?? true;
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AnyRole", policy =>
                    policy.RequireRole("Admin", "Staff", "Resident"));

                options.AddPolicy("StaffOrAdmin", policy =>
                    policy.RequireRole("Admin", "Staff"));

                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                options.AddPolicy("ResidentOnly", policy =>
                    policy.RequireRole("Resident"));
            });
            builder.Services.AddDataProtection();

            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            builder.Services.AddScoped<IApartmentRepository, ApartmentRepository>();
            builder.Services.AddScoped<IResidentRepository, ResidentRepository>();
            builder.Services.AddScoped<IApartmentResidentRepository, ApartmentResidentRepository>();
            builder.Services.AddScoped<IIssueRepository, IssueRepository>();
            builder.Services.AddScoped<IIssueCategoryRepository, IssueCategoryRepository>();
            builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IApartmentService, ApartmentService>();
            builder.Services.AddScoped<IEmailService, SmtpEmailService>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

            app.Run();
        }
    }
}
