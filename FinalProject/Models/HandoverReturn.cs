using FinalProject.Enums;
using FinalProject.Models.Base;

namespace FinalProject.Models
{
    public partial class HandoverReturn : EntityBase
    {
        public int HandoverTicketId { get; set; }
        public int? ReturnById { get; set; }
        public int? ReceivedById { get; set; }
        public DateTime? ReturnDate { get; set; }
        public AssetStatus AssetConditionOnReturn { get; set; } = AssetStatus.GOOD;
        public string? Note { get; set; }

        // Navigation properties
        public virtual HandoverTicket HandoverTicket { get; set; }
        public virtual AppUser? ReturnBy { get; set; }
        public virtual AppUser? ReceivedBy { get; set; }
    }
}
