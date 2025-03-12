using System;
using System.Collections.Generic;

namespace FinalProject.Models;

public partial class AssetCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? ActiveStatus { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
