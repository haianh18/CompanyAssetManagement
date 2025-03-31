using FinalProject.Models;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int TotalWarehouses { get; set; }
        public int TotalAssets { get; set; }
        public int TotalCategories { get; set; }
        public int TotalDepartments { get; set; }
        public List<AppUser> RecentUsers { get; set; } = new List<AppUser>();
    }

    public class UserManagementViewModel
    {
        public List<AppUser> Users { get; set; } = new List<AppUser>();
        public List<AppRole> Roles { get; set; } = new List<AppRole>();
        public List<Department> Departments { get; set; } = new List<Department>();
        public bool ShowInactive { get; set; } = false;
        public string SearchString { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class UserCreateViewModel
    {
        public AppUser User { get; set; } = new AppUser();

        // Loại bỏ việc thiết lập Role trong AppUser
        // Chỉ dùng RoleId để xác định vai trò

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public int SelectedRoleId { get; set; }

        [Display(Name = "Phòng ban")]
        public int? SelectedDepartmentId { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public List<AppRole> AvailableRoles { get; set; } = new List<AppRole>();
        public List<Department> AvailableDepartments { get; set; } = new List<Department>();
    }

    public class UserEditViewModel
    {
        public AppUser User { get; set; } = new AppUser();

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn vai trò")]
        public int SelectedRoleId { get; set; }

        public int? SelectedDepartmentId { get; set; }

        public List<AppRole> AvailableRoles { get; set; } = new List<AppRole>();

        public List<Department> AvailableDepartments { get; set; } = new List<Department>();

        public bool ResetPassword { get; set; } = false;

        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt.")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Custom validation for password reset
        public bool ValidatePasswordReset()
        {
            // If reset password is checked, both new password and confirm password are required
            if (ResetPassword)
            {
                if (string.IsNullOrWhiteSpace(NewPassword))
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    return false;
                }

                return NewPassword == ConfirmPassword;
            }

            // If not resetting password, validation passes
            return true;
        }
    }

    public class WarehouseManagementViewModel
    {
        public List<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
        public bool ShowInactive { get; set; } = false;
        public string SearchString { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }

    public class AdminReportViewModel
    {
        public int TotalAssets { get; set; }
        public double TotalAssetsValue { get; set; }
        public int ActiveAssets { get; set; }
        public int BorrowedAssets { get; set; }
        public int DisposedAssets { get; set; }
        public Dictionary<string, int> AssetsByCategory { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AssetsByWarehouse { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> BorrowTicketsByMonth { get; set; } = new Dictionary<string, int>();
    }
}