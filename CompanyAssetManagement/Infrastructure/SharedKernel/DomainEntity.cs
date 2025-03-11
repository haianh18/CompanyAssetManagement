namespace CompanyAssetManagement.Infrastructure.SharedKernel
{
    public abstract class DomainEntity<T>
    {
        public T Id { get; set; }

        //True if the entity is transient, i.e. just created and not yet persisted
        public bool IsTransient()
        {
            return Id.Equals(default(T));
        }
    }
}
