using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.EmployeeType
{
    public class CreateEmployeeTypeVM
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }
    }
}
