using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class UserService : BaseService<AppUser>, IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository) : base(unitOfWork)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string userName)
        {
            return await _userRepository.GetUserByUserNameAsync(userName);
        }

        public async Task<AppUser> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<IEnumerable<AppUser>> GetUsersByDepartmentAsync(int departmentId)
        {
            return await _userRepository.GetUsersByDepartmentAsync(departmentId);
        }

        public async Task<IEnumerable<AppUser>> GetUsersByRoleAsync(string roleName)
        {
            return await _userRepository.GetUsersByRoleAsync(roleName);
        }

        public async Task<IEnumerable<AppUser>> GetActiveUsersAsync()
        {
            return await _userRepository.GetActiveUsersAsync();
        }

        public async Task<IEnumerable<AppUser>> GetInactiveUsersAsync()
        {
            return await _userRepository.GetInactiveUsersAsync();
        }

        public async Task<bool> CreateUserAsync(AppUser user, string password)
        {
            return await _userRepository.CreateUserAsync(user, password);
        }

        public async Task<bool> UpdateUserAsync(AppUser user)
        {
            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<bool> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
        {
            return await _userRepository.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteUserAsync(id);
        }

        public async Task<bool> IsUserInRoleAsync(AppUser user, string roleName)
        {
            return await _userRepository.IsUserInRoleAsync(user, roleName);
        }

        public async Task<IList<string>> GetUserRolesAsync(AppUser user)
        {
            return await _userRepository.GetUserRolesAsync(user);
        }

        public async Task<bool> AddUserToRoleAsync(AppUser user, string roleName)
        {
            return await _userRepository.AddUserToRoleAsync(user, roleName);
        }

        public async Task<bool> RemoveUserFromRoleAsync(AppUser user, string roleName)
        {
            return await _userRepository.RemoveUserFromRoleAsync(user, roleName);
        }

        public async Task<int> CountUsersAsync()
        {
            return await _userRepository.CountUsersAsync();
        }

        public async Task<int> CountUsersByRoleAsync(string roleName)
        {
            return await _userRepository.CountUsersByRoleAsync(roleName);
        }

        public async Task<int> CountUsersByDepartmentAsync(int departmentId)
        {
            return await _userRepository.CountUsersByDepartmentAsync(departmentId);
        }

        public async Task HandleDeletedUserInTicketsAsync(int userId)
        {
            await _userRepository.HandleDeletedUserInTicketsAsync(userId);
        }
    }
}












