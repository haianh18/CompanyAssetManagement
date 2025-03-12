using FinalProject.Enums;
using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class DataSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DataSeeder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CompanyAssetManagementContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

            await context.Database.MigrateAsync(cancellationToken);

            // Seed roles
            if (!await context.Roles.AnyAsync(cancellationToken))
            {
                await roleManager.CreateAsync(new AppRole { Name = "Admin", RoleType = RoleType.ADMIN });
                await roleManager.CreateAsync(new AppRole { Name = "WarehouseManager", RoleType = RoleType.WAREHOUSE_MANAGER });
                await roleManager.CreateAsync(new AppRole { Name = "GeneralUser", RoleType = RoleType.GENERAL_USER });
            }

            // Seed users
            // Seed users
            if (!await context.Users.AnyAsync(cancellationToken))
            {
                var adminUserSeed = new AppUser { UserName = "admin", Email = "admin@example.com", FullName = "Admin User", RoleId = 1 };
                await userManager.CreateAsync(adminUserSeed, "Admin@123");
                await userManager.AddToRoleAsync(adminUserSeed, "Admin");

                var warehouseManagerUserSeed = new AppUser { UserName = "manager", Email = "manager@example.com", FullName = "Warehouse Manager", RoleId = 2 };
                await userManager.CreateAsync(warehouseManagerUserSeed, "Manager@123");
                await userManager.AddToRoleAsync(warehouseManagerUserSeed, "WarehouseManager");

                var generalUserSeed = new AppUser { UserName = "user", Email = "user@example.com", FullName = "General User", RoleId = 3 };
                await userManager.CreateAsync(generalUserSeed, "User@123");
                await userManager.AddToRoleAsync(generalUserSeed, "GeneralUser");

                for (int i = 1; i <= 3; i++)
                {
                    var userSeed = new AppUser { UserName = $"user{i}", Email = $"user{i}@example.com", FullName = $"User {i}", RoleId = 3 };
                    await userManager.CreateAsync(userSeed, $"User{i}@123");
                    await userManager.AddToRoleAsync(userSeed, "GeneralUser");
                }
            }

            // Retrieve user IDs
            var adminUser = await userManager.FindByNameAsync("admin");
            var warehouseManagerUser = await userManager.FindByNameAsync("manager");
            var generalUser = await userManager.FindByNameAsync("user");
            var user1 = await userManager.FindByNameAsync("user1");
            var user2 = await userManager.FindByNameAsync("user2");
            var user3 = await userManager.FindByNameAsync("user3");

            if (adminUser == null || warehouseManagerUser == null || generalUser == null || user1 == null || user2 == null || user3 == null)
            {
                throw new Exception("One or more users could not be found.");
            }


            var adminUserId = adminUser.Id;
            var warehouseManagerUserId = warehouseManagerUser.Id;
            var generalUserId = generalUser.Id;
            var user1Id = user1.Id;
            var user2Id = user2.Id;
            var user3Id = user3.Id;

            // Seed asset categories
            if (!await context.AssetCategories.AnyAsync(cancellationToken))
            {
                context.AssetCategories.AddRange(
                    new AssetCategory { Name = "Electronics" },
                    new AssetCategory { Name = "Furniture" },
                    new AssetCategory { Name = "Stationery" },
                    new AssetCategory { Name = "Vehicles" },
                    new AssetCategory { Name = "Tools" }
                );
                await context.SaveChangesAsync(cancellationToken);
            }

            // Seed assets
            if (!await context.Assets.AnyAsync(cancellationToken))
            {
                context.Assets.AddRange(
                    new Asset { Name = "Laptop", Price = 1000, Unit = "Piece", AssetCategoryId = 1, AssetStatus = AssetStatus.GOOD },
                    new Asset { Name = "Chair", Price = 50, Unit = "Piece", AssetCategoryId = 2, AssetStatus = AssetStatus.GOOD },
                    new Asset { Name = "Pen", Price = 1, Unit = "Piece", AssetCategoryId = 3, AssetStatus = AssetStatus.GOOD },
                    new Asset { Name = "Car", Price = 20000, Unit = "Piece", AssetCategoryId = 4, AssetStatus = AssetStatus.GOOD },
                    new Asset { Name = "Hammer", Price = 10, Unit = "Piece", AssetCategoryId = 5, AssetStatus = AssetStatus.GOOD }
                );
                await context.SaveChangesAsync(cancellationToken);
            }

            // Seed departments
            if (!await context.Departments.AnyAsync(cancellationToken))
            {
                context.Departments.AddRange(
                    new Department { Name = "HR" },
                    new Department { Name = "IT" },
                    new Department { Name = "Finance" },
                    new Department { Name = "Logistics" },
                    new Department { Name = "Sales" }
                );
                await context.SaveChangesAsync(cancellationToken);
            }

            // Seed warehouses
            if (!await context.Warehouses.AnyAsync(cancellationToken))
            {
                context.Warehouses.AddRange(
                    new Warehouse { Name = "Main Warehouse", Address = "123 Main St" },
                    new Warehouse { Name = "Secondary Warehouse", Address = "456 Secondary St" },
                    new Warehouse { Name = "Electronics Warehouse", Address = "789 Electronics St" },
                    new Warehouse { Name = "Furniture Warehouse", Address = "101 Furniture St" },
                    new Warehouse { Name = "Tools Warehouse", Address = "202 Tools St" }
                );
                await context.SaveChangesAsync(cancellationToken);
            }

            // Seed warehouse assets
            if (!await context.WarehouseAssets.AnyAsync(cancellationToken))
            {
                context.WarehouseAssets.AddRange(
                    new WarehouseAsset { WarehouseId = 1, AssetId = 1, Quantity = 10 },
                    new WarehouseAsset { WarehouseId = 2, AssetId = 2, Quantity = 20 },
                    new WarehouseAsset { WarehouseId = 3, AssetId = 3, Quantity = 100 },
                    new WarehouseAsset { WarehouseId = 4, AssetId = 4, Quantity = 5 },
                    new WarehouseAsset { WarehouseId = 5, AssetId = 5, Quantity = 50 }
                );
                await context.SaveChangesAsync(cancellationToken);
            }

            // Seed borrow tickets
            if (!await context.BorrowTickets.AnyAsync(cancellationToken))
            {
                context.BorrowTickets.AddRange(
                    new BorrowTicket { WarehouseAssetId = 1, BorrowById = adminUserId, OwnerId = adminUserId, Quantity = 1, ReturnDate = DateTime.Now.AddDays(7) },
                    new BorrowTicket { WarehouseAssetId = 2, BorrowById = warehouseManagerUserId, OwnerId = warehouseManagerUserId, Quantity = 2, ReturnDate = DateTime.Now.AddDays(7) },
                    new BorrowTicket { WarehouseAssetId = 3, BorrowById = generalUserId, OwnerId = generalUserId, Quantity = 3, ReturnDate = DateTime.Now.AddDays(7) },
                    new BorrowTicket { WarehouseAssetId = 4, BorrowById = user1Id, OwnerId = user1Id, Quantity = 4, ReturnDate = DateTime.Now.AddDays(7) },
                    new BorrowTicket { WarehouseAssetId = 5, BorrowById = user2Id, OwnerId = user2Id, Quantity = 5, ReturnDate = DateTime.Now.AddDays(7) }
                );
                await context.SaveChangesAsync(cancellationToken);
            }

            // Retrieve borrow ticket IDs
            var borrowTicket1 = await context.BorrowTickets.FirstOrDefaultAsync(bt => bt.WarehouseAssetId == 1 && bt.BorrowById == adminUserId);
            var borrowTicket2 = await context.BorrowTickets.FirstOrDefaultAsync(bt => bt.WarehouseAssetId == 2 && bt.BorrowById == warehouseManagerUserId);
            var borrowTicket3 = await context.BorrowTickets.FirstOrDefaultAsync(bt => bt.WarehouseAssetId == 3 && bt.BorrowById == generalUserId);
            var borrowTicket4 = await context.BorrowTickets.FirstOrDefaultAsync(bt => bt.WarehouseAssetId == 4 && bt.BorrowById == user1Id);
            var borrowTicket5 = await context.BorrowTickets.FirstOrDefaultAsync(bt => bt.WarehouseAssetId == 5 && bt.BorrowById == user2Id);

            if (borrowTicket1 == null || borrowTicket2 == null || borrowTicket3 == null || borrowTicket4 == null || borrowTicket5 == null)
            {
                throw new Exception("One or more borrow tickets could not be found.");
            }

            // Seed disposal tickets
            if (!await context.DisposalTickets.AnyAsync(cancellationToken))
            {
                context.DisposalTickets.AddRange(
                    new DisposalTicket { DisposalById = adminUserId, OwnerId = adminUserId, Reason = "Broken" },
                    new DisposalTicket { DisposalById = warehouseManagerUserId, OwnerId = warehouseManagerUserId, Reason = "Outdated" },
                    new DisposalTicket { DisposalById = generalUserId, OwnerId = generalUserId, Reason = "Damaged" },
                    new DisposalTicket { DisposalById = user1Id, OwnerId = user1Id, Reason = "Expired" },
                    new DisposalTicket { DisposalById = user2Id, OwnerId = user2Id, Reason = "Lost" }
                );
                await context.SaveChangesAsync(cancellationToken);
            }

            // Seed handover tickets
            if (!await context.HandoverTickets.AnyAsync(cancellationToken))
            {
                context.HandoverTickets.AddRange(
                    new HandoverTicket { WarehouseAssetId = 1, HandoverById = adminUserId, HandoverToId = warehouseManagerUserId, Quantity = 1 },
                    new HandoverTicket { WarehouseAssetId = 2, HandoverById = warehouseManagerUserId, HandoverToId = generalUserId, Quantity = 2 },
                    new HandoverTicket { WarehouseAssetId = 3, HandoverById = generalUserId, HandoverToId = user1Id, Quantity = 3 },
                    new HandoverTicket { WarehouseAssetId = 4, HandoverById = user1Id, HandoverToId = user2Id, Quantity = 4 },
                    new HandoverTicket { WarehouseAssetId = 5, HandoverById = user2Id, HandoverToId = adminUserId, Quantity = 5 }
                );
                await context.SaveChangesAsync(cancellationToken);
            }

            // Seed return tickets
            if (!await context.ReturnTickets.AnyAsync(cancellationToken))
            {
                context.ReturnTickets.AddRange(
                    new ReturnTicket { BorrowTicketId = borrowTicket1.Id, OwnerId = adminUserId, Quantity = 1 },
                    new ReturnTicket { BorrowTicketId = borrowTicket2.Id, OwnerId = warehouseManagerUserId, Quantity = 2 },
                    new ReturnTicket { BorrowTicketId = borrowTicket3.Id, OwnerId = generalUserId, Quantity = 3 },
                    new ReturnTicket { BorrowTicketId = borrowTicket4.Id, OwnerId = user1Id, Quantity = 4 },
                    new ReturnTicket { BorrowTicketId = borrowTicket5.Id, OwnerId = user2Id, Quantity = 5 }
                );
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

