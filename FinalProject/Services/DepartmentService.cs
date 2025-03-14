using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class DepartmentService : BaseService<Department>, IDepartmentService
    {
        public DepartmentService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
        {
            return await _unitOfWork.Departments.GetActiveDepartments();
        }

        public async Task<Department> GetDepartmentWithUsersAsync(int departmentId)
        {
            return await _unitOfWork.Departments.GetDepartmentWithUsers(departmentId);
        }

        public async Task<IEnumerable<Department>> GetDepartmentsWithUsersAsync()
        {
            return await _unitOfWork.Departments.GetDepartmentsWithUsers();
        }

        public async Task<Dictionary<string, int>> GetDepartmentStatisticsAsync()
        {
            return await _unitOfWork.Departments.GetDepartmentStatistics();
        }

        public async Task SoftDeleteDepartmentAsync(int departmentId)
        {
            await _unitOfWork.Departments.SoftDeleteDepartmentAsync(departmentId);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}






