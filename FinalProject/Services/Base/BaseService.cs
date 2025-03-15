using FinalProject.Models.Base;
using FinalProject.Repositories.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BaseService<T> : IBaseService<T> where T : class
{
    protected readonly IUnitOfWork _unitOfWork;

    public BaseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _unitOfWork.GetRepositoryForType<T>().GetAllAsync();
    }

    public async Task<IEnumerable<T>> GetAllInCludeDeletedAsync()
    {
        return await _unitOfWork.GetRepositoryForType<T>().GetAllIncludingDeletedAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _unitOfWork.GetRepositoryForType<T>().GetByIdAsync(id);
    }

    public async Task<T> GetByIdIncludeDeletedAsync(int id)
    {
        return await _unitOfWork.GetRepositoryForType<T>().GetByIdIncludingDeletedAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await _unitOfWork.GetRepositoryForType<T>().AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _unitOfWork.GetRepositoryForType<T>().Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        await _unitOfWork.GetRepositoryForType<T>().SoftDeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RestoreAsync(int id)
    {
        var repository = _unitOfWork.GetRepositoryForType<T>();
        var entity = await _unitOfWork.GetRepositoryForType<T>().GetByIdIncludingDeletedAsync(id);
        if (entity is EntityBase entityBase)
        {
            entityBase.IsDeleted = false;
            entityBase.DeletedDate = null;
            repository.Update(entity);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}



