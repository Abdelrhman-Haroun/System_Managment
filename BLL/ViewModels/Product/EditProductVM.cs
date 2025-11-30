using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.Product
{
    public class EditProductVM
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        [StringLength(100)]
        [Display(Name = "الاسم")]
        public string Name { get; set; }

        [StringLength(500)]
        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "المخزن مطلوب")]
        [Display(Name = "المخزن")]
        public int StoreId { get; set; }

        [Required(ErrorMessage = "نوع المنتج مطلوب")]
        [Display(Name = "نوع المنتج")]
        public ProductType ProductType { get; set; }


        [Required(ErrorMessage = "الفئة مطلوبة")]
        [Display(Name = "فئة المنتج")]
        public int CategoryId { get; set; }

    }

}
