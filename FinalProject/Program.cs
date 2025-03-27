using FinalProject.Models;
using FinalProject.Repositories;
using FinalProject.Repositories.Common;
using FinalProject.Services;
using FinalProject.Services.Interfaces;
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
        builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<CompanyAssetManagementContext>();

        // Add MVC services
        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register services
        builder.Services.AddScoped<IRoleRepository, RoleRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        builder.Services.AddScoped<IAssetService, AssetService>();
        builder.Services.AddScoped<IAssetCategoryService, AssetCategoryService>();
        builder.Services.AddScoped<IBorrowTicketService, BorrowTicketService>();
        builder.Services.AddScoped<IDepartmentService, DepartmentService>();
        builder.Services.AddScoped<IDisposalTicketService, DisposalTicketService>();
        builder.Services.AddScoped<IDisposalTicketAssetService, DisposalTicketAssetService>();
        builder.Services.AddScoped<IHandoverTicketService, HandoverTicketService>();
        builder.Services.AddScoped<IHandoverReturnService, HandoverReturnService>();
        builder.Services.AddScoped<IReturnTicketService, ReturnTicketService>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IWarehouseService, WarehouseService>();
        builder.Services.AddScoped<IWarehouseAssetService, WarehouseAssetService>();

        // Register the DataSeeder service
        builder.Services.AddHostedService<DataSeeder>();

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
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
