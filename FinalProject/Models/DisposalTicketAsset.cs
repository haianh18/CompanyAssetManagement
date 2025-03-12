using System;
using System.Collections.Generic;

namespace FinalProject.Models;

public partial class DisposalTicketAsset
{
    public int? DisposalTicketId { get; set; }

    public int? WarehouseAssetId { get; set; }

    public double? DisposedPrice { get; set; }

    public int Id { get; set; }

    public int? Quantity { get; set; }

    public virtual DisposalTicket? DisposalTicket { get; set; }

    public virtual WarehouseAsset? WarehouseAsset { get; set; }
}
