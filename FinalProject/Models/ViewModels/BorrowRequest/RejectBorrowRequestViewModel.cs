using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels.BorrowRequest
{
    public class RejectBorrowRequestViewModel
    {
        public int BorrowTicketId { get; set; }

        [Display(Name = "Tên tài sản")]
        public string AssetName { get; set; }

        [Display(Name = "Người mượn")]
        public string BorrowerName { get; set; }

        [Display(Name = "Số lượng")]
        public int? Quantity { get; set; }

        [Display(Name = "Ngày mượn")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? BorrowDate { get; set; }

        [Display(Name = "Ngày trả dự kiến")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? ReturnDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do từ chối")]
        [Display(Name = "Lý do từ chối")]
        public string RejectionReason { get; set; }
    }
}
