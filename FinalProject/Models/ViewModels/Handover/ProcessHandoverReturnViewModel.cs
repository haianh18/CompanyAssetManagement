using FinalProject.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels.Handover
{
    public class ProcessHandoverReturnViewModel
    {
        public int HandoverReturnId { get; set; }
        public int HandoverTicketId { get; set; }

        [Display(Name = "Tên tài sản")]
        public string AssetName { get; set; }

        [Display(Name = "Người trả")]
        public string ReturnedBy { get; set; }

        [Display(Name = "Ngày trả")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Số lượng")]
        public int? Quantity { get; set; }

        [Display(Name = "Ghi chú của người trả")]
        public string Notes { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tình trạng tài sản")]
        [Display(Name = "Tình trạng tài sản khi nhận lại")]
        public AssetStatus AssetCondition { get; set; }

        [Display(Name = "Ghi chú thêm")]
        public string AdditionalNotes { get; set; }
    }
}
