using FinalProject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FinalProject.Repositories.Common
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly CompanyAssetManagementContext _context;
        internal DbSet<T> _dbSet;

        public Repository(CompanyAssetManagementContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "", int? skip = null, int? take = null)
        {
            IQueryable<T> query = _dbSet;

            // Áp dụng filter
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Áp dụng include
            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            // Áp dụng order by
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Áp dụng paging
            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter = null, string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;

            // Áp dụng filter
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Áp dụng include
            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public async Task RemoveAsync(int id)
        {
            T entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                Remove(entity);
            }
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.CountAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                return await query.AnyAsync(filter);
            }

            return await query.AnyAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null && entity is FinalProject.Models.Base.EntityBase entityBase)
            {
                entityBase.IsDeleted = true;
                entityBase.DeletedDate = DateTime.Now;
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public async Task SoftDeleteAsync(T entity)
        {
            if (entity is FinalProject.Models.Base.EntityBase entityBase)
            {
                entityBase.IsDeleted = true;
                entityBase.DeletedDate = DateTime.Now;
                _context.Entry(entity).State = EntityState.Modified;
                await Task.CompletedTask;
            }
        }

        public async Task<IEnumerable<T>> GetAllIncludingDeletedAsync()
        {
            // Use NoTracking to bypass the global query filter
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }

        public async Task<T> GetByIdIncludingDeletedAsync(int id)
        {
            // Use NoTracking to bypass the global query filter
            return await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        public async Task<IEnumerable<T>> GetAllDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().Where(e => (e as FinalProject.Models.Base.EntityBase).IsDeleted).ToListAsync();
        }

        public async Task RestoreDeletedAsync(int id)
        {
            var entity = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
            if (entity != null && entity is FinalProject.Models.Base.EntityBase entityBase)
            {
                entityBase.IsDeleted = false;
                entityBase.DeletedDate = null;
                _context.Entry(entity).State = EntityState.Modified;
            }
        }
    }
}