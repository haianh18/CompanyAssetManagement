using System;
using System.Collections.Generic;

namespace FinalProject.Models;

public partial class Warehouse
{
    public int Id { get; set; }

    public int? ActiveStatus { get; set; }

    public string? Address { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<WarehouseAsset> WarehouseAssets { get; set; } = new List<WarehouseAsset>();
}
