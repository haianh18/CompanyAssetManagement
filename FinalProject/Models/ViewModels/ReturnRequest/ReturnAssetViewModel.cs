using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels.ReturnRequest
{
    public class ReturnAssetViewModel
    {
        public int BorrowTicketId { get; set; }

        [Display(Name = "Tên tài sản")]
        public string AssetName { get; set; }

        [Display(Name = "Ngày mượn")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? BorrowDate { get; set; }

        [Display(Name = "Ngày trả theo kế hoạch")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? ScheduledReturnDate { get; set; }

        [Display(Name = "Số lượng")]
        public int? Quantity { get; set; }

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }

        [Display(Name = "Trả sớm")]
        public bool IsEarlyReturn { get; set; }
    }
}
