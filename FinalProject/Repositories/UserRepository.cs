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
                .Where(u => !u.IsDeleted)
                .Include(u => u.Department)
                .Include(u => u.Role)
                .ToListAsync();
        }

        public async Task<IEnumerable<AppUser>> GetInactiveUsersAsync()
        {
            return await _userManager.Users
                .Where(u => u.IsDeleted)
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

        // Ví dụ với BorrowTicket
        public async Task HandleDeletedUserInTicketsAsync(int userId)
        {
            // Tìm hoặc tạo user mặc định
            var defaultUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == "system");

            if (defaultUser == null)
            {
                throw new Exception("No system user found");
            }



            // Cập nhật các tickets liên quan
            var borrowTickets = await _context.BorrowTickets
                .Where(bt => bt.BorrowById == userId || bt.OwnerId == userId)
                .ToListAsync();

            var handoverTickets = await _context.HandoverTickets
                .Where(ht => ht.HandoverById == userId || ht.OwnerId == userId)
                .ToListAsync();

            var disposalTickets = await _context.DisposalTickets
                .Where(dt => dt.DisposalById == userId || dt.OwnerId == userId)
                .ToListAsync();

            var returnTickets = await _context.ReturnTickets
                .Where(rt => rt.ReturnById == userId || rt.OwnerId == userId)
                .ToListAsync();

            foreach (var ticket in borrowTickets)
            {
                if (ticket.BorrowById == userId)
                    ticket.BorrowById = defaultUser.Id;

                if (ticket.OwnerId == userId)
                    ticket.OwnerId = defaultUser.Id;

                ticket.DateModified = DateTime.Now;
            }

            foreach (var ticket in handoverTickets)
            {
                if (ticket.HandoverById == userId)
                    ticket.HandoverById = defaultUser.Id;
                if (ticket.OwnerId == userId)
                    ticket.OwnerId = defaultUser.Id;
                ticket.DateModified = DateTime.Now;
            }

            foreach (var ticket in disposalTickets)
            {
                if (ticket.DisposalById == userId)
                    ticket.DisposalById = defaultUser.Id;
                if (ticket.OwnerId == userId)
                    ticket.OwnerId = defaultUser.Id;
                ticket.DateModified = DateTime.Now;
            }

            foreach (var ticket in returnTickets)
            {
                if (ticket.ReturnById == userId)
                    ticket.ReturnById = defaultUser.Id;
                if (ticket.OwnerId == userId)
                    ticket.OwnerId = defaultUser.Id;
                ticket.DateModified = DateTime.Now;
            }
            await _context.SaveChangesAsync();
        }
    }
}