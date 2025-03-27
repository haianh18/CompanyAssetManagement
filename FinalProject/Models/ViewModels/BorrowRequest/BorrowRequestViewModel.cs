using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels.BorrowRequest
{
    public class BorrowRequestViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn tài sản cần mượn")]
        [Display(Name = "Tài sản")]
        public int WarehouseAssetId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng cần mượn")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả")]
        [Display(Name = "Ngày trả")]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
    }
}
