using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels.ReturnRequest
{
    public class CreateReturnRequestViewModel
    {
        public int BorrowTicketId { get; set; }

        [Display(Name = "Tên tài sản")]
        public string AssetName { get; set; }

        [Display(Name = "Người mượn")]
        public string BorrowerName { get; set; }

        [Display(Name = "Ngày mượn")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? BorrowDate { get; set; }

        [Display(Name = "Ngày trả theo kế hoạch")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Số lượng")]
        public int? Quantity { get; set; }

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }
    }
}
