using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập?")]
        public bool RememberMe { get; set; }
    }

    public class LoginWith2faViewModel
    {
        [Required(ErrorMessage = "Mã xác thực là bắt buộc")]
        [StringLength(7, ErrorMessage = "{0} phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Mã xác thực")]
        public string TwoFactorCode { get; set; }

        [Display(Name = "Ghi nhớ thiết bị này")]
        public bool RememberMachine { get; set; }

        public bool RememberMe { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}

