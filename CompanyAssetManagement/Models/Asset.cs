using CompanyAssetManagement.Data.Interfaces;
using CompanyAssetManagement.Infrastructure.SharedKernel;

namespace CompanyAssetManagement.Models
{
    public class Asset : DomainEntity<int>, ISwitchable, IDateTracking
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AssetCategoryId { get; set; }
        public virtual AssetCategory AssetCategory { get; set; }
        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public string Unit { get; set; }

        public string? Description { get; set; }
        public string? Note { get; set; }
        public Data.Enums.ActiveStatus ActiveStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

    }

}
