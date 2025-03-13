using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Repositories
{


    public class RoleRepository : IRoleRepository
    {
        private readonly CompanyAssetManagementContext _context;
        private readonly RoleManager<AppRole> _roleManager;

        public RoleRepository(CompanyAssetManagementContext context, RoleManager<AppRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<AppRole>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.ToListAsync();
        }

        public async Task<AppRole> GetRoleByIdAsync(int id)
        {
            return await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<AppRole> GetRoleByNameAsync(string roleName)
        {
            return await _roleManager.FindByNameAsync(roleName);
        }

        public async Task<bool> CreateRoleAsync(AppRole role)
        {
            var result = await _roleManager.CreateAsync(role);
            return result.Succeeded;
        }

        public async Task<bool> UpdateRoleAsync(AppRole role)
        {
            var result = await _roleManager.UpdateAsync(role);
            return result.Succeeded;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await GetRoleByIdAsync(id);
            if (role == null)
                return false;

            var result = await _roleManager.DeleteAsync(role);
            return result.Succeeded;
        }

        public async Task<int> CountRolesAsync()
        {
            return await _roleManager.Roles.CountAsync();
        }

        public async Task<int> CountUsersByRoleAsync(int roleId)
        {
            return await _context.Users.CountAsync(u => u.RoleId == roleId);
        }

        public async Task<IEnumerable<AppRole>> GetRolesByTypeAsync(RoleType roleType)
        {
            return await _roleManager.Roles
                .Where(r => r.RoleType == roleType)
                .ToListAsync();
        }
    }
}