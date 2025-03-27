using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels.ReturnRequest
{
    public class RejectReturnViewModel
    {
        public int ReturnTicketId { get; set; }

        [Display(Name = "Tên tài sản")]
        public string AssetName { get; set; }

        [Display(Name = "Người trả")]
        public string ReturnedBy { get; set; }

        [Display(Name = "Ghi chú của người trả")]
        public string Notes { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do từ chối")]
        [Display(Name = "Lý do từ chối")]
        public string RejectionReason { get; set; }
    }
}
