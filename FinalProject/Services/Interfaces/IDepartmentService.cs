using FinalProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services.Interfaces
{
    public interface IDepartmentService : IBaseService<Department>
    {
        Task<IEnumerable<Department>> GetActiveDepartmentsAsync();
        Task<Department> GetDepartmentWithUsersAsync(int departmentId);
        Task<IEnumerable<Department>> GetDepartmentsWithUsersAsync();
        Task<Dictionary<string, int>> GetDepartmentStatisticsAsync();
        Task SoftDeleteDepartmentAsync(int departmentId);
    }
}






