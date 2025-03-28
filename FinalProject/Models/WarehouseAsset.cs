using FinalProject.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.Models
{
    public partial class WarehouseAsset : EntityBase
    {
        // Các thuộc tính đã được kế thừa từ EntityBase:
        // public int Id { get; set; }
        // public DateTime? DateCreated { get; set; }
        // public DateTime? DateModified { get; set; }
        // public int? ActiveStatus { get; set; }
        // public bool IsDeleted { get; set; }
        // public DateTime? DeletedDate { get; set; }

        public int? WarehouseId { get; set; }
        public int? AssetId { get; set; }

        // Số lượng theo tình trạng
        public int? GoodQuantity { get; set; } = 0;
        public int? BrokenQuantity { get; set; } = 0;
        public int? FixingQuantity { get; set; } = 0;
        public int? DisposedQuantity { get; set; } = 0;

        // Theo dõi cả số lượng mượn và số lượng bàn giao
        public int? BorrowedGoodQuantity { get; set; } = 0;  // Số lượng đang được mượn
        public int? HandedOverGoodQuantity { get; set; } = 0;  // Số lượng đã bàn giao lâu dài

        // Tính toán số lượng khả dụng
        [NotMapped]
        public int TotalQuantity => (GoodQuantity ?? 0) + (BrokenQuantity ?? 0) +
                                    (FixingQuantity ?? 0);

        [NotMapped]
        public int AvailableQuantity => (GoodQuantity ?? 0) - (BorrowedGoodQuantity ?? 0) - (HandedOverGoodQuantity ?? 0);


        // Navigation properties
        public virtual Asset? Asset { get; set; }
        public virtual Warehouse? Warehouse { get; set; }
        public virtual ICollection<BorrowTicket> BorrowTickets { get; set; } = new List<BorrowTicket>();
        public virtual ICollection<DisposalTicketAsset> DisposalTicketAssets { get; set; } = new List<DisposalTicketAsset>();
        public virtual ICollection<HandoverTicket> HandoverTickets { get; set; } = new List<HandoverTicket>();
    }
}