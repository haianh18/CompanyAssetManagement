using CompanyAssetManagement.Data.Enums;
using CompanyAssetManagement.Data.Interfaces;
using CompanyAssetManagement.Infrastructure.SharedKernel;

namespace CompanyAssetManagement.Models
{
    public class AssetCategory : DomainEntity<int>
    {
        public AssetCategory()
        {
            Assets = new List<Asset>();
        }
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Asset> Assets { get; set; }
    }
}
