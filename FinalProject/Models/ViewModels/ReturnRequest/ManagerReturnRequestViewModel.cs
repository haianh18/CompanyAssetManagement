using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels.ReturnRequest
{
    public class ManagerReturnRequestViewModel
    {
        public int BorrowTicketId { get; set; }

        [Display(Name = "Tài sản")]
        public string AssetName { get; set; }

        [Display(Name = "Người mượn")]
        public string BorrowerName { get; set; }

        [Display(Name = "Ngày mượn")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? BorrowDate { get; set; }

        [Display(Name = "Ngày hẹn trả")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Số lượng")]
        public int? Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do yêu cầu trả")]
        [Display(Name = "Lý do yêu cầu trả")]
        public string Reason { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời hạn trả")]
        [Display(Name = "Thời hạn trả")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }
    }
}
