using System;
using System.Collections.Generic;

namespace FinalProject.Models;

public partial class WarehouseAsset
{
    public int Id { get; set; }

    public int? WarehouseId { get; set; }

    public int? AssetId { get; set; }

    public int? Quantity { get; set; }

    public virtual Asset? Asset { get; set; }

    public virtual ICollection<BorrowTicket> BorrowTickets { get; set; } = new List<BorrowTicket>();

    public virtual ICollection<DisposalTicketAsset> DisposalTicketAssets { get; set; } = new List<DisposalTicketAsset>();

    public virtual ICollection<HandoverTicket> HandoverTickets { get; set; } = new List<HandoverTicket>();

    public virtual Warehouse? Warehouse { get; set; }
}
