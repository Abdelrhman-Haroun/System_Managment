using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.Supplier
{
    public class CreateSupplierVM
    {
        [Required(ErrorMessage = "اسم المورد مطلوب")]
        [Display(Name = "الاسم")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "الاسم يجب أن يكون بين 2 و 100 حرف")]
        public string Name { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
        [RegularExpression(@"^01\d{9}$", ErrorMessage = "رقم الجوال يجب أن يبدأ بـ 01 ويكون 11 رقمًا")]
        public string Phone { get; set; } 

        [Display(Name = "العنوان")]
        [StringLength(500, ErrorMessage = "العنوان لا يتجاوز 500 حرف")]
        public string? Address { get; set; }

        [Display(Name = "الرصيد الافتتاحي")]
        [Range(0, 999999999, ErrorMessage = "الرصيد يجب أن يكون رقم موجب")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? Balance { get; set; } = 0;
    }
}
