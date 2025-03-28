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
            return await GetAllAsync();
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

        public async Task SoftDeleteDepartmentAsync(int departmentId)
        {
            // Tìm hoặc tạo department mặc định
            var defaultDepartment = await _dbSet
                .FirstOrDefaultAsync(d => d.Name == "Unassigned");

            if (defaultDepartment == null)
            {
                defaultDepartment = new Department
                {
                    Name = "Unassigned",
                    DateCreated = DateTime.Now
                };
                _context.Departments.Add(defaultDepartment);
            }

            // Soft delete department gốc
            var originalDepartment = await _dbSet.FindAsync(departmentId);
            if (originalDepartment == null)
                throw new Exception("Department not found");

            originalDepartment.IsDeleted = true;
            originalDepartment.DeletedDate = DateTime.Now;

            // Chuyển users sang department mặc định
            var usersInDepartment = await _context.Users
                .Where(u => u.DepartmentId == departmentId)
                .ToListAsync();

            foreach (var user in usersInDepartment)
            {
                user.DepartmentId = defaultDepartment.Id;
                user.DateModified = DateTime.Now;
            }

        }
    }
}