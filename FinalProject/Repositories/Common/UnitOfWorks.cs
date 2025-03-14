using FinalProject.Models;
using FinalProject.Models.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Repositories.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CompanyAssetManagementContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        // Repositories
        private IAssetRepository _assetRepository;
        private IAssetCategoryRepository _assetCategoryRepository;
        private IWarehouseRepository _warehouseRepository;
        private IWarehouseAssetRepository _warehouseAssetRepository;
        private IDepartmentRepository _departmentRepository;
        private IBorrowTicketRepository _borrowTicketRepository;
        private IReturnTicketRepository _returnTicketRepository;
        private IHandoverTicketRepository _handoverTicketRepository;
        private IDisposalTicketRepository _disposalTicketRepository;
        private IDisposalTicketAssetRepository _disposalTicketAssetRepository;
        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;

        // Dictionary to map Type to Repository
        private readonly Dictionary<Type, object> _repositories;

        public UnitOfWork(
            CompanyAssetManagementContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _repositories = new Dictionary<Type, object>();
        }

        public IAssetRepository Assets => _assetRepository ??= new AssetRepository(_context);
        public IAssetCategoryRepository AssetCategories => _assetCategoryRepository ??= new AssetCategoryRepository(_context);
        public IWarehouseRepository Warehouses => _warehouseRepository ??= new WarehouseRepository(_context);
        public IWarehouseAssetRepository WarehouseAssets => _warehouseAssetRepository ??= new WarehouseAssetRepository(_context);
        public IDepartmentRepository Departments => _departmentRepository ??= new DepartmentRepository(_context);
        public IBorrowTicketRepository BorrowTickets => _borrowTicketRepository ??= new BorrowTicketRepository(_context);
        public IReturnTicketRepository ReturnTickets => _returnTicketRepository ??= new ReturnTicketRepository(_context);
        public IHandoverTicketRepository HandoverTickets => _handoverTicketRepository ??= new HandoverTicketRepository(_context);
        public IDisposalTicketRepository DisposalTickets => _disposalTicketRepository ??= new DisposalTicketRepository(_context);
        public IDisposalTicketAssetRepository DisposalTicketAssets => _disposalTicketAssetRepository ??= new DisposalTicketAssetRepository(_context);
        public IUserRepository Users => _userRepository ??= new UserRepository(_context, _userManager);
        public IRoleRepository Roles => _roleRepository ??= new RoleRepository(_context, _roleManager);

        // Method to get repository by type
        public IRepository<T> GetRepositoryForType<T>() where T : class
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                if (type == typeof(Asset))
                    _repositories[type] = Assets;
                else if (type == typeof(AssetCategory))
                    _repositories[type] = AssetCategories;
                else if (type == typeof(Warehouse))
                    _repositories[type] = Warehouses;
                else if (type == typeof(WarehouseAsset))
                    _repositories[type] = WarehouseAssets;
                else if (type == typeof(Department))
                    _repositories[type] = Departments;
                else if (type == typeof(BorrowTicket))
                    _repositories[type] = BorrowTickets;
                else if (type == typeof(ReturnTicket))
                    _repositories[type] = ReturnTickets;
                else if (type == typeof(HandoverTicket))
                    _repositories[type] = HandoverTickets;
                else if (type == typeof(DisposalTicket))
                    _repositories[type] = DisposalTickets;
                else if (type == typeof(DisposalTicketAsset))
                    _repositories[type] = DisposalTicketAssets;
                else
                    _repositories[type] = new Repository<T>(_context);
            }

            return (IRepository<T>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}


