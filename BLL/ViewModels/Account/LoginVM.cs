using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.Account
{
    public class LoginVM
    {
    
        [Required(ErrorMessage = "الايميل مطلوب")]
        [EmailAddress(ErrorMessage = "هذا الايميل غير صالح")]
        [Display(Name = "البريد الالكترونى")]
        public string Email {  get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [PasswordPropertyText]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }
        [Display(Name = "تذكرنى")]
        public bool RememberMe { get; set; }
    }
}
