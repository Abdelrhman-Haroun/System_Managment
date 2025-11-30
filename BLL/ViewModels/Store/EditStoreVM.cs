using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.Product
{
    public class EditStoreVM
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المخزن مطلوب")]
        [Display(Name = "الاسم")]
        [StringLength(100, ErrorMessage = "اسم المخزن لا يمكن أن يزيد عن 100 حرف")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "الوصف لا يمكن أن يزيد عن 500 حرف")]
        [Display(Name = "الوصف")]
        public string? Description { get; set; }

    }

}
