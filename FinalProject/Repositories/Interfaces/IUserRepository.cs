public interface IUserRepository
{
    Task<IEnumerable<AppUser>> GetAllUsersAsync();
    Task<AppUser> GetUserByIdAsync(int id);
    Task<AppUser> GetUserByUserNameAsync(string userName);
    Task<AppUser> GetUserByEmailAsync(string email);
    Task<IEnumerable<AppUser>> GetUsersByDepartmentAsync(int departmentId);
    Task<IEnumerable<AppUser>> GetUsersByRoleAsync(string roleName);
    Task<IEnumerable<AppUser>> GetActiveUsersAsync();
    Task<IEnumerable<AppUser>> GetInactiveUsersAsync();
    Task<bool> CreateUserAsync(AppUser user, string password);
    Task<bool> UpdateUserAsync(AppUser user);
    Task<bool> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> IsUserInRoleAsync(AppUser user, string roleName);
    Task<IList<string>> GetUserRolesAsync(AppUser user);
    Task<bool> AddUserToRoleAsync(AppUser user, string roleName);
    Task<bool> RemoveUserFromRoleAsync(AppUser user, string roleName);
    Task<int> CountUsersAsync();
    Task<int> CountUsersByRoleAsync(string roleName);
    Task<int> CountUsersByDepartmentAsync(int departmentId);
}