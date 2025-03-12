using System;
using System.Collections.Generic;

namespace FinalProject.Models;

public partial class ReturnTicket
{
    public int Id { get; set; }

    public int? BorrowTicketId { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public string? Note { get; set; }

    public int? OwnerId { get; set; }

    public int? Quantity { get; set; }

    public int? ReturnById { get; set; }

    public virtual BorrowTicket? BorrowTicket { get; set; }

    public virtual AppUser? Owner { get; set; }

    public virtual AppUser? ReturnBy { get; set; }
}
