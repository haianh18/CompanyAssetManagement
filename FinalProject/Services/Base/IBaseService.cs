using System.Collections.Generic;
using System.Threading.Tasks;

public interface IBaseService<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();

    Task<IEnumerable<T>> GetAllInCludeDeletedAsync();
    Task<T> GetByIdAsync(int id);

    Task<T> GetByIdIncludeDeletedAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task SoftDeleteAsync(int id);
    Task RestoreAsync(int id);
}




