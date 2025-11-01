using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.Account
{
    public class LoginVM
    {
    
        [Required(ErrorMessage = "الايميل مطلوب")]
        [EmailAddress(ErrorMessage = "هذا الايميل غير صالح")]
        public string Email {  get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [PasswordPropertyText]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
