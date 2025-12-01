using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.Supplier
{
   public class EditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المورد مطلوب")]
        [Display(Name = "الاسم")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        [Phone]
        [RegularExpression(@"^01\d{9}$", ErrorMessage = "رقم الجوال يجب أن يبدأ بـ 01 ويكون 11 رقمًا")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "العنوان")]
        [StringLength(500)]
        public string? Address { get; set; }

        public decimal? Balance { get; set; }
    }
}
