using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.Account
{
    public class RegisterationVM
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        public string FullName { get; set; }

        [Required (ErrorMessage ="الايميل مطلوب")]
        [EmailAddress(ErrorMessage ="هذا الايميل غير صالح")]
        public string Email { get; set; }

        [Required (ErrorMessage ="رقم الهاتف مطلوب")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage ="كلمة المرور مطلوبة")]
        [PasswordPropertyText]
        public string Password { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [PasswordPropertyText]
        [Compare("Password",ErrorMessage = "كلمة المرور غير متطابقة")]
        public string ConfirmPassword { get; set; }

        public IFormFile? ProfilePicture { get; set; }
        [Required(ErrorMessage = "درجة المستخدم مطلوبة")]
        public string UserRole { get; set; }
    }
}
