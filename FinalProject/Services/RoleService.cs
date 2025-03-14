using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class RoleService : BaseService<AppRole>, IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IUnitOfWork unitOfWork, IRoleRepository roleRepository) : base(unitOfWork)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<AppRole>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllRolesAsync();
        }

        public async Task<AppRole> GetRoleByIdAsync(int id)
        {
            return await _roleRepository.GetRoleByIdAsync(id);
        }

        public async Task<AppRole> GetRoleByNameAsync(string roleName)
        {
            return await _roleRepository.GetRoleByNameAsync(roleName);
        }

        public async Task<bool> CreateRoleAsync(AppRole role)
        {
            return await _roleRepository.CreateRoleAsync(role);
        }

        public async Task<bool> UpdateRoleAsync(AppRole role)
        {
            return await _roleRepository.UpdateRoleAsync(role);
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            return await _roleRepository.DeleteRoleAsync(id);
        }

        public async Task<int> CountRolesAsync()
        {
            return await _roleRepository.CountRolesAsync();
        }

        public async Task<int> CountUsersByRoleAsync(int roleId)
        {
            return await _roleRepository.CountUsersByRoleAsync(roleId);
        }

        public async Task<IEnumerable<AppRole>> GetRolesByTypeAsync(RoleType roleType)
        {
            return await _roleRepository.GetRolesByTypeAsync(roleType);
        }
    }
}











