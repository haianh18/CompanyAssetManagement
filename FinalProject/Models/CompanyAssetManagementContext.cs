using FinalProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AppRoles__3214EC07D250C7FA");
            entity.Property(e => e.RoleType).HasConversion<string>(); // Add this line to handle enum conversion
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
        });

        modelBuilder.Entity<AssetCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AssetCat__3214EC07D7F87C44");
        });

        modelBuilder.Entity<BorrowTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BorrowTi__3214EC079484E399");

            entity.HasOne(d => d.BorrowBy).WithMany(p => p.BorrowTicketBorrowBies).HasForeignKey(d => d.BorrowById);

            entity.HasOne(d => d.Owner).WithMany(p => p.BorrowTicketOwners).HasForeignKey(d => d.OwnerId);

            entity.HasOne(d => d.WarehouseAsset).WithMany(p => p.BorrowTickets).HasForeignKey(d => d.WarehouseAssetId);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departme__3214EC07F78AE89E");
        });

        modelBuilder.Entity<DisposalTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disposal__3214EC07A33255BB");

            entity.HasOne(d => d.DisposalBy).WithMany(p => p.DisposalTicketDisposalBies).HasForeignKey(d => d.DisposalById);

            entity.HasOne(d => d.Owner).WithMany(p => p.DisposalTicketOwners).HasForeignKey(d => d.OwnerId);
        });

        modelBuilder.Entity<DisposalTicketAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disposal__3214EC077EA7A1E4");

            entity.HasOne(d => d.DisposalTicket).WithMany(p => p.DisposalTicketAssets).HasForeignKey(d => d.DisposalTicketId);

            entity.HasOne(d => d.WarehouseAsset).WithMany(p => p.DisposalTicketAssets).HasForeignKey(d => d.WarehouseAssetId);
        });

        modelBuilder.Entity<HandoverTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Handover__3214EC07861FC02F");

            entity.HasOne(d => d.Department).WithMany(p => p.HandoverTickets).HasForeignKey(d => d.DepartmentId);

            entity.HasOne(d => d.HandoverBy).WithMany(p => p.HandoverTicketHandoverBies).HasForeignKey(d => d.HandoverById);

            entity.HasOne(d => d.HandoverTo).WithMany(p => p.HandoverTicketHandoverTos).HasForeignKey(d => d.HandoverToId);

            entity.HasOne(d => d.Owner).WithMany(p => p.HandoverTicketOwners).HasForeignKey(d => d.OwnerId);

            entity.HasOne(d => d.WarehouseAsset).WithMany(p => p.HandoverTickets).HasForeignKey(d => d.WarehouseAssetId);
        });

        modelBuilder.Entity<ReturnTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReturnTi__3214EC0769D136BF");

            entity.HasOne(d => d.BorrowTicket).WithMany(p => p.ReturnTickets).HasForeignKey(d => d.BorrowTicketId);

            entity.HasOne(d => d.Owner).WithMany(p => p.ReturnTicketOwners).HasForeignKey(d => d.OwnerId);

            entity.HasOne(d => d.ReturnBy).WithMany(p => p.ReturnTicketReturnBies).HasForeignKey(d => d.ReturnById);
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Warehous__3214EC073C957F29");
        });

        modelBuilder.Entity<WarehouseAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Warehous__3214EC07DC3A910A");

            entity.HasIndex(e => new { e.WarehouseId, e.AssetId }, "UQ__Warehous__123C3DCDB02651C6").IsUnique();

            entity.HasOne(d => d.Asset).WithMany(p => p.WarehouseAssets).HasForeignKey(d => d.AssetId);

            entity.HasOne(d => d.Warehouse).WithMany(p => p.WarehouseAssets).HasForeignKey(d => d.WarehouseId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
