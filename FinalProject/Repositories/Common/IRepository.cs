using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FinalProject.Repositories.Common
{
    public interface IRepository<T> where T : class
    {
        // Lấy tất cả các đối tượng
        Task<IEnumerable<T>> GetAllAsync();

        // Lấy các đối tượng theo điều kiện
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "", int? skip = null, int? take = null);

        // Lấy một đối tượng theo Id
        Task<T> GetByIdAsync(int id);

        // Lấy một đối tượng theo điều kiện
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter = null,
            string includeProperties = "");

        // Thêm một đối tượng mới
        Task AddAsync(T entity);

        // Thêm nhiều đối tượng mới
        Task AddRangeAsync(IEnumerable<T> entities);

        // Cập nhật một đối tượng
        void Update(T entity);

        // Xóa một đối tượng
        void Remove(T entity);

        // Xóa một đối tượng theo Id
        Task RemoveAsync(int id);

        // Xóa nhiều đối tượng
        void RemoveRange(IEnumerable<T> entities);

        // Đếm số lượng đối tượng theo điều kiện
        Task<int> CountAsync(Expression<Func<T, bool>> filter = null);

        // Kiểm tra có tồn tại đối tượng theo điều kiện
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter = null);
    }
}