using FinalProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public partial class CompanyAssetManagementContext : IdentityDbContext<AppUser, AppRole, int>
{
    public CompanyAssetManagementContext(DbContextOptions<CompanyAssetManagementContext> options)
        : base(options)
    {
    }

    // Existing DbSets
    public virtual DbSet<Asset> Assets { get; set; }
    public virtual DbSet<AssetCategory> AssetCategories { get; set; }
    public virtual DbSet<BorrowTicket> BorrowTickets { get; set; }
    public virtual DbSet<Department> Departments { get; set; }
    public virtual DbSet<DisposalTicket> DisposalTickets { get; set; }
    public virtual DbSet<DisposalTicketAsset> DisposalTicketAssets { get; set; }
    public virtual DbSet<HandoverTicket> HandoverTickets { get; set; }
    public virtual DbSet<ReturnTicket> ReturnTickets { get; set; }
    public virtual DbSet<Warehouse> Warehouses { get; set; }
    public virtual DbSet<WarehouseAsset> WarehouseAssets { get; set; }
    public virtual DbSet<HandoverReturn> HandoverReturn { get; set; }
    public virtual DbSet<ManagerReturnRequest> ManagerReturnRequests { get; set; }
    //public virtual DbSet<Notification> Notifications { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Áp dụng global query filter cho soft delete
        modelBuilder.Entity<Asset>().HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<AssetCategory>().HasQueryFilter(ac => !ac.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Warehouse>().HasQueryFilter(w => !w.IsDeleted);
        modelBuilder.Entity<BorrowTicket>().HasQueryFilter(bt => !bt.IsDeleted);
        modelBuilder.Entity<ReturnTicket>().HasQueryFilter(rt => !rt.IsDeleted);
        modelBuilder.Entity<HandoverTicket>().HasQueryFilter(ht => !ht.IsDeleted);
        modelBuilder.Entity<DisposalTicket>().HasQueryFilter(dt => !dt.IsDeleted);
        modelBuilder.Entity<DisposalTicketAsset>().HasQueryFilter(dta => !dta.IsDeleted);
        modelBuilder.Entity<WarehouseAsset>().HasQueryFilter(wa => !wa.IsDeleted);

        //modelBuilder.Entity<Notification>(entity =>
        //{
        //    entity.HasKey(e => e.Id);

        //    entity.HasOne(d => d.User)
        //        .WithMany()
        //        .HasForeignKey(d => d.UserId)
        //        .OnDelete(DeleteBehavior.Cascade);

        //    entity.Property(e => e.Type).HasMaxLength(50);
        //    entity.Property(e => e.IsRead).HasDefaultValue(false);
        //});

        modelBuilder.Entity<ManagerReturnRequest>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(d => d.BorrowTicket)
       .WithOne()
       .HasForeignKey<ManagerReturnRequest>(d => d.BorrowTicketId)
       .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.RequestedBy)
                .WithMany()
                .HasForeignKey(d => d.RequestedById);

            entity.HasOne(d => d.ReturnTicket)
                .WithOne()
                .HasForeignKey<ManagerReturnRequest>(d => d.RelatedReturnTicketId)
                .IsRequired(false);

            // Apply global query filter for soft delete
            entity.HasQueryFilter(mr => !mr.IsDeleted);
        });

        // Configuration for HandoverReturn
        modelBuilder.Entity<HandoverReturn>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(d => d.HandoverTicket)
                .WithMany()
                .HasForeignKey(d => d.HandoverTicketId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.ReturnBy)
                .WithMany()
                .HasForeignKey(d => d.ReturnById)
                .IsRequired(false);

            entity.HasOne(d => d.ReceivedBy)
                .WithMany()
                .HasForeignKey(d => d.ReceivedById)
                .IsRequired(false);

            // Apply global query filter for soft delete
            entity.HasQueryFilter(hr => !hr.IsDeleted);
        });

        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AppRoles__3214EC07D250C7FA");
            entity.Property(e => e.RoleType).HasConversion<string>();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AppUsers__3214EC07D250C7FA");

            entity.HasOne(d => d.Department).WithMany(p => p.AppUsers).HasForeignKey(d => d.DepartmentId);

            entity.HasOne(d => d.Role).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Assets__3214EC07670D2D82");

            entity.HasOne(d => d.AssetCategory).WithMany(p => p.Assets).HasForeignKey(d => d.AssetCategoryId);

            // Thêm cấu hình cho các trường từ EntityBase
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<AssetCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AssetCat__3214EC07D7F87C44");

            // Thêm cấu hình cho các trường từ EntityBase
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<BorrowTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BorrowTi__3214EC079484E399");

            entity.HasOne(d => d.BorrowBy).WithMany(p => p.BorrowTicketBorrowBies).HasForeignKey(d => d.BorrowById);

            entity.HasOne(d => d.Owner).WithMany(p => p.BorrowTicketOwners).HasForeignKey(d => d.OwnerId);

            entity.HasOne(d => d.WarehouseAsset).WithMany(p => p.BorrowTickets).HasForeignKey(d => d.WarehouseAssetId);

            // Thêm cấu hình cho các trường từ EntityBase
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departme__3214EC07F78AE89E");

            // Thêm cấu hình cho các trường từ EntityBase
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<DisposalTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disposal__3214EC07A33255BB");

            entity.HasOne(d => d.DisposalBy).WithMany(p => p.DisposalTicketDisposalBies).HasForeignKey(d => d.DisposalById);

            entity.HasOne(d => d.Owner).WithMany(p => p.DisposalTicketOwners).HasForeignKey(d => d.OwnerId);

            // Thêm cấu hình cho các trường từ EntityBase
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<DisposalTicketAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disposal__3214EC077EA7A1E4");

            entity.HasOne(d => d.DisposalTicket).WithMany(p => p.DisposalTicketAssets).HasForeignKey(d => d.DisposalTicketId);

            entity.HasOne(d => d.WarehouseAsset).WithMany(p => p.DisposalTicketAssets).HasForeignKey(d => d.WarehouseAssetId);

            // Thêm cấu hình cho các trường từ EntityBase
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<HandoverTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Handover__3214EC07861FC02F");

            entity.HasOne(d => d.Department).WithMany(p => p.HandoverTickets).HasForeignKey(d => d.DepartmentId);

            entity.HasOne(d => d.HandoverBy).WithMany(p => p.HandoverTicketHandoverBies).HasForeignKey(d => d.HandoverById);

            entity.HasOne(d => d.HandoverTo).WithMany(p => p.HandoverTicketHandoverTos).HasForeignKey(d => d.HandoverToId);

            entity.HasOne(d => d.Owner).WithMany(p => p.HandoverTicketOwners).HasForeignKey(d => d.OwnerId);

            entity.HasOne(d => d.WarehouseAsset).WithMany(p => p.HandoverTickets).HasForeignKey(d => d.WarehouseAssetId);

            // Thêm cấu hình cho các trường từ EntityBase
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<ReturnTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReturnTi__3214EC0769D136BF");

            entity.HasOne(d => d.BorrowTicket).WithMany(p => p.ReturnTickets).HasForeignKey(d => d.BorrowTicketId);

            entity.HasOne(d => d.Owner).WithMany(p => p.ReturnTicketOwners).HasForeignKey(d => d.OwnerId);

            entity.HasOne(d => d.ReturnBy).WithMany(p => p.ReturnTicketReturnBies).HasForeignKey(d => d.ReturnById);

            // Thêm cấu hình cho các trường từ EntityBase
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Warehous__3214EC073C957F29");

            // Thêm cấu hình cho các trường từ EntityBase
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<WarehouseAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Warehous__3214EC07DC3A910A");

            entity.HasIndex(e => new { e.WarehouseId, e.AssetId }, "UQ__Warehous__123C3DCDB02651C6").IsUnique();

            entity.HasOne(d => d.Asset).WithMany(p => p.WarehouseAssets).HasForeignKey(d => d.AssetId);

            entity.HasOne(d => d.Warehouse).WithMany(p => p.WarehouseAssets).HasForeignKey(d => d.WarehouseId);

            // Thêm cấu hình cho các trường từ EntityBase
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}