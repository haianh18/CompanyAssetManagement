using FinalProject.Models.Base;
using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class Warehouse : EntityBase
    {
        // Các thuộc tính đã được kế thừa từ EntityBase:
        // public int Id { get; set; }
        // public DateTime? DateCreated { get; set; }
        // public DateTime? DateModified { get; set; }
        // public int? ActiveStatus { get; set; }
        // public bool IsDeleted { get; set; }
        // public DateTime? DeletedDate { get; set; }

        public string? Address { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<WarehouseAsset> WarehouseAssets { get; set; } = new List<WarehouseAsset>();
    }
}