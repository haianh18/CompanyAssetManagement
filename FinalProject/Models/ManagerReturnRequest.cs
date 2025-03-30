using FinalProject.Enums;
using FinalProject.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public class ManagerReturnRequest : EntityBase
    {
        public int BorrowTicketId { get; set; }
        public int RequestedById { get; set; } // Manager ID
        public string Reason { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public int? RelatedReturnTicketId { get; set; } // Liên kết đến ReturnTicket khi user trả tài sản

        public TicketStatus Status { get; set; } = TicketStatus.Pending; // Sử dụng enum TicketStatus đã có

        // Navigation properties
        [ForeignKey("BorrowTicketId")]
        public virtual BorrowTicket BorrowTicket { get; set; }

        [ForeignKey("RequestedById")]
        public virtual AppUser RequestedBy { get; set; }

        [ForeignKey("RelatedReturnTicketId")]
        public virtual ReturnTicket ReturnTicket { get; set; }
    }
}
