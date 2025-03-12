using FinalProject.Enums;
using System;
using System.Collections.Generic;

namespace FinalProject.Models;

public partial class Asset
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public double Price { get; set; }

    public string Unit { get; set; } = null!;

    public int? ActiveStatus { get; set; }

    public int? AssetCategoryId { get; set; }

    public AssetStatus AssetStatus { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public string? Description { get; set; }

    public string? Note { get; set; }

    public virtual AssetCategory? AssetCategory { get; set; }

    public virtual ICollection<WarehouseAsset> WarehouseAssets { get; set; } = new List<WarehouseAsset>();
}
