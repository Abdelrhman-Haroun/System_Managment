using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.Account
{
    public class ForgetPasswordVM
    {
        [Required(ErrorMessage = "الايميل مطلوب")]
        [EmailAddress(ErrorMessage = "هذا الايميل غير صالح")]
        [Display(Name = "البريد الالكترونى")]
        public string Email { get; set; }
    }

}
