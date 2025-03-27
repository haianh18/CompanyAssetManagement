using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels.Handover
{
    public class CreateHandoverReturnViewModel
    {
        public int HandoverTicketId { get; set; }

        [Display(Name = "Tên tài sản")]
        public string AssetName { get; set; }

        [Display(Name = "Người trả")]
        public string ReturnName { get; set; }

        [Display(Name = "Người trả ID")]
        public int? ReturnById { get; set; }

        [Display(Name = "Ngày bàn giao")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? HandoverDate { get; set; }

        [Display(Name = "Số lượng")]
        public int? Quantity { get; set; }

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }
    }
}
