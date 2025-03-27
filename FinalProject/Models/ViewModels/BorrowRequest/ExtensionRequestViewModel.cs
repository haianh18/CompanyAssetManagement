using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels.BorrowRequest
{
    public class ExtensionRequestViewModel
    {
        public int BorrowTicketId { get; set; }

        [Display(Name = "Tên tài sản")]
        public string AssetName { get; set; }

        [Display(Name = "Ngày mượn ban đầu")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? OriginalBorrowDate { get; set; }

        [Display(Name = "Ngày trả hiện tại")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? CurrentReturnDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả mới")]
        [Display(Name = "Ngày trả mới")]
        [DataType(DataType.Date)]
        public DateTime? NewReturnDate { get; set; }

        [Display(Name = "Số lượng")]
        public int? Quantity { get; set; }

        [Display(Name = "Lý do gia hạn")]
        public string Reason { get; set; }
    }
}
