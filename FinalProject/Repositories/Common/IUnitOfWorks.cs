using System;
using System.Threading.Tasks;

namespace FinalProject.Repositories.Common
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        IAssetRepository Assets { get; }
        IAssetCategoryRepository AssetCategories { get; }
        IWarehouseRepository Warehouses { get; }
        IWarehouseAssetRepository WarehouseAssets { get; }
        IDepartmentRepository Departments { get; }
        IBorrowTicketRepository BorrowTickets { get; }
        IReturnTicketRepository ReturnTickets { get; }
        IHandoverTicketRepository HandoverTickets { get; }
        IDisposalTicketRepository DisposalTickets { get; }
        IDisposalTicketAssetRepository DisposalTicketAssets { get; }
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }

        // Lưu thay đổi
        Task<int> SaveChangesAsync();
    }
}