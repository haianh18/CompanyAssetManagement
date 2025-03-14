using FinalProject.Models;
using FinalProject.Repositories.Common;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<IEnumerable<Department>> GetActiveDepartments();
    Task<Department> GetDepartmentWithUsers(int departmentId);
    Task<IEnumerable<Department>> GetDepartmentsWithUsers();
    Task<Dictionary<string, int>> GetDepartmentStatistics();

    Task SoftDeleteDepartmentAsync(int departmentId);
}