using FinalProject.Models.Base;
using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class HandoverTicket : EntityBase
    {
        // Các thuộc tính đã được kế thừa từ EntityBase:
        // public int Id { get; set; }
        // public DateTime? DateCreated { get; set; }
        // public DateTime? DateModified { get; set; }
        // public int? ActiveStatus { get; set; }
        // public bool IsDeleted { get; set; }
        // public DateTime? DeletedDate { get; set; }

        public int? WarehouseAssetId { get; set; }
        public int? DepartmentId { get; set; }
        public int? HandoverById { get; set; }
        public int? HandoverToId { get; set; }
        public string? Note { get; set; }
        public int? OwnerId { get; set; }
        public int? Quantity { get; set; }

        public virtual Department? Department { get; set; }
        public virtual AppUser? HandoverBy { get; set; }
        public virtual AppUser? HandoverTo { get; set; }
        public virtual AppUser? Owner { get; set; }
        public virtual WarehouseAsset? WarehouseAsset { get; set; }
    }
}