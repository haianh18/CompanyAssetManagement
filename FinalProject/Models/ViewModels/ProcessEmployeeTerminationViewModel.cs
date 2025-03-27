using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels
{

    public class ProcessEmployeeTerminationViewModel
    {
        public int EmployeeId { get; set; }

        [Display(Name = "Tên nhân viên")]
        public string EmployeeName { get; set; }

        [Display(Name = "Phòng ban")]
        public string DepartmentName { get; set; }

        [Display(Name = "Tài sản đang bàn giao")]
        public List<HandoverTicket> ActiveHandoverAssets { get; set; }

        [Display(Name = "Tài sản đang mượn")]
        public List<BorrowTicket> ActiveBorrowedAssets { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã xác nhận")]
        [Display(Name = "Xác nhận nghỉ việc (Nhập 'TERMINATE-[ID nhân viên]')")]
        public string ConfirmText { get; set; }

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }
    }
}

