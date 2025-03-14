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

        // Dictionary để map Type với Repository
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