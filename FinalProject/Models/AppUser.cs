using FinalProject.Enums;
using FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

public partial class AppUser : IdentityUser<int>
{
    public int RoleId { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime? BirthDay { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public int? DepartmentId { get; set; }
    public string? Specification { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedDate { get; set; }
    public virtual ICollection<BorrowTicket> BorrowTicketBorrowBies { get; set; } = new List<BorrowTicket>();
    public virtual ICollection<BorrowTicket> BorrowTicketOwners { get; set; } = new List<BorrowTicket>();
    public virtual Department? Department { get; set; }
    public virtual ICollection<DisposalTicket> DisposalTicketDisposalBies { get; set; } = new List<DisposalTicket>();
    public virtual ICollection<DisposalTicket> DisposalTicketOwners { get; set; } = new List<DisposalTicket>();
    public virtual ICollection<HandoverTicket> HandoverTicketHandoverBies { get; set; } = new List<HandoverTicket>();
    public virtual ICollection<HandoverTicket> HandoverTicketHandoverTos { get; set; } = new List<HandoverTicket>();
    public virtual ICollection<HandoverTicket> HandoverTicketOwners { get; set; } = new List<HandoverTicket>();
    public virtual ICollection<ReturnTicket> ReturnTicketOwners { get; set; } = new List<ReturnTicket>();
    public virtual ICollection<ReturnTicket> ReturnTicketReturnBies { get; set; } = new List<ReturnTicket>();

    [ForeignKey("RoleId")]
    public virtual AppRole Role { get; set; } = null!;
}
