using FinalProject.Enums;
using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Repositories
{


    public class UserRepository : IUserRepository
    {
        private readonly CompanyAssetManagementContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserRepository(CompanyAssetManagementContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
        {
            return await _userManager.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _userManager.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        public async Task<AppUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IEnumerable<AppUser>> GetUsersByDepartmentAsync(int departmentId)
        {
            return await _userManager.Users
                .Where(u => u.DepartmentId == departmentId)
                .Include(u => u.Department)
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersByRoleAsync(string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            return users.ToList();
        }

        public async Task<IEnumerable<AppUser>> GetActiveUsersAsync()
        {
            return await _userManager.Users
                .Where(u => u.ActiveStatus == ActiveStatus.ACTIVE)
                .Include(u => u.Department)
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<IEnumerable<AppUser>> GetInactiveUsersAsync()
        {
            return await _userManager.Users
                .Where(u => u.ActiveStatus == ActiveStatus.INACTIVE)
                .Include(u => u.Department)
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<bool> CreateUserAsync(AppUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded;
        }

        public async Task<bool> UpdateUserAsync(AppUser user)
        {
            user.DateModified = DateTime.Now;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
                return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> IsUserInRoleAsync(AppUser user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<IList<string>> GetUserRolesAsync(AppUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> AddUserToRoleAsync(AppUser user, string roleName)
        {
            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<bool> RemoveUserFromRoleAsync(AppUser user, string roleName)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<int> CountUsersAsync()
        {
            return await _userManager.Users.CountAsync();
        }

        public async Task<int> CountUsersByRoleAsync(string roleName)
        {
            var users = await GetUsersByRoleAsync(roleName);
            return users.Count();
        }

        public async Task<int> CountUsersByDepartmentAsync(int departmentId)
        {
            return await _userManager.Users.CountAsync(u => u.DepartmentId == departmentId);
        }
    }
}