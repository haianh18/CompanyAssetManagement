using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Repositories
{


    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Department>> GetActiveDepartments()
        {
            return await _dbSet.Where(d => d.ActiveStatus == (int)ActiveStatus.ACTIVE)
                .ToListAsync();
        }

        public async Task<Department> GetDepartmentWithUsers(int departmentId)
        {
            return await _dbSet
                .Include(d => d.AppUsers)
                .FirstOrDefaultAsync(d => d.Id == departmentId);
        }

        public async Task<IEnumerable<Department>> GetDepartmentsWithUsers()
        {
            return await _dbSet
                .Include(d => d.AppUsers)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetDepartmentStatistics()
        {
            var departments = await _dbSet
                .Include(d => d.AppUsers)
                .ToListAsync();

            return departments.ToDictionary(
                d => d.Name,
                d => d.AppUsers.Count
            );
        }
    }
}