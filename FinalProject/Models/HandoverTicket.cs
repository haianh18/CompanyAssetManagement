using System;
using System.Collections.Generic;

namespace FinalProject.Models;

public partial class HandoverTicket
{
    public int Id { get; set; }

    public int? WarehouseAssetId { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

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
