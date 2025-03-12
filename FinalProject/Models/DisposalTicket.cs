using System;
using System.Collections.Generic;

namespace FinalProject.Models;

public partial class DisposalTicket
{
    public int Id { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public int? DisposalById { get; set; }

    public string? Note { get; set; }

    public int? OwnerId { get; set; }

    public string? Reason { get; set; }

    public virtual AppUser? DisposalBy { get; set; }

    public virtual ICollection<DisposalTicketAsset> DisposalTicketAssets { get; set; } = new List<DisposalTicketAsset>();

    public virtual AppUser? Owner { get; set; }
}
