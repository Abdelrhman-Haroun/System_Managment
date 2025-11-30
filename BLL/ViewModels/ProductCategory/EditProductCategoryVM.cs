using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.ProductCategory
{
   public class EditProductCategoryVM
    {
        [Required]
        public int Id { get; set; } 

        [Required(ErrorMessage = "اسم الفئة مطلوب")]
        [Display(Name = "الاسم")]
        [StringLength(100, ErrorMessage = "اسم الفئة لا يمكن أن يزيد عن 100 حرف")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "الوصف لا يمكن أن يزيد عن 500 حرف")]
        [Display(Name = "الوصف")]
        public string? Description { get; set; }
    }
}
