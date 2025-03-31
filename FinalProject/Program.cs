using FinalProject.Models;
using FinalProject.Repositories;
using FinalProject.Repositories.Common;
using FinalProject.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinalProject;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<CompanyAssetManagementContext>(options =>
            options.UseSqlServer(connectionString));

        // Add Identity services
        builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<CompanyAssetManagementContext>();

        // Add MVC services
        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register services
        builder.Services.AddScoped<IRoleRepository, RoleRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        builder.Services.AddScoped<IWarehouseAssetRepository, WarehouseAssetRepository>();
        builder.Services.AddScoped<IAssetRepository, AssetRepository>();
        builder.Services.AddScoped<IAssetCategoryRepository, AssetCategoryRepository>();
        builder.Services.AddScoped<IBorrowTicketRepository, BorrowTicketRepository>();
        builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        builder.Services.AddScoped<IDisposalTicketRepository, DisposalTicketRepository>();
        builder.Services.AddScoped<IDisposalTicketAssetRepository, DisposalTicketAssetRepository>();
        builder.Services.AddScoped<IHandoverTicketRepository, HandoverTicketRepository>();
        builder.Services.AddScoped<IHandoverReturnRepository, HandoverReturnRepository>();
        builder.Services.AddScoped<IReturnTicketRepository, ReturnTicketRepository>();
        builder.Services.AddScoped<IManagerReturnRequestRepository, ManagerReturnRequestRepository>();

        // Register the DataSeeder service
        //builder.Services.AddHostedService<DataSeeder>();

        // Configure authentication and authorization
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            // Use Developer Exception Page in development
            app.UseDeveloperExceptionPage();
        }
        else
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
            pattern: "{controller=Account}/{action=Login}/{id?}");

        app.Run();
    }
}