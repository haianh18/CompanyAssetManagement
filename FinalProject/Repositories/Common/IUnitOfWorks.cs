using FinalProject.Repositories.Interfaces;
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
        IHandoverReturnRepository HandoverReturns { get; }

        IDisposalTicketRepository DisposalTickets { get; }
        IDisposalTicketAssetRepository DisposalTicketAssets { get; }
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }

        // Method to get repository by type
        IRepository<T> GetRepositoryForType<T>() where T : class;
        // Lưu thay đổi
        Task<int> SaveChangesAsync();
    }
}