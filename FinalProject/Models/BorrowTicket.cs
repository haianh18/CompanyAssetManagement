using FinalProject.Enums;
using FinalProject.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public partial class BorrowTicket : EntityBase
    {
        // Các thuộc tính đã được kế thừa từ EntityBase:
        // public int Id { get; set; }
        // public DateTime? DateCreated { get; set; }
        // public DateTime? DateModified { get; set; }
        // public int? ActiveStatus { get; set; }
        // public bool IsDeleted { get; set; }
        // public DateTime? DeletedDate { get; set; }

        public int? WarehouseAssetId { get; set; }
        public int? BorrowById { get; set; }
        public string? Note { get; set; }
        public int? OwnerId { get; set; }
        public int? Quantity { get; set; }
        public DateTime? ReturnDate { get; set; }
        public TicketStatus ApproveStatus { get; set; } = TicketStatus.Pending;

        // New property to track which condition of assets were borrowed
        // (assuming only good condition assets can be borrowed)
        public AssetStatus BorrowedAssetStatus { get; set; } = AssetStatus.GOOD;

        // Extension properties
        public int? ExtensionBorrowTicketId { get; set; }
        public bool IsExtended { get; set; } = false;
        public DateTime? ExtensionRequestDate { get; set; }
        public TicketStatus ExtensionApproveStatus { get; set; } = TicketStatus.Pending;
        public bool IsReturned { get; set; } = false;

        // Navigation properties
        public virtual AppUser? BorrowBy { get; set; }
        public virtual AppUser? Owner { get; set; }
        public virtual ICollection<ReturnTicket> ReturnTickets { get; set; } = new List<ReturnTicket>();
        public virtual WarehouseAsset? WarehouseAsset { get; set; }

        [ForeignKey("ExtensionBorrowTicketId")]
        public virtual BorrowTicket? OriginalBorrowTicket { get; set; }
        public virtual ICollection<BorrowTicket> ExtendedBorrowTickets { get; set; } = new List<BorrowTicket>();
    }
}