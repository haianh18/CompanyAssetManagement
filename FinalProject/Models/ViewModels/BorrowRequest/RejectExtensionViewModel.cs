using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels.BorrowRequest
{
    public class RejectExtensionViewModel
    {
        public int BorrowTicketId { get; set; }

        [Display(Name = "Tên tài sản")]
        public string AssetName { get; set; }

        [Display(Name = "Người mượn")]
        public string BorrowerName { get; set; }

        [Display(Name = "Ngày trả hiện tại")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? CurrentReturnDate { get; set; }

        [Display(Name = "Ngày yêu cầu gia hạn")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? RequestDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do từ chối")]
        [Display(Name = "Lý do từ chối")]
        public string RejectionReason { get; set; }
    }
}
