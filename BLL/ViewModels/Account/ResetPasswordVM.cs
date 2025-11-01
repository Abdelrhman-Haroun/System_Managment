using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.Account
{
    public class ResetPasswordVM
    {
        [Required(ErrorMessage = "الايميل مطلوب")]
        [EmailAddress(ErrorMessage = "هذا الايميل غير صالح")]
        public string Email { get; set; }


        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [PasswordPropertyText]
        public string Password { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [PasswordPropertyText]
        [Compare("Password", ErrorMessage = "كلمة المرور غير متطابقة")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Code { get; set; } // Token from reset link
        public string UserId { get; set; } // User ID from reset link
    }
}
