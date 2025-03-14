using FinalProject.Enums;
using FinalProject.Models.Base;
using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class Asset : EntityBase
    {
        // Các thuộc tính đã được kế thừa từ EntityBase:
        // public int Id { get; set; }
        // public DateTime? DateCreated { get; set; }
        // public DateTime? DateModified { get; set; }
        // public int? ActiveStatus { get; set; }
        // public bool IsDeleted { get; set; }
        // public DateTime? DeletedDate { get; set; }

        public string Name { get; set; } = null!;
        public double Price { get; set; }
        public string Unit { get; set; } = null!;
        public AssetStatus AssetStatus { get; set; }
        public int? AssetCategoryId { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }

        public virtual AssetCategory? AssetCategory { get; set; }
        public virtual ICollection<WarehouseAsset> WarehouseAssets { get; set; } = new List<WarehouseAsset>();
    }
}