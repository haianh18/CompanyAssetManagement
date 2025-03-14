using FinalProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services.Interfaces
{
    public interface IRoleService : IBaseService<AppRole>
    {
        Task<IEnumerable<AppRole>> GetAllRolesAsync();
        Task<AppRole> GetRoleByIdAsync(int id);
        Task<AppRole> GetRoleByNameAsync(string roleName);
        Task<bool> CreateRoleAsync(AppRole role);
        Task<bool> UpdateRoleAsync(AppRole role);
        Task<bool> DeleteRoleAsync(int id);
        Task<int> CountRolesAsync();
        Task<int> CountUsersByRoleAsync(int roleId);
        Task<IEnumerable<AppRole>> GetRolesByTypeAsync(RoleType roleType);
    }
}











