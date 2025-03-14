using FinalProject.Models.Base;
using System;
using System.Collections.Generic;

namespace FinalProject.Models
{
    public partial class DisposalTicket : EntityBase
    {
        // Các thuộc tính đã được kế thừa từ EntityBase:
        // public int Id { get; set; }
        // public DateTime? DateCreated { get; set; }
        // public DateTime? DateModified { get; set; }
        // public int? ActiveStatus { get; set; }
        // public bool IsDeleted { get; set; }
        // public DateTime? DeletedDate { get; set; }

        public int? DisposalById { get; set; }
        public string? Note { get; set; }
        public int? OwnerId { get; set; }
        public string? Reason { get; set; }

        public virtual AppUser? DisposalBy { get; set; }
        public virtual ICollection<DisposalTicketAsset> DisposalTicketAssets { get; set; } = new List<DisposalTicketAsset>();
        public virtual AppUser? Owner { get; set; }
    }
}