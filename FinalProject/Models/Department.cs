using System;
using System.Collections.Generic;

namespace FinalProject.Models;

public partial class Department
{
    public int Id { get; set; }

    public int? ActiveStatus { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public string? Email { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();

    public virtual ICollection<HandoverTicket> HandoverTickets { get; set; } = new List<HandoverTicket>();
}
