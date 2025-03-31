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

                for (int i = 1; i <= 10; i++)
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
            var user4 = await userManager.FindByNameAsync("user4");
            var user5 = await userManager.FindByNameAsync("user5");
            var user6 = await userManager.FindByNameAsync("user6");
            var user7 = await userManager.FindByNameAsync("user7");
            var user8 = await userManager.FindByNameAsync("user8");
            var user9 = await userManager.FindByNameAsync("user9");
            var user10 = await userManager.FindByNameAsync("user10");

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
            var user4Id = user4.Id;
            var user5Id = user5.Id;
            var user6Id = user6.Id;
            var user7Id = user7.Id;
            var user8Id = user8.Id;
            var user9Id = user9.Id;
            var user10Id = user10.Id;

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
                    new Asset { Name = "Laptop", Price = 1000, Unit = "Piece", AssetCategoryId = 1 },
                    new Asset { Name = "Chair", Price = 50, Unit = "Piece", AssetCategoryId = 2 },
                    new Asset { Name = "Pen", Price = 1, Unit = "Piece", AssetCategoryId = 3 },
                    new Asset { Name = "Car", Price = 20000, Unit = "Piece", AssetCategoryId = 4 },
                    new Asset { Name = "Hammer", Price = 10, Unit = "Piece", AssetCategoryId = 5 }
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
            // Xóa tất cả warehouse assets hiện có trước khi thêm mới
            if (await context.WarehouseAssets.AnyAsync(cancellationToken))
            {
                // First, handle the related borrow tickets
                // Using Any
                //var warehouseAssets = context.WarehouseAssets.Include(b => b.BorrowTickets).Include(h => h.HandoverTickets).ToList();

                //foreach (var wa in warehouseAssets)
                //{
                //    wa.BorrowTickets.Clear();
                //    wa.HandoverTickets.Clear();
                //}
                //// Then remove warehouse assets
                //context.WarehouseAssets.RemoveRange(context.WarehouseAssets);
                //await context.SaveChangesAsync(cancellationToken);
            }

            if (!await context.WarehouseAssets.AnyAsync(cancellationToken))
            {

                // Thêm dữ liệu mới
                context.WarehouseAssets.AddRange(
                    // Laptop ở kho chính - một số đã bàn giao, một số đang được mượn
                    new WarehouseAsset
                    {
                        WarehouseId = 1,
                        AssetId = 1,
                        GoodQuantity = 15,
                        BrokenQuantity = 2,
                        FixingQuantity = 3,
                        DisposedQuantity = 1,
                        BorrowedGoodQuantity = 5,
                        HandedOverGoodQuantity = 4,
                        DateCreated = DateTime.Now
                    },

                    // Ghế ở kho phụ - hầu hết đã bàn giao cho các phòng ban
                    new WarehouseAsset
                    {
                        WarehouseId = 2,
                        AssetId = 2,
                        GoodQuantity = 50,
                        BrokenQuantity = 5,
                        FixingQuantity = 0,
                        DisposedQuantity = 10,
                        BorrowedGoodQuantity = 2,
                        HandedOverGoodQuantity = 35,
                        DateCreated = DateTime.Now
                    },

                    // Bút ở kho điện tử - số lượng lớn, phần lớn khả dụng
                    new WarehouseAsset
                    {
                        WarehouseId = 3,
                        AssetId = 3,
                        GoodQuantity = 200,
                        BrokenQuantity = 0,
                        FixingQuantity = 0,
                        DisposedQuantity = 15,
                        BorrowedGoodQuantity = 25,
                        HandedOverGoodQuantity = 0,
                        DateCreated = DateTime.Now
                    },

                    // Xe ô tô ở kho nội thất - số lượng ít, tất cả trong tình trạng tốt
                    new WarehouseAsset
                    {
                        WarehouseId = 4,
                        AssetId = 4,
                        GoodQuantity = 5,
                        BrokenQuantity = 0,
                        FixingQuantity = 1,
                        DisposedQuantity = 0,
                        BorrowedGoodQuantity = 2,
                        HandedOverGoodQuantity = 1,
                        DateCreated = DateTime.Now
                    },

                    // Búa ở kho công cụ - một số đang được sửa
                    new WarehouseAsset
                    {
                        WarehouseId = 5,
                        AssetId = 5,
                        GoodQuantity = 30,
                        BrokenQuantity = 10,
                        FixingQuantity = 15,
                        DisposedQuantity = 5,
                        BorrowedGoodQuantity = 8,
                        HandedOverGoodQuantity = 12,
                        DateCreated = DateTime.Now
                    },

                    // Laptop ở kho phụ
                    new WarehouseAsset
                    {
                        WarehouseId = 2,
                        AssetId = 1,
                        GoodQuantity = 8,
                        BrokenQuantity = 3,
                        FixingQuantity = 2,
                        DisposedQuantity = 0,
                        BorrowedGoodQuantity = 3,
                        HandedOverGoodQuantity = 2,
                        DateCreated = DateTime.Now
                    },

                    // Ghế ở kho chính
                    new WarehouseAsset
                    {
                        WarehouseId = 1,
                        AssetId = 2,
                        GoodQuantity = 25,
                        BrokenQuantity = 8,
                        FixingQuantity = 4,
                        DisposedQuantity = 3,
                        BorrowedGoodQuantity = 5,
                        HandedOverGoodQuantity = 15,
                        DateCreated = DateTime.Now
                    },

                    // Bút ở kho công cụ
                    new WarehouseAsset
                    {
                        WarehouseId = 5,
                        AssetId = 3,
                        GoodQuantity = 100,
                        BrokenQuantity = 20,
                        FixingQuantity = 0,
                        DisposedQuantity = 30,
                        BorrowedGoodQuantity = 15,
                        HandedOverGoodQuantity = 5,
                        DateCreated = DateTime.Now
                    }
                );

                await context.SaveChangesAsync(cancellationToken);
            }
            // Seed borrow tickets
            if (!await context.BorrowTickets.AnyAsync(cancellationToken))
            {
                //// Xóa tất cả phiếu mượn hiện có trước khi thêm mới
                //var borrowTickets = context.BorrowTickets
                //    .Include(bt => bt.WarehouseAsset)
                //    .ToList();


                if (adminUser == null || warehouseManagerUser == null || generalUser == null || user1 == null || user2 == null)
                {
                    throw new Exception("One or more users could not be found.");
                }

                // Tạo nhiều phiếu mượn với trạng thái khác nhau
                context.BorrowTickets.AddRange(
                    // Phiếu chờ duyệt
                    new BorrowTicket
                    {
                        WarehouseAssetId = 1,
                        BorrowById = generalUserId,
                        Quantity = 2,
                        ReturnDate = DateTime.Now.AddDays(14),
                        DateCreated = DateTime.Now.AddDays(-2),
                        ApproveStatus = TicketStatus.Pending,
                        Note = "Mượn laptop để làm việc tại nhà"
                    },
                    new BorrowTicket
                    {
                        WarehouseAssetId = 3,
                        BorrowById = user1Id,
                        Quantity = 5,
                        ReturnDate = DateTime.Now.AddDays(7),
                        DateCreated = DateTime.Now.AddDays(-1),
                        ApproveStatus = TicketStatus.Pending,
                        Note = "Mượn bút để phát cho học viên tham gia đào tạo"
                    },

                    // Phiếu đã duyệt
                    new BorrowTicket
                    {
                        WarehouseAssetId = 2,
                        BorrowById = user2Id,
                        OwnerId = warehouseManagerUserId,
                        Quantity = 3,
                        ReturnDate = DateTime.Now.AddDays(21),
                        DateCreated = DateTime.Now.AddDays(-5),
                        DateModified = DateTime.Now.AddDays(-4),
                        ApproveStatus = TicketStatus.Approved,
                        Note = "Mượn ghế để tổ chức sự kiện"
                    },
                    new BorrowTicket
                    {
                        WarehouseAssetId = 4,
                        BorrowById = generalUserId,
                        OwnerId = warehouseManagerUserId,
                        Quantity = 1,
                        ReturnDate = DateTime.Now.AddDays(1),
                        DateCreated = DateTime.Now.AddDays(-10),
                        DateModified = DateTime.Now.AddDays(-9),
                        ApproveStatus = TicketStatus.Approved,
                        Note = "Mượn xe cho chuyến công tác"
                    },

                    // Phiếu bị từ chối
                    new BorrowTicket
                    {
                        WarehouseAssetId = 5,
                        BorrowById = user3Id,
                        OwnerId = warehouseManagerUserId,
                        Quantity = 10,
                        ReturnDate = DateTime.Now.AddDays(30),
                        DateCreated = DateTime.Now.AddDays(-8),
                        DateModified = DateTime.Now.AddDays(-7),
                        ApproveStatus = TicketStatus.Rejected,
                        Note = "Từ chối: Số lượng yêu cầu quá nhiều"
                    },

                    // Phiếu đã trả
                    new BorrowTicket
                    {
                        WarehouseAssetId = 1,
                        BorrowById = user1Id,
                        OwnerId = warehouseManagerUserId,
                        Quantity = 1,
                        ReturnDate = DateTime.Now.AddDays(-5),
                        DateCreated = DateTime.Now.AddDays(-20),
                        DateModified = DateTime.Now.AddDays(-19),
                        ApproveStatus = TicketStatus.Approved,
                        IsReturned = true,
                        Note = "Đã trả đúng hạn"
                    }
                );

                await context.SaveChangesAsync(cancellationToken);
            }

            // Retrieve borrow ticket IDs
            var borrowTicket1 = await context.BorrowTickets.FirstOrDefaultAsync(bt => bt.WarehouseAssetId == 1);
            var borrowTicket2 = await context.BorrowTickets.FirstOrDefaultAsync(bt => bt.WarehouseAssetId == 2);
            var borrowTicket3 = await context.BorrowTickets.FirstOrDefaultAsync(bt => bt.WarehouseAssetId == 3);
            var borrowTicket4 = await context.BorrowTickets.FirstOrDefaultAsync(bt => bt.WarehouseAssetId == 4);
            var borrowTicket5 = await context.BorrowTickets.FirstOrDefaultAsync(bt => bt.WarehouseAssetId == 5);



            // Seed disposal tickets
            if (!await context.DisposalTickets.AnyAsync(cancellationToken))
            {
                context.DisposalTickets.AddRange(
                    new DisposalTicket { DisposalById = warehouseManagerUserId, OwnerId = user3Id, Reason = "Broken" },
                    new DisposalTicket { DisposalById = warehouseManagerUserId, OwnerId = warehouseManagerUserId, Reason = "Outdated" },
                    new DisposalTicket { DisposalById = warehouseManagerUserId, OwnerId = generalUserId, Reason = "Damaged" },
                    new DisposalTicket { DisposalById = user1Id, OwnerId = user1Id, Reason = "Expired" },
                    new DisposalTicket { DisposalById = user2Id, OwnerId = user2Id, Reason = "Lost" }
                );
                await context.SaveChangesAsync(cancellationToken);
            }

            // Seed handover tickets
            if (!await context.HandoverTickets.AnyAsync(cancellationToken))
            {
                context.HandoverTickets.AddRange(
                    new HandoverTicket { WarehouseAssetId = 1, HandoverById = warehouseManagerUserId, HandoverToId = user3Id, Quantity = 1 },
                    new HandoverTicket { WarehouseAssetId = 2, HandoverById = warehouseManagerUserId, HandoverToId = generalUserId, Quantity = 2 },
                    new HandoverTicket { WarehouseAssetId = 3, HandoverById = warehouseManagerUserId, HandoverToId = user1Id, Quantity = 3 },
                    new HandoverTicket { WarehouseAssetId = 4, HandoverById = warehouseManagerUserId, HandoverToId = user2Id, Quantity = 4 },
                    new HandoverTicket { WarehouseAssetId = 5, HandoverById = warehouseManagerUserId, HandoverToId = user4Id, Quantity = 5 }
                );
                await context.SaveChangesAsync(cancellationToken);
            }

            // Seed return tickets
            if (!await context.ReturnTickets.AnyAsync(cancellationToken))
            {
                context.ReturnTickets.AddRange(
                    new ReturnTicket { BorrowTicketId = borrowTicket1.Id, OwnerId = generalUserId, Quantity = 1 },
                    new ReturnTicket { BorrowTicketId = borrowTicket2.Id, OwnerId = user2Id, Quantity = 2 },
                    new ReturnTicket { BorrowTicketId = borrowTicket3.Id, OwnerId = user1Id, Quantity = 3 },
                    new ReturnTicket { BorrowTicketId = borrowTicket4.Id, OwnerId = generalUserId, Quantity = 4 },
                    new ReturnTicket { BorrowTicketId = borrowTicket5.Id, OwnerId = user3Id, Quantity = 5 }
                );
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

